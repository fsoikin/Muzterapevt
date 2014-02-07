/// <reference path="../tsd/require.d.ts"/>
/// <reference path="../tsd/polyfill.d.ts"/>
/// <reference path="../tsd/jquery.amd.d.ts"/>
/// <reference path="../tsd/jqueryui.d.ts"/>
/// <reference path="../tsd/knockout-2.2.d.ts"/>
/// <reference path="../tsd/knockout.mapping-2.0.d.ts"/>
/// <reference path="../tsd/rx.d.ts" />
/// <reference path="../tsd/rx.dom.d.ts" />
/// <reference path="../tsd/rx-ko.d.ts" />
/// <reference path="../tsd/linq.d.ts"/>
/// <reference path="../tsd/linq.amd.d.ts"/>
/// <reference path="../tsd/serverTypes.d.ts"/>

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

export function dataSourceFromNameValuePair<T>( items: NameValuePair<T>[] ): IDataSource<NameValuePair<T>> {
	var ii = ko.observable<NameValuePair<T>[]>();
	setImmediate( () => ii( items ) );
	return {
		Items: ii,
		GetId: ( i: NameValuePair<T> ) => i.Value,
		ToString: ( i: NameValuePair<T> ) => i.Name,
	};
}

export class VmBase {
	IsLoading = ko.observable( false );
	IsSaving = ko.observable( false );
}

/**
 * Represents virtual sequence of items of a certain type
 */
export interface IDataSource<T> {
	/**
	 * The items comprising the datasource
	 */
	Items: Ko.Observable<T[]>;

	/** Extract unique identifier from a given item */
	GetId( i: T ): any;

	/** Convert a given item to a user-friendly string */
	ToString( i: T ): string;


	/** Signifies whether this datasource loads all of its items
	 *  right after creation or only some of them (or even none).
	 *  When this property is true, the datasource usually supports
	 *  lookup (see the Lookup method).
	 */
	HasPartialItems?: boolean;

	/** Returns a subset of items filtered by the given substring */
	Lookup?: ( term: string ) => Rx.IObservable<T[]>;

	/** Returns a subset of items by given ids */
	GetById?: ( ids: any[] ) => Rx.IObservable<T[]>;

	/** This flag will be set to true when the datasource has finished
	 * loading/preparing items (or whatever other preparation it has to do).
	 * If this flag is not present, then the datasource will be considered
	 * always "ready". See the "dataSourceReady" function.
	 */
	IsReady?: Ko.Observable<boolean>;


	/** True when the items are being fetched from the server, False when done. */
	IsLoading?: Ko.Observable<boolean>;

	/** Pushes messages about errors occurring while fetching items from the
	 *  server.
	 */
	Error?: Ko.Subscribable<string>;
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

export class TemplatedControl implements IVirtualControl {
	constructor( template: JQuery ) {
		this.OnLoaded = ApplyTemplate( template );
	}

