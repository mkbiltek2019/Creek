using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Eto.Parse;
using Lib.Parsing.Eto;
using Lib.Parsing.Eto.Parsers;
using Lib.Parsing.Eto.Scanners;
using Lib.Parsing.Eto.Writers;

namespace Creek.Parsing.Eto.Grammars
{
    /// <summary>
    /// Grammar to build a parser grammar using Backus-Naur Form
    /// </summary>
    /// <remarks>
    /// See https://en.wikipedia.org/wiki/Backus-Naur_Form.
    /// 
    /// This implements certain enhancements, such as grouping using brackets, repeating using curly braces, and
    /// optional values using square brackets.
    /// 
    /// This grammar is not thread-safe.
    /// </remarks>
    public class BnfGrammar : Grammar
    {
        private readonly Dictionary<string, Parser> baseLookup =
            new Dictionary<string, Parser>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Parser dq = Terminals.Set('"');
        private readonly Parser list;
        private readonly Parser listRepeat;
        private readonly Parser literal;
        private readonly Parser optionalRule;
        private readonly Parser repeatRule;
        private readonly Parser rule;
        private readonly Parser ruleName;
        private readonly LiteralTerminal ruleSeparator = new LiteralTerminal("::=");
        private readonly Parser sq = Terminals.Set('\'');
        private readonly Parser sws = Terminals.SingleLineWhiteSpace.Repeat(0);
        private readonly Parser ws = Terminals.WhiteSpace.Repeat(0);

        private Dictionary<string, Parser> parserLookup =
            new Dictionary<string, Parser>(StringComparer.InvariantCultureIgnoreCase);

        private string startParserName;

        public BnfGrammar(bool enhanced = true)
            : base("bnf")
        {
            if (enhanced)
            {
                foreach (PropertyInfo property in typeof (Terminals).GetProperties())
                {
                    if (typeof (Parser).IsAssignableFrom(property.PropertyType))
                    {
                        var parser = property.GetValue(null, null) as Parser;
                        baseLookup[property.Name] = parser.Named(property.Name);
                    }
                }
            }

            SequenceParser lineEnd = sws & +(sws & Terminals.Eol);

            literal = (
                          (sq & (+!sq).Named("value").Optional() & sq)
                          | (dq & (+!dq).Named("value").Optional() & dq)
                      ).Named("parser");


            RuleNameParser = "<" & Terminals.Set('>').Inverse().Repeat().Named("name") & ">";

            RuleParser = new AlternativeParser(); // defined later 

            TermParser = literal | (ruleName = RuleNameParser.Named("parser"));
            if (enhanced)
            {
                TermParser.Items.Add('(' & sws & RuleParser & sws & ')');
                TermParser.Items.Add(repeatRule = ('{' & sws & RuleParser & sws & '}').Named("parser"));
                TermParser.Items.Add(optionalRule = ('[' & sws & RuleParser & sws & ']').Named("parser"));
            }

            list =
                (TermParser.Named("term") &
                 -(~((+Terminals.SingleLineWhiteSpace).Named("ws")) & TermParser.Named("term"))).Named("parser");

            listRepeat = (list.Named("list") & ws & '|' & sws & ~(RuleParser.Named("expression"))).Named("parser");
            RuleParser.Items.Add(listRepeat);
            RuleParser.Items.Add(list);

            rule =
                (~lineEnd & sws & RuleNameParser.Named("ruleName") & ws & ruleSeparator & sws & RuleParser & lineEnd).
                    Named("parser");
            Expresssions = new AlternativeParser();
            Expresssions.Items.Add(rule);

            Inner = ws & +Expresssions & ws;

            AttachEvents();
        }

        /// <summary>
        /// Gets or sets the separator for rules, which is usually '::=' for BNF
        /// </summary>
        /// <value>The rule separator</value>
        protected string RuleSeparator
        {
            get { return ruleSeparator.Value; }
            set { ruleSeparator.Value = value; }
        }

        /// <summary>
        /// Gets the rule parser for each of the rule types
        /// </summary>
        /// <value>The rule parser.</value>
        protected AlternativeParser RuleParser { get; private set; }

        /// <summary>
        /// Gets the term parser for each term, such as optional, repeating, grouping, etc
        /// </summary>
        /// <value>The term parser.</value>
        protected AlternativeParser TermParser { get; private set; }

