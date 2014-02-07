/// <amd-dependency path="css!./Templates/Moderator.css" />
/// <amd-dependency path="text!./Templates/Moderator.html" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import upl = require( "../../Lib/Uploader" );
import infoBox = require( "../../Controls/InfoBox" );
import sel = require( "../../Controls/Select" );
import dsApi = require( "../../Lib/DataSourceApi" );
import att = require( "../attachment" );

var Ajax = {
	Load: "specialist/list",
	Approve: "specialist/approve",
	Archive: "specialist/archive",
	Delete: "specialist/delete",
};

export class ListVm extends c.TemplatedControl {
	IsLoading = ko.observable( false );
	IsSaving = ko.observable( false );
	InfoBox = new infoBox();
	Specialists = ko.observableArray<server.SpecialistView>();

	constructor( args: { id: number }) {
		super( Template );
		c.Api.Get( Ajax.Load, null, this.IsLoading, this.InfoBox.Error, this.Specialists );
	}

	_action = ( prompt: string, url: string ) => ( s: server.SpecialistView ) => {
		this.InfoBox.Info( prompt );
		c.Api.Post( url, { id: s.Id }, this.IsSaving, this.InfoBox.Error, () => {
			this.InfoBox.Clear();
			this.Specialists.remove( s );
		} );
	}

	Approve = this._action( "Approving...", Ajax.Approve );
	Archive = this._action( "Archiving...", Ajax.Archive );
	Delete = this._action( "Deleting...", Ajax.Delete );
}

var Template = $( require( "text!./Templates/Moderator.html" ) );