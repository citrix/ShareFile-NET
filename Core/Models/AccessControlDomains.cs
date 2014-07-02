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
	public class AccessControlDomains : ODataObject 
	{

		/// <summary>
		/// Specifies that the list is interpreted as a list of allowed or disallowed domains
		/// </summary>
		public SafeEnum<AccessControlFilter> AccessControlType { get; set; }

		/// <summary>
		/// A list of domain names
		/// </summary>
		public IEnumerable<string> Domains { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as AccessControlDomains;
			if(typedSource != null)
			{
				AccessControlType = typedSource.AccessControlType;
				Domains = typedSource.Domains;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("AccessControlType", out token) && token.Type != JTokenType.Null)
				{
					AccessControlType = (SafeEnum<AccessControlFilter>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<AccessControlFilter>));
				}
				if(source.TryGetProperty("Domains", out token) && token.Type != JTokenType.Null)
				{
					Domains = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
			}
		}
	}
}