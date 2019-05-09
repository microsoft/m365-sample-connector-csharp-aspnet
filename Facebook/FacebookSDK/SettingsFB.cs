// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    public class SettingsFB
    {
        // Constant Settings

        public const string FacebookOauthEndpoint = "oauth/access_token";

        public const string FacebookGraphApiEndpoint = "me/accounts";

        public const string FacebookBaseUrl = "https://graph.facebook.com/v3.0";

        public const string FacebookOauthEndpointWithPermissions = "https://www.facebook.com/v3.0/dialog/oauth?scope=manage_pages,pages_show_list,email";

        public const string FacebookQueryFields = "id,created_time,from{name,id,picture},to,message,story,likes.summary(true),reactions.summary(true),comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags,comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags}},attachments,source,message_tags,type,status_type";

        // Configurable settings at run time
        public static string FacebookAppId = string.Empty;

        public static string FacebookAppSecret = string.Empty;

        public static string FacebookVerifyToken = string.Empty;
    }
}
