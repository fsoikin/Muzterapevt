///<amd-dependency path="css!./Templates/AdminMenu.css" />
///<amd-dependency path="text!./Templates/AdminMenu.html" />
import c = require( "./common" );
import ko = require( "ko" );
import $ = require( "jQuery" );

export = Vm;

class Vm extends c.TemplatedControl {
	IsExpanded = ko.observable( false );

	constructor() {
		super( Template );
	}

	Go( url: string ) {
		return () => location.href = c.Api.AbsoluteUrl( url );
	}

	Toggle() {
		this.IsExpanded( !this.IsExpanded() );
	}
}

var Template = $( require( "text!./Templates/AdminMenu.html" ) );