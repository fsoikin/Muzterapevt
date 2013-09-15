define(["require", "exports"], function(require, exports) {
    function indexOfFirst(array, predicate) {
        if (!array || !predicate)
            return -1;
        for (var i = 0; i < array.length; i++) {
            if (predicate(array[i]))
                return i;
        }
        return -1;
    }
    exports.indexOfFirst = indexOfFirst;

    function copy(array) {
        return array.filter(function () {
            return true;
        });
    }
    exports.copy = copy;
});
