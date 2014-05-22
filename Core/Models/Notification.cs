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

	}
}