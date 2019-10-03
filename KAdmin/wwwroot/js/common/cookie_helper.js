(function (global) {
    var CookieHelper = {
        getCookieByName: function (cookieName) {
            var value = document.cookie.match('(^|;)\\s*' + cookieName + '\\s*=\\s*([^;]+)');
            return value ? value.pop() : '';
        },

        isAliCloud: function () {
            return (this.getCookieByName('CLOUD_PROVIDER') === 'ALI_CLOUD');
        }
    };

    global.CookieHelper = CookieHelper;
})(window);