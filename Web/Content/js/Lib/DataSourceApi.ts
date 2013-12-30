import er = require( "../common" );
import ko = require( "ko" );
import rx = require( "rx" );
import linq = require( "linq" );

export interface IDataSourceApi<T> {
	/** The datasource from which this API wrapper was created */
	dataSource: er.IDataSource<T>;

	/** If the datasource has items, returns them, otherwise
	 *  (i.e. for datasources that only support lookups) returns
	 *  en empty array. */
	itemsOrEmpty(): Ko.Observable<T[]>;

	/** Looks up one or more items by substring (see the IDataSource.Lookup method).
	 *  If the datasource supports lookups, uses that support, otherwise
	 *  simply obtains all items and filters them down. */
	lookup( term: string ): Rx.IObservable<T[]>;

	/** Creates a computed observable that responds to changes in the
	 *  given string observable and uses its value to look up items in the
	 *  datasource.
	 *  If the datasource supports lookups, uses that support, otherwise
	 *  simply obtains all items and filters them down. */
	lookup( term: Ko.Observable<string> ): Ko.Observable<T[]>;

	/** Looks up one or more items by IDs (see the IDataSource.GetById method).
	 *  If the datasource supports lookups, uses that support, otherwise
	 *  simply obtains all items and filters them down. */
	getById( ids: any[] ): Rx.IObservable<T[]>;

	/** Creates a computed observable that responds to changes in the
	 *  given any[] observable and uses its value to look up items in the
	 *  datasource.
	 *  If the datasource supports lookups, uses that support, otherwise
	 *  simply obtains all items and filters them down. */
	getById( ids: Ko.Observable<any[]> ): Ko.Observable<T[]>;

	/** Encapsulates the logic of the IDataSource.IsReady flag,
	 *  that is, returns the flag itself when it is present,
	 *  or always-true observable otherwise. */
	isReady(): Ko.Observable<boolean>;

	/** Returns a sequence that pulses every time when the datasource
	 *  becomes ready. */
	whenReady(): Rx.IObservable<any>;

	/** Calls the specified callback function when the specified
	 *  datasource becomes ready */
	whenReady( fn: () => void );
}

export function from<T>( ds: er.IDataSource<T> ): IDataSourceApi<T> {
	return new DataSourceApi<T>( ds );
}

class DataSourceApi<T> implements IDataSourceApi<T> {

	private static _alwaysTrue = ko.computed( () => true );
	constructor( public dataSource: er.IDataSource<T> ) { }

	itemsOrEmpty(): Ko.Observable<T[]> {
		return this.dataSource.Items
			? ko.computed( () => this.dataSource.Items() || <T[]>[] )
			: ko.computed( () => <T[]>[] );
	}

	lookup( term: string ): Rx.IObservable<T[]>;
	lookup( term: Ko.Observable<string> ): Ko.Observable<T[]>;
	lookup( term: any ): any {
		var observable = ko.isObservable( term );
		var termRx = observable
			? er.koToRx( <Ko.Observable<string>>term )
			: rx.Observable.returnValue( <string>term );

		var result = termRx
			.select( t => (t||"").toLowerCase() )
			.selectMany( t => {
				if ( !t ) return rx.Observable.returnValue( [] );
				if ( this.dataSource.Lookup ) return this.dataSource.Lookup( t );
			
				if ( this.dataSource.Items ) return rx.Observable.returnValue( <T[]>
					linq.from( this.dataSource.Items() )
						.where( i => ( this.dataSource.ToString(i) || "" ).toLowerCase().indexOf( t ) >= 0 )
						.toArray() );

				return rx.Observable.returnValue( [] );
			});

		return observable ? <any>er.rxToKo(result) : <any>result;
	}

