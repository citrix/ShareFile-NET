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
	public class EnsSubscriptionRequest : ODataObject 
	{

		public string EnsServerUrl { get; set; }

		public string ClientId { get; set; }

		public ODataObject Entity { get; set; }

		public SafeEnum<EnsEventType> EventTypes { get; set; }

		public bool IncludeProgeny { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as EnsSubscriptionRequest;
			if(typedSource != null)
			{
				EnsServerUrl = typedSource.EnsServerUrl;
				ClientId = typedSource.ClientId;
				Entity = typedSource.Entity;
				EventTypes = typedSource.EventTypes;
				IncludeProgeny = typedSource.IncludeProgeny;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("EnsServerUrl", out token) && token.Type != JTokenType.Null)
				{
					EnsServerUrl = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ClientId", out token) && token.Type != JTokenType.Null)
				{
					ClientId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Entity", out token) && token.Type != JTokenType.Null)
				{
					Entity = (ODataObject)serializer.Deserialize(token.CreateReader(), typeof(ODataObject));
				}
				if(source.TryGetProperty("EventTypes", out token) && token.Type != JTokenType.Null)
				{
					EventTypes = (SafeEnum<EnsEventType>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<EnsEventType>));
				}
				if(source.TryGetProperty("IncludeProgeny", out token) && token.Type != JTokenType.Null)
				{
					IncludeProgeny = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
			}
		}
	}
}