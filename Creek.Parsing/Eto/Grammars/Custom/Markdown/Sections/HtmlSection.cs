using Lib.Parsing.Eto.Parsers;

namespace Lib.Parsing.Eto.Grammars.Custom.Markdown.Sections
{
	public class HtmlSection : SequenceParser, IMarkdownReplacement
	{
		public HtmlSection()
		{
			Name = "html";
		}

		public void Initialize(MarkdownGrammar grammar)
		{
			Add(new HtmlElementParser { Name = "html" }, -Terms.blankLine);
		}

		#if PERF_TEST
		protected override int InnerParse(ParseArgs args)
		{
			return base.InnerParse(args);
		}
		#endif

		public void Transform(Match match, MarkdownReplacementArgs args)
		{
			var text = match.Text;
			if (text.Contains("<script") || text.Contains("javascript:"))
				args.Output.AppendUnixLine(MarkdownEncoding.Encode(text));
			else
				args.Output.AppendUnixLine(text);
		}
	}
}

