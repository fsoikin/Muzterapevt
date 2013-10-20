/// <amd-dependency path="css!styles/BBQuickReference.css" />
import er = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export = BBQuickReference;

class BBQuickReference implements er.IControl {
	OnLoaded( e: Element ) {
		setTimeout( () => template.clone().appendTo( "body" ).position( { my: "left top", at: "right top", of: e, collision: 'fit' }), 200 );
	}
	ControlsDescendantBindings = true;
}

declare module "text!./Templates/BBQuickReference.html" { }
import _template = require( "text!./Templates/BBQuickReference.html" );
var template = $( <string>_template );