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
	/// <summary>
	/// Information related to a specific ShareFile Client's webhook configuration
	/// </summary>
	public class WebhookClient : ODataObject 
	{
		/// <summary>
		/// ShareFile Client Identifier
		/// </summary>
		public string OAuthClientId { get; set; }
		/// <summary>
		/// The keys used to sign webhook payloads to verify ShareFile is the sender
		/// </summary>
		public WebhookSignatureKeys SignatureKeys { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as WebhookClient;
			if(typedSource != null)
			{
				OAuthClientId = typedSource.OAuthClientId;
				SignatureKeys = typedSource.SignatureKeys;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("OAuthClientId", out token) && token.Type != JTokenType.Null)
				{
					OAuthClientId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("SignatureKeys", out token) && token.Type != JTokenType.Null)
				{
					SignatureKeys = (WebhookSignatureKeys)serializer.Deserialize(token.CreateReader(), typeof(WebhookSignatureKeys));
				}
			}
		}
	}
}