angular.module('MainApp').controller('NativeConnectorSetupCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.jobType = "Facebook";
    $scope.isLoginComplete = false;
    $scope.sharedSecretKey = "";
    $scope.isSetupComplete = false;
    $scope.noPageSelected = false;
    $scope.noPagesToArchive = false;
    $scope.noPageinAccount = false;
    $scope.pageSaveMessage = "";
    $scope.entities = [];
    $scope.isbusy = true;
    $scope.isTokenDeleted = false;
    $scope.isAuthenticated = false;
    $scope.authenticationUrl = null;
    $scope.isEntityListVisible = false;
    $scope.facebookRedirectUrl = $location.protocol() + "://" + location.host + "/Views/FacebookOAuth";
    $scope.facebookBaseUrl = $location.protocol() + "://" + location.host + "/Views/FacebookOAuth";

    var jobId = getParameter("jobId");
    var tenantId = getParameter("tenantId");

    var getOAuthUrl = "api/ConnectorSetup/OAuthUrl" + "?jobType=" + $scope.jobType + "&redirectUrl=" + $scope.facebookRedirectUrl;
    var getEntitiesUrl = "api/ConnectorSetup/GetEntities" + "?jobType=" + $scope.jobType;
    var deleteTokenUrl = "api/ConnectorSetup/DeleteToken" + "?jobType=" + $scope.jobType;
    
    
    $scope.login = () => {
        if ($scope.sharedSecretKey != null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("jobId", jobId);
            $cookies.put("tenantId", tenantId);

            getEntitiesUrl = getEntitiesUrl + "&jobId=" + jobId;
            deleteTokenUrl = deleteTokenUrl + "&jobId=" + jobId;
            $scope.isLoginComplete = true;

            $http.get(getOAuthUrl).then((response) => {
                $scope.authenticationUrl = response.data;
                $scope.isbusy = false;
            });
        }
    }

    $scope.openPopop = () => {
        var encodedAuthUrl = encodeURIComponent($scope.authenticationUrl);
        var url = $scope.facebookBaseUrl + "?loginUrl=" + encodedAuthUrl;
        $scope.isbusy = true;
        openPopup(this, url, function authenticationCallback() {
            $http.get(getEntitiesUrl).then((response) => {
                $scope.isbusy = false;
                if (response.data) {
                    $scope.isAuthenticated = true;
                    $scope.entities = response.data;
                }
                else {
                    $scope.noPageinAccount = true;
                    $scope.errorMessage = "No Pages linked to the account."
                }
            });
        });
    };

    $scope.showEntityList = () => {
        $scope.isEntityListVisible = true;
    }

    $scope.saveJob = () => {
        $scope.noPageSelected = false;
        $scope.noPagesToArchive = false;
        var savePageurl = "api/ConnectorSetup/SavePage" + "?jobId=" + jobId + "&jobType=" + $scope.jobType + "&tenantId=" + tenantId;
        var selectedPage = $scope.entities[0];
        var selected = false;

        for (i = 0; i < $scope.entities.length; i++) {
            if ($scope.entities[i].selected)
            {
                selectedPage = $scope.entities[i];
                selected = true;
                break;
            }
        }

        if (!selected && $scope.entities.filter(function (e) { return e.AlreadyUsed === false; }).length === 0) {
            $scope.noPagesToArchive = true;
        }

        if (selectedPage && selected) {
            var pageToBeSaved = {
                Name: selectedPage.Name,
                Id: selectedPage.Id
            };
            $http.post(savePageurl, pageToBeSaved).then((response) => {
                var res = response.data;
                setTimeout(function () {
                }, 500);

                if (res == true)
                {
                    $scope.pageSaveMessage = "Facebook Connector Job Successfully set up.";
                }
                else
                {
                    $scope.pageSaveMessage = "Facebook Connector Job Successfully set up. Webhook Subscription failed for this page. Please get your app reviewed by Facebook with manage_pages permission."
                }

                $scope.isSetupComplete = true;
            }).catch((error) => { });
        }
        else {
            if ($scope.noPagesToArchive === true) {
                $scope.errorMessage = "All pages are already archived. No new pages to archive."
            }
            else {
                $scope.errorMessage = "Select a page to proceed."
            }
            $scope.noPageSelected = true;
        }
    }

    $scope.finishSetup = () => {
        $http.get(deleteTokenUrl).then((response) => {
            $scope.isTokenDeleted = response.data;
        });
        window.close();
    }
}]);

function openPopup(context, path, callback) {
    var windowName = 'AuthenticationPopup';
    var windowOptions = 'location=0,status=0,width=800,height=400';
    var popupCallback = callback || function () { window.location.reload(); };
    var _oauthWindow = window.open(path, windowName, windowOptions);
    var _oauthInterval = window.setInterval(function () {
        if (_oauthWindow.closed) {
            window.clearInterval(_oauthInterval);
            popupCallback.call(context);
        }
    }, 2000);
}

function getParameter(paramName) {
    var searchString = window.location.search.substring(1),
        i, val, params = searchString.split("&");

    for (i = 0; i < params.length; i++) {
        val = params[i].split("=");
        if (val[0] == paramName) {
            return val[1];
        }
    }
    return null;
}