define(["require", "exports", "ko"], function(require, exports, __ko__) {
    
    var ko = __ko__;

    function parse(classRef, args) {
        var parts = (classRef || "").split(',');
        if (parts.length == 1)
            parts = ["", parts[0]];
        if (parts.length != 2) {
            console.warn("Invalid autobind controller specification: " + classRef);
            return null;
        }

        if (typeof args === "string") {
            try  {
                args = new Function("return (" + args + ");")();
            } catch (_) {
            }
        }

        return { Module: parts[1].trim(), Class: parts[0].trim(), Arguments: args };
    }
    exports.parse = parse;

    function bind(ref) {
        return exports.bindMany([ref])[0];
    }
    exports.bind = bind;

    function bindMany(refs) {
        refs = refs || [];
        return refs.map(function (cr) {
            var r = ko.observable(null);
            if (cr)
                req([cr.Module], function (m) {
                    return r(instantiate(m, cr));
                });
            return r;
        });
    }
    exports.bindMany = bindMany;

    function bindAll(refs) {
        if (!refs)
            return ko.observableArray();

        var result = ko.observableArray();
        req(refs.map(function (r) {
            return r.Module;
        }), function () {
            result(ko.utils.makeArray(arguments).map(function (m, idx) {
                return instantiate(m, refs[idx]);
            }));
        });
        return result;
    }
    exports.bindAll = bindAll;

    function instantiate(mod, ref) {
        var ctor = ref.Class ? getClass(mod, ref.Class) : mod;

        if (!ctor)
            throw new Error("Cannot find class '" + ref.Class + "' in module '" + ref.Module + "'.");

        if (ref.Arguments !== undefined && ref.Arguments !== null) {
            ctor = Function.prototype.bind.apply(ctor, Array.isArray(ref.Arguments) ? [null].concat(ref.Arguments) : [null, ref.Arguments]);
        }

        return new ctor();
    }

    function getClass(mod, cls) {
        return cls.split('.').reduce(function (m, p) {
            return m[p];
        }, mod);
    }

    var req = (function () {
        var loadedModules = {};

        return function (deps, cb) {
            var _this = this;
            var loaded = deps.map(function (d) {
                return loadedModules[d];
            });
            if (loaded.every(function (d) {
                return d;
            }))
                cb.apply(this, loaded);
else
                require(deps, function () {
                    var resolved = arguments;
                    deps.every(function (d, idx) {
                        return loadedModules[d] = loaded[idx] = resolved[idx];
                    });
                    cb.apply(_this, loaded);
                });
        };
    })();
});
