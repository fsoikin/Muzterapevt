/// <amd-dependency path="text!./Templates/Subscription.html" />
/// <amd-dependency path="css!./Templates/Subscription.css" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );

export class Link extends c.TemplatedControl {
	Page = new Page( () => this.Hide() );
	PageVisible = ko.observable( false );

	constructor( private Text: string ) {
		super( Template.Link );
	}

	Show() { this.PageVisible( true ); setTimeout( () => this.Page.EmailFocused( true ) ); }
	Hide() { this.PageVisible( false ); this.Page.IsSubmitted( false ); this.Page.Email( "" ); }
}

export class Page extends c.TemplatedControl {
	IsSubmitting = ko.observable( false );
	IsSubmitted = ko.observable( false );
	Email = ko.observable( "" );
	EmailFocused = ko.observable( false );
	Error = ko.observable( "" );

	constructor( private Hide?: () => void ) {
		super( Template.Page );
	}

	Submit() {
		if ( !this.Email() ) return;
		c.Api.Post( "subscription/submit", { Email: this.Email() }, this.IsSubmitting, this.Error, () => this.IsSubmitted( true ) );
	}
}

module Template {
	var t = $( require( "text!./Templates/Subscription.html" ) );
	export var Link = t.filter( ".part_Link" );
	export var Page = t.filter( ".part_Page" );
}