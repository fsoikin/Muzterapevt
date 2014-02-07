/// <amd-dependency path="text!./Templates/youtubePlaylist.html" />
/// <amd-dependency path="css!./Templates/youtubePlaylist.css" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import linq = require( "linq" );

export = Vm;
var _template = $( require( "text!./Templates/youtubePlaylist.html" ) );

interface Video {
	Thumbnail: string;
	Title: string;
	Url: string;
	Description: string;
}

class Vm extends c.TemplatedControl {
	private Videos = ko.observableArray<Video>();
	private Error = ko.observable( "" );

	constructor( args: { id: number } ) {
		super( _template );

		$.get( "http://gdata.youtube.com/feeds/api/playlists/" + args.id )
			.done( r => this.ParseXml( r ) )
			.error( ( xhr, st, err ) => this.Error( err ) );
	}

	ParseXml( xml: string ) {
		var entries = linq
			.from( $( xml ).find( "entry" ) )
			.select( x => $( x ) ) //https://typescript.codeplex.com/workitem/1988
			.where( x => x.find("state[name=restricted]").length == 0 )
			.select( e => <Video>{
				Title: e.children( "title" ).text(),
				Description: e.children( "content" ).text(),
				Url: e.find( "player" ).attr("url"),
				Thumbnail: linq
					.from( e.find( "thumbnail" ) )
					.select( x => $( x ) ) //https://typescript.codeplex.com/workitem/1988
					.orderBy( x => parseInt( x.attr( "width" ) ) )
					.thenBy( x => parseInt( x.attr( "height" ) ) )
					.select( x => x.attr( "url" ) )
					.firstOrDefault()
			})
			.toArray();
		this.Videos( entries );
	}
}