        /// <summary>
        /// Gets the expresssions parser to support different expressions. By default, only one expression
        /// is defined: RuleNameParser &amp; RuleSeparator &amp; RuleParser &amp; EOL
        /// </summary>
        /// <value>The expresssions this parser supports</value>
        protected AlternativeParser Expresssions { get; private set; }

        /// <summary>
        /// Gets the rule name parser, by default is a string surrounded by angle brackets
        /// </summary>
        /// <value>The rule name parser.</value>
        protected SequenceParser RuleNameParser { get; private set; }

        /// <summary>
        /// Gets the parsed rules
        /// </summary>
        /// <value>The rules.</value>
        public Dictionary<string, Parser> Rules
        {
            get { return parserLookup; }
            protected set { parserLookup = value; }
        }

        private void AttachEvents()
        {
            ruleName.Matched += m =>
                                    {
                                        Parser parser;
                                        string name = m["name"].Text;
                                        if (!parserLookup.TryGetValue(name, out parser) &&
                                            !baseLookup.TryGetValue(name, out parser))
                                        {
                                            parser = Terminals.LetterOrDigit.Repeat();
                                            parser.Name = name;
                                        }
                                        m.Tag = parser;
                                    };

            literal.Matched += m => m.Tag = new LiteralTerminal(m["value"].Text);
            optionalRule.Matched += m => m.Tag = new OptionalParser((Parser) m["parser"].Tag);
            repeatRule.Matched += m => m.Tag = new RepeatParser((Parser) m["parser"].Tag, 0) {Separator = sws};
            list.Matched += m =>
                                {
                                    if (m.Matches.Count > 1)
                                    {
                                        var parser = new SequenceParser();
                                        foreach (Match child in m.Matches)
                                        {
                                            if (child.Parser.Name == "ws")
                                                parser.Items.Add(sws);
                                            else if (child.Parser.Name == "term")
                                                parser.Items.Add((Parser) child["parser"].Tag);
                                        }
                                        m.Tag = parser;
                                    }
                                    else
                                    {
                                        m.Tag = m["term"]["parser"].Tag;
                                    }
                                };

            listRepeat.Matched += m =>
                                      {
                                          // collapse alternatives to one alternative parser
                                          var parser = (Parser) m["expression"]["parser"].Tag;
                                          AlternativeParser alt = parser as AlternativeParser ??
                                                                  new AlternativeParser(parser);
                                          alt.Items.Insert(0, (Parser) m["list"]["parser"].Tag);
                                          m.Tag = alt;
                                      };

            rule.Matched += m =>
                                {
                                    var parser = (UnaryParser) m.Tag;
                                    parser.Inner = (Parser) m["parser"].Tag;
                                    m.Tag = parser;
                                };
            rule.PreMatch += m =>
                                 {
                                     string name = m["ruleName"]["name"].Text;
                                     Parser parser;
                                     if (name == startParserName)
                                         parser = new Grammar(name);
                                     else
                                         parser = new UnaryParser(name);
                                     m.Tag = parser;
                                     parserLookup[parser.Name] = parser;
                                 };
        }

        protected override int InnerParse(ParseArgs args)
        {
            parserLookup = new Dictionary<string, Parser>();
            return base.InnerParse(args);
        }

        public Grammar Build(string bnf, string startParserName)
        {
            this.startParserName = startParserName;
            Parser parser;
            GrammarMatch match = Match(new StringScanner(bnf));

            if (!match.Success)
            {
                throw new FormatException(string.Format("Error parsing bnf: \n{0}", match.ErrorMessage));
            }
            if (!parserLookup.TryGetValue(startParserName, out parser))
                throw new ArgumentException("the topParser specified is not found in this bnf");
            return parser as Grammar;
        }

        public string ToCode(string bnf, string startParserName, string className = "GeneratedGrammar")
        {
            using (var writer = new StringWriter())
            {
                ToCode(bnf, startParserName, writer, className);
                return writer.ToString();
            }
        }

        public void ToCode(string bnf, string startParserName, TextWriter writer, string className = "GeneratedGrammar")
        {
            Grammar parser = Build(bnf, startParserName);
            var iw = new IndentedTextWriter(writer, "    ");

            iw.WriteLine("/* Date Created: {0}, Source BNF:", DateTime.Now);
            iw.Indent ++;
            foreach (string line in bnf.Split('\n'))
                iw.WriteLine(line);
            iw.Indent --;
            iw.WriteLine("*/");

            var parserWriter = new CodeParserWriter
                                   {
                                       ClassName = className
                                   };
            parserWriter.Write(parser, writer);
        }
    }
}