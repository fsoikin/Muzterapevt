/// <amd-dependency path="css!styles/Page.css" />
/// <amd-dependency path="text!./Templates/page-editor.html" />
import c = require( "../common" );
import $ = require( "jQuery" );
import ed = require( "./InPlaceEditor" );
import bbTextField = require( "./BBTextField" );

var Ajax = {
	Load: id => "page/load?id=" + id,
	GetAttachments: page => "page/attachment-getAll?pageId=" + page.Id,
	UploadAttachment: page => "page/attachment-upload?pageId=" + page.Id,
	Update: "page/update",
	Reorder: "page/reorder"
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
			ajax: <ed.IInPlaceEditorAjax<PageEditor>>Ajax,
			id: args.id,
			emptyData: <any>new PageEditor(),
			onSaved: ( e: JQuery, data: PageSaveResult ) => e.html( data.Html ).prepend( $( "<h2>" ).text( data.Title ) ),
			editorTemplate: EditorTemplate
		});
	}
}

export class ChildPagesVm implements c.IControl {
	constructor( private args: { pageId: number }) { }

	ControlsDescendantBindings = false;
	OnLoaded( e: Element ) {
		$( e ).sortable( {
			items: 'li',
			placeholder: 'drop-target',
			update: () => c.Api.PostAsRx( Ajax.Reorder, <server.PageReorderRequest>{
				ParentId: this.args.pageId,
				Children: $( e ).children( "li[data-pageid]" ).toArray().map( c => parseInt( $(c).data( "pageid" ) ) )
			}).subscribe()
		});
	}
}

var EditorTemplate = $( require( "text!./Templates/page-editor.html" ) );