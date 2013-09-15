define(["require", "exports", "jQuery"], function(require, exports, __$__) {
    /// <reference path="../../tsd/jquery.amd.d.ts"/>
    /// <reference path="../../tsd/knockout-2.2.d.ts"/>
    var $ = __$__;

    function Get(url, data, inProgress, error, onSuccess, options) {
        return exports.Call("GET", url, data, inProgress, error, onSuccess, options);
    }
    exports.Get = Get;

    function Post(url, data, inProgress, error, onSuccess, options) {
        return exports.Call("POST", url, data, inProgress, error, onSuccess, options);
    }
    exports.Post = Post;

    function Call(method, url, data, inProgress, error, onSuccess, options) {
        inProgress && inProgress(true);
        return $.ajax($.extend(options || {}, { type: method, url: exports.AbsoluteUrl(url), data: data })).fail(function (e) {
            return error && error.notifySubscribers(e.statusText);
        }).done(function (e) {
            return e.Success ? (onSuccess && onSuccess(e.Result)) : (error && error.notifySubscribers((e.Messages || []).join()));
        }).always(function () {
            return inProgress && inProgress(false);
        });
    }
    exports.Call = Call;

    function AbsoluteUrl(url) {
        if (url && url[0] == '/')
            url = url.substring(1);
        return exports.RootUrl + url;
    }
    exports.AbsoluteUrl = AbsoluteUrl;

    ;
    

    exports.RootUrl = function (_root) {
        return typeof _root !== "string" || !_root.length ? "/" : _root[_root.length] == '/' ? _root : _root + '/';
    };
});
//# sourceMappingURL=api.js.map
