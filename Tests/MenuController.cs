using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Moq;
using Mut.Data;
using Xunit;

namespace Mut.Tests
{
	public class MenuController
	{
		[Fact]
		public void Should_insert_menu_items_into_empty_menu() {
			t( ( repo, mc ) => {
				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest { Items = ji( "A", "B" ) } );	
				Assert.Equal( new[] { "A", "B" }, repo.Items.Select( i => i.Text ).OrderBy( x => x ) );
				Assert.Equal( new[] { 1, 2 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_rename_items() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B" ) );
				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { new JS.Menu.Item { Text = "X", Id = 1 }, new JS.Menu.Item { Text = "Y", Id = 2 } }
				} );
				Assert.Equal( new[] { "X:1", "Y:2" }, repo.Items.Select( i => i.Text + ":" + i.Id ).OrderBy( x => x ) );
			} );
		}

		[Fact]
		public void Should_remove_items() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B" ) );
				repo.NextId = 5;
				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest { Items = ji( "X" ) } );
				Assert.Equal( "X", repo.Items.Single().Text );
				Assert.Equal( 5, repo.Items.Single().Id );
			} );
		}

		[Fact]
		public void Should_add_subitems_to_empty_submenu() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B" ) );
				repo.NextId = 5;
				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { new JS.Menu.Item { Text = "A", Id = 1, SubItems = ji( "X", "Y" ) } }
				} );
				Assert.Equal( new[] { "A", "X", "Y" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 5, 6 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_add_subitems_to_nonempty_submenu() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B", "X", "Y" ) );
				repo.Items[0].Children = new[] { repo.Items[2], repo.Items[3] };
				repo.Items[0].Children.ToList().ForEach( i => i.Parent = repo.Items[0] );
				repo.NextId = 10;

				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { new JS.Menu.Item { Text = "A", Id = 1, 
						SubItems = new[] { new JS.Menu.Item { Text = "X", Id = 3 }, new JS.Menu.Item { Text = "U" }, new JS.Menu.Item { Text = "Y", Id = 4 } } 
					} }
				} );
				Assert.Equal( new[] { "A", "X", "Y", "U" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 3, 4, 10 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_remove_and_add_subitems_to_nonempty_submenu_at_the_same_time() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B", "X", "Y" ) );
				repo.Items[0].Children = new[] { repo.Items[2], repo.Items[3] };
				repo.Items[0].Children.ToList().ForEach( i => i.Parent = repo.Items[0] );
				repo.NextId = 10;

				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { 
						new JS.Menu.Item { Text = "A", Id = 1, 
							SubItems = new[] { new JS.Menu.Item { Text = "U" }, new JS.Menu.Item { Text = "X", Id = 3 } } 
						},
						new JS.Menu.Item { Text = "B", Id = 2 }
					}
				} );
				Assert.Equal( new[] { "A", "B", "X", "U" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 2, 3, 10 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_add_subitems_to_nonempty_submenu_when_another_nonempty_submenu_exists() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B", "X", "Y", "I", "J" ) );
				repo.Items[0].Children = new[] { repo.Items[2], repo.Items[3] };
				repo.Items[0].Children.ToList().ForEach( i => i.Parent = repo.Items[0] );
				repo.Items[1].Children = new[] { repo.Items[4], repo.Items[5] };
				repo.Items[1].Children.ToList().ForEach( i => i.Parent = repo.Items[1] );
				repo.NextId = 10;

				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { 
						new JS.Menu.Item { Text = "A", Id = 1, 
							SubItems = new[] { new JS.Menu.Item { Text = "U" }, new JS.Menu.Item { Text = "X", Id = 3 }, new JS.Menu.Item { Text = "Y", Id = 4 }  } 
						},
						new JS.Menu.Item { Text = "B", Id = 2,
							SubItems = new[] { new JS.Menu.Item { Text = "I", Id = 5 }, new JS.Menu.Item { Text = "J", Id = 6 } } 
						}
					}
				} );
				Assert.Equal( new[] { "A", "B", "X", "Y", "I", "J", "U" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 2, 3, 4, 5, 6, 10 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_rename_subitems() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B", "X", "Y" ) );
				repo.Items[0].Children = new[] { repo.Items[2], repo.Items[3] };
				repo.Items[0].Children.ToList().ForEach( i => i.Parent = repo.Items[0] );
				repo.NextId = 5;

				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { 
						new JS.Menu.Item { Text = "A", Id = 1, 
							SubItems = new[] { new JS.Menu.Item { Text = "U", Id = 3 }, new JS.Menu.Item { Text = "V", Id = 4 } } 
						},
						new JS.Menu.Item { Text = "B", Id = 2 }
					}
				} );
				Assert.Equal( new[] { "A", "B", "U", "V" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 2, 3, 4 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		[Fact]
		public void Should_rename_subitems_when_they_are_given_in_different_order() {
			t( ( repo, mc ) => {
				repo.Items.AddRange( ii( "A", "B", "X", "Y" ) );
				repo.Items[0].Children = new[] { repo.Items[2], repo.Items[3] };
				repo.Items[0].Children.ToList().ForEach( i => i.Parent = repo.Items[0] );
				repo.NextId = 5;

				mc.UpdateSubItems( new JS.Menu.SubItemsSaveRequest {
					Items = new[] { 
						new JS.Menu.Item { Text = "A", Id = 1, 
							SubItems = new[] { new JS.Menu.Item { Text = "V", Id = 4 }, new JS.Menu.Item { Text = "U", Id = 3 } } 
						},
						new JS.Menu.Item { Text = "B", Id = 2 }
					}
				} );
				Assert.Equal( new[] { "A", "B", "U", "V" }, repo.Items.Select( i => i.Text ) );
				Assert.Equal( new[] { 1, 2, 3, 4 }, repo.Items.Select( i => i.Id ) );
			} );
		}

		IEnumerable<JS.Menu.Item> ji( params string[] items ) {
			return items.Select( i => new JS.Menu.Item { Text = i } );
		}

		IEnumerable<NavigationItem> ii( params string[] items ) {
			return items.Select( (i,idx) => new NavigationItem { Id = idx+1, Text = i } );
		}

		void t( Action<Repo<NavigationItem>, Mut.Controllers.MenuController> f ) {
			var repo = new Repo<NavigationItem> { Key = i => i.Id, SetKey = ( i, id ) => i.Id = id };

			var tm = new Moq.Mock<ITopMenuUI>();
			tm.Setup( x => x.GetItemsForParent( It.IsAny<int?>(), It.IsAny<string>() ) ).Returns(
				( int? parentId, string menuId ) => repo.Items
					.Where( i => ((i.Parent == null && parentId == null) || (i.Parent != null && i.Parent.Id == parentId)) && menuId == i.MenuId )
					.AsQueryable() );

			var mc = new Mut.Controllers.MenuController {
				MenuItems = repo,
				UnitOfWork = Moq.Mock.Of<IUnitOfWork>(),
				TopMenu = tm.Object,
				Site = Mock.Of<ISiteService>()
			};

			f( repo, mc );
		}

		class Repo<T> : IRepository<T> where T : class
		{
			public int NextId = 1;
			public List<T> Items = new List<T>();
			public Action<T, int> SetKey;
			public Func<T, int> Key;

			public T Find( object key ) {
				return Items.FirstOrDefault( i => Equals( Key( i ), key ) );
			}

			public T Add( T t ) { Items.Add( t ); SetKey( t, NextId++ ); return t; }
			public void Remove( T t ) { Items.Remove( t ); }
			public IQueryable<T> All { get { return Items.AsQueryable(); } }
		}
	}
}
