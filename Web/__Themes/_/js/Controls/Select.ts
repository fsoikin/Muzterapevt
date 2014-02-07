/// <amd-dependency path="select2" />
import c = require( "../common" );
import dsApi = require( "../Lib/DataSourceApi" );
import $ = require( "jQuery" );
import ko = require( "ko" );
import rx = require( "rx" );

export function CreateSelect<T>( dataSource: c.IDataSource<T>, extraOptions?: SelectExtraOptions<T> ): ISelect<T> {
	return new Select<T>( dataSource, extraOptions );
}

export function CreateMultiSelect<T>( dataSource: c.IDataSource<T>, extraOptions?: SelectExtraOptions<T> ): IMultiSelect<T> {
	return new MultiSelect<T>( dataSource, extraOptions );
}

// These are just select2 options, they get passed directly to select2 on initialization
// See http://ivaynberg.github.io/select2/
export interface SelectExtraOptions<T> {
	placeholder?: string;
	allowClear?: boolean;
	formatSelection?: ( t: T ) => string;
	formatResult?: ( t: T ) => string;
	formatNoMatches?: ( term: string ) => string;
	formatSearching?: () => string;
	formatInputTooShort?: ( term: string, minLength: number ) => string;
	minimumInputLength?: number;
	createSearchChoice?: ( term: string ) => T;
}

export interface ISelectBase<T>{
	DataSource: c.IDataSource<T>;
	IsReady: Ko.Observable<boolean>;
	Focus();
}

export interface ISelect<T> extends ISelectBase<T>, c.IControl {
	SelectedItem: Ko.Observable<T>;
	SelectedId: Ko.Observable<any>;
}

export interface IMultiSelect<T> extends ISelectBase<T>, c.IControl {
	SelectedItems: Ko.ObservableArray<T>;
	SelectedIds: Ko.Observable<any[]>;
}




class SelectBase<TItem> implements c.IControl {
	private _element = ko.observable<Element>();
	DataSource: c.IDataSource<TItem>;
	IsReady: Ko.Observable<boolean>;

	Init( dataSource: c.IDataSource<TItem>, extraOptions: SelectExtraOptions<TItem>, selected: Ko.Observable<any>, multiple: boolean )
	{
		this.DataSource = dataSource;
		var getId = ( t: TItem ) => this.DataSource.GetId( t );
		var getText = ( t: TItem ) => this.DataSource.ToString( t ) || "";
		var ds = dsApi.from( this.DataSource );
		var dsReady = ds.isReady();
		this.IsReady = dsReady;

		ko.computed( () => {
			if( !this._element() ) return;

			var opts = $.extend( {}, extraOptions || {}, {
				multiple: multiple,
				id: getId,
				formatSelection: (extraOptions && extraOptions.formatSelection) || getText,
				formatResult: (extraOptions && extraOptions.formatResult) || getText,
			});
	
			if ( !dsReady() ) {
				opts.query = () => { }; // This will never return anything, causing select2 to display the "searching..." message
			} else if ( !this.DataSource.HasPartialItems ) {
				opts.data = {
					results: this.DataSource.Items(),
					text: getText
				};
			} else if ( this.DataSource.Lookup ) {
				opts.query = ( q: Select2QueryOptions ) => {
					if ( !q.term ) return;
					else this.DataSource.Lookup( q.term ).subscribe( res => {
						q.callback( { results: res, text: getText });
					});
				};
			}
			
			var $e = <any>$( this._element() );
			$e.select2( "destroy" );
			$e.select2( opts );
			$e.select2( 'data', selected.peek() );

			var mute = false;
			$e.change( () => {
				if( mute ) return; mute = true;
				var s = $e.select2( 'data' );
				if ( selected() != s ) selected( s );
				mute = false;
			} );
			selected.subscribe( i => {
				if ( mute ) return; mute = true;
				$e.select2( 'data', i );
				mute = false;
			} );
		});
	}

	ControlsDescendantBindings = true;
	OnLoaded( element: Element ) { this._element( element ); }

	Focus() { setTimeout( () => $( this._element() )['select2']( 'open' ), 20 ); }
}

class Select<T> extends SelectBase<T> {
	SelectedItem = ko.observable<T>();
	SelectedId: Ko.Observable<any>;

	constructor( dataSource: c.IDataSource<T>, extraOptions?: SelectExtraOptions<T> ) {
		super();
		this.Init( dataSource, extraOptions, this.SelectedItem, false );

		var ds = dsApi.from( dataSource );
		var incomingId = new rx.Subject<any>();
		var getRealId = () => this.SelectedItem() && dataSource.GetId(this.SelectedItem());

		this.SelectedId = c
			.switchObservable( [ds.whenReady()], ko.observable<any>(),
				ko.computed( {
					read: getRealId,
					write: x => (<any>incomingId).onNext(x)
				}) );

		incomingId.subscribe( id => {
			if( id == null || id == undefined ) this.SelectedItem( null );
			else if( id == getRealId() ) return;
			else ds.getById( [id] )
				.takeUntil( incomingId )
				.subscribe( ii => this.SelectedItem( ii[0] ) );
		});
	}
}

class MultiSelect<T> extends SelectBase<T> {
	SelectedItems = ko.observableArray<T>();
	SelectedIds: Ko.Observable<any[]>;

	constructor( dataSource: c.IDataSource<T>, extraOptions?: SelectExtraOptions<T> ) {
		super();
		this.Init( dataSource, extraOptions, this.SelectedItems, true );

		var ds = dsApi.from( dataSource );
		var incomingIds = new rx.Subject<any[]>();
		var getRealIds = () => (this.SelectedItems() || []).map(dataSource.GetId);
		var distinctChanges = c.distinctObservable( ko.computed( getRealIds ), c.compareArrays );

		this.SelectedIds = c
			.switchObservable( ds.whenReady(), ko.observable<any[]>([]),
				ko.computed( {
					read: () => distinctChanges(),
					write: (x: any[]) => incomingIds.onNext(x)
				}) );

		incomingIds.subscribe( ( ids: any[] ) => {
			if( !ids || !ids.length ) this.SelectedItems( [] );
			else if( c.compareArrays( ids, getRealIds() ) ) return;
			else ds.getById( ids )
				.takeUntil( incomingIds )
				.subscribe( this.SelectedItems );
		});
	}
}

interface Select2QueryOptions {
	term: string;
	callback( data: { results: any[]; } );
}