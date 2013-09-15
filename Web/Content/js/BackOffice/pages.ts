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
export interface Page {
	Id: number;
	Path: string;
}

export class PagesVm extends c.VmBase implements c.IControl {
	Pages = ko.observableArray<PageVm>();
	
	constructor() {
		super();
		c.Api.Get(Ajax.Load, null, this.IsLoading, this.Error,
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

	constructor(public Parent: PagesVm, public Page: Page) {
		if (Page) {
			map.fromJS(Page, {}, this);
			this.IsNew(false);
		}
		this.IsEditing.subscribe(e => {
			if (e) this.Parent.Pages().forEach(p => p == this || p.IsEditing(false));
		});
	}

	Edit() {
		this.IsEditing(true);
	}

	CommitEdit() {
		this.Page = map.toJS(this);
		c.Api.Post(this.IsNew() ? Ajax.Create : Ajax.Update,
			null, this.IsSaving, this.Parent.Error, () => {
				this.IsEditing(false);
				this.IsNew(false);
			});
	}

	CandelEdit() {
		this.IsEditing(false);
		map.fromJS(this.Page, {}, this);
	}
}

declare module "text!./Templates/pages.html" { }
import _tpl = require( "text!./Templates/pages.html" );
var _onLoaded = c.ApplyTemplate( <string>_tpl );