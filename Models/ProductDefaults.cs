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

namespace ShareFile.Api.Models 
{
	public class ProductDefaults : ODataObject 
	{
		public string ProductName { get; set; }

		public string DefaultWindowTitle { get; set; }

		public string TopLevelDomain { get; set; }

		public string APITopLevelDomain { get; set; }

		public string DefaultApiVersion { get; set; }

		public string DefaultSmtpServer { get; set; }

		public string NoReplyUserName { get; set; }

		public string NoReplyUserEmail { get; set; }

		public string SupportUserName { get; set; }

		public string SupportUserEmail { get; set; }

		public string DefaultEmailFooter { get; set; }

		public string DefaultEmailFooterHtml { get; set; }

		public string DefaultEmailFooterPlaintext { get; set; }

		public string DefaultEmailOverview { get; set; }

		public string SupportUserNotificationEmail { get; set; }

		public string SystemType { get; set; }

	}
}