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
import dyn = require( "../Lib/dynamic" ); ( () => dyn )();
import uploader = require( "../Lib/Uploader" );
import att = require( "./attachment" );
import bbTextField = require( "./BBTextField" );

var MenuTemplate = $( require( "text!./Templates/InPlaceEditor-DefaultMenu.html" ) );
var EditorTemplate = $( require( "text!./Templates/InPlaceEditor.html" ) );
var BBRefTemplate = c.ApplyTemplate( require( "text!./Templates/InPlaceEditor-BBQuickReference.html" ) );

export interface IInPlaceEditorAjax<T> {
	Load( id: any ): string;
	GetAttachments( obj: T ): string;
	UploadAttachment( obj: T ): string;
	Update: string;
};

export class InPlaceEditorVm<T> extends c.VmBase implements c.IControl {
	ObjectId: number;
	ViewElement: JQuery;
	Ajax: IInPlaceEditorAjax<T>;
	EditorTemplate: (e: Element) => void;
	EmptyData: T;
	private CtxMenu = new contextMenu();
	private MenuTemplate: JQuery;
	private _onSaved: ( element: JQuery, data ) => void;
	private Editor: EditorVm<T>;

	constructor( args: {
		id: any;
		ajax: IInPlaceEditorAjax<T>;
		emptyData: T;
		onSaved: ( element: JQuery, data ) => void;
		editorTemplate: JQuery;
		menuTemplate?: JQuery;
	}) {
		super();
		this.EmptyData = args.emptyData;
		this._onSaved = args.onSaved;
		this.EditorTemplate = c.ApplyTemplate( args.editorTemplate );
		this.MenuTemplate = args.menuTemplate || MenuTemplate;
		this.Ajax = args.ajax;
		this.ObjectId = args.id;
	}

	OnEdit() {
		var e = this.Editor || ( this.Editor = new EditorVm<T>( this ) );
		e.Show();
	}

	OnLoaded( element: Element ) {
		this.ViewElement = $( element );
		if( !this.ViewElement.text().trim() && !this.ViewElement.children().length ) {
			this.ViewElement.text( "Right-click here" );
		}

		this.CtxMenu.MenuElement( this.MenuTemplate.clone().appendTo( "body" ).hide()[0] );
		this.CtxMenu.OnLoaded( element );
		ko.applyBindings( this, this.CtxMenu.MenuElement() );
	}

	OnSaved( data ) { this._onSaved( this.ViewElement, data ); }

	ControlsDescendantBindings = false;
}

class EditorVm<T> {
	Element: JQuery;
	IsLoading = ko.observable( false );
	IsLoadingAttachments = ko.observable( false );
	Loaded = false;
	InfoBox = new infoBox();
	BBQuickReference = <c.IVirtualControl>{
		OnLoaded: BBRefTemplate,
		ControlsDescendantBindings: true,
		SupportsVirtualElements: true
	};

	LastFocusedBBField: bbTextField.BBTextFieldVm;
	FromJS = (_: Hash) => void(0);
	ToJS = () => (<Hash>{ });

	Data: T;
	DataControl: { [key: string]: any } = <any><c.IControl>{
		OnLoaded: ( e: Element ) => {
			this.Parent.EditorTemplate.call( this.DataControl, e );
		},
		ControlsDescendantBindings: true
	};

	private IsAcceptingAttachments = ko.observable( false );
	private Attachments = ko.observableArray<AttachmentVm<T>>();
	private Uploader = new uploader.Uploader();

	constructor( public Parent: InPlaceEditorVm<T> ) {
		for ( var x in Parent.EmptyData ) {
			var v = Parent.EmptyData[x];
			var mapped: Ko.Observable<any>;

			if ( v == bbTextField.IAm ) {
				var field = new bbTextField.BBTextFieldVm();
				this.DataControl[x] = field;
				this.BindToFieldDrop( field );
				mapped = field.Value;
			}
			else {
				this.DataControl[x] = mapped = ko.observable( v );
			}

			this.FromJS = ( ( prev, mapped, x ) => h => { prev( h ); mapped( h[x] ); })( this.FromJS, mapped, x );
			this.ToJS = ( ( prev, mapped, x ) => () => { var h = prev(); h[x] = mapped(); return h; })( this.ToJS, mapped, x );
		}

		this.EnsureLoaded();
	}

