/// <reference path="../tsd/require.d.ts"/>
/// <reference path="../tsd/polyfill.d.ts"/>
/// <reference path="../tsd/jquery.amd.d.ts"/>
/// <reference path="../tsd/jqueryui.d.ts"/>
/// <reference path="../tsd/knockout-2.2.d.ts"/>
/// <reference path="../tsd/knockout.mapping-2.0.d.ts"/>
/// <reference path="../tsd/rx.d.ts" />
/// <reference path="../tsd/rx-ko.d.ts" />
/// <reference path="../tsd/linq.d.ts"/>
/// <reference path="../tsd/linq.amd.d.ts"/>

import rx = require( "rx" );
import ko = require( "ko" );
import $ = require( "jQuery" );
export import _api = require( "./Base/api" );
export import _seq = require( "./Base/seq" );
export var Seq = _seq;
export var Api = _api;

export interface NameValuePair<T> {
	Name: string;
	Value: T;
}

export interface Range<T> {
	Start: T;
	End: T;
}

export interface IHaveId<TId> { Id: TId; }
export interface IHaveName { Name: string; }
export interface IHaveNameAndId<TId> extends IHaveId<TId>, IHaveName { }

export interface IControl {
	OnLoaded(element: Element): void;
	ControlsDescendantBindings: boolean;
}

export interface IUnloadableControl extends IControl {
	OnUnloaded( element: Element ): void;
}

export interface IVirtualControl extends IControl {
	SupportsVirtualElements: boolean;
}

export function enumToString<TEnum>(theEnum: TEnum, value: TEnum): string {
	return (<any>theEnum)[value];
}

export function mapEnum<TEnum>( obj: TEnum): NameValuePair<TEnum>[]{
	var res = new Array<NameValuePair<TEnum>>();
	for( var k in obj ) {
		var v = parseInt( k );
		if( v !== null && v !== undefined && !isNaN( v ) ) res.push( { Name: obj[k], Value: <TEnum><any>v });
	}
	return res;
}

export function dataSourceFromEnum<TEnum>( theEnum: TEnum ): IDataSource<NameValuePair<TEnum>> {
	return {
		Items: ko.observable( mapEnum( theEnum ) ),
		GetId: ( x: NameValuePair<TEnum> ) => x.Value,
		ToString: ( x: NameValuePair<TEnum> ) => x.Name
	};
}

export function dataSourceFromNameValuePair<T>( items: NameValuePair<T>[] ): IDataSource<T> {
	var ii = ko.observable<NameValuePair<T>[]>();
	setImmediate( () => ii( items ) );
	return {
		Items: ii,
		GetId: ( i: NameValuePair<T> ) => i.Value,
		ToString: ( i: NameValuePair<T> ) => i.Name,
	};
}

export function dataSourceFromServer<T extends IHaveId<any>>( url: string, toString: ( t: T ) => string );
export function dataSourceFromServer<T>( url: string, toString: ( t: T ) => string, getId: ( t: T ) => any );
export function dataSourceFromServer<T extends IHaveNameAndId<any>>( url: string, toString?: ( t: T ) => string, getId?: (t:T) => any ) {
	return () => new RemoteDataSource<T>( {
		url: url,
		getId: getId || ( u: T ) => u.Id,
		toString: toString || u => u.Name
	});
}

export class VmBase {
	IsLoading = ko.observable( false );
	IsSaving = ko.observable( false );
}

export interface IDataSource<T> {
	Items: Ko.Observable<T[]>;
	GetId( i: T ): any;
	ToString( i: T ): string;

	IsLoading?: Ko.Observable<boolean>;
	Error?: Ko.Subscribable<string>;
}

export class RemoteDataSource<T> implements IDataSource<T> {
	IsLoading = ko.observable( false );
	Error = new ko.subscribable<string>();

	Items = ko.observableArray<T>();

	GetId( i: T ) { return null; } // TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1450
	ToString( i: T ) { return ""; }

	private Url: string;
	private LookupUrl: (term:string) => string;

	constructor( args: {
		url: string;
		lookupUrl?: ( term: string ) => string;
		getId?: Function; // ( i: T ) => any; // TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1450
		toString?: Function; // ( i: T ) => string;
	}) {
		// TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1450
		var me = <any>this;
		me['GetId'] = args.getId || (i: T) => {
			if( !i ) return null;
			if( !( 'Id' in i ) ) throw new Error( "'Id' property not found on object " + JSON.stringify( i ) );
			return i['Id'];
		};
		me['ToString'] = args.toString || i => ( i || "" ).toString();

		this.Url = args.url;
		this.LookupUrl = args.lookupUrl;
		this.Reload();
	}

	Reload() {
		Api.Get( this.Url, null, this.IsLoading, err => this.Error.notifySubscribers( err ), this.Items );
	}

	Lookup( term: string ) {
		if( !this.LookupUrl ) {
			term = ( term || "" ).toLowerCase();
			return this.Items.asRx()
				.select( ( items: any[] ) => items.filter( item => {
					var t = this.ToString( item );
					return t && t.toLowerCase().indexOf( term ) >= 0;
				}) )
				.take( 1 );
		}

		return rx.Observable.create<T[]>( o => {
			var xhr = Api.Get( this.LookupUrl( term ), null, this.IsLoading,
				err => { o.onError( err ); this.Error.notifySubscribers( err ); },
				res => { o.onNext( res ); o.onCompleted(); });

			return () => xhr.abort();
		} );
	}

	GetById( id ) {
		var dd = this.Items();
		return dd && ko.utils.arrayFirst( dd, d => d.Id == id );
	}
}

export function compareArrays<T>( a: T[], b: T[],
	comparer: ( x: T, y: T ) => boolean = ( x, y ) => x == y ) {
	if( !a ) return !b;
	if( !b ) return false;
	if( a.length != b.length ) return false;

	for( var i = 0;i < a.length;i++ ) if( !comparer( a[i], b[i] ) ) return false;

	return true;
}


export function koStringToDate( str: Ko.Observable<string> ) {
	return ko.computed<Date>( {
		read: () => new Date( str() ),
		write: d => str( d && d.toJSON() )
	});
}

export function koDateToString( d: Ko.Observable<Date> ) {
	return ko.computed<string>( {
		read: () => d().toJSON(),
		write: s => d( new Date( s ) )
	});
}

// TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1423
export interface IHaveDefaultConstructor<TClass> { new (): TClass; }

export function bindClass<TArgs, TClass>(
	theClass: { new ( arg: TArgs ): TClass; }, args: TArgs )
	: IHaveDefaultConstructor<TClass>
// TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1423
{
	function _f() {
		this.constructor = theClass;
		theClass.call( this, args );
	}
	_f.prototype = theClass.prototype;
	return <any>_f;
}

export function ApplyTemplate( template: string ): ( e: Element ) => void;
export function ApplyTemplate( template: JQuery ): ( e: Element ) => void;

export function ApplyTemplate( template: any ) {

	var t = typeof template == "string" ? $( template ) : template;

	return function ( element: Element ) {
		if ( $.makeArray(
			ko.virtualElements.childNodes( element ) )
			.every( e => e.nodeType != 1 ) ) {
			ko.virtualElements.setDomNodeChildren( element, t.clone() );
		}
		ko.applyBindingsToDescendants( this, element );
	};

}