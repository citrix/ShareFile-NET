// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     
//	   Copyright (c) 2015 Citrix ShareFile. All rights reserved.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Models 
{
	public class Share : ODataObject 
	{

		/// <summary>
		/// When a Share is sent to multiple users, with RequireLogin or RequireUserInfo set, then a different
		/// Share Alias is created for each user. The email ShareFile sends to these users will contain different
		/// AliasIDs, allowing ShareFile to track the user activity on the share.
		/// For anonymous Shares, the AliasID will be the same as the Share ID.
		/// </summary>
		public string AliasID { get; set; }

		/// <summary>
		/// Either "Send" or "Request". Send Shares are used to Send files and folders to the specified users. Request
		/// shares are used to allow users to upload files to the share owner chosen location.
		/// </summary>
		public SafeEnum<ShareType> ShareType { get; set; }

		/// <summary>
		/// Share title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Flag to indicate if ShareFile has sent email messages for this Share
		/// </summary>
		public bool? HasSentMessage { get; set; }

		/// <summary>
		/// Subject of Share email message
		/// </summary>
		public string SentMessageTitle { get; set; }

		/// <summary>
		/// If set, only authenticated users can download files from this share.
		/// </summary>
		public bool? RequireLogin { get; set; }

		/// <summary>
		/// If set, users must provide Name, Email and Company information to download files from the share.
		/// </summary>
		public bool? RequireUserInfo { get; set; }

		/// <summary>
		/// Folder location that contain the share files (Send); or the folder were files will be uploaded to
		/// (Request).
		/// </summary>
		public Item Parent { get; set; }

		/// <summary>
		/// User that created this Share.
		/// </summary>
		public User Creator { get; set; }

		/// <summary>
		/// User given permission to use this share - used for Aliases.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// List of shared Items (for Send Shares only)
		/// </summary>
		public IEnumerable<Item> Items { get; set; }

		/// <summary>
		/// Date the share was created
		/// </summary>
		public DateTime? CreationDate { get; set; }

		/// <summary>
		/// Date the share expires
		/// </summary>
		public DateTime? ExpirationDate { get; set; }

		/// <summary>
		/// Maximum number of downloads each user can perform.
		/// </summary>
		public int? MaxDownloads { get; set; }

		public int? TotalDownloads { get; set; }

		/// <summary>
		/// Used for Virtual Data Room accounts - indicates the files in the share can only be
		/// downloaded with an applied watermark.
		/// </summary>
		public bool? IsViewOnly { get; set; }

		/// <summary>
		/// User activity on this share will be tracked up to this date.
		/// </summary>
		public DateTime? TrackUntilDate { get; set; }

		public int? SendFrequency { get; set; }

		public int? SendInterval { get; set; }

		public DateTime? LastDateSent { get; set; }

		/// <summary>
		/// Indicates whether or not this Share has been downloaded
		/// </summary>
		public bool? IsConsumed { get; set; }

		/// <summary>
		/// Indicates whether the contents of this share have been viewed by a valid, authenticated recipient
		/// </summary>
		public bool? IsRead { get; set; }

		public bool? IsArchived { get; set; }

		public string SendTool { get; set; }

		public string SendMethod { get; set; }

		/// <summary>
		/// When enabled the items are identified by stream IDs instead of item IDs.
		/// Applies to Send Shares only.
		/// </summary>
		public bool? UsesStreamIDs { get; set; }

		/// <summary>
		/// Uri to access the share through the Web portal
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// List of users that have access to this share.
		/// </summary>
		public IEnumerable<ShareAlias> Recipients { get; set; }

		/// <summary>
		/// The Storage Zone that contains this Share.
		/// </summary>
		public Zone Zone { get; set; }

		/// <summary>
		/// HMAC Signature for the Share data
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// Defines whether the request to retrieve Share Items is to be navigated to a remote endpoint.
		/// </summary>
		public bool? HasRemoteChildren { get; set; }

		/// <summary>
		/// Redirection endpoint for this Share.
		/// </summary>
		public Redirection Redirection { get; set; }

		public SafeEnum<ShareSubType> ShareSubType { get; set; }

		/// <summary>
		/// Shared item history.
		/// </summary>
		public IEnumerable<ShareItemHistory> ShareItemHistory { get; set; }

		/// <summary>
		/// Current Settings for the Share
		/// </summary>
		public ShareSettings Settings { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as Share;
			if(typedSource != null)
			{
				AliasID = typedSource.AliasID;
				ShareType = typedSource.ShareType;
				Title = typedSource.Title;
				HasSentMessage = typedSource.HasSentMessage;
				SentMessageTitle = typedSource.SentMessageTitle;
				RequireLogin = typedSource.RequireLogin;
				RequireUserInfo = typedSource.RequireUserInfo;
				Parent = typedSource.Parent;
				Creator = typedSource.Creator;
				User = typedSource.User;
				Items = typedSource.Items;
				CreationDate = typedSource.CreationDate;
				ExpirationDate = typedSource.ExpirationDate;
				MaxDownloads = typedSource.MaxDownloads;
				TotalDownloads = typedSource.TotalDownloads;
				IsViewOnly = typedSource.IsViewOnly;
				TrackUntilDate = typedSource.TrackUntilDate;
				SendFrequency = typedSource.SendFrequency;
				SendInterval = typedSource.SendInterval;
				LastDateSent = typedSource.LastDateSent;
				IsConsumed = typedSource.IsConsumed;
				IsRead = typedSource.IsRead;
				IsArchived = typedSource.IsArchived;
				SendTool = typedSource.SendTool;
				SendMethod = typedSource.SendMethod;
				UsesStreamIDs = typedSource.UsesStreamIDs;
				Uri = typedSource.Uri;
				Recipients = typedSource.Recipients;
				Zone = typedSource.Zone;
				Signature = typedSource.Signature;
				HasRemoteChildren = typedSource.HasRemoteChildren;
				Redirection = typedSource.Redirection;
				ShareSubType = typedSource.ShareSubType;
				ShareItemHistory = typedSource.ShareItemHistory;
				Settings = typedSource.Settings;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("AliasID", out token) && token.Type != JTokenType.Null)
				{
					AliasID = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ShareType", out token) && token.Type != JTokenType.Null)
				{
					ShareType = (SafeEnum<ShareType>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<ShareType>));
				}
				if(source.TryGetProperty("Title", out token) && token.Type != JTokenType.Null)
				{
					Title = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("HasSentMessage", out token) && token.Type != JTokenType.Null)
				{
					HasSentMessage = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("SentMessageTitle", out token) && token.Type != JTokenType.Null)
				{
					SentMessageTitle = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("RequireLogin", out token) && token.Type != JTokenType.Null)
				{
					RequireLogin = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("RequireUserInfo", out token) && token.Type != JTokenType.Null)
				{
					RequireUserInfo = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Parent", out token) && token.Type != JTokenType.Null)
				{
					Parent = (Item)serializer.Deserialize(token.CreateReader(), typeof(Item));
				}
				if(source.TryGetProperty("Creator", out token) && token.Type != JTokenType.Null)
				{
					Creator = (User)serializer.Deserialize(token.CreateReader(), typeof(User));
				}
				if(source.TryGetProperty("User", out token) && token.Type != JTokenType.Null)
				{
					User = (User)serializer.Deserialize(token.CreateReader(), typeof(User));
				}
				if(source.TryGetProperty("Items", out token) && token.Type != JTokenType.Null)
				{
					Items = (IEnumerable<Item>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<Item>));
				}
				if(source.TryGetProperty("CreationDate", out token) && token.Type != JTokenType.Null)
				{
					CreationDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("ExpirationDate", out token) && token.Type != JTokenType.Null)
				{
					ExpirationDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("MaxDownloads", out token) && token.Type != JTokenType.Null)
				{
					MaxDownloads = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("TotalDownloads", out token) && token.Type != JTokenType.Null)
				{
					TotalDownloads = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("IsViewOnly", out token) && token.Type != JTokenType.Null)
				{
					IsViewOnly = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("TrackUntilDate", out token) && token.Type != JTokenType.Null)
				{
					TrackUntilDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("SendFrequency", out token) && token.Type != JTokenType.Null)
				{
					SendFrequency = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("SendInterval", out token) && token.Type != JTokenType.Null)
				{
					SendInterval = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("LastDateSent", out token) && token.Type != JTokenType.Null)
				{
					LastDateSent = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("IsConsumed", out token) && token.Type != JTokenType.Null)
				{
					IsConsumed = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsRead", out token) && token.Type != JTokenType.Null)
				{
					IsRead = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsArchived", out token) && token.Type != JTokenType.Null)
				{
					IsArchived = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("SendTool", out token) && token.Type != JTokenType.Null)
				{
					SendTool = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("SendMethod", out token) && token.Type != JTokenType.Null)
				{
					SendMethod = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("UsesStreamIDs", out token) && token.Type != JTokenType.Null)
				{
					UsesStreamIDs = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Uri", out token) && token.Type != JTokenType.Null)
				{
					Uri = (Uri)serializer.Deserialize(token.CreateReader(), typeof(Uri));
				}
				if(source.TryGetProperty("Recipients", out token) && token.Type != JTokenType.Null)
				{
					Recipients = (IEnumerable<ShareAlias>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<ShareAlias>));
				}
				if(source.TryGetProperty("Zone", out token) && token.Type != JTokenType.Null)
				{
					Zone = (Zone)serializer.Deserialize(token.CreateReader(), typeof(Zone));
				}
				if(source.TryGetProperty("Signature", out token) && token.Type != JTokenType.Null)
				{
					Signature = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("HasRemoteChildren", out token) && token.Type != JTokenType.Null)
				{
					HasRemoteChildren = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Redirection", out token) && token.Type != JTokenType.Null)
				{
					Redirection = (Redirection)serializer.Deserialize(token.CreateReader(), typeof(Redirection));
				}
				if(source.TryGetProperty("ShareSubType", out token) && token.Type != JTokenType.Null)
				{
					ShareSubType = (SafeEnum<ShareSubType>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<ShareSubType>));
				}
				if(source.TryGetProperty("ShareItemHistory", out token) && token.Type != JTokenType.Null)
				{
					ShareItemHistory = (IEnumerable<ShareItemHistory>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<ShareItemHistory>));
				}
				if(source.TryGetProperty("Settings", out token) && token.Type != JTokenType.Null)
				{
					Settings = (ShareSettings)serializer.Deserialize(token.CreateReader(), typeof(ShareSettings));
				}
			}
		}
	}
}