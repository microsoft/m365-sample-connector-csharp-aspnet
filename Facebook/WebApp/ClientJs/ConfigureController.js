angular.module('MainApp').controller('ConfigureCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.isLoginComplete = false;
    $scope.isDisabledSaveButton = false;

    var checkSharedSecret = "api/ConnectorSetup/ValidateSetup";
    var configurationUrl = "api/Configuration";
    $scope.login = () => {
        if ($scope.sharedSecretKey != null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("tenantId", $scope.tenantId);
            $http.get(checkSharedSecret).then((response) => {
                $scope.isLoginComplete = response;
            });

            $http.get(configurationUrl).then((response) => {
                var result = response.data;
                setTimeout(function () {
                }, 500);
                $scope.FBAppIdValue = result["FBAppIdValue"];
                $scope.FBAppSecretValue = result["FBAppSecretValue"] ;
                $scope.FBVerifyTokenValue = result["FBVerifyTokenValue"];
                $scope.AADAppIdValue = result["AADAppIdValue"];
                $scope.AADAppSecretValue = result["AADAppSecretValue"];
                $scope.AADAppUriValue = result["AADAppUriValue"];
                $scope.InstrumentationKeyValue = result["InstrumentationKeyValue"];
            });
        }
    }
    
    $scope.SaveConfigSettings = () => {
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
        $http.post(configurationUrl, settings).then((response) => {
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