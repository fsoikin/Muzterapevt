/// <amd-dependency path="css!styles/BBQuickReference.css" />
import er = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export = BBQuickReference;

class BBQuickReference implements er.IControl {
	private _element: JQuery;

	Show() { this._element && this._element.fadeIn(); }
	Hide() { this._element && this._element.fadeOut(); }

	OnLoaded( e: Element ) {
		setTimeout( () => (this._element = template.clone().appendTo( "body" )).position( { my: "left top", at: "right top", of: e, collision: 'fit' }), 200 );
	}
	ControlsDescendantBindings = true;
}

declare module "text!./Templates/BBQuickReference.html" { }
import _template = require( "text!./Templates/BBQuickReference.html" );
var template = $( <string>_template );