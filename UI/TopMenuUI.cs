using System.Collections.Generic;
using System.Linq;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;

namespace Mut
{
	public interface ITopMenuUI
	{
		TopMenuModel GetTopMenu( string menuId );
		IQueryable<NavigationItem> GetItemsForParent( int? parentId, string menuId = null );
		JS.Menu.Item ToJson( NavigationItem i );
	}

	[Export, TransactionScoped]
	class TopMenuUI : ITopMenuUI
	{
		[Import] public IRepository<NavigationItem> Items { get; set; }
		[Import] public IAuthService Auth { get; set; }
		
		public TopMenuModel GetTopMenu( string menuId ) {
			return new TopMenuModel {
				MenuId = menuId,
				Items = GetItemsForParent( null, menuId ).Select( ToJson ),
				AllowEdit = Auth.CurrentActor.IsAdmin
			};
		}

		public IQueryable<NavigationItem> GetItemsForParent( int? parentId, string menuId = null ) {
			var res = parentId != null 
				? Items.All.Where( x => x.Parent.Id == parentId )
				: Items.All.Where( x => x.Parent == null );
			if ( !menuId.NullOrEmpty() ) res = res.Where( i => i.MenuId == menuId );
			return res;
		}

		public JS.Menu.Item ToJson( NavigationItem i ) {
			return new JS.Menu.Item { Id = i.Id, Text = i.Text, Link = i.Link, Order = i.Order, SubItems = i.Children.Select( ToJson ) };
		}
	}

	public class TopMenuModel
	{
		public string MenuId { get; set; }
		public IEnumerable<JS.Menu.Item> Items { get; set; }
		public int CurrentItemId { get; set; }
		public bool AllowEdit { get; set; }
	}
}