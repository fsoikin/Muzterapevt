/// <amd-dependency path="css!styles/Menu.css" />
import c = require( "../common" );
import ctr = require( "../lib/template" );
import contextMenu = require( "../Controls/ContextMenu" );
import ko = require( "ko" );
import map = require( "ko.mapping" );
import rx = require( "rx" ); ( () => rx )();
import $ = require( "jQuery" );
import bb = require( "./bbCode" );

var Ajax = {
	Load: "menu/load",
	UpdateSubItems: "menu/updateSubItems",
};

export class Item {
	Id = 0;
	Text = "";
	Link = "";
	Order = 0;
	SubItems = new Array<Item>();
}

interface SubItemsSaveRequest {
	ParentId: number;
	Items: Item[];
}

export class MenuVm extends c.VmBase implements c.IControl {
	private RootItemId: number;
	private Items = ko.observableArray<ItemVm>();
	private CtxMenu = new contextMenu();
	private CtxMenuItemRowsVisible = ko.computed( () => this.CtxMenu.CurrentTarget() instanceof ItemVm );
	Parent: ItemVm;
	AllowEdit: boolean;
	EditingItem = ko.observable<ItemVm>();
	UIRoot = ko.observable<Element>();
	Editor = ko.observable<Element>();
	_moveEditor = ko.computed( () => this.Editor() && $( this.Editor() ).appendTo( "body" ) );

	MustBeVisible = ko.observable( false );
	EffectiveMustBeVisible = ko.computed( () =>
		this.MustBeVisible() ||
		this.Items().some( i => i.Children.EffectiveMustBeVisible() ) );
	IsVisible = ko.observable( false );

	constructor( args?: {
		rootItemId?: number;
		items?: Item[];
		parent?: ItemVm;
		allowEdit: boolean;
	}) {
		super();
		this.Parent = args.parent;
		this.AllowEdit = args.allowEdit;

		// When EffectiveMustBeVisible goes to false, I wait 1000ms and then hide myself.
		this.EffectiveMustBeVisible.asRx()
			.throttle( 1000 )
			.subscribe( v => !v && this.IsVisible( false ) );

		// When EffectiveMustBeVisible goes to true, I show myself immediately and hide all my siblings.
		this.EffectiveMustBeVisible
			.subscribe( v => {
				if( v ) {
					this.IsVisible( true );
					if( this.Parent ) this.Parent.Parent.Items()
						.forEach( i => i.Children == this || i.Children.IsVisible( false ) );
				}
			} );

		if( args.rootItemId ) {
			c.Api.Get( Ajax.Load, { parentId: args && args.rootItemId },
				this.IsLoading, this.Error,
				ii => this.Items( ii.map( (i, idx) => new ItemVm( i, this, idx ) ) ) );
		}
		else if( args.items ) {
			this.Items( args.items.map( ( i, idx ) => new ItemVm( i, this, idx ) ) );
		}

		var onKey = ( e: JQueryEventObject ) => {
			if( e.which == 27 ) {
				this.EditingItem() && this.EditingItem().Cancel();
			}
			else if( e.which == 13 && e.ctrlKey ) {
				this.EditingItem() && this.EditingItem().Save();
			}
		};
		this.EditingItem.subscribe( e => {
			if( e ) $( document ).on( "keydown", onKey );
			else $( document ).off( "keydown", onKey );
		});
	}

	OnMustShow = () => this.MustBeVisible( true );
	OnCanHide = () => this.MustBeVisible( false );

	OnLoaded = Template;
	ControlsDescendantBindings = true;

	OnItemsSorted = () => {
		$( "li", this.UIRoot() ).each( ( idx, e ) => {
			var ctx = ko.contextFor( e );
			var i: ItemVm = ctx && ctx.$data;
			if( !( i instanceof ItemVm ) ) return;
			i.SortOrder( idx );
		});
		this.Items.sort( ( x, y ) => x.SortOrder() - y.SortOrder() );
		this.Save();
	};

	Add() {
		if( !this.AllowEdit ) return;

		var i = new ItemVm( new Item(), this, this.Items().length );
		this.Items.push( i );
		this.EditingItem( i );

		var onCancel = i.Cancelled.subscribe( () => this.Items.remove( i ) );
		var onSave = i.Saved.subscribe( () => {
			onCancel.dispose();
			onSave.dispose();
		});
	}

	Edit() {
		if( !this.AllowEdit ) return;

		var i = <ItemVm>this.CtxMenu.CurrentTarget();
		if ( i instanceof ItemVm ) this.EditingItem( i );
	}

	Delete() {
		if( !this.AllowEdit ) return;

		var i = <ItemVm>this.CtxMenu.CurrentTarget();
		if( i instanceof ItemVm ) {
			this.Items.remove( i );
			this.Save();
		}
	}

	Save() {
		if( !this.AllowEdit ) return;

		if( this.Parent ) this.Parent.Save();
		else {
			c.Api.Post( Ajax.UpdateSubItems,
				<SubItemsSaveRequest>{ ParentId: this.RootItemId, Items: this.ToJson() },
				this.IsSaving, this.Error, null );
		}
	}

	ToJson() {
		return this.Items().map( i => i.ToJson() );
	}
}

export class ItemVm
{
	IsTopLevel: boolean;
	Children: MenuVm;
	ChildrenElement = ko.observable<Element>();
	Element = ko.observable<Element>();
	SortOrder = ko.observable( 0 );
	Cancelled = new ko.subscribable();
	Saved = new ko.subscribable();
	IsSaving = ko.observable( false );
	Link = ko.observable( "" );
	AbsoluteLink = ko.computed( () => c.Api.PageUrl( this.Link() || "" ) );

	constructor( public Item: Item, public Parent: MenuVm, index: number ) {
		map.fromJS( Item, { ignore: "SubItems" }, this );
		this.IsTopLevel = !Parent.Parent;
		this.Children = new MenuVm( { items: Item.SubItems, parent: this, allowEdit: Parent.AllowEdit });

		this.Children.IsVisible.subscribe( v => {
			if( v ) setImmediate( () => $( this.ChildrenElement() ).position( {
				my: 'left top', at: this.IsTopLevel ? 'left bottom' : 'right top', of: this.Element()
			}) );
		});
	}

	ToJson() {
		var i = <Item>map.toJS( this, {});
		i.SubItems = this.Children.ToJson();
		return i;
	}

	Save() {
		this.Parent.Save();
		this.Saved.notifySubscribers( null );
		this.Parent.EditingItem( null );
	}

	Cancel() {
		this.Cancelled.notifySubscribers( null );
		this.Parent.EditingItem( null );
	}
}

declare module "text!./Templates/menu.html" { }
import _template = require( "text!./Templates/menu.html" );
var Template = ctr.ApplyTemplate( <string>_template );