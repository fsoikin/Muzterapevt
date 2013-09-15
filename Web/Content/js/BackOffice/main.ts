import c = require( "../common" );
import $ = require( "jQuery" );
import pages = require( "./pages" );

new pages.PagesVm().OnLoaded( $( "body" )[0] );