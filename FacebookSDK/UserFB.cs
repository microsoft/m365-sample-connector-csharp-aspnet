// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook User
    /// </summary>
    public class UserFB
    {
        /// <summary>
        /// User Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// User First Name
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User Gender
        /// </summary>
        [JsonProperty("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// User Last Name
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// User Email, Requires email permission
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Link to User's Profile
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; set; }

        /// <summary>
        /// User's Location, Requires user_location permission
        /// </summary>
        [JsonProperty("location")]
        public UserLocationFB Location { get; set; }

        /// <summary>
        /// User's Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// User's Profile Picture
        /// </summary>
        [JsonProperty("picture")]
        public ProfilePictureDataFB Picture { get; set; }

        /// <summary>
        /// User's TimeZone
        /// </summary>
        [JsonProperty("timezone")]
        public int TimeZone { get; set; }

        /// <summary>
        /// Is User verified or not
        /// </summary>
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        /// <summary>
        /// Extra Field, used in FBReaction
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