	BindToFieldDrop( f: bbTextField.BBTextFieldVm ) {
		f.JQueryDrop.subscribe( element => {
			var ctx = ko.contextFor( element );
			var att = ctx && <AttachmentVm<T>>ctx.$data;
			if ( att instanceof AttachmentVm ) this.InsertAttachment( att, f );
		});

		f.NativeDrop.subscribe( dt => {
			if ( dt && dt.files && dt.files.length ) {
				this.UploadAndInsert( dt.files, f );
			}
		});

		if ( !this.LastFocusedBBField ) this.LastFocusedBBField = f;
		f.IsFocused.subscribe( focused => focused && ( this.LastFocusedBBField = f ) );
	}

	InsertAttachment( att: AttachmentVm<T>, field: bbTextField.BBTextFieldVm ) {
		field = field || this.LastFocusedBBField;
		if ( !field || !att.ViewModel() ) return;

		var pos = field.GetCaretPosition();
		var value = field.Value();
		field.Value( value.substring( 0, pos ) + att.ViewModel().AsBBCode() + value.substring( pos ) );
	}

	EnsureLoaded() {
		if( this.Loaded ) return;

		this.InfoBox.Info( "Loading..." );
		c.Api.Get( this.Parent.Ajax.Load( this.Parent.ObjectId ), null, this.IsLoading,
			this.InfoBox.Error, r => {
				this.InfoBox.Clear();

				this.Data = r;
				this.FromJS( r );
				this.Loaded = true;

				c.Api.Get( this.Parent.Ajax.GetAttachments( this.Data ), null, this.IsLoadingAttachments,
					this.InfoBox.Error, ( r: dyn.ClassRef[] ) => this.Attachments( r.map( cr => new AttachmentVm<T>( this, dyn.bind( cr ) ) ) ) );
			});
	}

	Save() {
		this.InfoBox.Info( "Saving..." );
		c.Api.Post( this.Parent.Ajax.Update, this.ToJS(), this.IsLoading, this.InfoBox.Error, result => {
			this.InfoBox.Clear();
			this.Close();
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

	DropAttachments( _, je: JQueryEventObject ) {
		this.IsAcceptingAttachments( false );

		var e = <DragEvent>(je.originalEvent || je);
		e && e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length && this.Upload( e.dataTransfer.files ).subscribe();
	}

	DragOverAttachments( _, je: JQueryEventObject ) { this.IsAcceptingAttachments( true ); }
	DragOutAttachments() { this.IsAcceptingAttachments( false ); }

	ChooseAttachments() {
		if ( this.LastFocusedBBField ) this.UploadAndInsert( null, this.LastFocusedBBField );
	}

	Upload( files: FileList ): Rx.IObservable<AttachmentVm<T>> {
		return this.Uploader
			.Upload( this.Parent.Ajax.UploadAttachment( this.Data ), null, files )
			.selectMany( att.fromUploadResult )
			.select( a => {
				var res = new AttachmentVm<T>( this, a );
				this.Attachments.push( res );
				return res;
			})
			.doAction( () => { }, ( ex: any ) => this.InfoBox.Error( ex ) );
	}

	UploadAndInsert( files: FileList, field: bbTextField.BBTextFieldVm ) {
		var pos = field.GetCaretPosition();
		this.Upload( files )
			.selectMany( a => c.koToRx( a.ViewModel ) )
			.where( vm => !!vm )
			.take( 1 )
			.select( avm => avm.AsBBCode() )
			.where( bb => !!bb )
			.subscribe( bb => { field.Value( field.Value().substring(0, pos) + bb + field.Value().substring(pos) ); pos += bb.length; });
	}

	_onKey = ( e: JQueryEventObject ) => {
		if( e.which == 27 ) this.Close();
		else if( e.which == 13 && e.ctrlKey ) this.Save();
	};
}

class AttachmentVm<T> {
	private Visual = ko.computed( () => this.ViewModel() && this.ViewModel().Render() );

	private DraggableDefinition: JQueryUI.DraggableOptions = {
		helper: ( () => { 
			var uiRoot = this.Parent.Element;
			return function () { return $( this ).clone().addClass( "in-place-editor--file--dragging" ).appendTo( uiRoot ); };
		})()
	};

	constructor( private Parent: EditorVm<T>, public ViewModel: Ko.Observable<att.IAttachment> ) { }

	Remove() {
	}

	Insert() {
		this.Parent.InsertAttachment( this, null );
	}
}

interface Hash { [key: string]: any }