/// <amd-dependency path="css!styles/Page.css" />
import c = require( "../common" );
import contextMenu = require( "../Controls/ContextMenu" );
import ko = require( "ko" );
import map = require( "ko.mapping" );
import rx = require( "rx" );
import $ = require( "jQuery" );
import bb = require( "./bbCode" );

export = PageVm;

var Ajax = {
	Load: "page/load",
	Update: "page/update"
};

class PageVm extends c.VmBase implements c.IControl {
	private Editor: EditorVm;
	PageId: number;
	ViewElement: JQuery;
	CtxMenu = new contextMenu();

	constructor( args: { id: number }) {
		super();
		this.PageId = args.id;
	}

	OnEdit() {
		var e = this.Editor || ( this.Editor = new EditorVm( this ) );
		e.Show();
	}

	OnLoaded( element: Element ) {
		this.ViewElement = $( element );
		if( !this.ViewElement.text().trim() ) {
			this.ViewElement.text( "Right-click here" );
		}

		this.CtxMenu.MenuElement( Template.Menu.clone().appendTo( "body" ).hide()[0] );
		this.CtxMenu.OnLoaded( element );
		ko.applyBindings( this, this.CtxMenu.MenuElement() );
	}

	ControlsDescendantBindings = false;
}

// Defined in JS/PageEditor.cs
class EditorVm {
	private Element: JQuery;
	IsLoading = ko.observable( false );
	Error = ko.observable<string>();
	Text = ko.observable( "" );
	Title = ko.observable( "" );

	constructor( public Page: PageVm ) {
		c.Api.Get( Ajax.Load, { id: Page.PageId }, this.IsLoading, this.Error,
			r => map.fromJS( r, {}, this ) );
	}

	Save() {
		var page = map.toJS( this );
		page.Id = this.Page.PageId;
		c.Api.Post( Ajax.Update, page, this.IsLoading, this.Error, (html:string) => {
			this.Close();
			this.Page.ViewElement.html( html ).prepend(
				$( "<h2>" ).text( this.Title() ) );
		} );
	}

	Cancel() { this.Close(); }

	Close() {
		this.Element && this.Element.fadeOut();
	}

	Show() {
		(this.Element || ( this.Element = this.CreateElement() )).fadeIn();
	}

	CreateElement() {
		var e = Template.Editor.clone().appendTo( 'body' );
		ko.applyBindings( this, e[0] );
		return e;
	}
}

declare module "text!./Templates/page.html" { }
import _template = require( "text!./Templates/page.html" );
var Template = ( () => {
	var t = $( _template );
	return {
		Menu: t.filter( ".page-menu" ),
		Editor: t.filter( ".page-editor" )
	};
})();