/// <amd-dependency path="css!./Templates/Playlist.css" />
import c = require( "../../common" );
import $ = require( "jQuery" );
import audioPlayer = require( "./audioPlayer" );

export class Vm implements c.IControl {
    constructor( private _args: { songs: { href: string; name: string; index?: string; length?: string }[] } ) {
    }

    ControlsDescendantBindings = true;
    OnLoaded( element: Element ) {
        var table = $( "<table>" ).addClass( "Playlist" ).appendTo( element );
        var closePlayer = null;
        $.each( this._args.songs, function () {
            var song = this;
            var tr = $( "<tr>" )
                .appendTo( table )
                .append( $( "<td class='Index'>" ).text( song.index ) );

            var name = $( "<td class='Name'>" ).appendTo( tr );
            if ( song.name ) name.append( $( "<div class='Name'>" ).text( song.name ) );
            if ( song.length ) name.append( $( "<div class='Length'>" ).text( "(" + song.length + ")" ) );

            if ( song.href ) {
                tr.append( "<td class='Link'>" ).append(
                    $( "<a>" ).text( "[play]" ).attr( "href", "#" )
                        .click( function () {
                            if ( closePlayer ) closePlayer();
                            closePlayer = audioPlayer( { location: $( "td.Link", tr ), audioFile: song.href });
                            return false;
                        })
                    );
            }
        });
    }
}