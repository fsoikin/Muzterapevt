/// <amd-dependency path="css!styles/Page.css" />
import c = require( "../common" );
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

class PageVm implements c.IControl {
	PageElement: JQuery;
	MenuElement: JQuery;
	private Editor: EditorVm;
	PageId: number;

	constructor( args: { id: number }) {
		this.PageId = args.id;
	}
	
	OnLoaded( element: Element ) {
		this.PageElement = $( element );
		if( !this.PageElement.text().trim() ) {
			this.PageElement.text( "Right-click here" );
		}

		this.MenuElement = Template.Menu.clone().appendTo( "body" ).hide();
		ko.applyBindings( this, this.MenuElement[0] );

		this.PageElement.on("contextmenu", e => {
			this.MenuElement.css( { top: e.pageY, left: e.pageX }).fadeIn(100);
			$( document ).one( "click", () => this.MenuElement.fadeOut(100) );
			return false;
		});
	}

	OnEdit() {
		var e = this.Editor || ( this.Editor = new EditorVm( this ) );
		e.Show();
	}

	ControlsDescendantBindings = true;
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
			this.Page.PageElement.html( html ).prepend(
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