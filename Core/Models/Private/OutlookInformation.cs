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
#if ShareFile
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

	}
#endif
}