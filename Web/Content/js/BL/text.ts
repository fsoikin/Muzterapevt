/// <amd-dependency path="css!styles/Text.css" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ed = require( "./InPlaceEditor" );

export = TextVm;

var Ajax = {
	Load: id => "text/load?id=" + id,
	Update: "text/update"
};

// Defined in UI/JS/TextSaveResult.cs
interface TextSaveResult {
	Html: string;
}

// Defined in UI/JS/TextEditor.cs
class TextEditor {
	Id = "";
	Text = "";
}

class TextVm extends ed.InPlaceEditorVm {
	constructor( args: { id: number }) {
		super( {
			ajax: Ajax,
			id: args.id,
			emptyData: new TextEditor(),
			onSaved: ( e: JQuery, data: TextSaveResult ) => e.html( data.Html ),
			editorTemplate: EditorTemplate
		});
	}
}

declare module "text!./Templates/text-editor.html" { }
import _template = require( "text!./Templates/text-editor.html" );
var EditorTemplate = $( _template );