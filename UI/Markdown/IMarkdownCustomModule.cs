using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using erecruit.JS;
using erecruit.Utils;

namespace Mut.UI
{
	public interface IMarkdownCustomModule
	{
		string Name { get; }
		IEnumerable<string> Parameters { get; }
		ClassRef GetClassRef( IDictionary<string, string> args );
	}

	public static class MarkdownCustomModule
	{
		public static IMarkdownCustomModule Create( string name, ClassRef classRef ) {
			Contract.Requires( !String.IsNullOrEmpty( name ) );
			Contract.Requires( classRef != null );
			Contract.Ensures( Contract.Result<IMarkdownCustomModule>() != null );
			return Create( name, null, _ => classRef );
		}

		public static IMarkdownCustomModule Create( string name, IEnumerable<string> args, Func<IDictionary<string, string>, ClassRef> getClassRef ) {
			Contract.Requires( getClassRef != null );
			Contract.Requires( !String.IsNullOrEmpty( name ) );
			Contract.Ensures( Contract.Result<IMarkdownCustomModule>() != null );
			return new Implementation( name, args, getClassRef );
		}

		class Implementation : IMarkdownCustomModule
		{
			public string Name { get; private set; }

			public IEnumerable<string> Parameters { get; private set; }

			private readonly Func<IDictionary<string, string>, ClassRef> _getClassRef;
			public ClassRef GetClassRef( IDictionary<string, string> args ) {
				return _getClassRef( args );
			}

			public Implementation( string name, IEnumerable<string> args, Func<IDictionary<string, string>, ClassRef> getClassRef ) {
				Name = name;
				Parameters = args.EmptyIfNull();
				_getClassRef = getClassRef;
			}
		}
	}
}