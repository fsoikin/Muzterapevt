/// <amd-dependency path="text!./Templates/Attachment.html" />
/// <amd-dependency path="css!./Templates/Attachment.css" />
import c = require( "../common" );
import api = require( "../Base/api" );
import $ = require( "jQuery" );
import rx = require( "rx" );
import ko = require( "ko" );
import dyn = require( "../Lib/dynamic" );

export function fromUploadResult( res: api.JsonResponse ): Rx.IObservable<Ko.Observable<IAttachment>> {
	return ( res.Success 
			? rx.Observable.fromArray( <dyn.ClassRef[]>( res.Result || [] ) )
			: rx.Observable.throwException<dyn.ClassRef>( ( res.Messages || [] ).join( ", " ) )
		)
		.select( cr => dyn.bind( cr ) );
}

export interface AttachmentDef {
	FileId: number;
	Path: string;
	Download: string;
	IconClass?: string;
	SmallThumb?: string;
	BigThumb?: string;
}

export interface IAttachment {
	FileId(): number;
	Render(): c.IControl;
	AsBBCode(): string;
}

export class File implements IAttachment {
	constructor( public Def: AttachmentDef ) { }

	FileId() { return this.Def.FileId; }

	Render(): c.IControl {
		return new Visual( this.Def, Templates.File );
	}
	AsBBCode() {
		return "[file=\"" + this.Def.Path + "\"]" + this.Def.Path + "[/file]";
	}
}

export class Picture implements IAttachment {
	constructor( public Def: AttachmentDef ) { }

	FileId() { return this.Def.FileId; }

	Render(): c.IControl {
		return new Visual( this.Def, Templates.Picture );
	}
	AsBBCode() {
		return "[img=\"" + this.Def.Path + "\"]";
	}
}

class Visual implements c.IControl {
	constructor( public Def: AttachmentDef, template: ( e: Element ) => void ) {
		this.OnLoaded = template;
	}

	OnLoaded: ( e: Element ) => void;
	ControlsDescendantBindings = true;
}

module Templates {
	var t = $( require( "text!./Templates/Attachment.html" ) );
	export var File = c.ApplyTemplate( t.filter( ".attachment-file" ) );
	export var Picture = c.ApplyTemplate( t.filter( ".attachment-picture" ) );
}