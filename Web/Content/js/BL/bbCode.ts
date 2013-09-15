import c = require( "../common" );
import $ = require( "jQuery" );
import rx = require( "rx" );
import ko = require( "ko" );

var Ajax = {
	ToHtml: "bbcode/toHtml"
};

export function toHtml( bbCode: string ): Rx.IObservable<string> {
	return rx.Observable.create( ss => {
		var err = ko.observable();
		err.subscribe( e => ss.onError( e ) ); 

		var req = c.Api.Get( Ajax.ToHtml, { bbText: bbCode }, null, err,
			d => { ss.onNext( d ); ss.onCompleted(); } );

		return () => req.abort();
	});
}