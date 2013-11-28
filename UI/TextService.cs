using System.Collections.Generic;
using erecruit.Composition;
using Mut.Data;

namespace Mut.Models
{
	[Export, TransactionScoped]
	public class TextService
	{
		public static string AttachmentDomain( string textId ) { return "TextAttachment." + textId; }
	}
}