/// <amd-dependency path="css!styles/Page.css" />
/// <amd-dependency path="text!./Templates/page-editor.html" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ed = require( "./InPlaceEditor" );
import bbTextField = require( "./BBTextField" );

var Ajax = <ed.IInPlaceEditorAjax<PageEditor>>{
	Load: id => "page/load?id=" + id,
	GetAttachments: page => "page/attachment-getAll?pageId=" + page.Id,
	UploadAttachment: page => "page/attachment-upload?pageId=" + page.Id,
	Update: "page/update"
};

// Defined in UI/JS/PageSaveResult.cs
interface PageSaveResult {
	Title: string;
	Html: string;
}

// Defined in UI/JS/TextSaveResult.cs
export class PageEditor {
	Id = 0;
	Path = "";
	Title = "";
	Text = bbTextField.IAm;
	TagsStandIn = "";
	ReferenceName = "";
}

export class PageVm extends ed.InPlaceEditorVm<PageEditor> {
	constructor( args: { id: number }) {
		super( {
			ajax: Ajax,
			id: args.id,
			emptyData: <any>new PageEditor(),
			onSaved: ( e: JQuery, data: PageSaveResult ) => e.html( data.Html ).prepend( $( "<h2>" ).text( data.Title ) ),
			editorTemplate: EditorTemplate
		});
	}
}

var EditorTemplate = $( require( "text!./Templates/page-editor.html" ) );