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
	public class ShareSendParams : ODataObject 
	{

		public IEnumerable<string> Items { get; set; }

		public IEnumerable<string> Emails { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }

		public bool CcSender { get; set; }

		public bool RequireLogin { get; set; }

		public bool SendAnon { get; set; }

		public bool RequireUserInfo { get; set; }

		public int ExpirationDays { get; set; }

		public bool NotifyOnDownload { get; set; }

		public bool IsViewOnly { get; set; }

		public int MaxDownloads { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as ShareSendParams;
			if(typedSource != null)
			{
				Items = typedSource.Items;
				Emails = typedSource.Emails;
				Subject = typedSource.Subject;
				Body = typedSource.Body;
				CcSender = typedSource.CcSender;
				RequireLogin = typedSource.RequireLogin;
				SendAnon = typedSource.SendAnon;
				RequireUserInfo = typedSource.RequireUserInfo;
				ExpirationDays = typedSource.ExpirationDays;
				NotifyOnDownload = typedSource.NotifyOnDownload;
				IsViewOnly = typedSource.IsViewOnly;
				MaxDownloads = typedSource.MaxDownloads;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("Items", out token) && token.Type != JTokenType.Null)
				{
					Items = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("Emails", out token) && token.Type != JTokenType.Null)
				{
					Emails = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("Subject", out token) && token.Type != JTokenType.Null)
				{
					Subject = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Body", out token) && token.Type != JTokenType.Null)
				{
					Body = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CcSender", out token) && token.Type != JTokenType.Null)
				{
					CcSender = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("RequireLogin", out token) && token.Type != JTokenType.Null)
				{
					RequireLogin = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("SendAnon", out token) && token.Type != JTokenType.Null)
				{
					SendAnon = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("RequireUserInfo", out token) && token.Type != JTokenType.Null)
				{
					RequireUserInfo = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("ExpirationDays", out token) && token.Type != JTokenType.Null)
				{
					ExpirationDays = (int)serializer.Deserialize(token.CreateReader(), typeof(int));
				}
				if(source.TryGetProperty("NotifyOnDownload", out token) && token.Type != JTokenType.Null)
				{
					NotifyOnDownload = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("IsViewOnly", out token) && token.Type != JTokenType.Null)
				{
					IsViewOnly = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("MaxDownloads", out token) && token.Type != JTokenType.Null)
				{
					MaxDownloads = (int)serializer.Deserialize(token.CreateReader(), typeof(int));
				}
			}
		}
	}
}