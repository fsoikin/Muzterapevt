// Type definitions for Knockout.Mapping 2.0
// Project: https://github.com/SteveSanderson/knockout.mapping
// Definitions by: Boris Yankov <https://github.com/borisyankov/>
// Definitions https://github.com/borisyankov/DefinitelyTyped

/// <reference path="./knockout-2.2.d.ts" />

declare module "ko.mapping" {
	function isMapped( viewModel: any ): boolean;
	function fromJS( jsObject: any ): any;
	function fromJS( jsObject: any, targetOrOptions: any ): any;
	function fromJS( jsObject: any, inputOptions: any, target: any ): any;
	function fromJSON( jsonString: string ): any;
	function toJS( rootObject: any, options?: Ko.MappingOptions ): any;
	function toJSON( rootObject: any, options?: Ko.MappingOptions ): any;
	function defaultOptions(): Ko.MappingOptions;
	function resetDefaultOptions(): void;
	function getType( x: any ): any;
	function visitModel( rootObject: any, callback: Function, options?: { visitedObjects?; parentName?; ignore?; copy?; include?; }): any;
}

declare module Ko {
	interface MappingCreateOptions {
		data: any;
		parent: any;
	}

	interface MappingUpdateOptions {
		data: any;
		parent: any;
		observable: Observable<any>;
	}

	interface MappingOptions {
		ignore?: string[];
		include?: string[];
		copy?: string[];
		mappedProperties?: string[];
		deferEvaluation?: boolean;
		create?: ( options: MappingCreateOptions ) => void;
		update?: ( options: MappingUpdateOptions ) => void;
	}
}