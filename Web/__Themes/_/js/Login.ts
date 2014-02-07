/// <amd-dependency path="text!./Templates/Login.html" />
/// <amd-dependency path="css!./Templates/Login.css" />
import c = require( "./common" );
import infoBox = require( "./Controls/InfoBox" );
import $ = require( "jQuery" );
import rx = require( "rx" );
import ko = require( "ko" );

export function ShowPopup() {
	var vm = new LoginVm();
	var div = $( "<div>" ).appendTo( "body" );
	vm.OnDone.subscribe( () => div.remove() );
	vm.OnLoaded( div[0] );
}

var Ajax = {
	GetMethods: "login/GetMethods",
	GetUrlForMethod: "login/GetRedirectUrl"
};

export class LoginButton implements c.IControl {
	OnLoaded( e: Element ) { $( e ).click( ShowPopup ); }
	ControlsDescendantBindings = false;
}

export class LoginVm extends c.TemplatedControl {
	ChosenMethod = ko.observable<server.LoginMethodModel>();
	InfoBox = new infoBox();
	Methods = ko.observableArray<server.LoginMethodModel>();
	OnDone: Rx.IObservable<any> = new rx.Subject<any>();

	constructor() {
		super( Template );

		this.InfoBox.Info( "Loading..." );
		c.Api.Get( Ajax.GetMethods, null, null, this.InfoBox.Error, mm => { this.Methods( mm ); this.InfoBox.Clear(); } );

		c.koToRx( this.ChosenMethod )
			.where( x => !!x )
			.doAction( () => this.InfoBox.Info( "Loading..." ) )
			.selectMany( m => c.Api
				.GetAsRx<string>( Ajax.GetUrlForMethod, { method: m.Id, returnUrl: window.location.href })
				.takeUntil( c.koToRx( this.ChosenMethod ).skip(1) )
			)
			.doAction( url => window.location.href = url )
			.catchException( e => {
				this.InfoBox.Error( e );
				this.ChosenMethod( null );
				return rx.Observable.empty<any>();
			})
			.repeat()
			.subscribe();
	}

	OnMethodChosen( m: server.LoginMethodModel ) {
		return () => this.ChosenMethod( m );
	}
}

export var Template = $( require( "text!./Templates/Login.html" ) );