	getById( ids: any[] ): Rx.IObservable<T[]>;
	getById( ids: Ko.Observable<any[]> ): Ko.Observable<T[]>;
	getById( ids: any ) {
		var observable = ko.isObservable( ids )
		var idsRx = observable
			? er.koToRx(<Ko.Observable<any[]>>ids)
			: rx.Observable.returnValue( <any[]>ids );

		var result = idsRx.selectMany( (ii: any[]) => {
			if ( !ii || !ii.length ) return rx.Observable.returnValue( [] );
			if ( this.dataSource.GetById ) return this.dataSource.GetById( ii );

			if ( this.dataSource.Items ) return rx.Observable.returnValue( <T[]>
				linq.from( this.dataSource.Items() )
					.join( ii, this.dataSource.GetId, x => x, ( x, _ ) => x )
					.toArray() );

			return rx.Observable.returnValue( [] );
		});

		return observable ? <any>er.rxToKo( result ) : <any>result;
	}

	isReady(): Ko.Observable<boolean> { return this.dataSource.IsReady || DataSourceApi._alwaysTrue; }

	whenReady(): Rx.IObservable<any>;
	whenReady( fn: () => void );
	whenReady( fn? ) {
		var res = er.koToRx( this.isReady() ).where( r => r );

		if ( fn && typeof fn == "function" ) {
			res.take( 1 ).subscribe( fn );
		}
		else {
			return res;
		}
	}
}

/** A set of helper functions for creating an IDataSource from
 *  various actual sources */
export module create {
	/**
	 * Produces a static implementation of IDataSource that draws its elements from
	 * the specified enum.
	 */
	export function fromEnum<TEnum>( theEnum: TEnum ): er.IDataSource<er.NameValuePair<TEnum>> {
		return fromNameValuePair( er.mapEnum( theEnum ) );
	}

	/**
	 * Produces a static implementation of IDataSource that draws its elements from
	 * the specified array.
	 */
	export function fromNameValuePair<T>( items: er.NameValuePair<T>[] ): er.IDataSource<er.NameValuePair<T>> {
		return {
			Items: ko.observableArray( items ),
			GetId: ( i: er.NameValuePair<T> ) => i.Value,
			ToString: ( i: er.NameValuePair<T> ) => i.Name,
			CanLookup: false,
			HasPartialItems: false,
			Lookup: () => rx.Observable.empty<T[]>()
		};
	}

	/**
	 * Produces an implementation of IDataSource that fetches its elements from
	 * the specified URL.
	 * @param toString A function that can convert a given datasource item to a user-friendly string.
	 * @param getId A function that extracts the unique identifier from a given datasource item.
	 */
	export function fromServer<T>( url: string, toString: ( t: T ) => string, getId: ( t: T ) => any ): () => er.IDataSource<T>;
	export function fromServer<T extends er.IHaveId<any>>( url: string, getId?: ( t: T ) => any ): () => er.IDataSource<T>;
	export function fromServer<T extends er.IHaveNameAndId<any>>( url: string, toString?: ( t: T ) => string, getId?: ( t: T ) => any ): () => er.IDataSource<T>;

	export function fromServer<T>( url: string, toString?: ( t: T ) => string, getId?: ( t: T ) => any ) : () => er.IDataSource<T> {
		return () => new RemoteDataSource<T>( {
			url: url,
			getId: getId || <(t: T) => any><any>( ( u: er.IHaveNameAndId<any> ) => u.Id ),
			toString: toString || <( t: T ) => string><any>( ( u: er.IHaveNameAndId<any> ) => u.Name )
		});
	}
}

/** A set of helper functions for creating an IDataSource that is
 *  capable of looking up items by substring or id */
export module createLookup {
	/** Creates an implementation of IDataSource that does not load
	 *  all its items at once, but is able to look them up either by
	 *  a substring or by ID.
	 *  @param lookupUrl A function that returns URL from where to fetch the data given a lookup substring.
	 *  @param getByIdUrl A function that returns URL from where to fetch the data given a set of item IDs.
	 *  @param toString A function that can convert a given datasource item to a user-friendly string.
	 *  @param getId A function that extracts the unique identifier from a given datasource item. */
	export function fromServer<T extends er.IHaveNameAndId<any>>(
		lookupUrl: ( term: string ) => string, getByIdUrl: ( ids: any[] ) => string,
		toString?: ( t: T ) => string, getId?: ( t: T ) => any )
		: () => er.IDataSource<T> {

		return () => new RemoteDataSource<T>( {
			lookupUrl: lookupUrl,
			getByIdUrl: getByIdUrl,
			getId: getId || ( ( u: T ) => u.Id ),
			toString: toString || ( ( u: T ) => u.Name )
		});
	}

