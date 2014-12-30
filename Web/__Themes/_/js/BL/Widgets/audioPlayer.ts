/// <amd-dependency path="jquery.jplayer" />
import c = require( "../../common" );
import $ = require( 'jQuery' );

export = audioPlayer;

function audioPlayer( options: { location: JQuery; audioFile: string } ) {
    var size = { width: options.location.width(), height: options.location.height() };

    var controlsId = "_" + Math.floor(Math.random() * 10000).toString();
    var controls = $(template)
        .appendTo("body")
        .attr("id", controlsId)
        .css({ position: "absolute" })
        .position({ my: "left top", at: "left top", of: options.location });

    var player = 
				(<any>$("<div>").appendTo("body"))
        .jPlayer({
            ready: function () { (<any>$(this)).jPlayer("setMedia", { mp3: options.audioFile }).jPlayer("play"); },
            cssSelectorAncestor: "#" + controlsId,
            //swfPath: require. + "audio",
            supplied: "mp3"
        });

    return function () { player.jPlayer("destroy"); controls.remove(); }
}

var template = 
    '<div class="jp-audio">                                                                                                              \
        <div class="jp-controls">                                                                                                       \
            <a href="javascript:;" class="jp-play" tabindex="1">&nbsp;</a>                                                              \
            <a href="javascript:;" class="jp-pause" tabindex="1">&nbsp;</a>                                                             \
        </div>                                                                                                                          \
        <div class="jp-current-time"></div>                                                                                             \
        <div class="jp-progress">                                                                                                       \
            <div class="jp-seek-bar">                                                                                                   \
                <div class="jp-play-bar"></div>                                                                                         \
            </div>                                                                                                                      \
        </div>                                                                                                                          \
        <div class="jp-duration"></div>                                                                                                 \
        <div class="jp-volume-bar">                                                                                                     \
            <div class="jp-volume-bar-value"></div>                                                                                     \
        </div>                                                                                                                          \
    </div>';