using System.Collections.Generic;

namespace Mut.UI
{
	public interface IMarkdownProvider
	{
		IEnumerable<MarkdownNodeDefinition<MarkdownParseArgs>> Definitions { get; }
	}

	public static class MarkdownProvider
	{
		public static IMarkdownProvider Create( params MarkdownNodeDefinition<MarkdownParseArgs>[] nodes ) {
			return new Implementation { Definitions = nodes };
		}

		public static IMarkdownProvider Create( IEnumerable<MarkdownNodeDefinition<MarkdownParseArgs>> nodes ) {
			return new Implementation { Definitions = nodes };
		}

		class Implementation : IMarkdownProvider
		{
			public IEnumerable<MarkdownNodeDefinition<MarkdownParseArgs>> Definitions {
				get;
				set;
			}
		}
	}
}