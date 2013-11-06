using System.Collections.Generic;
using erecruit.Composition;
using Mut.Data;

namespace Mut.Models
{
	[Export, TransactionScoped]
	public class TextService
	{
		public static string AttachmentDomain( int textId ) { return "TextAttachment." + textId; }
	}
}