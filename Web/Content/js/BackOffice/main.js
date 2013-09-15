define(["require", "exports", "jQuery", "./pages"], function(require, exports, __$__, __pages__) {
    
    var $ = __$__;
    var pages = __pages__;

    

    function init(root) {
        new pages.PagesVm().OnLoaded(root[0]);
    }
    ;
    return init;
});
