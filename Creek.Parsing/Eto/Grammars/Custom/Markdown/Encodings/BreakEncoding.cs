using Lib.Parsing.Eto.Parsers;

namespace Lib.Parsing.Eto.Grammars.Custom.Markdown.Encodings
{
	public class BreakEncoding : SequenceParser, IMarkdownReplacement
	{
		public bool AddLinesBefore { get { return false; } }

		public BreakEncoding()
		{
			Name = "br";
		}

		public void Initialize(MarkdownGrammar grammar)
		{
			Add("  ", Terminals.Eol);
		}

#if PERF_TEST
		protected override int InnerParse(ParseArgs args)
		{
			return base.InnerParse(args);
		}
#endif

		public void Transform(Match match, MarkdownReplacementArgs args)
		{
			args.Output.Append("<br />\n");
		}
	}
}