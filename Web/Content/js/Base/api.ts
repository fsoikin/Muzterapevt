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
	inProgress: Ko.Observable<boolean>,
	onError: ( err: string ) => void,
	onSuccess: ( result: any ) => void, options?: {} )
{
	return Call( "GET", url, data, inProgress, onError, onSuccess, options );
}

export function Post( url: string, data: any,
	inProgress: Ko.Observable<boolean>,
	onError: ( err: string ) => void,
	onSuccess: ( result: any ) => void, options?: {} )
{
	return Call( "POST", url, JSON.stringify( data ), inProgress, onError, onSuccess, options );
}

export function Call( method: string, url: string, data: any,
	inProgress: Ko.Observable<boolean>,
	onError: ( err: string ) => void,
	onSuccess: ( result: any ) => void, options?: {} )
{
	inProgress && inProgress( true );
	return $
		.ajax( $.extend( options || {}, { type: method, url: AbsoluteUrl( url ), data: data, contentType: 'application/json' }) )
		.fail( e => onError && onError( e.statusText ) )
		.always( () => inProgress && inProgress( false ) )
		.done( ( e: JsonResponse ) => e.Success
			? ( onSuccess && onSuccess( e.Result ) )
			: ( onError && onError( ( e.Messages || [] ).join() ) ) );
}

export function PageUrl( url: string ) {
	if( url && url[0] == '/' ) url = url.substring( 1 );
	return RootUrl + url;
}

export function AbsoluteUrl( url: string ) {
	if( url && url[0] == '/' ) url = url.substring( 1 );
	return RootUrl + '-/' + url;
}

declare module "RootUrl" { };
import _root = require( "RootUrl" );

export var RootUrl = ((r: string) =>
	typeof r !== "string" || !r.length
		? "/"
		: r[r.length-1] == '/' ? r : r + '/')(<string>_root);