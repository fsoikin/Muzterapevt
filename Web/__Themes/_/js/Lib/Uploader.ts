import er = require( "../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import rx = require( "rx" );
import api = require( "../Base/api" );

export class Uploader {
	CurrentFiles = ko.observableArray<UploadingFile>();
	IsUploading = ko.computed( () => this.CurrentFiles().some( f => !f.IsFinished() ) );
	ProgressPercent = ko.computed( () => {
		var files = this.CurrentFiles();
		return files.length ? files.reduce( ( sum: number, f ) => ( sum + f.Progress() ), 0 ) / files.length : 0; //https://typescript.codeplex.com/workitem/1989
	} )
    private _input: HTMLInputElement;

	public Upload( url: string, data?: { [key: string]: string; }, files?: FileList ): Rx.IObservable<api.JsonResponse> {
		this.CleanFinishedFiles();

		if ( files ) {
			return this.AddFiles( files, url, data );
		} else {
            this.EnsureInput();
            $( this._input ).click();
            return rx.DOM
                .fromEvent( this._input, "change" )
                .take( 1 )
                .selectMany( () => this.AddFiles( this._input.files, url, data ) );
		}
	}

	private EnsureInput() {
		this._input = <HTMLInputElement>
		$( "<input type='file' multiple='multiple'>" )
			.css( { position: "absolute", left: -1000, top: -1000 })
			.appendTo( "body" )[0];
	}

	private CleanFinishedFiles() {
		var finished = ko.utils.arrayFilter( this.CurrentFiles(), ( f: UploadingFile ) => f.IsFinished() );
		ko.utils.arrayForEach( finished, f => this.CurrentFiles.remove( f ) );
	}

	private AddFiles( files: FileList, url: string, data: { [key: string]: string; }): Rx.IObservable<api.JsonResponse> {
		var result = new rx.Subject<api.JsonResponse>();
		for ( var i = 0; i < files.length; i++ )
			this.CurrentFiles.push( new UploadingFile( files[i], url, data, result ) );
		return result;
	}

	public Dispose() {
		var ff = this.CurrentFiles();
		ko.utils.arrayForEach( ff, ( f: UploadingFile ) => f.Dispose() );
		this.CurrentFiles.removeAll();

		!this._input || $( this._input ).remove();
	}
}

export class UploadingFile {
	public Name = ko.observable( "" );
	public Progress = ko.observable( 0 ); // Percent
	public IsFinished = ko.observable( false );
	public Error = ko.observable( "" );
	public DataUrl = ko.observable( "" );
	private _xhr: XMLHttpRequest;

	constructor( public File: File, url: string, data: { [key: string]: string; }, result: Rx.IObserver<api.JsonResponse>, headers?: any ) {
		var rd = new FileReader();
		rd.onloadend = () => this.DataUrl( rd.result );
		rd.readAsDataURL( this.File );

		this.Name( this.File.name );

		var form = new FormData();
		$.each( data || {}, ( key, value ) => form.append( key, value ) );
		form.append( "file", File );

		this._xhr = new XMLHttpRequest();
		this._xhr.upload.onprogress = e => e.lengthComputable && this.Progress( 100 * e.loaded / e.total );
		this._xhr.onloadend = e => {
			this.IsFinished( true );

			var resp;
			try { resp = JSON.parse( this._xhr.response ); } catch ( ex ) { resp = this._xhr.response; }
			result.onNext( resp );
		};
		this._xhr.onerror = e => this.Error( this._xhr.responseText );
		this._xhr.onabort = e => this.Error( "Upload aborted" );
		this._xhr.ontimeout = e => this.Error( "Upload timed out" );
		this._xhr.open( "POST", er.Api.AbsoluteUrl( url ), true );
		if ( headers ) for ( var h in headers ) this._xhr.setRequestHeader( h, headers[h] );
		this._xhr.send( form );
	}

	public Dispose() {
		if ( !this.IsFinished() ) this._xhr.abort();
	}
}