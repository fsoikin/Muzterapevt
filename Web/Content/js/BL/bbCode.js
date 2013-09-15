define(["require", "exports", "../common", "jQuery", "rx", "ko"], function(require, exports, __c__, __$__, __rx__, __ko__) {
    var c = __c__;
    var $ = __$__;
    var rx = __rx__;
    var ko = __ko__;

    var Ajax = {
        ToHtml: "bbcode/toHtml"
    };

    function toHtml(bbCode) {
        return rx.Observable.create(function (ss) {
            var err = ko.observable();
            err.subscribe(function (e) {
                return ss.onError(e);
            });

            var req = c.Api.Get(Ajax.ToHtml, { bbText: bbCode }, null, err, function (d) {
                ss.onNext(d);
                ss.onCompleted();
            });

            return function () {
                return req.abort();
            };
        });
    }
    exports.toHtml = toHtml;
});
