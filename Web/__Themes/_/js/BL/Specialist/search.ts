/// <amd-dependency path="css!./Templates/Search.css" />
/// <amd-dependency path="text!./Templates/Search.html" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import infoBox = require( "../../Controls/InfoBox" );
import sel = require( "../../Controls/Select" );
import dsApi = require( "../../Lib/DataSourceApi" );

var Ds = {
	regions: dsApi.create.fromServer<server.Region>( "specialist/regions" )
};

var Ajax = {
	Search: "specialist/search"
};

export class SearchVm extends c.TemplatedControl {
	Regions = sel.CreateMultiSelect( Ds.regions() );
	Keywords = ko.observable( "" );

	InfoBox = new infoBox();
	IsLoading = ko.observable( false );
	TriedToSearch = ko.observable( false );

	Specialists = ko.observableArray<server.SpecialistPublicView>();

	ShowList = ko.computed(() => this.Specialists().length && !this.IsLoading() );
	ShowPrompt = ko.computed(() => !this.IsLoading() && !this.TriedToSearch() );
	ShowNothingFound = ko.computed(() => !this.IsLoading() && this.TriedToSearch() && !this.Specialists().length );

	constructor() {
		super( Template );
	}
	
	Search() {
		var req: server.SpecialistSearchRequest = { regions: this.Regions.SelectedIds(), keywords: this.Keywords() };
		c.Api.Post( Ajax.Search, req, this.IsLoading, this.InfoBox.Error, this.Specialists );
		this.TriedToSearch( true );
		this.InfoBox.Clear();
	}
}

var Template = $( require( "text!./Templates/Search.html" ) );