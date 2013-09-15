import c = require( "../common" );
import $ = require( "jQuery" );
import pages = require( "./pages" );

export = init;

function init( root: JQuery ) {
	new pages.PagesVm().OnLoaded( root[0] );
};