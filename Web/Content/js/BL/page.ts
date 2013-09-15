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
	private PageElement: JQuery;
	private MenuElement: JQuery;
	private Editor: EditorVm;
	PageId: number;

	constructor( args: { id: number }) {
		this.PageId = args.id;
	}
	
	OnLoaded( element: Element ) {
		this.PageElement = $( element );
		if( !this.PageElement.html() ) {
			this.PageElement.text( "Right-click here" );
		}

		this.MenuElement = Template.Menu.clone().appendTo( "body" ).hide();
		ko.applyBindings( this, this.MenuElement[0] );

		this.PageElement.click( e => {
			if( e.which == 3 ) // right click
			{
				this.MenuElement.css( { top: e.pageY, left: e.pageY }).fadeIn(100);
				$( document ).one( "click", () => this.MenuElement.fadeOut(100) );
				return false;
			}
		});
	}

	OnEdit() {
		var e = this.Editor || ( this.Editor = new EditorVm( this ) );
		e.Show();
	}

	ControlsDescendantBindings = true;
}

class EditorVm {
	private Element: JQuery;
	IsLoading = ko.observable( false );
	Error = ko.observable<string>();
	Text = ko.observable( "" );
	Title = ko.observable( "" );

	constructor( public Page: PageVm ) {
		c.Api.Get( Ajax.Load, Page.PageId, this.IsLoading, this.Error,
			r => map.fromJS( r, {}, this ) );
	}

	Save() {
		var page = map.toJS( this );
		page.Id = this.Page.PageId;
		c.Api.Post( Ajax.Update, page, this.IsLoading, this.Error, () => this.Close() );
	}

	Cancel() { this.Close(); }

	Close() {
		this.Element && this.Element.fadeOut();
	}

	Show() {
		var e = this.Element || ( this.Element = Template.Editor.clone().appendTo( 'body' ) );
		e.fadeIn();
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