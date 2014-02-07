import c = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export = Vm;

class Vm extends c.VmBase implements c.IControl {
	ViewElement: JQuery;
	CtxMenuElement: JQuery;

	constructor( private CtxMenuTemplate: JQuery ) { super(); }

	OnLoaded( element: Element ) {
		this.ViewElement = $( element );
		if( !this.ViewElement.text().trim() ) {
			this.ViewElement.text( "Right-click here" );
		}

		this.CtxMenuElement = this.CtxMenuTemplate.clone().appendTo( "body" ).hide();
		ko.applyBindings( this, this.CtxMenuElement[0] );

		this.ViewElement.on("contextmenu", e => {
			this.CtxMenuElement.css( { top: e.pageY, left: e.pageX }).fadeIn(100);
			$( document ).one( "click", () => this.CtxMenuElement.fadeOut(100) );
			return false;
		});
	}

	ControlsDescendantBindings = true;
}