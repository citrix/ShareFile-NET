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
	public class OutlookInformation : ODataObject 
	{

		public OutlookInformationOptionString DownloadInfoLevel { get; set; }

		public OutlookInformationOptionString UploadInfoLevel { get; set; }

		public OutlookInformationOptionBool NotifyOnDownload { get; set; }

		public OutlookInformationOptionBool NotifyOnUpload { get; set; }

		public OutlookInformationOptionInt MaxDownloads { get; set; }

		public OutlookInformationOptionString LinkExpiration { get; set; }

		public OutlookInformationOptionString RequestText { get; set; }

		public OutlookInformationOptionString SendText { get; set; }

		public OutlookInformationOptionString BannerHTML { get; set; }

		public OutlookInformationOptionBool UseBanner { get; set; }

		public OutlookInformationOptionBool AutoConvert { get; set; }

		public OutlookInformationOptionInt ConvertAbove { get; set; }

		public OutlookInformationOptionBool AttachPaperclip { get; set; }

		public OutlookInformationOptionString EncryptedEmailRecipientVerification { get; set; }

		public OutlookInformationOptionString EncryptedEmailExpiration { get; set; }

		public OutlookInformationOptionBool EncryptedEmailNotifyOnRead { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as OutlookInformation;
			if(typedSource != null)
			{
				DownloadInfoLevel = typedSource.DownloadInfoLevel;
				UploadInfoLevel = typedSource.UploadInfoLevel;
				NotifyOnDownload = typedSource.NotifyOnDownload;
				NotifyOnUpload = typedSource.NotifyOnUpload;
				MaxDownloads = typedSource.MaxDownloads;
				LinkExpiration = typedSource.LinkExpiration;
				RequestText = typedSource.RequestText;
				SendText = typedSource.SendText;
				BannerHTML = typedSource.BannerHTML;
				UseBanner = typedSource.UseBanner;
				AutoConvert = typedSource.AutoConvert;
				ConvertAbove = typedSource.ConvertAbove;
				AttachPaperclip = typedSource.AttachPaperclip;
				EncryptedEmailRecipientVerification = typedSource.EncryptedEmailRecipientVerification;
				EncryptedEmailExpiration = typedSource.EncryptedEmailExpiration;
				EncryptedEmailNotifyOnRead = typedSource.EncryptedEmailNotifyOnRead;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("DownloadInfoLevel", out token) && token.Type != JTokenType.Null)
				{
					DownloadInfoLevel = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("UploadInfoLevel", out token) && token.Type != JTokenType.Null)
				{
					UploadInfoLevel = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("NotifyOnDownload", out token) && token.Type != JTokenType.Null)
				{
					NotifyOnDownload = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
				if(source.TryGetProperty("NotifyOnUpload", out token) && token.Type != JTokenType.Null)
				{
					NotifyOnUpload = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
				if(source.TryGetProperty("MaxDownloads", out token) && token.Type != JTokenType.Null)
				{
					MaxDownloads = (OutlookInformationOptionInt)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionInt));
				}
				if(source.TryGetProperty("LinkExpiration", out token) && token.Type != JTokenType.Null)
				{
					LinkExpiration = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("RequestText", out token) && token.Type != JTokenType.Null)
				{
					RequestText = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("SendText", out token) && token.Type != JTokenType.Null)
				{
					SendText = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("BannerHTML", out token) && token.Type != JTokenType.Null)
				{
					BannerHTML = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("UseBanner", out token) && token.Type != JTokenType.Null)
				{
					UseBanner = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
				if(source.TryGetProperty("AutoConvert", out token) && token.Type != JTokenType.Null)
				{
					AutoConvert = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
				if(source.TryGetProperty("ConvertAbove", out token) && token.Type != JTokenType.Null)
				{
					ConvertAbove = (OutlookInformationOptionInt)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionInt));
				}
				if(source.TryGetProperty("AttachPaperclip", out token) && token.Type != JTokenType.Null)
				{
					AttachPaperclip = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
				if(source.TryGetProperty("EncryptedEmailRecipientVerification", out token) && token.Type != JTokenType.Null)
				{
					EncryptedEmailRecipientVerification = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("EncryptedEmailExpiration", out token) && token.Type != JTokenType.Null)
				{
					EncryptedEmailExpiration = (OutlookInformationOptionString)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionString));
				}
				if(source.TryGetProperty("EncryptedEmailNotifyOnRead", out token) && token.Type != JTokenType.Null)
				{
					EncryptedEmailNotifyOnRead = (OutlookInformationOptionBool)serializer.Deserialize(token.CreateReader(), typeof(OutlookInformationOptionBool));
				}
			}
		}
	}
}