/// <amd-dependency path="css!./Templates/Search.css" />
/// <amd-dependency path="text!./Templates/Search.html" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import infoBox = require( "../../Controls/InfoBox" );
import sel = require( "../../Controls/Select" );
import dsApi = require( "../../Lib/DataSourceApi" );

var Ajax = {
	Search: "specialist/search",
	Regions: "specialist/regions"
};

export class SearchVm extends c.TemplatedControl {
	Regions = ko.observableArray<RegionVm>();
	SelectedRegion = ko.observable<RegionVm>();
	Keywords = ko.observable( "" );

	InfoBox = new infoBox();
	IsLoading = ko.observable( false );
	TriedToSearch = ko.observable( false );

	Specialists = ko.observableArray<server.SpecialistPublicView>();

	ShowList = ko.computed(() => this.Specialists().length && !this.IsLoading() );
	ShowPrompt = ko.computed(() => !this.IsLoading() && !this.TriedToSearch() );
	ShowNothingFound = ko.computed(() => !this.IsLoading() && this.TriedToSearch() && !this.Specialists().length );

	constructor() {
		super(Template.Root);
		c.Api.Post(Ajax.Regions, null, this.IsLoading, this.InfoBox.Error,
			(rs: server.Region[]) => this.Regions((rs || []).map(r => new RegionVm(r, this.Search))));
	}

	Search = (r: RegionVm) => {
		this.SelectedRegion(r);
		if (!r) return;

		var ids = r.SelfAndChildrenRecursive().map(x => x.Id);
		var req: server.SpecialistSearchRequest = { regions: ids, keywords: this.Keywords() };
		c.Api.Post( Ajax.Search, req, this.IsLoading, this.InfoBox.Error, this.Specialists );
		this.TriedToSearch( true );
		this.InfoBox.Clear();
	}
}

class RegionVm extends c.TemplatedControl {
	Name = this._model.Name;
	Id = this._model.Id;
	Children = (this._model.children || []).map(r => new RegionVm(r, this._select));
	Visible = this._model.totalSpecialists > 0 || this.Children.some(c => c.Visible);

	constructor(private _model: server.Region, private _select: (r: RegionVm) => void) {
		super(Template.Region);
	}

	Select() {
		this._select(this);
	}

	SelfAndChildrenRecursive() {
		var r: RegionVm[] = [this];
		this.Children.forEach(
			c => r.push( ... c.SelfAndChildrenRecursive() ));
		return r;
	}
}

module Template {
	var t = $(require("text!./Templates/Search.html"));
	export var Root = t.filter(".PART_Root");
	export var Region = t.filter(".PART_Region");
}