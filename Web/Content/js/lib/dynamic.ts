import er = require( "../common" );
import ko = require( "ko" );

export interface ClassRef {
	// TODO: [fs] currently this definition assumes that the module ID
	// should be an "absolute" one (i.e. not starting with a dot or two).
	// For my current purposes (instantiating search filters), this is
	// enough. The general plan for the future is to add a "require"
	// field to this interface, which would hold the local require
	// function of whoever created the particular instance.
	Module: string;
	Class?: string;
	Arguments?: any;
}

export function parse( classRef: string, args?: any ): ClassRef {
	var parts = ( classRef || "" ).split( ',' );
	if( parts.length == 1 ) parts = ["", parts[0]];
	if( parts.length != 2 ) { console.warn( "Invalid autobind controller specification: " + classRef ); return null; }

	if( typeof args === "string" ) {
		try { args = new Function( "return (" + args + ");" )(); }
		catch( _ ) { }
	}

	return { Module: parts[1].trim(), Class: parts[0].trim(), Arguments: args };
}

export function bind( ref: ClassRef ) {
	return bindMany( [ref] )[0];
}

export function bindMany( refs: ClassRef[] ): Ko.Observable<any>[] {
	refs = refs || [];
	return refs.map( cr => {
		var r = ko.observable(null);
		if ( cr ) req( [cr.Module], m => r( instantiate( m, cr ) ) );
		return r;
	} );
}

export function bindAll( refs: ClassRef[] ) {
	if( !refs ) return ko.observableArray();

	var result = ko.observableArray();
	req( refs.map( r => r.Module ), function () {
		result(
			ko.utils.makeArray( arguments )
			.map( ( m, idx ) => instantiate( m, refs[idx] ) )
		);
	} );
	return result;
}

function instantiate( mod, ref: ClassRef ) {
	var ctor = ref.Class ? getClass(mod, ref.Class) : mod;

	// TODO: [fs] this probably shouldn't throw, but should return the error up the chain so that the caller can handle it
	if( !ctor ) throw new Error( "Cannot find class '" + ref.Class + "' in module '" + ref.Module + "'." );

	if( ref.Arguments !== undefined && ref.Arguments !== null ) {
		ctor = Function.prototype.bind.apply( ctor,
			Array.isArray( ref.Arguments )
				? [null].concat( ref.Arguments )
				: [null, ref.Arguments] );
	}

	return new ctor();
}

function getClass( mod, cls: string ) {
	return cls.split( '.' ).reduce( (m,p) => m[p], mod );
}

var req = (() => {
	var loadedModules = {};

	return function( deps: string[], cb: Function ) {
		var loaded = deps.map( d => loadedModules[d] );
		if( loaded.every( d => d ) ) cb.apply( this, loaded );
		else require( deps, () => {
			var resolved = arguments;
			deps.every( ( d, idx ) => loadedModules[d] = loaded[idx] = resolved[idx] );
			cb.apply( this, loaded );
		} );
	};
} )();