	OnLoaded: ( element: Element ) => void;
	ControlsDescendantBindings = true;
	SupportsVirtualElements = true;
}

export function rxToKo<T>( rx: Rx.IObservable<T> ): Ko.Observable<T> {
	var res = ko.observable<T>();
	rx && rx.subscribe( res );
	return res;
}

export function koToRx<T>( ko: Ko.Subscribable<T> ): Rx.IObservable<T> {
	return rx.Observable.create( ( o: Rx.IObserver<T> ) => {
		var d = ko.subscribe( o.onNext, o );
		o.onNext( ko.peek() );
		return () => d.dispose();
	});
}

/**
 * Creates a Computed Observable, which is initially bound to 'initialObservable',
 * but after at least one pulse from ALL (not any!) of 'triggers' gets switched
 * to 'nextObservable'.
 *
 * This is useful for defining a class with a complex property, whose value is
 * constructed in a complex way from many asynchronous sources, but the initial
 * value of that property must be kept in a temporary location until those
 * asynchronous sources become available. In this scenario, 'initialObservable'
 * is the temporary location, 'triggers' pulse when the asynchronous sources
 * become available, and 'nextObservable' represents the actual computation of
 * the complex property.
 */
export function switchObservable<T>( triggers: Ko.Subscribable<any>[],
	initialObservable: Ko.Observable<T>, nextObservable: Ko.Observable<T> )
	: Ko.Observable<T>;

/**
 * Creates a Computed Observable, which is initially bound to 'initialObservable',
 * but after at least one pulse from ALL (not any!) of 'triggers' gets switched
 * to 'nextObservable'.
 *
 * This is useful for defining a class with a complex property, whose value is
 * constructed in a complex way from many asynchronous sources, but the initial
 * value of that property must be kept in a temporary location until those
 * asynchronous sources become available. In this scenario, 'initialObservable'
 * is the temporary location, 'triggers' pulse when the asynchronous sources
 * become available, and 'nextObservable' represents the actual computation of
 * the complex property.
 */
export function switchObservable<T>( triggers: Rx.IObservable<any>[],
	initialObservable: Ko.Observable<T>, nextObservable: Ko.Observable<T> )
	: Ko.Observable<T>;

/**
 * Creates a Computed Observable, which is initially bound to 'initialObservable',
 * but after a pulse from 'trigger' gets switched to 'nextObservable'
 *
 * This is useful for defining a class with a complex property, whose value is
 * constructed in a complex way from many asynchronous sources, but the initial
 * value of that property must be kept in a temporary location until those
 * asynchronous sources become available. In this scenario, 'initialObservable'
 * is the temporary location, 'trigger' pulses when the asynchronous sources
 * become available, and 'nextObservable' represents the actual computation of
 * the complex property.
 */
export function switchObservable<T>( trigger: Ko.Subscribable<any>,
	initialObservable: Ko.Observable<T>, nextObservable: Ko.Observable<T> )
	: Ko.Observable<T>;

/**
 * Creates a Computed Observable, which is initially bound to 'initialObservable',
 * but after a pulse from 'trigger' gets switched to 'nextObservable'
 *
 * This is useful for defining a class with a complex property, whose value is
 * constructed in a complex way from many asynchronous sources, but the initial
 * value of that property must be kept in a temporary location until those
 * asynchronous sources become available. In this scenario, 'initialObservable'
 * is the temporary location, 'trigger' pulses when the asynchronous sources
 * become available, and 'nextObservable' represents the actual computation of
 * the complex property.
 */
export function switchObservable<T>( trigger: Rx.IObservable<any>,
	initialObservable: Ko.Observable<T>, nextObservable: Ko.Observable<T> )
	: Ko.Observable<T>;

export function switchObservable<T>( triggers: any,
	initialObservable: Ko.Observable<T>, nextObservable: Ko.Observable<T> )
	: Ko.Observable<T> {
	var tgrs: Rx.IObservable<any>[] =
		( $.isArray( triggers ) ? triggers : [triggers] )
			.map( o => {
				if ( ko.isSubscribable( o ) ) {
					var obs = <Ko.Observable<any>>o;
					if ( ko.isObservable( obs ) && obs() === true ) return rx.Observable.returnValue( null );
					return koToRx( obs ).skip( 1 ).where( x => x !== false );
				}
				return <Rx.IObservable<any>>o;
			});

	var whenTriggerFired = tgrs
		.reduce( ( zipped: Rx.IObservable<any>, s: Rx.IObservable<any> ) =>
			zipped.zip( s, () => 0 ), rx.Observable.returnValue( 0 ) )
		.take( 1 );

	var write = initialObservable;
	var read = rxToKo(
		koToRx( initialObservable )
			.takeUntil( whenTriggerFired )
			.concat( rx.Observable.defer( () => {
				write = nextObservable;
				nextObservable( initialObservable() );
				return rx.Observable.empty<T>();
			}) )
			.concat( koToRx( nextObservable ).skip( 1 ) ) );

	return ko.computed( { read: read, write: ( v: T ) => write( v ) });
}

/**
 * This function takes in an observable and creates a wrapper for it that only lets
 * through change notifications that carry a new value. That is, if the inner observable
 * produces several notifications with the same value in a row, only one of them will get
 * through.
 */
export function distinctObservable<T>( o: Ko.Observable<T>,
	comparer: ( a: T, b: T ) => boolean = ( a, b ) => a == b ): Ko.Observable<T> {

	var read = rxToKo( koToRx( o ).distinctUntilChanged( x => x, comparer ) );
	return ko.computed( {
		read: () => <T>read(),
		write: ( x: T ) => o( x )
	});

}