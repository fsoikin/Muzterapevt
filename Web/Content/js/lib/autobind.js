define(["require", "exports", "ko", "./dynamic", "jQuery", "rx"], function(require, exports, __ko__, __dyn__, __$__, __rx__) {
    
    var ko = __ko__;
    var dyn = __dyn__;
    var $ = __$__;
    var rx = __rx__;
    (function () {
        return rx;
    })();

    var defaultOptions = {
        controllerKey: "controller",
        argumentsKey: "args",
        getClassRef: function (e, o) {
            return dyn.parse(e.data(o.controllerKey), e.data(o.argumentsKey));
        }
    };

    function autobindAll(root, opts) {
        opts = $.extend({}, defaultOptions, opts);

        return root.each(function (e) {
            var $e = $(this);
            exports.autobind(opts.getClassRef($e, opts), $e);
        });
    }
    exports.autobindAll = autobindAll;
    ;

    function autobind(ref, element, onDone) {
        dyn.bind(ref).asRx().where(function (o) {
            return !!o;
        }).subscribe(function (obj) {
            //		if( obj == null ) { console.error( "Unable to autobind. Controller = '" + ref.Class + ", " + ref.Module + "', args = '" + JSON.stringify( ref.Arguments ) + "'" ); return; }
            var template = obj.constructor && obj.constructor.Template;
            var where = $.isFunction(element) ? element() : $(element);

            if (typeof obj.OnLoaded === "function")
                obj.OnLoaded(where[0]);
else {
                if (typeof template === "function")
                    template = template();
                if (typeof template === "string")
                    where.html(template);
else if (template instanceof $)
                    where.empty().append(template);
                ko.applyBindings(obj, where[0]);
            }

            onDone && onDone(where, obj);
        });
    }
    exports.autobind = autobind;
});
