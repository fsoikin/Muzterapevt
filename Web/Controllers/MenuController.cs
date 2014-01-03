using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	[EditPermission]
	public class MenuController : Controller
	{
		[Import] public IRepository<NavigationItem> MenuItems { get; set; }

		public JsonResponse<IEnumerable<JS.Menu.Item>> Load( string menuId )
		{
			return JsonResponse.Catch( () => TopMenu.GetItemsForParent( null, menuId ).Select( TopMenu.ToJson ), Log );
		}

		public JsonResponse<IEnumerable<JS.Menu.Item>> UpdateSubItems( JS.Menu.SubItemsSaveRequest req ) {
			return JsonResponse.Catch( () => {
				var items = TopMenu.GetItemsForParent( null, req.MenuId ).ToList();
				UpdateItems( null, req.MenuId, items, req.Items.EmptyIfNull() );
				UnitOfWork.Commit();

				return Load( req.MenuId );
			}, Log );
		}

		private void UpdateItems( NavigationItem parent, string menuId, IEnumerable<NavigationItem> existing, IEnumerable<JS.Menu.Item> incoming ) {
			var toAddIds = incoming.Select( i => i.Id ).Except( existing.Select( i => i.Id ) );
			var toRemoveIds = existing.Select( i => i.Id ).Except( incoming.Select( i => i.Id ) );

			var toRemove = from e in existing
										 join id in toRemoveIds on e.Id equals id into rs
										 where rs.Any()
										 select e;
			var toAdd = from i in incoming
									join id in toAddIds on i.Id equals id into ads
									where ads.Any()
									select i;

			toRemove.ToList().ForEach( Remove );

			foreach ( var i in toAdd ) {
				var item = MenuItems.Add( new NavigationItem { Parent = parent, MenuId = menuId } );
				Copy( item, i );
				UpdateItems( item, menuId, item.Children, i.SubItems.EmptyIfNull() );
			}

			foreach ( var x in existing.Join( incoming, i => i.Id, i => i.Id, ( e, i ) => new { e, i } ) ) {
				Copy( x.e, x.i );
				UpdateItems( x.e, menuId, x.e.Children, x.i.SubItems.EmptyIfNull() );
			}
		}

		private void Remove( NavigationItem i ) {
			i.Children.ToList().ForEach( Remove );
			MenuItems.Remove( i );
		}

		private void Copy( NavigationItem e, JS.Menu.Item i ) {
			e.Link = i.Link; e.Text = i.Text; e.Order = i.Order;
		}
	}
}