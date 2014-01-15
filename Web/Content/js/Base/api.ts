/// <reference path="../../tsd/jquery.amd.d.ts"/>
/// <reference path="../../tsd/knockout-2.2.d.ts"/>
/// <amd-dependency path="RootUrl" />
import $ = require( "jQuery" );
import rx = require( "rx" );

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
	return makeXhr( method, url, data, options )
		.fail( e => onError && onError( e.statusText ) )
		.always( () => inProgress && inProgress( false ) )
		.done( ( e: JsonResponse ) => e.Success
			? ( onSuccess && onSuccess( e.Result ) )
			: ( onError && onError( ( e.Messages || [] ).join() ) ) );
}


export function GetAsRx<T>( url: string, data?: any, options?: any ) {
	return CallAsRx<T>( "GET", url, data, options );
}

export function PostAsRx<T>( url: string, data?: any, options?: any ) {
	return CallAsRx<T>( "POST", url, JSON.stringify( data ), options );
}

export function CallAsRx<T>( method: string, url: string, data?: any, options?: any ) {
	return rx.Observable.create<T>( o => {
		var xhr = makeXhr( method, url, data, options )
			.fail( e => o.onError( e ) )
			.done( ( x: JsonResponse ) => x.Success
				? ( o.onNext( x.Result ) )
				: ( o.onError( ( x.Messages || [] ).join() ) ) )
			.always( e => o.onCompleted() );
		return () => xhr.abort();
	});
}

function makeXhr( method: string, url: string, data: any, options?: {} )
{
	return $.ajax( $.extend( options || {}, { type: method, url: AbsoluteUrl( url ), data: data, contentType: 'application/json' }) );
}

export function PageUrl( url: string ) {
	if( url && url[0] == '/' ) url = url.substring( 1 );
	return RootUrl + url;
}

export function AbsoluteUrl( url: string ) {
	if( url && url[0] == '/' ) url = url.substring( 1 );
	return RootUrl + '-/' + url;
}

var _root = require( "RootUrl" );

export var RootUrl = ((r: string) =>
	typeof r !== "string" || !r.length
		? "/"
		: r[r.length-1] == '/' ? r : r + '/')(<string>_root);