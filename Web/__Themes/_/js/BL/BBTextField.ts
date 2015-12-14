import c = require( "../common" );
import ko = require( "ko" );
import rx = require( "rx" );
import $ = require( "jQuery" );

export var IAm = "{EBA9F6C3-0A31-46AA-BB1C-6B7A183CE37F}";

export class BBTextFieldVm implements c.IControl {
	Element: Element;
	_value = ko.observable( "" );
	Value = ko.computed<string>( {
		read: () => this._value(), //https://typescript.codeplex.com/workitem/1988
		write: v => {
			var txtarea = <HTMLTextAreaElement>this.Element;
			if ( !( txtarea instanceof HTMLTextAreaElement ) )
				throw "Not supported";

			var scrollPos = txtarea.scrollTop;
			var strPos = this.GetCaretPosition();
			this._value( v );
			this.SetCaretPosition( strPos );
			txtarea.scrollTop = scrollPos;
		}
	});

	IsFocused = ko.observable( false );
	NativeDrop = new rx.Subject<DataTransfer>();
	JQueryDrop = new rx.Subject<Element>();
	ControlsDescendantBindings = false;

	OnLoaded( element ) {
		this.Element = element;

		ko.applyBindingsToNode( element, {
			'value': this._value,
			'valueUpdate': 'keyup',
			'event': {
				drop: function ( _, e ) {
					var de = <DragEvent>( e.originalEvent || e );
					var dt = de.dataTransfer;
					if ( dt ) this.NativeDrop.onNext( dt );
				},
				focus: () => this.IsFocused( true ),
				blur: () => this.IsFocused( false )
			},
			'jqWidget': {
				droppable: {
					drop: ( e, source: { draggable: JQuery }) => {
						var element = source.draggable[0];
						if ( element ) this.JQueryDrop.onNext( element );
					}
				}
			}
		}, this );
	}

	GetCaretPosition() {
		var txtarea = <HTMLTextAreaElement>this.Element;
		if ( !( txtarea instanceof HTMLTextAreaElement ) ) throw "Not supported";

		var br = getBr( txtarea );
		if ( br == "ie" ) {
			txtarea.focus();
			var range = ieCreateRange();
			range.moveStart( 'character', -txtarea.value.length );
			return range.text.length;
		} else if ( br == "ff" ) {
			return txtarea.selectionStart;
		}
	}

	SetCaretPosition( pos: number ) {
		var txtarea = <HTMLTextAreaElement>this.Element;
		if ( !( txtarea instanceof HTMLTextAreaElement ) ) throw "Not supported";

		var br = getBr( txtarea );
		if ( br == "ie" ) {
			txtarea.focus();
			var range = ieCreateRange();
			range.moveStart( 'character', -txtarea.value.length );
			range.moveStart( 'character', pos );
			range.moveEnd( 'character', 0 );
			range.select();
		} else if ( br == "ff" ) {
			txtarea.selectionStart = pos;
			txtarea.selectionEnd = pos;
			txtarea.focus();
		}
	}
}

function getBr( txtarea: HTMLTextAreaElement ) {
	return ( txtarea.selectionStart || txtarea.selectionStart == 0 ) ? "ff" : (<any>document).selection ? "ie" : null;
}

interface Range {
	moveStart( by: string, len: number ): void;
	moveEnd( by: string, len: number ): void;
	select(): void;
	text: string;
}

function ieCreateRange(): Range {
	return ( <any>document ).selection.createRange();
}