var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
define(["require", "exports", "../common", "ko", "ko.mapping", "jQuery"], function(require, exports, __c__, __ko__, __map__, __$__) {
    var c = __c__;
    var ko = __ko__;
    var map = __map__;
    var $ = __$__;

    var Ajax = {
        Load: "BackOffice/Pages/All",
        Update: "BackOffice/Pages/Update",
        Create: "BackOffice/Pages/Create"
    };

    var PagesVm = (function (_super) {
        __extends(PagesVm, _super);
        function PagesVm() {
            var _this = this;
            _super.call(this);
            this.Pages = ko.observableArray();
            c.Api.Get(Ajax.Load, null, this.IsLoading, this.Error, function (ps) {
                return _this.Pages(ps.map(function (p) {
                    return new PageVm(_this, p);
                }));
            });
        }
        PagesVm.prototype.Create = function () {
            var p = new PageVm(this, null);
            this.Pages.push(p);
            p.Edit();
        };
        return PagesVm;
    })(c.VmBase);
    exports.PagesVm = PagesVm;

    var PageVm = (function () {
        function PageVm(Parent, Page) {
            var _this = this;
            this.Parent = Parent;
            this.Page = Page;
            this.IsNew = ko.observable(true);
            this.IsSaving = ko.observable(false);
            this.IsEditing = ko.observable(false);
            this.Path = ko.observable("");
            this.Id = ko.observable(0);
            if (Page) {
                map.fromJS(Page, {}, this);
                this.IsNew(false);
            }
            this.IsEditing.subscribe(function (e) {
                if (e)
                    _this.Parent.Pages().forEach(function (p) {
                        return p == _this || p.IsEditing(false);
                    });
            });
        }
        PageVm.prototype.Edit = function () {
            this.IsEditing(true);
        };

        PageVm.prototype.CommitEdit = function () {
            var _this = this;
            this.Page = map.toJS(this);
            c.Api.Post(this.IsNew() ? Ajax.Create : Ajax.Update, null, this.IsSaving, this.Parent.Error, function () {
                _this.IsEditing(false);
                _this.IsNew(false);
            });
        };

        PageVm.prototype.CandelEdit = function () {
            this.IsEditing(false);
            map.fromJS(this.Page, {}, this);
        };
        return PageVm;
    })();
    exports.PageVm = PageVm;
});
//# sourceMappingURL=pages.js.map
