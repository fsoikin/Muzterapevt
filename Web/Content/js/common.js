/// <reference path="../tsd/require.d.ts"/>
/// <reference path="../tsd/polyfill.d.ts"/>
/// <reference path="../tsd/jquery.amd.d.ts"/>
/// <reference path="../tsd/jqueryui.d.ts"/>
/// <reference path="../tsd/knockout-2.2.d.ts"/>
/// <reference path="../tsd/knockout.mapping-2.0.d.ts"/>
/// <reference path="../tsd/rx.d.ts" />
/// <reference path="../tsd/rx-ko.d.ts" />
define(["require", "exports", "rx", "ko", "jQuery", "./Base/api", "./Base/seq"], function(require, exports, __rx__, __ko__, __$__, ___api__, ___seq__) {
    var rx = __rx__;
    var ko = __ko__;
    var $ = __$__;
    var _api = ___api__;
    exports._api = _api;
    var _seq = ___seq__;
    exports._seq = _seq;
    exports.Seq = exports._seq;
    exports.Api = exports._api;

    function enumToString(theEnum, value) {
        return (theEnum)[value];
    }
    exports.enumToString = enumToString;

    function mapEnum(obj) {
        var res = new Array();
        for (var k in obj) {
            var v = parseInt(k);
            if (v !== null && v !== undefined && !isNaN(v))
                res.push({ Name: obj[k], Value: v });
        }
        return res;
    }
    exports.mapEnum = mapEnum;

    function dataSourceFromEnum(theEnum) {
        return {
            Items: ko.observable(exports.mapEnum(theEnum)),
            GetId: function (x) {
                return x.Value;
            },
            ToString: function (x) {
                return x.Name;
            }
        };
    }
    exports.dataSourceFromEnum = dataSourceFromEnum;

    function dataSourceFromNameValuePair(items) {
        var ii = ko.observable();
        setImmediate(function () {
            return ii(items);
        });
        return {
            Items: ii,
            GetId: function (i) {
                return i.Value;
            },
            ToString: function (i) {
                return i.Name;
            }
        };
    }
    exports.dataSourceFromNameValuePair = dataSourceFromNameValuePair;

    function dataSourceFromServer(url, toString, getId) {
        return function () {
            return new RemoteDataSource({
                url: url,
                getId: getId || function (u) {
                    return u.Id;
                },
                toString: toString || function (u) {
                    return u.Name;
                }
            });
        };
    }
    exports.dataSourceFromServer = dataSourceFromServer;

    var VmBase = (function () {
        function VmBase() {
            this.IsLoading = ko.observable(false);
            this.IsSaving = ko.observable(false);
            this.Message = new ko.subscribable();
            this.Error = new ko.subscribable();
        }
        return VmBase;
    })();
    exports.VmBase = VmBase;

    var RemoteDataSource = (function () {
        function RemoteDataSource(args) {
            this.IsLoading = ko.observable(false);
            this.Error = new ko.subscribable();
            this.Items = ko.observableArray();
            // TODO: [fs] waiting for https://typescript.codeplex.com/workitem/1450
            var me = this;
            me['GetId'] = args.getId || function (i) {
                if (!i)
                    return null;
                if (!('Id' in i))
                    throw new Error("'Id' property not found on object " + JSON.stringify(i));
                return i['Id'];
            };
            me['ToString'] = args.toString || function (i) {
                return (i || "").toString();
            };

            this.Url = args.url;
            this.LookupUrl = args.lookupUrl;
            this.Reload();
        }
        RemoteDataSource.prototype.GetId = function (i) {
            return null;
        };
        RemoteDataSource.prototype.ToString = function (i) {
            return "";
        };

        RemoteDataSource.prototype.Reload = function () {
            exports.Api.Get(this.Url, null, this.IsLoading, this.Error, this.Items);
        };

        RemoteDataSource.prototype.Lookup = function (term) {
            var _this = this;
            if (!this.LookupUrl) {
                term = (term || "").toLowerCase();
                return this.Items.asRx().select(function (items) {
                    return items.filter(function (item) {
                        var t = _this.ToString(item);
                        return t && t.toLowerCase().indexOf(term) >= 0;
                    });
                }).take(1);
            }

            return rx.Observable.create(function (o) {
                var err = new ko.subscribable();
                var d = err.subscribe(function (e) {
                    o.onError(e);
                    _this.Error.notifySubscribers(e);
                });

                var xhr = exports.Api.Get(_this.LookupUrl(term), null, _this.IsLoading, err, function (res) {
                    o.onNext(res);
                    o.onCompleted();
                });

                return function () {
                    d.dispose();
                    xhr.abort();
                };
            });
        };

        RemoteDataSource.prototype.GetById = function (id) {
            var dd = this.Items();
            return dd && ko.utils.arrayFirst(dd, function (d) {
                return d.Id == id;
            });
        };
        return RemoteDataSource;
    })();
    exports.RemoteDataSource = RemoteDataSource;

    function compareArrays(a, b, comparer) {
        if (typeof comparer === "undefined") { comparer = function (x, y) {
            return x == y;
        }; }
        if (!a)
            return !b;
        if (!b)
            return false;
        if (a.length != b.length)
            return false;

        for (var i = 0; i < a.length; i++)
            if (!comparer(a[i], b[i]))
                return false;

        return true;
    }
    exports.compareArrays = compareArrays;

    function koStringToDate(str) {
        return ko.computed({
            read: function () {
                return new Date(str());
            },
            write: function (d) {
                return str(d && d.toJSON());
            }
        });
    }
    exports.koStringToDate = koStringToDate;

    function koDateToString(d) {
        return ko.computed({
            read: function () {
                return d().toJSON();
            },
            write: function (s) {
                return d(new Date(s));
            }
        });
    }
    exports.koDateToString = koDateToString;

    function bindClass(theClass, args) {
        function _f() {
            this.constructor = theClass;
            theClass.call(this, args);
        }
        _f.prototype = theClass.prototype;
        return _f;
    }
    exports.bindClass = bindClass;

    function ApplyTemplate(template) {
        var t = typeof template == "string" ? $(template) : template;

        return function (element) {
            if ($.makeArray(ko.virtualElements.childNodes(element)).every(function (e) {
                return e.nodeType != 1;
            })) {
                ko.virtualElements.setDomNodeChildren(element, t.clone());
            }
            ko.applyBindingsToDescendants(this, element);
        };
    }
    exports.ApplyTemplate = ApplyTemplate;
});
//# sourceMappingURL=common.js.map
