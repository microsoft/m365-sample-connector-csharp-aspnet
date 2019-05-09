angular.module('MainApp', ['ngCookies']).factory('authInterceptor', function ($cookies) {
    return {
        request: function (config) {
            config.headers = config.headers || {};
            var secret = $cookies.get("sharedSecret");
            var tenantId = $cookies.get("tenantId");
            if (secret && tenantId) {
                config.headers.Authorization = 'Basic ' + btoa(tenantId + ':' + secret);

                return config;
            }
        },
        responseError: function (rejection) {
            if (rejection.status === 401) {
                console.log("Response Error 401", rejection);
                window.confirm("Invalid shared secret");
                window.location.reload();
            }
            return;
        }
    }
})

angular.module('MainApp').config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptor');
})