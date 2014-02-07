import er = require( "../common" );
import ko = require( "ko" );
import dyn = require( "./dynamic" );
import $ = require( "jQuery" );
import rx = require( "rx" ); (() => rx )();

export interface AutobindOptions {
	controllerKey?: string;
	argumentsKey?: string;

	getClassRef?: ( e: JQuery, opts: AutobindOptions ) => dyn.ClassRef;
}

var defaultOptions: AutobindOptions = {
	controllerKey: "controller",
	argumentsKey: "args",
	getClassRef: (e, o) => dyn.parse( e.data(o.controllerKey), e.data(o.argumentsKey) )
};

export function autobindAll( root: JQuery, opts?: AutobindOptions ) {
	opts = $.extend( {}, defaultOptions, opts );

	return root.each( function ( e ) {
		var $e = $( this );
		autobind( opts.getClassRef( $e, opts ), $e );
	} );
};

export function autobind( ref: dyn.ClassRef,
	element: JQuery, onDone?: ( JQuery, viewModel: any ) => void );
export function autobind( ref: dyn.ClassRef,
	element: Element, onDone?: ( JQuery, viewModel: any ) => void );
export function autobind( ref: dyn.ClassRef,
	element: () => JQuery, onDone?: ( JQuery, viewModel: any ) => void );

export function autobind( ref: dyn.ClassRef, element: any, onDone?: ( JQuery, viewModel: any ) => void ) {
	er.koToRx( dyn.bind( ref ) )
		.where( o => !!o )
		.subscribe( obj =>
		{
			//		if( obj == null ) { console.error( "Unable to autobind. Controller = '" + ref.Class + ", " + ref.Module + "', args = '" + JSON.stringify( ref.Arguments ) + "'" ); return; }

			var template = obj.constructor && obj.constructor.Template;
			var where: JQuery = $.isFunction( element ) ? element() : $( element );

			if( typeof obj.OnLoaded === "function" ) obj.OnLoaded( where[0] );
			else {
				if( typeof template === "function" ) template = template();
				if( typeof template === "string" ) where.html( template );
				else if( template instanceof $ ) where.empty().append( template );
				ko.applyBindings( obj, where[0] );
			}

			onDone && onDone( where, obj );
		} );
}