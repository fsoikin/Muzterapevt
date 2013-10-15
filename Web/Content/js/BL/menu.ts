/// <amd-dependency path="css!styles/Menu.css" />
import c = require( "../common" );
import ctr = require( "../lib/template" );
import contextMenu = require( "../Controls/ContextMenu" );
import infoBox = require( "../Controls/InfoBox" );
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

export class MenuVm implements c.IControl {
	AllowEdit: boolean;
	CtxMenu = null;
	Items = ko.observableArray<ItemVm>();
	Parent: ItemVm;
	UIRoot = ko.observable<Element>();

	MustBeVisible = ko.observable( false );
	EffectiveMustBeVisible = ko.computed( () =>
		this.MustBeVisible() ||
		this.Items().some( i => i.Children.EffectiveMustBeVisible() ) );
	IsVisible = ko.observable( false );

	constructor( args?: {
		items?: Item[];
		parent?: ItemVm;
		allowEdit: boolean;
	}) {
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

		if( args.items ) {
			this.Items( args.items.map( ( i, idx ) => new ItemVm( i, this, idx ) ) );
		}
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
		this.Save( null );
	};

	EditItem( i: ItemVm ) { this.Parent.Parent.EditItem( i ); }
	Save( onDone: () => void ) { this.Parent.Parent.Save( onDone ); }
	ToJson() { return this.Items().map( i => i.ToJson() ); }
}

export class RootMenuVm extends MenuVm {
	private RootItemId: number;
	private InfoBox = new infoBox();
	CtxMenu = new contextMenu();
	CtxMenuItemRowsVisible = ko.computed( () => this.CtxMenu.CurrentTarget() instanceof ItemVm );
	AllowEdit: boolean;
	EditingItem = ko.observable<ItemVm>();
	Editor = ko.observable<Element>();
	_moveEditor = ko.computed( () => this.Editor() && $( this.Editor() ).appendTo( "body" ) );

	IsSaving = ko.observable( false );
	IsLoading = ko.observable( false );

	OnLoaded = RootTemplate;

	constructor( args?: {
		rootItemId?: number;
		items?: Item[];
		allowEdit: boolean;
	}) {
		super( { parent: null, items: args.items, allowEdit: args.allowEdit });

		if( args.rootItemId ) {
			c.Api.Get( Ajax.Load, { parentId: args && args.rootItemId },
				this.IsLoading, err => this.InfoBox.Error,
				ii => this.Items( ii.map( ( i, idx ) => new ItemVm( i, this, idx ) ) ) );
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

	EditItem( i: ItemVm ) { this.EditingItem( i ); }

	Save( onDone: () => void ) {
		if( !this.AllowEdit ) return;

		this.InfoBox.Info( "Saving..." );
		c.Api.Post( Ajax.UpdateSubItems,
			<SubItemsSaveRequest>{ ParentId: this.RootItemId, Items: this.ToJson() },
			this.IsSaving, this.InfoBox.Error, () => {
				this.InfoBox.Info( null );
				onDone && onDone();
			} );
	}

	Add() {
		if( !this.AllowEdit ) return;

		var t = this.CtxMenu.CurrentTarget();
		var menu: MenuVm =
			t instanceof MenuVm ? t :
			t instanceof ItemVm ? ( <ItemVm>t ).Parent
			: null;
		if( !menu ) return;

		var i = new ItemVm( new Item(), menu, menu.Items().length );
		menu.Items.push( i );
		menu.EditItem( i );

		var onCancel = i.Cancelled.subscribe( () => menu.Items.remove( i ) );
		var onSave = i.Saved.subscribe( () => {
			onCancel.dispose();
			onSave.dispose();
		});
	}

	Edit() {
		if( !this.AllowEdit ) return;

		var i = <ItemVm>this.CtxMenu.CurrentTarget();
		if( i instanceof ItemVm ) this.EditItem( i );
	}

	Delete() {
		if( !this.AllowEdit ) return;

		var i = <ItemVm>this.CtxMenu.CurrentTarget();
		if( i instanceof ItemVm ) {
			i.Parent.Items.remove( i );
			this.Save( null );
		}
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
		this.Children = new MenuVm( { items: Item.SubItems, parent: this, allowEdit: this.Parent.AllowEdit });

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
		this.Parent.Save( () => {
			this.Saved.notifySubscribers( null );
			this.Parent.EditItem( null );
		});
	}

	Cancel() {
		this.Cancelled.notifySubscribers( null );
		this.Parent.EditItem( null );
	}
}

declare module "text!./Templates/menu.html" { }
import _template = require( "text!./Templates/menu.html" );
var _t = $( <string>_template );
var Template = ctr.ApplyTemplate( _t.filter( ".menu" ) );
var RootTemplate = ctr.ApplyTemplate( _t );