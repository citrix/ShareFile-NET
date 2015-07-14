// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     
//	   Copyright (c) 2015 Citrix ShareFile. All rights reserved.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Models 
{
#if ShareFile
	public class PlanAddon : ODataObject 
	{

		public string PlanState { get; set; }

		public object PlanInfo { get; set; }

		public IEnumerable<string> AvailablePlans { get; set; }

		public IEnumerable<string> Features { get; set; }

		public string ProductCodeName { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as PlanAddon;
			if(typedSource != null)
			{
				PlanState = typedSource.PlanState;
				PlanInfo = typedSource.PlanInfo;
				AvailablePlans = typedSource.AvailablePlans;
				Features = typedSource.Features;
				ProductCodeName = typedSource.ProductCodeName;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("PlanState", out token) && token.Type != JTokenType.Null)
				{
					PlanState = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("PlanInfo", out token) && token.Type != JTokenType.Null)
				{
					PlanInfo = (object)serializer.Deserialize(token.CreateReader(), typeof(object));
				}
				if(source.TryGetProperty("AvailablePlans", out token) && token.Type != JTokenType.Null)
				{
					AvailablePlans = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("Features", out token) && token.Type != JTokenType.Null)
				{
					Features = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("ProductCodeName", out token) && token.Type != JTokenType.Null)
				{
					ProductCodeName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
			}
		}
	}
#endif
}