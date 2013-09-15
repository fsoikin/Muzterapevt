/// <reference path="../../tsd/jquery.amd.d.ts"/>
/// <reference path="../../tsd/knockout-2.2.d.ts"/>
import $ = require( "jQuery" );

// TODO: [fs] provide RX-based API alternative

export interface JsonResponse {
	Success: boolean;
	Result: any;
	Messages: string[];
}

export function Get( url: string, data: any,
	inProgress: Ko.Observable<boolean>, error: Ko.Subscribable<string>,
	onSuccess: ( result: any ) => void, options?: {} )
{
	return Call( "GET", url, data, inProgress, error, onSuccess, options );
}

export function Post( url: string, data: any,
	inProgress: Ko.Observable<boolean>, error: Ko.Subscribable<string>,
	onSuccess: ( result: any ) => void, options?: {} )
{
	return Call( "POST", url, data, inProgress, error, onSuccess, options );
}

export function Call( method: string, url: string, data: any,
	inProgress: Ko.Observable<boolean>, error: Ko.Subscribable<string>,
	onSuccess: ( result: any ) => void, options?: {} )
{
	inProgress && inProgress( true );
	return $
		.ajax( $.extend( options || {}, { type: method, url: AbsoluteUrl( url ), data: data } ) )
		.fail( e => error && error.notifySubscribers( e.statusText ) )
		.done( ( e: JsonResponse ) => e.Success
			? ( onSuccess && onSuccess( e.Result ) )
			: ( error && error.notifySubscribers( ( e.Messages || [] ).join() ) ) )
		.always( () => inProgress && inProgress( false ) );
}

export function AbsoluteUrl( url: string ) {
	if( url && url[0] == '/' ) url = url.substring( 1 );
	return RootUrl + url;
}

declare module "RootUrl" { };
import _root = require( "RootUrl" );

export var RootUrl = _root =>
	typeof _root !== "string" || !_root.length
		? "/"
		: _root[_root.length] == '/' ? _root : _root + '/';