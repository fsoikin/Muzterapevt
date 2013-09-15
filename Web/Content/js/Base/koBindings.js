define(["require", "exports", "ko", "jQuery"], function(require, exports, __ko__, __$__) {
    
    var ko = __ko__;
    var $ = __$__;
    var u = ko.utils.unwrapObservable;

    ko.virtualElements.allowedBindings['control'] = true;
    ko.bindingHandlers['control'] = {
        init: function (element, valueAccessor) {
            var o = valueAccessor();
            var vm = o.vm || o;
            var defer = (o.defer === undefined) ? false : o.defer;
            vm = ko.utils.peekObservable(vm);
            if (!vm)
                return;

            var vc = vm;
            if (!(element instanceof HTMLElement) && !vc.SupportsVirtualElements) {
                console.warn("Control " + vm + " doesn't support virtual elements. Adding an empty <span> to accomodate it.");
                var span = $("<span>");
                ko.virtualElements.prepend(element, span[0]);
                element = span;
            }

            function f() {
                vm.OnLoaded(element);
            }
            if (defer)
                setImmediate(f);
else
                f();

            return { controlsDescendantBindings: vm.ControlsDescendantBinidngs };
        }
    };

    ko.bindingHandlers['flashWhen'] = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var v = valueAccessor();
            if (ko.isSubscribable(v)) {
                v.subscribe(function () {
                    $(element).hide().slideDown(300);
                    setTimeout(function () {
                        return $(element).slideUp(600);
                    }, 2000);
                });
            }
        }
    };

    ko.bindingHandlers['jqButton'] = {
        init: function (e) {
            $(e).button();
            ko.utils.domNodeDisposal.addDisposeCallback(e, function () {
                return $(e).button("destroy");
            });
        }
    };

    ko.bindingHandlers['disableTree'] = {
        update: function (element, valueAccessor) {
            return $(element).find("input,select,textarea,button").andSelf().prop("disabled", u(valueAccessor()));
        }
    };

    // KO's built-in hasFocus doesn't work for some reason.
    // No time to investigate, much quicker to roll my own.
    ko.bindingHandlers['hasFocus'] = {
        init: function (element, valueAccessor) {
            var e = $(element);
            var v = valueAccessor();

            u(v) && e.focus();

            if (ko.isObservable(v)) {
                var evts = { focus: function () {
                        v(true);
                    }, blur: function () {
                        v(false);
                    } };
                e.on(evts);
                ko.utils.domNodeDisposal.addDisposeCallback(e, function () {
                    return e.off(evts);
                });
            }
        },
        update: function (element, valueAccessor) {
            return u(valueAccessor()) && $(element).focus();
        }
    };

    ko.bindingHandlers['assignTo'] = {
        init: function (element, valueAccessor) {
            var v = valueAccessor();
            if (ko.isObservable(v))
                v(element);
        }
    };

    (function () {
        ko.bindingHandlers['jqWidget'] = {
            init: function (e) {
                return ko.utils.domNodeDisposal.addDisposeCallback(e, destroyWidgets);
            },
            update: setWidgets
        };

        var destroyKey = "{1E981B28-C65C-4E4A-BF28-DA2F399BC5F6}";
        var constantWidgetKey = "";

        function destroyWidgets(e) {
            var d = $(e).data(destroyKey);
            $(e).removeData(destroyKey);
            d && d();
        }

        function setWidgets(element, valueAccessor) {
            var e = $(element);
            if (e.data(constantWidgetKey))
                return;

            destroyWidgets(element);

            var rawValue = valueAccessor();
            var canChange = ko.isObservable(rawValue);
            var v = u(rawValue);
            if (typeof v === "string" && e[v]) {
                var x = {};
                x[v] = null;
                v = x;
            }

            if (!canChange)
                e.data(constantWidgetKey, true);

            function cons(f, g) {
                return function () {
                    g();
                    f();
                };
            }
            function widget(ctor, args) {
                if (!ctor)
                    return function () {
                    };
                if (!args)
                    args = [];
                if (!Array.isArray(args))
                    args = [args];
                ctor.apply(e, args);
                return function () {
                    return ctor.call(e, "destroy");
                };
            }

            var destroy = e.children().length ? function () {
            } : function () {
                return e.empty();
            };
            for (var k in v) {
                var removeWidget = widget(e[k], v[k]);
                destroy = cons(destroy, removeWidget);
            }

            if (canChange)
                e.data(destroyKey, destroy);
        }
        ;
    })();

    ko.bindingHandlers['jqData'] = {
        init: function (e, v) {
            return $(e).data(u(v()) || {});
        },
        update: function (e, v) {
            return $(e).data(u(v()) || {});
        }
    };

    ko.bindingHandlers["loadingWhen"] = (function () {
        var key = "{32DD9D9D-36B6-4DA3-8883-547AB89D6A33}";

        return {
            update: function (e, v, all) {
                var $e = $(e);
                var loading = $e.data(key);
                var overlay = all().overlayLoadingMode;

                if (u(v())) {
                    if (!loading) {
                        loading = $("<div>").addClass("Loading").insertBefore($e);
                        $e.data(key, loading);
                        if (overlay)
                            loading.css({ position: 'absolute' }).position({ my: "left top", at: "left top", of: $e }).css({ width: $e.width(), height: $e.height() });
else
                            $e.hide();
                    }
                } else {
                    $e.show().removeData(key);
                    if (loading)
                        loading.remove();
                }
            }
        };
    })();
});
