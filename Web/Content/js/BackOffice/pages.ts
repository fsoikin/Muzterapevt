import c = require( "../common" );
import ko = require("ko");
import map = require("ko.mapping");
import $ = require( "jQuery" );

var Ajax = {
	Load: "BackOffice/Pages/All",
	Update: "BackOffice/Pages/Update",
	Create: "BackOffice/Pages/Create"
};

// Defined in UI/JS/Page.cs
export class Page {
	Id = 0;
	Path = "";
}

export class PagesVm extends c.VmBase implements c.IControl {
	Pages = ko.observableArray<PageVm>();
	
	constructor() {
		super();
		c.Api.Get(Ajax.Load, null, this.IsLoading, null,
			ps => this.Pages(ps.map(p => new PageVm(this, p))));
	}

	Create() {
		var p = new PageVm(this, null);
		this.Pages.push(p);
		p.Edit();
	}

	OnLoaded = _onLoaded;
	ControlsDescendantBindings = true;
}

export class PageVm {
	IsNew = ko.observable(true);
	IsSaving = ko.observable(false);
	IsEditing = ko.observable(false);
	Path = ko.observable("");
	Id = ko.observable(0);

	constructor(public Parent: PagesVm, public Model: Page) {
		map.fromJS( Model || new Page(), {}, this);
		this.IsNew( !Model);
		this.IsEditing.subscribe(e => {
			if (e) this.Parent.Pages().forEach(p => p == this || p.IsEditing(false));
		});
	}

	Edit() {
		this.IsEditing(true);
	}

	CommitEdit() {
		this.Model = map.toJS( this );
		c.Api.Post( this.IsNew() ? Ajax.Create : Ajax.Update, this.Model,
			this.IsSaving, null, r => {
				map.fromJS( this.Model = r, {}, this );
				this.IsEditing( false );
				this.IsNew( false );
			});
	}

	CancelEdit() {
		this.IsEditing(false);
		map.fromJS(this.Model, {}, this);
	}
}

declare module "text!./Templates/pages.html" { }
import _tpl = require( "text!./Templates/pages.html" );
var _onLoaded = c.ApplyTemplate( <string>_tpl );