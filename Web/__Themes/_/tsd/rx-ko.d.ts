/// <reference path="rx.d.ts" />
/// <reference path="knockout-2.2.d.ts" />

declare module Ko {
	export interface SubscribableFunctionsBase<T> {
		asRx(): Rx.IObservable<T>;
	}
}

declare module Rx {
	export interface IObservable<T> {
		asKo(): Ko.Observable<T>;
	}
}