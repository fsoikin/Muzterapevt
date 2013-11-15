/// <amd-dependency path="css!styles/Page.css" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ed = require( "./InPlaceEditor" );

export = PageVm;

var Ajax = {
	Load: id => "page/load?id=" + id,
	UploadAttachment: id => "page/uploadAttachment?id=" + id,
	Update: "page/update"
};

// Defined in UI/JS/PageSaveResult.cs
interface PageSaveResult {
	Title: string;
	Html: string;
}

// Defined in UI/JS/TextSaveResult.cs
class PageEditor {
	Id = 0;
	Path = "";
	Title = "";
	Text = "";
	TagsStandIn = "";
	ReferenceName = "";
}

class PageVm extends ed.InPlaceEditorVm {
	constructor( args: { id: number }) {
		super( {
			ajax: Ajax,
			id: args.id,
			emptyData: new PageEditor(),
			onSaved: ( e: JQuery, data: PageSaveResult ) => e.html( data.Html ).prepend( $( "<h2>" ).text( data.Title ) ),
			editorTemplate: EditorTemplate
		});
	}
}

declare module "text!./Templates/page-editor.html" { }
import _template = require( "text!./Templates/page-editor.html" );
var EditorTemplate = $( _template );