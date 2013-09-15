/// <reference path="../../../tsd/require.d.ts" />
/// <reference path="../../../tsd/knockout-2.2.d.ts" />
/// <reference path="../../../tsd/rx.d.ts" />
/// <reference path="../../../tsd/rx-ko.d.ts" />

require.config( {
	map: {
		'app/External/Rx/rx.time.min': {
			rx: 'app/External/Rx/rx.min'
		}
	}
});

define( [
	'app/External/Rx/rx.min',
	'ko',
	'app/External/Rx/rx.time.min'],
( rx, ko ) => {
	extend( rx, ko );
	return rx;
}); 

// TODO: [fs]
// Waiting for this issue to be resolved: https://typescript.codeplex.com/workitem/1100
function extend( rx, ko ) {
	ko.subscribable.fn.asRx = function () {

		var s = <Ko.Subscribable> this;

		return rx.Observable.create( o => {
			var d = s.subscribe( o.onNext, o );
			o.onNext( s.peek() );
			return () => d.dispose();
		} );

	};
}