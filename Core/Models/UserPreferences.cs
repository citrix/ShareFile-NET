// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     
//	   Copyright (c) 2014 Citrix ShareFile. All rights reserved.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Models 
{
	public class UserPreferences : ODataObject 
	{

		public bool? EnableFlashUpload { get; set; }

		public bool? EnableJavaUpload { get; set; }

		public bool? EnableJavaDownload { get; set; }

		public bool? RememberCustomMessages { get; set; }

		public bool? RequireLoginByDefault { get; set; }

		public bool? NotifyOnUploadByDefault { get; set; }

		public bool? NotifyOnDownloadByDefault { get; set; }

		public bool? CanResetPassword { get; set; }

		public bool? CanViewMySettings { get; set; }

		public bool? IsSharedUserAccount { get; set; }

		public string TimeZone { get; set; }

		public string DaylightSavingMode { get; set; }

		public int? TimeZoneOffset { get; set; }

		public int? TimeZoneOffsetMins { get; set; }

		public bool? DisplayUserMessage { get; set; }

		public string UserMessageCode { get; set; }

		public int? NotificationInterval { get; set; }

		public int? ShowTutorial { get; set; }

		public int? EnableToolOverride { get; set; }

		public bool? IsResetSecurityQuestionRequired { get; set; }

		public string TimeFormat { get; set; }

		public string LongTimeFormat { get; set; }

		public string DateFormat { get; set; }

		public bool? EnableShareConnect { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as UserPreferences;
			if(typedSource != null)
			{
				EnableFlashUpload = typedSource.EnableFlashUpload;
				EnableJavaUpload = typedSource.EnableJavaUpload;
				EnableJavaDownload = typedSource.EnableJavaDownload;
				RememberCustomMessages = typedSource.RememberCustomMessages;
				RequireLoginByDefault = typedSource.RequireLoginByDefault;
				NotifyOnUploadByDefault = typedSource.NotifyOnUploadByDefault;
				NotifyOnDownloadByDefault = typedSource.NotifyOnDownloadByDefault;
				CanResetPassword = typedSource.CanResetPassword;
				CanViewMySettings = typedSource.CanViewMySettings;
				IsSharedUserAccount = typedSource.IsSharedUserAccount;
				TimeZone = typedSource.TimeZone;
				DaylightSavingMode = typedSource.DaylightSavingMode;
				TimeZoneOffset = typedSource.TimeZoneOffset;
				TimeZoneOffsetMins = typedSource.TimeZoneOffsetMins;
				DisplayUserMessage = typedSource.DisplayUserMessage;
				UserMessageCode = typedSource.UserMessageCode;
				NotificationInterval = typedSource.NotificationInterval;
				ShowTutorial = typedSource.ShowTutorial;
				EnableToolOverride = typedSource.EnableToolOverride;
				IsResetSecurityQuestionRequired = typedSource.IsResetSecurityQuestionRequired;
				TimeFormat = typedSource.TimeFormat;
				LongTimeFormat = typedSource.LongTimeFormat;
				DateFormat = typedSource.DateFormat;
				EnableShareConnect = typedSource.EnableShareConnect;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("EnableFlashUpload", out token) && token.Type != JTokenType.Null)
				{
					EnableFlashUpload = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("EnableJavaUpload", out token) && token.Type != JTokenType.Null)
				{
					EnableJavaUpload = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("EnableJavaDownload", out token) && token.Type != JTokenType.Null)
				{
					EnableJavaDownload = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("RememberCustomMessages", out token) && token.Type != JTokenType.Null)
				{
					RememberCustomMessages = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("RequireLoginByDefault", out token) && token.Type != JTokenType.Null)
				{
					RequireLoginByDefault = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("NotifyOnUploadByDefault", out token) && token.Type != JTokenType.Null)
				{
					NotifyOnUploadByDefault = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("NotifyOnDownloadByDefault", out token) && token.Type != JTokenType.Null)
				{
					NotifyOnDownloadByDefault = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("CanResetPassword", out token) && token.Type != JTokenType.Null)
				{
					CanResetPassword = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("CanViewMySettings", out token) && token.Type != JTokenType.Null)
				{
					CanViewMySettings = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsSharedUserAccount", out token) && token.Type != JTokenType.Null)
				{
					IsSharedUserAccount = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("TimeZone", out token) && token.Type != JTokenType.Null)
				{
					TimeZone = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("DaylightSavingMode", out token) && token.Type != JTokenType.Null)
				{
					DaylightSavingMode = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("TimeZoneOffset", out token) && token.Type != JTokenType.Null)
				{
					TimeZoneOffset = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("TimeZoneOffsetMins", out token) && token.Type != JTokenType.Null)
				{
					TimeZoneOffsetMins = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("DisplayUserMessage", out token) && token.Type != JTokenType.Null)
				{
					DisplayUserMessage = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("UserMessageCode", out token) && token.Type != JTokenType.Null)
				{
					UserMessageCode = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("NotificationInterval", out token) && token.Type != JTokenType.Null)
				{
					NotificationInterval = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("ShowTutorial", out token) && token.Type != JTokenType.Null)
				{
					ShowTutorial = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("EnableToolOverride", out token) && token.Type != JTokenType.Null)
				{
					EnableToolOverride = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("IsResetSecurityQuestionRequired", out token) && token.Type != JTokenType.Null)
				{
					IsResetSecurityQuestionRequired = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("TimeFormat", out token) && token.Type != JTokenType.Null)
				{
					TimeFormat = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("LongTimeFormat", out token) && token.Type != JTokenType.Null)
				{
					LongTimeFormat = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("DateFormat", out token) && token.Type != JTokenType.Null)
				{
					DateFormat = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("EnableShareConnect", out token) && token.Type != JTokenType.Null)
				{
					EnableShareConnect = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
			}
		}
	}
}