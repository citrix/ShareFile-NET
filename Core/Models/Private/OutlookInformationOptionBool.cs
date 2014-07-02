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
#if ShareFile
	public class OutlookInformationOptionBool : ODataObject 
	{

		public bool Locked { get; set; }

		public bool Value { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as OutlookInformationOptionBool;
			if(typedSource != null)
			{
				Locked = typedSource.Locked;
				Value = typedSource.Value;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("Locked", out token) && token.Type != JTokenType.Null)
				{
					Locked = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("Value", out token) && token.Type != JTokenType.Null)
				{
					Value = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
			}
		}
	}
#endif
}