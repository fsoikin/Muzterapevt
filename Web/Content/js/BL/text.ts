/// <amd-dependency path="css!styles/Text.css" />
/// <amd-dependency path="text!./Templates/text-editor.html" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ed = require( "./InPlaceEditor" );
import bbTextField = require( "./BBTextField" );

var Ajax = <ed.IInPlaceEditorAjax<TextEditor>>{
	Load: id => "text/load?id=" + id,
	GetAttachments: text => "text/attachment-getAll?textId=" + text.Id,
	UploadAttachment: text => "text/attachment-upload?textId=" + text.Id,
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
	constructor( args: { id: number }) {
		super( {
			ajax: Ajax,
			id: args.id,
			emptyData: <any>new TextEditor(),
			onSaved: ( e: JQuery, data: TextSaveResult ) => e.html( data.Html ),
			editorTemplate: EditorTemplate
		});
	}
}

var EditorTemplate = $( require( "text!./Templates/text-editor.html" ) );