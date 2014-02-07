import c = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export function ApplyTemplate( template: string ): ( e: Element ) => void;
export function ApplyTemplate( template: JQuery ): ( e: Element ) => void;

export function ApplyTemplate( template: any ) {

	var t = typeof template == "string" ? $( template ) : template;

	return function ( element: Element ) {
		if( $.makeArray(
			ko.virtualElements.childNodes( element ) )
			.every( e => e.nodeType != 1 ) ) {
			ko.virtualElements.setDomNodeChildren( element, t.clone() );
		}
		ko.applyBindingsToDescendants( this, element );
	};

}

export class TemplatedControl implements c.IVirtualControl {
	constructor( template: JQuery ) {
		this.OnLoaded = ApplyTemplate( template );
	}

	OnLoaded: ( element: Element ) => void;
	ControlsDescendantBindings = true;
	SupportsVirtualElements = true;
}