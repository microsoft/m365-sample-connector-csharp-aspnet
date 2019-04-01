// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.ActiveDirectory;

[assembly: OwinStartupAttribute(typeof(Sample.Connector.Startup))]
namespace Sample.Connector
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            app.UseWebApi(config);
        }

        private void ConfigureOAuth(IAppBuilder app)
        {
            // The audience is app uri of application 7e3f2f3a-acbb-4457-904a-c39b82c9e861
            // For what resource, the token has been generated
            var audience = "https://microsoft.onmicrosoft.com/7716be20-38a1-461b-9ba8-fb0bcc73609c";
            var metaDataAddress = "https://login.microsoftonline.com/common/federationmetadata/2007-06/federationmetadata.xml";
            
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    MetadataAddress = metaDataAddress,
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters() { ValidateIssuer = false, ValidAudience = audience }
                });
        }
    }
}