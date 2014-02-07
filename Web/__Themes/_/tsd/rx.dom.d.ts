/// <reference path="rx.d.ts" />

declare module Rx {
	var DOM: {
		fromEvent( element: Element, event: string ): Rx.IObservable<Event>;
	};
}