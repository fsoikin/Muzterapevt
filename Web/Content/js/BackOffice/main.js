define(["require", "exports", "jQuery", "./pages"], function(require, exports, __$__, __pages__) {
    
    var $ = __$__;
    var pages = __pages__;

    new pages.PagesVm().OnLoaded($("body")[0]);
});
