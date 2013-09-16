define(["require", "exports", "../common", "ko", "ko.mapping", "rx", "jQuery", "text!./Templates/page.html", "css!styles/Page.css"], function(require, exports, __c__, __ko__, __map__, __rx__, __$__, ___template__) {
    /// <amd-dependency path="css!styles/Page.css" />
    var c = __c__;
    var ko = __ko__;
    var map = __map__;
    var rx = __rx__;
    var $ = __$__;
    

    

    var Ajax = {
        Load: "page/load",
        Update: "page/update"
    };

    var PageVm = (function () {
        function PageVm(args) {
            this.ControlsDescendantBindings = true;
            this.PageId = args.id;
        }
        PageVm.prototype.OnLoaded = function (element) {
            var _this = this;
            this.PageElement = $(element);
            if (!this.PageElement.text().trim()) {
                this.PageElement.text("Right-click here");
            }

            this.MenuElement = Template.Menu.clone().appendTo("body").hide();
            ko.applyBindings(this, this.MenuElement[0]);

            this.PageElement.on("contextmenu", function (e) {
                if (e.which == 3) {
                    _this.MenuElement.css({ top: e.pageY, left: e.pageY }).fadeIn(100);
                    $(document).one("click", function () {
                        return _this.MenuElement.fadeOut(100);
                    });
                    return false;
                }
            });
        };

        PageVm.prototype.OnEdit = function () {
            var e = this.Editor || (this.Editor = new EditorVm(this));
            e.Show();
        };
        return PageVm;
    })();

    // Defined in JS/PageEditor.cs
    var EditorVm = (function () {
        function EditorVm(Page) {
            var _this = this;
            this.Page = Page;
            this.IsLoading = ko.observable(false);
            this.Error = ko.observable();
            this.Text = ko.observable("");
            this.Title = ko.observable("");
            c.Api.Get(Ajax.Load, { id: Page.PageId }, this.IsLoading, this.Error, function (r) {
                return map.fromJS(r, {}, _this);
            });
        }
        EditorVm.prototype.Save = function () {
            var _this = this;
            var page = map.toJS(this);
            page.Id = this.Page.PageId;
            c.Api.Post(Ajax.Update, page, this.IsLoading, this.Error, function (html) {
                _this.Close();
                _this.Page.PageElement.html(html).prepend($("<h2>").text(_this.Title()));
            });
        };

        EditorVm.prototype.Cancel = function () {
            this.Close();
        };

        EditorVm.prototype.Close = function () {
            this.Element && this.Element.fadeOut();
        };

        EditorVm.prototype.Show = function () {
            (this.Element || (this.Element = this.CreateElement())).fadeIn();
        };

        EditorVm.prototype.CreateElement = function () {
            var e = Template.Editor.clone().appendTo('body');
            ko.applyBindings(this, e[0]);
            return e;
        };
        return EditorVm;
    })();

    var _template = ___template__;
    var Template = (function () {
        var t = $(_template);
        return {
            Menu: t.filter(".page-menu"),
            Editor: t.filter(".page-editor")
        };
    })();
    return PageVm;
});
