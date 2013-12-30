/// <amd-dependency path="css!styles/Text.css" />
/// <amd-dependency path="text!./Templates/text-editor.html" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import rx = require( "rx" );
import ed = require( "./InPlaceEditor" );
import bbTextField = require( "./BBTextField" );

var Ajax = {
	Load: id => "text/load?id=" + id,
	LoadHtml: id => "text/loadHtml?id=" + id,
	GetAttachments: ( text: TextEditor ) => "text/attachment-getAll?textId=" + text.Id,
	UploadAttachment: ( text: TextEditor ) => "text/attachment-upload?textId=" + text.Id,
	Update: "text/update"
};

// Defined in UI/JS/TextSaveResult.cs
interface TextSaveResult {
	Html: string;
}

// Defined in UI/JS/TextEditor.cs
export class TextEditor {
	Id = "";
	Text = bbTextField.IAm;
}

export class TextVm extends ed.InPlaceEditorVm<TextEditor> {
	constructor( args: { id: string; onSaved?: (html: string) => void }) {
		super( {
			ajax: Ajax,
			id: args.id,
			emptyData: <any>new TextEditor(),
			onSaved: ( e: JQuery, data: TextSaveResult ) => {
				e.html( data.Html );
				args.onSaved && args.onSaved( data.Html );
			},
			editorTemplate: EditorTemplate
		});
	}
}

export class TextView implements c.IControl {
	Html = ko.observable( "" );
	Vm = ko.observable<TextVm>();

	constructor( args: { id: string }) {
		c.Api.Get( Ajax.LoadHtml( args.id ), null, null, null, ( t: server.TextView) => {
			this.Html( t.Text );
			if ( t.AllowEdit ) this.Vm( new TextVm( { id: args.id, onSaved: this.Html } ) );
		});
	}

	OnLoaded( e: Element ) {
		var $e = $( e );
		var pipe = ko.computed( () => $e.html( this.Html() ) );
		ko.utils.domNodeDisposal.addDisposeCallback( e, () => pipe.dispose() );

		c.koToRx( this.Vm ).where( v => !!v ).take( 1 ).subscribe( vm => vm.OnLoaded( e ) );
	}

	ControlsDescendantBindings = true;
}

var EditorTemplate = $( require( "text!./Templates/text-editor.html" ) );