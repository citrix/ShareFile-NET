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
	public class UserUsage : ODataObject 
	{

		public int? EmployeeMax { get; set; }

		public int? EmployeeCount { get; set; }

		public int? ClientCount { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as UserUsage;
			if(typedSource != null)
			{
				EmployeeMax = typedSource.EmployeeMax;
				EmployeeCount = typedSource.EmployeeCount;
				ClientCount = typedSource.ClientCount;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("EmployeeMax", out token) && token.Type != JTokenType.Null)
				{
					EmployeeMax = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("EmployeeCount", out token) && token.Type != JTokenType.Null)
				{
					EmployeeCount = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("ClientCount", out token) && token.Type != JTokenType.Null)
				{
					ClientCount = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
			}
		}
	}
}