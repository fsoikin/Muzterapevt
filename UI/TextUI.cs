using System;
using System.Linq;
using System.Collections.Generic;
using erecruit.Composition;
using Mut.Data;

namespace Mut
{
	[Export, TransactionScoped]
	public class TextUI
	{
		[Import] public IRepository<Data.Text> Texts { get; set; }
		[Import] public IAuthService Auth { get; set; }
		[Import] public ISiteService Sites { get; set; }

		public TextModel TextModel( string id ) {
			var t = Texts.All.FirstOrDefault( x => x.Id == id && x.SiteId == Sites.CurrentSiteId );
			return new TextModel {
				Id = id,
				Html = t == null ? "" : t.HtmlText,
				AllowEdit = Auth.CurrentActor.IsAdmin
			};
		}
	}

	public class TextModel
	{
		public string Id { get; set; }
		public string Html { get; set; }
		public bool AllowEdit { get; set; }
	}
}