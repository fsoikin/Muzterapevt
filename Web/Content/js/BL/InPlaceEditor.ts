﻿/// <amd-dependency path="css!styles/Page.css" />
import c = require( "../common" );
import contextMenu = require( "../Controls/ContextMenu" );
import ko = require( "ko" );
import map = require( "ko.mapping" );
import rx = require( "rx" );
import $ = require( "jQuery" );
import bb = require( "./bbCode" );

export interface IInPlaceEditorAjax {
	Load( id: any ): string;
	Update: string;
};

export class InPlaceEditorVm extends c.VmBase implements c.IControl {
	ObjectId: number;
	ViewElement: JQuery;
	Ajax: IInPlaceEditorAjax;
	EditorTemplate: JQuery;
	EmptyData: any;
	private CtxMenu = new contextMenu();
	private MenuTemplate: JQuery;
	private _onSaved: ( element: JQuery, data ) => void;
	private Editor: EditorVm;

	constructor( args: {
		id: any;
		ajax: IInPlaceEditorAjax;
		emptyData: any;
		onSaved: ( element: JQuery, data ) => void;
		editorTemplate: JQuery;
		menuTemplate?: JQuery;
	}) {
		super();
		this.EmptyData = args.emptyData;
		this._onSaved = args.onSaved;
		this.EditorTemplate = args.editorTemplate;
		this.MenuTemplate = args.menuTemplate || MenuTemplate;
		this.Ajax = args.ajax;
		this.ObjectId = args.id;
	}

	OnEdit() {
		var e = this.Editor || ( this.Editor = new EditorVm( this ) );
		e.Show();
	}

	OnLoaded( element: Element ) {
		this.ViewElement = $( element );
		if( !this.ViewElement.text().trim() ) {
			this.ViewElement.text( "Right-click here" );
		}

		this.CtxMenu.MenuElement( this.MenuTemplate.clone().appendTo( "body" ).hide()[0] );
		this.CtxMenu.OnLoaded( element );
		ko.applyBindings( this, this.CtxMenu.MenuElement() );
	}

	OnSaved( data ) { this._onSaved( this.ViewElement, data ); }

	ControlsDescendantBindings = false;
}

class EditorVm {
	private Element: JQuery;
	IsLoading = ko.observable( false );
	Error = ko.observable<string>();

	constructor( public Parent: InPlaceEditorVm ) {
		map.fromJS( Parent.EmptyData, {}, this );
		c.Api.Get( Parent.Ajax.Load( Parent.ObjectId ), null, this.IsLoading, this.Error,
			r => map.fromJS( r, {}, this ) );
	}

	Save() {
		var obj = map.toJS( this );
		c.Api.Post( this.Parent.Ajax.Update, obj, this.IsLoading, this.Error, result => {
			this.Close();
			map.fromJS( result, {}, this );
			this.Parent.OnSaved( result );
		} );
	}

	Cancel() { this.Close(); }

	Close() {
		this.Element && this.Element.fadeOut();	
		$( document ).off( "keydown", this._onKey );
	}

	Show() {
		( this.Element || ( this.Element = this.CreateElement() ) ).fadeIn();
		$( document ).on( "keydown", this._onKey );
	}

	CreateElement() {
		var e = this.Parent.EditorTemplate.clone().appendTo( 'body' );
		ko.applyBindings( this, e[0] );
		return e;
	}

	_onKey = ( e: JQueryEventObject ) => {
		if( e.which == 27 ) this.Close();
		else if( e.which == 13 && e.ctrlKey ) this.Save();
	};
}

declare module "text!./Templates/InPlaceEditor-DefaultMenu.html" { }
import _template = require( "text!./Templates/InPlaceEditor-DefaultMenu.html" );
var MenuTemplate = $( _template );