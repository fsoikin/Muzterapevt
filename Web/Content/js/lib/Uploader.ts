import er = require( "../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );

export class Uploader {
	CurrentFiles = ko.observableArray<UploadingFile>();
	IsUploading = ko.computed( () => this.CurrentFiles().some( f => !f.IsFinished() ) )
   private _input: HTMLInputElement;

	public Upload( url: string, data: { [key: string]: string; }, files?: FileList ) {
		this.CleanFinishedFiles();

		if ( files ) {
			this.AddFiles( files, url, data );
		} else {
			this.EnsureInput();
			$( this._input )
				.one( "change", () => this.AddFiles( this._input.files, url, data ) )
				.click();
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

	private AddFiles( files: FileList, url: string, data: { [key: string]: string; }) {
		for ( var i = 0; i < files.length; i++ )
			this.CurrentFiles.push( new UploadingFile( files[i], url, data ) );
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

	constructor( public File: File, url: string, data: { [key: string]: string; }) {
		var rd = new FileReader();
		rd.onloadend = () => this.DataUrl( rd.result );
		rd.readAsDataURL( this.File );

		this.Name( this.File.name );

		var form = new FormData();
		$.each( data, ( key, value ) => form.append( key, value ) );
		form.append( "file", File );

		this._xhr = new XMLHttpRequest();
		this._xhr.upload.onprogress = e => e.lengthComputable && this.Progress( 100 * e.loaded / e.total );
		this._xhr.onloadend = e => this.IsFinished( true );
		this._xhr.onerror = e => this.Error( this._xhr.responseText );
		this._xhr.onabort = e => this.Error( "Upload aborted" );
		this._xhr.ontimeout = e => this.Error( "Upload timed out" );
		this._xhr.open( "POST", url, true );
		this._xhr.send( form );
	}

	public Dispose() {
		if ( !this.IsFinished() ) this._xhr.abort();
	}
}