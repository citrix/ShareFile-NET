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
	public class DiskSpace : ODataObject 
	{

		public int? Max { get; set; }

		public int? Used { get; set; }

		public int? Free { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as DiskSpace;
			if(typedSource != null)
			{
				Max = typedSource.Max;
				Used = typedSource.Used;
				Free = typedSource.Free;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("Max", out token) && token.Type != JTokenType.Null)
				{
					Max = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("Used", out token) && token.Type != JTokenType.Null)
				{
					Used = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("Free", out token) && token.Type != JTokenType.Null)
				{
					Free = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
			}
		}
	}
}