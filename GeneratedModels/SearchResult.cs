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

namespace ShareFile.Api.Models 
{
	public class SearchResult : ODataObject 
	{
		public int Rank { get; set; }

		public decimal Score { get; set; }

		public string ItemID { get; set; }

		public string ParentID { get; set; }

		public string ParentName { get; set; }

		public string ItemType { get; set; }

		public string FileName { get; set; }

		public string DisplayName { get; set; }

		public long Size { get; set; }

		public string CreatorID { get; set; }

		public string CreatorName { get; set; }

		public string CreatorFirstName { get; set; }

		public string CreatorLastName { get; set; }

		public string CreationDate { get; set; }

		public string Details { get; set; }

		public string MD5 { get; set; }

		public int PreviewStatus { get; set; }

		public int VirusStatus { get; set; }

		public string Url { get; set; }

		public bool CanDownload { get; set; }

		public bool CanView { get; set; }

		public string ParentSemanticPath { get; set; }

		public string StreamID { get; set; }

		public string AccountID { get; set; }

	}
}