	/** Using a standard URL scheme of "<prefix>lookup?term=xyz" for lookup
	 *  and "<prefix>getById?ids=1,2,3" for get by ID, creates an implementation
	 *  of IDataSource that does not load all its items at once, but is able to
	 *  look them up either by a substring or by ID.
	 *  @param urlPrefix The prefix to use for generating fetch URLs.
	 *  @param toString A function that can convert a given datasource item to a user-friendly string.
	 *  @param getId A function that extracts the unique identifier from a given datasource item. */
	export function fromStdUrlScheme<T extends er.IHaveNameAndId<any>>(
		urlPrefix: string, toString?: ( t: T ) => string, getId?: ( t: T ) => any )
		: () => er.IDataSource<T> {

		return fromServer(
			term => urlPrefix + "lookup?term=" + term,
			ids => urlPrefix + "getById?ids=" + ids.join(),
			toString, getId );
	}
}

/**
 * An implementation of IDataSource that fetches its elements from the server.
 */
export class RemoteDataSource<T> implements er.IDataSource<T> {
	IsLoading = ko.observable( false );
	IsReady = ko.observable( false );
	Error = new ko.subscribable<string>();

	Items = ko.observableArray<T>();
	GetId: ( i: T ) => any;
	ToString: ( i: T ) => string;
	HasPartialItems: boolean;
	Lookup: ( term: string ) => Rx.IObservable<T[]>;
	GetById: ( ids: any[] ) => Rx.IObservable<T[]>;

	private Url: string;
	private LookupUrl: ( term: string ) => string;
	private GetByIdUrl: ( ids: any[] ) => string;

	constructor( args: {
		url?: string;
		lookupUrl?: ( term: string ) => string;
		getByIdUrl?: ( ids: any[] ) => string;
		getId?: ( i: T ) => any;
		toString?: ( i: T ) => string;
	}) {
		if ( !args.url && ( !args.lookupUrl || !args.getByIdUrl ) ) {
			throw new Error( "When creating a RemoteDataSource, you have to specify either 'url' or both 'lookupUrl' and 'getByIdUrl'." );
		}

		this.GetId = args.getId || ( ( i: T ) => {
			if ( !i ) return null;
			if ( !( 'Id' in i ) ) throw new Error( "'Id' property not found on object " + JSON.stringify( i ) );
			return i['Id'];
		});
		this.ToString = args.hasOwnProperty( "toString" )
			? args.toString
			: ( i => ( i || "" ).toString() );

		this.Url = args.url;
		this.HasPartialItems = !args.url;
		this.LookupUrl = args.lookupUrl;
		this.GetByIdUrl = args.getByIdUrl;
		this.Lookup = !args.lookupUrl ? null :
		( term: string ) => this._Get( this.LookupUrl( term ) );
		this.GetById = !args.getByIdUrl ? null :
		( ids: any[] ) => this._Get( this.GetByIdUrl( ids ) );

		if ( this.Url ) {
			this.Reload();
			er.koToRx( this.Items ).skip( 1 ).take( 1 ).subscribe( () => this.IsReady( true ) );
		}
		else this.IsReady( true );
	}

	Reload() {
		if ( this.Url ) {
			er.Api.Get( this.Url, null, this.IsLoading, er => this.Error.notifySubscribers(er), this.Items );
		}
	}

	_Get( url: string ) {
		return rx.Observable.create<T[]>( o => {
			var err = new ko.subscribable<any>();
			var d = err.subscribe( e => {
				o.onError( e );
				this.Error.notifySubscribers( e );
			});

			var xhr = er.Api.Get( url, null, this.IsLoading, er => err.notifySubscribers(er),
				res => {
					o.onNext( res );
					o.onCompleted();
				});

			return () => {
				d.dispose();
				xhr.abort();
			};
		});
	}
}
