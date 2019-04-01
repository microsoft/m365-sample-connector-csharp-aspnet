angular.module('MainApp').controller('ConfigureCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.isLoginComplete = false;
    $scope.isDisabledSaveButton = false;

    var checkSharedSecret = "api/ConnectorSetup/ValidateSetup";
    $scope.login = () => {
        if ($scope.sharedSecretKey != null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("tenantId", $scope.tenantId);
            $http.get(checkSharedSecret).then((response) => {
                $scope.isLoginComplete = response;
            });
        }
    }

    $scope.SaveConfigSettings = () => {
        $scope.isDisabledSaveButton = true;
        $scope.configurationSavedMsg = "Saving Configuration ...";
        var configureUrl = "api/Configuration";
        var settings = {
            FBAppIdValue: (typeof $scope.FBAppIdValue !== 'undefined') ? $scope.FBAppIdValue : "",
            FBAppSecretValue: (typeof $scope.FBAppSecretValue !== 'undefined') ? $scope.FBAppSecretValue : "",
            FBVerifyTokenValue: (typeof $scope.FBVerifyTokenValue !== 'undefined') ? $scope.FBVerifyTokenValue : "",
            AADAppIdValue: (typeof $scope.AADAppIdValue !== 'undefined') ? $scope.AADAppIdValue : "",
            AADAppSecretValue: (typeof $scope.AADAppSecretValue !== 'undefined') ? $scope.AADAppSecretValue : "",
            InstrumentationKeyValue: (typeof $scope.InstrumentationKeyValue !== 'undefined') ? $scope.InstrumentationKeyValue : ""
        };
        $http.post(configureUrl, settings).then((response) => {
            var res = response.data;
            setTimeout(function () {
            }, 500);
            $scope.configurationSavedMsg = "Configuration Saved Successfully.";
        }).catch((error) => { });
    }

    $scope.Home = () => {
        window.location.href = "/index.cshtml";
    }

}]);