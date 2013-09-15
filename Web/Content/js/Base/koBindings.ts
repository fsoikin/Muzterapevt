﻿import er = require( "../common" );
import ko = require( "ko" );
import $ = require( "jQuery" );
var u = ko.utils.unwrapObservable;

ko.virtualElements.allowedBindings['control'] = true;
ko.bindingHandlers['control'] = {
	init: (element, valueAccessor) => {

		var o = valueAccessor();
		var vm = o.vm || o;
		var defer = (o.defer === undefined) ? false : o.defer;
		vm = <er.IControl>ko.utils.peekObservable(vm);
		if (!vm) return;

		var vc = <er.IVirtualControl>vm;
		if (!(element instanceof HTMLElement) && !vc.SupportsVirtualElements) {
			console.warn("Control " + vm + " doesn't support virtual elements. Adding an empty <span> to accomodate it.");
			var span = $("<span>");
			ko.virtualElements.prepend(element, span[0]);
			element = span;
		}

		function f() { vm.OnLoaded(element); }
		if (defer) setImmediate(f); else f();

		return { controlsDescendantBindings: vm.ControlsDescendantBinidngs };
	}
};

ko.bindingHandlers['flashWhen'] = {
	init: ( element, valueAccessor, allBindingsAccessor ) => {
		var v: Ko.Subscribable<any> = valueAccessor();
		if( ko.isSubscribable( v ) ) {
			v.subscribe( () => {
				$( element ).hide().slideDown( 300 );
				setTimeout( () => $( element ).slideUp( 600 ), 2000 );
			});
		}

		return null;
	}
};

ko.bindingHandlers['jqButton'] = {
	init: e => {
		$(e).button();
		ko.utils.domNodeDisposal.addDisposeCallback( e, () => $( e ).button( "destroy" ) );
		return null;
	}
};

ko.bindingHandlers['disableTree'] = {
	update: (element, valueAccessor) =>
		$(element).find("input,select,textarea,button").andSelf().prop("disabled", u(valueAccessor()))
};

// KO's built-in hasFocus doesn't work for some reason. 
// No time to investigate, much quicker to roll my own.
ko.bindingHandlers['hasFocus'] = {
	init: (element, valueAccessor) => {
		var e = $(element);
		var v = valueAccessor();

		u(v) && e.focus();

		if (ko.isObservable(v)) {
			var evts = { focus: () => { v(true); }, blur: () => { v(false); } };
			e.on(evts);
			ko.utils.domNodeDisposal.addDisposeCallback(e, () => e.off(evts));
		}
		return null;
	},
	update: (element, valueAccessor) => u(valueAccessor()) && $(element).focus()
};

ko.bindingHandlers['assignTo'] = {
	init: (element, valueAccessor) => {
		var v = valueAccessor();
		if (ko.isObservable(v)) v(element);
		return null;
	}
};

(() => {
	ko.bindingHandlers['jqWidget'] = {
		init: e => ko.utils.domNodeDisposal.addDisposeCallback( e, destroyWidgets ),
		update: setWidgets
	};

	var destroyKey = "{1E981B28-C65C-4E4A-BF28-DA2F399BC5F6}";
	var constantWidgetKey = "";

	function destroyWidgets( e ) {
		var d = $( e ).data( destroyKey );
		$( e ).removeData( destroyKey );
		d && d();
	}

	function setWidgets( element, valueAccessor ) {
		var e = <any>$( element );
		if( e.data( constantWidgetKey ) ) return;

		destroyWidgets( element );

		var rawValue = valueAccessor();
		var canChange = ko.isObservable( rawValue );
		var v = u( rawValue );
		if( typeof v === "string" && e[v] ) { var x = {}; x[v] = null; v = x; }

		if( !canChange ) e.data( constantWidgetKey, true );

		function cons( f: () => void , g: () => void ) { return () => { g(); f(); }; }
		function widget( ctor: Function, args ) {
			if( !ctor ) return () => { };
			if( !args ) args = [];
			if( !Array.isArray( args ) ) args = [args];
			ctor.apply( e, args );
			return () => ctor.call( e, "destroy" );
		}

		var destroy = e.children().length ? () => { } : () => e.empty();
		for( var k in v ) {
			var removeWidget = widget( e[k], v[k] );
			destroy = cons( destroy, removeWidget );
		}

		if( canChange ) e.data( destroyKey, destroy );
	};
} )();

ko.bindingHandlers['jqData'] = {
	init: (e, v) => $(e).data(u(v()) || {}),
	update: (e, v) => $(e).data(u(v()) || {})
};

ko.bindingHandlers["loadingWhen"] = (() => {
	var key = "{32DD9D9D-36B6-4DA3-8883-547AB89D6A33}";

	return {
		update: ( e, v, all ) => {
			var $e = $( e );
			var loading = <JQuery>$e.data( key );
			var overlay = all().overlayLoadingMode;

			if ( u( v() ) ) {
				if ( !loading ) {
					loading = $( "<div>" ).addClass( "Loading" ).insertBefore( $e );
					$e.data( key, loading );
					if( overlay ) loading
						.css( { position: 'absolute' } )
						.position( { my: "left top", at: "left top", of: $e } )
						.css( { width: $e.width(), height: $e.height() } );
					else
						$e.hide();
				}
			}
			else {
				$e.show().removeData(key);
				if ( loading ) loading.remove();
			}
		}
	};
})();