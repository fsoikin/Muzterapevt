import er = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export = ContextMenu;

class ContextMenu implements er.IUnloadableControl {
	MenuElement = ko.observable<Element>();
	CurrentTarget = ko.observable<any>();

	_hideMenuElement = ko.computed( () => this.MenuElement() && $( this.MenuElement() ).hide() );

	private OnDisplay = ( e: JQueryEventObject ) => {
		var vm = ko.contextFor( e.target );
		this.CurrentTarget( vm && vm.$data );

		$(this.MenuElement()).css( { top: e.pageY, left: e.pageX }).appendTo("body").fadeIn( 100 );
		$( document ).one( "click", this.OnHide );
		return false;
	};

	private OnHide = () => {
		$( this.MenuElement() ).fadeOut( 100 );
	};

	OnLoaded( element: Element ) {
		$( element ).on( "contextmenu", this.OnDisplay );
	}

	OnUnloaded( element: Element ) {
		$( element ).off( "contextmenu", this.OnDisplay );
	}

	ControlsDescendantBindings = false;
}