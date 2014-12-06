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
	public class Notification : ODataObject 
	{

		public string NotificationType { get; set; }

		public string EventID { get; set; }

		public string SenderID { get; set; }

		public string RecipientID { get; set; }

		public string FromName { get; set; }

		public string FromEmail { get; set; }

		public string ReplyTo { get; set; }

		public string To { get; set; }

		public string CC { get; set; }

		public string BCC { get; set; }

		public string Subject { get; set; }

		public string Message { get; set; }

		public string PlainTextMessage { get; set; }

		public DateTime? DateSent { get; set; }

		public string Status { get; set; }

		public bool? IsImportant { get; set; }

		public bool? ReadReceipt { get; set; }

		public IEnumerable<string> AttachmentPaths { get; set; }

		public bool ForceEmailFromShareFile { get; set; }

		public IEnumerable<string> MergeNames { get; set; }

		public IEnumerable<object> MergeValues { get; set; }

		public bool? RecordSend { get; set; }

		public bool? IsModelBased { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as Notification;
			if(typedSource != null)
			{
				NotificationType = typedSource.NotificationType;
				EventID = typedSource.EventID;
				SenderID = typedSource.SenderID;
				RecipientID = typedSource.RecipientID;
				FromName = typedSource.FromName;
				FromEmail = typedSource.FromEmail;
				ReplyTo = typedSource.ReplyTo;
				To = typedSource.To;
				CC = typedSource.CC;
				BCC = typedSource.BCC;
				Subject = typedSource.Subject;
				Message = typedSource.Message;
				PlainTextMessage = typedSource.PlainTextMessage;
				DateSent = typedSource.DateSent;
				Status = typedSource.Status;
				IsImportant = typedSource.IsImportant;
				ReadReceipt = typedSource.ReadReceipt;
				AttachmentPaths = typedSource.AttachmentPaths;
				ForceEmailFromShareFile = typedSource.ForceEmailFromShareFile;
				MergeNames = typedSource.MergeNames;
				MergeValues = typedSource.MergeValues;
				RecordSend = typedSource.RecordSend;
				IsModelBased = typedSource.IsModelBased;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("NotificationType", out token) && token.Type != JTokenType.Null)
				{
					NotificationType = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("EventID", out token) && token.Type != JTokenType.Null)
				{
					EventID = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("SenderID", out token) && token.Type != JTokenType.Null)
				{
					SenderID = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("RecipientID", out token) && token.Type != JTokenType.Null)
				{
					RecipientID = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("FromName", out token) && token.Type != JTokenType.Null)
				{
					FromName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("FromEmail", out token) && token.Type != JTokenType.Null)
				{
					FromEmail = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ReplyTo", out token) && token.Type != JTokenType.Null)
				{
					ReplyTo = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("To", out token) && token.Type != JTokenType.Null)
				{
					To = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CC", out token) && token.Type != JTokenType.Null)
				{
					CC = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BCC", out token) && token.Type != JTokenType.Null)
				{
					BCC = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Subject", out token) && token.Type != JTokenType.Null)
				{
					Subject = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Message", out token) && token.Type != JTokenType.Null)
				{
					Message = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("PlainTextMessage", out token) && token.Type != JTokenType.Null)
				{
					PlainTextMessage = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("DateSent", out token) && token.Type != JTokenType.Null)
				{
					DateSent = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("Status", out token) && token.Type != JTokenType.Null)
				{
					Status = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("IsImportant", out token) && token.Type != JTokenType.Null)
				{
					IsImportant = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("ReadReceipt", out token) && token.Type != JTokenType.Null)
				{
					ReadReceipt = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("AttachmentPaths", out token) && token.Type != JTokenType.Null)
				{
					AttachmentPaths = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("ForceEmailFromShareFile", out token) && token.Type != JTokenType.Null)
				{
					ForceEmailFromShareFile = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("MergeNames", out token) && token.Type != JTokenType.Null)
				{
					MergeNames = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("MergeValues", out token) && token.Type != JTokenType.Null)
				{
					MergeValues = (IEnumerable<object>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<object>));
				}
				if(source.TryGetProperty("RecordSend", out token) && token.Type != JTokenType.Null)
				{
					RecordSend = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsModelBased", out token) && token.Type != JTokenType.Null)
				{
					IsModelBased = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
			}
		}
	}
}