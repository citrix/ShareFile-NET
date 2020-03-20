// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     
//	   Copyright (c) 2018 Citrix ShareFile. All rights reserved.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Client.Models 
{
	public class AccountZoneUsage : ODataObject 
	{
		public long? TotalFileSizeBytes { get; set; }
		public Zone Zone { get; set; }
		public long? RootFolderCount { get; set; }
		public long? FileCount { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as AccountZoneUsage;
			if(typedSource != null)
			{
				TotalFileSizeBytes = typedSource.TotalFileSizeBytes;
				Zone = typedSource.Zone;
				RootFolderCount = typedSource.RootFolderCount;
				FileCount = typedSource.FileCount;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("TotalFileSizeBytes", out token) && token.Type != JTokenType.Null)
				{
					TotalFileSizeBytes = (long?)serializer.Deserialize(token.CreateReader(), typeof(long?));
				}
				if(source.TryGetProperty("Zone", out token) && token.Type != JTokenType.Null)
				{
					Zone = (Zone)serializer.Deserialize(token.CreateReader(), typeof(Zone));
				}
				if(source.TryGetProperty("RootFolderCount", out token) && token.Type != JTokenType.Null)
				{
					RootFolderCount = (long?)serializer.Deserialize(token.CreateReader(), typeof(long?));
				}
				if(source.TryGetProperty("FileCount", out token) && token.Type != JTokenType.Null)
				{
					FileCount = (long?)serializer.Deserialize(token.CreateReader(), typeof(long?));
				}
			}
		}
	}
}