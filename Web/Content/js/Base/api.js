define(["require", "exports", "jQuery", "RootUrl"], function(require, exports, __$__, ___root__) {
    /// <reference path="../../tsd/jquery.amd.d.ts"/>
    /// <reference path="../../tsd/knockout-2.2.d.ts"/>
    var $ = __$__;

    function Get(url, data, inProgress, error, onSuccess, options) {
        return exports.Call("GET", url, data, inProgress, error, onSuccess, options);
    }
    exports.Get = Get;

    function Post(url, data, inProgress, error, onSuccess, options) {
        return exports.Call("POST", url, JSON.stringify(data), inProgress, error, onSuccess, options);
    }
    exports.Post = Post;

    function Call(method, url, data, inProgress, error, onSuccess, options) {
        inProgress && inProgress(true);
        return $.ajax($.extend(options || {}, { type: method, url: exports.AbsoluteUrl(url), data: data, contentType: 'application/json' })).fail(function (e) {
            return error && error.notifySubscribers(e.statusText);
        }).always(function () {
            return inProgress && inProgress(false);
        }).done(function (e) {
            return e.Success ? (onSuccess && onSuccess(e.Result)) : (error && error.notifySubscribers((e.Messages || []).join()));
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
    var _root = ___root__;

    exports.RootUrl = (function (r) {
        return typeof r !== "string" || !r.length ? "/" : r[r.length - 1] == '/' ? r : r + '/';
    })(_root);
});
