﻿/// <reference path="../../../tsd/require.d.ts" />
/// <reference path="../../../tsd/knockout-2.2.d.ts" />
/// <reference path="../../../tsd/rx.d.ts" />
/// <reference path="../../../tsd/rx-ko.d.ts" />

require.config( {
	map: {
		'External/Rx/rx.time.min': {
			rx: 'External/Rx/rx.min'
		},
		'External/Rx/rx.dom.min': {
			rx: 'External/Rx/rx.min'
		},
		'External/Rx/rx.binding.min': {
			rx: 'External/Rx/rx.min'
		}
	}
});

define( [
	'External/Rx/rx.min',
	'ko',
	'External/Rx/rx.time.min',
	'External/Rx/rx.binding.min',
	'External/Rx/rx.dom.min'],
( rx, ko ) => {
	extend( rx, ko );
	return rx;
}); 

// TODO: [fs]
// Waiting for this issue to be resolved: https://typescript.codeplex.com/workitem/1100
function extend( rx, ko ) {
	ko.subscribable.fn.asRx = function () {

		var s = <Ko.Subscribable<any>> this;

		return rx.Observable.create( o => {
			var d = s.subscribe( o.onNext, o );
			o.onNext( s.peek() );
			return () => d.dispose();
		} );

	};
}