angular.module('MainApp').controller('ConfigureCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.isLoginComplete = false;
    $scope.isDisabledSaveButton = false;

    var checkSharedSecret = "setup/Validate";
    var configurationUrl = "api/Configuration";
    $scope.login = function () {
        if ($scope.sharedSecretKey != null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("tenantId", $scope.tenantId);
            $http.get(checkSharedSecret).then(function (response) {
                $scope.isLoginComplete = response.data.Status;
            });

            $http.get(configurationUrl).then(function (response) {
                var result = response.data;
                setTimeout(function () {
                }, 500);
                $scope.FBAppIdValue = result["FBAppIdValue"];
                $scope.FBAppSecretValue = result["FBAppSecretValue"];
                $scope.FBVerifyTokenValue = result["FBVerifyTokenValue"];
                $scope.AADAppIdValue = result["AADAppIdValue"];
                $scope.AADAppSecretValue = result["AADAppSecretValue"];
                $scope.AADAppUriValue = result["AADAppUriValue"];
                $scope.InstrumentationKeyValue = result["InstrumentationKeyValue"];
            });
        }
    }

    $scope.SaveConfigSettings = function () {
        $scope.isDisabledSaveButton = true;
        $scope.configurationSavedMsg = "Saving Configuration ...";
        var settings = {
            FBAppIdValue: (typeof $scope.FBAppIdValue !== 'undefined') ? $scope.FBAppIdValue : "",
            FBAppSecretValue: (typeof $scope.FBAppSecretValue !== 'undefined') ? $scope.FBAppSecretValue : "",
            FBVerifyTokenValue: (typeof $scope.FBVerifyTokenValue !== 'undefined') ? $scope.FBVerifyTokenValue : "",
            AADAppIdValue: (typeof $scope.AADAppIdValue !== 'undefined') ? $scope.AADAppIdValue : "",
            AADAppSecretValue: (typeof $scope.AADAppSecretValue !== 'undefined') ? $scope.AADAppSecretValue : "",
            AADAppUriValue: (typeof $scope.AADAppUriValue !== 'undefined') ? $scope.AADAppUriValue : "",
            InstrumentationKeyValue: (typeof $scope.InstrumentationKeyValue !== 'undefined') ? $scope.InstrumentationKeyValue : ""
        };
        $http.post(configurationUrl, settings).then(function (response) {
            var res = response.data;
            setTimeout(function () {
            }, 500);
            $scope.configurationSavedMsg = "Configuration Saved Successfully.";
        }).catch(function (error) { });
    }

    $scope.Home = function () {
        window.location.href = "/index.cshtml";
    }

}]);