// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    // Generic item schema to uniformly capture data from various third-party messaging data sources (like Facebook, Twitter, etc.,)
    public class Item
    {
        public Version SchemaVersion { get; set; }

        // Opaque id of the item as passed down by the data source
        [Required]
        public string Id { get; set; }

        // Source of the content (like Facebook, Twitter, etc.,)
        [Required]
        public string SourceType { get; set; }

        // Granualar type of the content (like Facebook Post, Facebook Comment, Twitter Tweet, Twitter Retweet, etc.,)
        [Required]
        public string ItemType { get; set; }

        // Container of the items (like Facebook Page, etc.,)
        [Required]
        public string ContainerId { get; set; }

        // Container name (like Facebook Page, etc.,)
        public string ContainerName { get; set; }

        // Thread is a single discussion with in the container (like Facebook Post along with it's comment and replies in a Facebook Page)
        public string ThreadId { get; set; }

        // Opaque id of the parent item as passed down by the content source.
        // To capture the parent-child relationship of the Converstation tree. This is null/empty for root of the conversation (like Facebook Post)
        public string ParentId { get; set; }

        // User (or any principle/identity) who sent this item
        [Required]
        public User Sender { get; set; }

        // Users (or any principles/identities) who recieved this item
        public User[] Recipients { get; set; }

        // Time at which item is sent/created in Utc
        public DateTime SentTimeUtc { get; set; }

        // Type of the item content (like Text, Html, etc.,)
        public ContentType ContentType { get; set; }

        // Content of the item
        public string Content { get; set; }

        // Pre Context for an item
        public List<Item> PreContext { get; set; }

        // Post Context for an item
        public List<Item> PostContext { get; set; }

        // Number of likes on the item
        public long NumOfLikes { get; set; }

        // Preview/snippet text for concise displaying the item
        public string MessagePreviewText { get; set; }

        // Attachments with the item
        public List<ContentAttachment> ContentAttachments { get; set; }
    }

    public class User
    {
        public string Id { get; set; }
        public string UserProfilePhotoUrl { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
    }

    public enum ContentType
    {
        HTML = 0,
        Text = 1
    }

    public class ContentAttachment
    {
        public string Content;
        public Uri Uri { get; set; }
        public string AttachmentType { get; set; }
        public string AttachmentFileName { get; set; }
    }
}