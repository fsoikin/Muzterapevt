using System;
using System.Web;
using erecruit.Composition;
using log4net;

namespace Mut.UI
{
	public class TypeScriptModuleAttribute : Attribute
	{
		public TypeScriptModuleAttribute( string module ) { }
	}
}