/// <amd-dependency path="css!./Templates/InPlaceEditor.css" />
/// <amd-dependency path="text!./Templates/InPlaceEditor.html" />
/// <amd-dependency path="text!./Templates/InPlaceEditor-DefaultMenu.html" />
/// <amd-dependency path="text!./Templates/InPlaceEditor-BBQuickReference.html" />
import c = require( "../common" );
import contextMenu = require( "../Controls/ContextMenu" );
import infoBox = require( "../Controls/InfoBox" );
import ko = require( "ko" );
import map = require( "ko.mapping" );
import rx = require( "rx" );
import $ = require( "jQuery" );
import bb = require( "./bbCode" );
import uploader = require( "../Lib/Uploader" );

var MenuTemplate = $( require( "text!./Templates/InPlaceEditor-DefaultMenu.html" ) );
var EditorTemplate = c.ApplyTemplate( require( "text!./Templates/InPlaceEditor.html" ) );
var BBRefTemplate = c.ApplyTemplate( require( "text!./Templates/InPlaceEditor-BBQuickReference.html" ) );

export interface IInPlaceEditorAjax {
	Load( id: any ): string;
	UploadAttachment( id: any ): string;
	Update: string;
};

export class InPlaceEditorVm extends c.VmBase implements c.IControl {
	ObjectId: number;
	ViewElement: JQuery;
	Ajax: IInPlaceEditorAjax;
	EditorTemplate: JQuery;
	EmptyData: any;
	IsAcceptingFiles = ko.observable( false );
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

	DropFiles( e: DragEvent ) {
		this.IsAcceptingFiles( false );
		//e.dataTransfer.files.length && this.Upload( e.dataTransfer.files );
	}

	DragOver( e: DragEvent ) { this.IsAcceptingFiles( true ); }
	DragOut( e: DragEvent ) { this.IsAcceptingFiles( false ); }

	ControlsDescendantBindings = false;
}

class EditorVm {
	private Element: JQuery;
	IsLoading = ko.observable( false );
	Loaded = false;
	InfoBox = new infoBox();
	BBQuickReference = <c.IControl>{
		OnLoaded: BBRefTemplate,
		ControlsDescendantBindings: true
	};
	Data = <c.IControl>{
		OnLoaded: this.Parent.EditorTemplate,
		ControlsDescendantBindings: true
	};

	constructor( public Parent: InPlaceEditorVm ) {
		map.fromJS( Parent.EmptyData, {}, this.Data );
		this.EnsureLoaded();
	}

	EnsureLoaded() {
		if( this.Loaded ) return;

		this.InfoBox.Clear();
		c.Api.Get( this.Parent.Ajax.Load( this.Parent.ObjectId ), null, this.IsLoading,
			this.InfoBox.Error, r => {
				map.fromJS( r, {}, this );
				this.Loaded = true;
			} );
	}

	Save() {
		var obj = map.toJS( this.Data );
		c.Api.Post( this.Parent.Ajax.Update, obj, this.IsLoading, this.InfoBox.Error, result => {
			this.Close();
			map.fromJS( result, {}, this.Data );
			this.Parent.OnSaved( result );
		} );
	}

	Cancel() { this.Close(); }

	Close() {
		this.Element && this.Element.fadeOut();	
		$( document ).off( "keydown", this._onKey );
	}

	Show() {
		this.EnsureLoaded();
		( this.Element || ( this.Element = this.CreateElement() ) ).fadeIn();
		$( document ).on( "keydown", this._onKey );
	}

	CreateElement() {
		var e = EditorTemplate.clone().appendTo( 'body' );
		ko.applyBindings( this, e[0] );
		return e;
	}

	_onKey = ( e: JQueryEventObject ) => {
		if( e.which == 27 ) this.Close();
		else if( e.which == 13 && e.ctrlKey ) this.Save();
	};
}