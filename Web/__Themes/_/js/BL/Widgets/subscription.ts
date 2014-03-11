/// <amd-dependency path="text!./Templates/Subscription.html" />
/// <amd-dependency path="css!./Templates/Subscription.css" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );

export class Vm extends c.TemplatedControl {
	IsVisible = ko.observable( false );
	IsSubmitting = ko.observable( false );
	IsSubmitted = ko.observable( false );
	Email = ko.observable( "" );
	EmailFocused = ko.observable( false );
	Error = ko.observable( "" );

	constructor() {
		super( Template );
	}

	Show() { this.IsVisible( true ); setTimeout( () => this.EmailFocused( true ) ); }
	Hide() { this.IsVisible( false ); this.IsSubmitted( false ); this.Email( "" ); }

	Submit() {
		if ( !this.Email() ) return;
		c.Api.Post( "subscription/submit", { Email: this.Email() }, this.IsSubmitting, this.Error, () => this.IsSubmitted( true ) );
	}
}

var Template = $( require( "text!./Templates/Subscription.html" ) );