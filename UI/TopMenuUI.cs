using System.Collections.Generic;
using System.Linq;
using erecruit.Composition;
using Mut.Data;

namespace Mut
{
	[Export, TransactionScoped]
	public class TopMenuUI
	{
		[Import] public IRepository<NavigationItem> Items { get; set; }
		[Import] public IAuthService Auth { get; set; }
		
		public TopMenuModel GetTopMenu() {
			return new TopMenuModel {
				Items = GetItemsForParent( null ).Select( ToJson ),
				AllowEdit = Auth.CurrentActor.IsAdmin
			};
		}

		public IQueryable<NavigationItem> GetItemsForParent( int? parentId ) {
			return parentId != null 
				? Items.All.Where( x => x.Parent.Id == parentId )
				: Items.All.Where( x => x.Parent == null );
		}

		public static JS.Menu.Item ToJson( NavigationItem i ) {
			return new JS.Menu.Item { Id = i.Id, Text = i.Text, Link = i.Link, Order = i.Order, SubItems = i.Children.Select( ToJson ) };
		}
	}

	public class TopMenuModel
	{
		public int? ParentId { get; set; }
		public IEnumerable<JS.Menu.Item> Items { get; set; }
		public int CurrentItemId { get; set; }
		public bool AllowEdit { get; set; }
	}
}