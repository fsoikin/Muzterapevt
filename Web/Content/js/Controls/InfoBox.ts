/// <amd-dependency path="text!./Templates/InfoBox.html" />
import er = require( "../common" );
import ko = require( "ko" );

export = InfoBox;

class InfoBox implements er.IVirtualControl {
	private Text = ko.observable( "" );
	private IsError = ko.observable( false );
	private IsWarning = ko.observable( false );
	private IsInfo = ko.observable( false );
	private Visible = ko.observable( false );

	Error = this._makeSwitcher( this.IsError );
	Warning = this._makeSwitcher( this.IsWarning );
	Info = this._makeSwitcher( null );
	Clear = () => this.Info( null );

	private _makeSwitcher( flag: Ko.Observable<boolean> ) {
		return ( text: string ) => {
			this.IsError( false );
			this.IsWarning( false );
			flag && flag( true );
			this.Visible( !!text );
			this.Text( text );
		};
	}

	private Dismiss() { this.Visible( false ); }
	OnLoaded = _template;
	ControlsDescendantBindings = true;
	SupportsVirtualElements = true;
}

// TODO: [fs] have to come up with a better way
var _template = er.ApplyTemplate( <string>require( "text!./Templates/InfoBox.html" ) );