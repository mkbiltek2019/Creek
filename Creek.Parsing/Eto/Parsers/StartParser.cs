using Eto.Parse;

namespace Lib.Parsing.Eto.Parsers
{
	public class StartParser : Parser
	{
		protected StartParser(StartParser other, ParserCloneArgs chain)
			: base(other, chain)
		{
		}

		public StartParser()
		{
		}

		protected override int InnerParse(ParseArgs args)
		{
			var pos = args.Scanner.Position;
			return pos <= 0 ? 0 : -1;
		}

		public override Parser Clone(ParserCloneArgs args)
		{
			return new StartParser(this, args);
		}
	}
}
