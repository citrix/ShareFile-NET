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
	public class StorageCenter : ODataObject 
	{

		public Zone Zone { get; set; }

		public string Address { get; set; }

		public string LocalAddress { get; set; }

		public string ExternalAddress { get; set; }

		public string DefaultExternalUrl { get; set; }

		public string HostName { get; set; }

		public SafeEnum<ZoneService> Services { get; set; }

		public string Version { get; set; }

		public bool? Enabled { get; set; }

		public DateTime? LastHeartBeat { get; set; }

		public string ExternalUrl { get; set; }

		public string MetadataProxyAddress { get; set; }

		public DateTime? LastPingBack { get; set; }

		public IEnumerable<Metadata> Metadata { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as StorageCenter;
			if(typedSource != null)
			{
				Zone = typedSource.Zone;
				Address = typedSource.Address;
				LocalAddress = typedSource.LocalAddress;
				ExternalAddress = typedSource.ExternalAddress;
				DefaultExternalUrl = typedSource.DefaultExternalUrl;
				HostName = typedSource.HostName;
				Services = typedSource.Services;
				Version = typedSource.Version;
				Enabled = typedSource.Enabled;
				LastHeartBeat = typedSource.LastHeartBeat;
				ExternalUrl = typedSource.ExternalUrl;
				MetadataProxyAddress = typedSource.MetadataProxyAddress;
				LastPingBack = typedSource.LastPingBack;
				Metadata = typedSource.Metadata;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("Zone", out token) && token.Type != JTokenType.Null)
				{
					Zone = (Zone)serializer.Deserialize(token.CreateReader(), typeof(Zone));
				}
				if(source.TryGetProperty("Address", out token) && token.Type != JTokenType.Null)
				{
					Address = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("LocalAddress", out token) && token.Type != JTokenType.Null)
				{
					LocalAddress = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ExternalAddress", out token) && token.Type != JTokenType.Null)
				{
					ExternalAddress = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("DefaultExternalUrl", out token) && token.Type != JTokenType.Null)
				{
					DefaultExternalUrl = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("HostName", out token) && token.Type != JTokenType.Null)
				{
					HostName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Services", out token) && token.Type != JTokenType.Null)
				{
					Services = (SafeEnum<ZoneService>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<ZoneService>));
				}
				if(source.TryGetProperty("Version", out token) && token.Type != JTokenType.Null)
				{
					Version = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Enabled", out token) && token.Type != JTokenType.Null)
				{
					Enabled = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("LastHeartBeat", out token) && token.Type != JTokenType.Null)
				{
					LastHeartBeat = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("ExternalUrl", out token) && token.Type != JTokenType.Null)
				{
					ExternalUrl = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("MetadataProxyAddress", out token) && token.Type != JTokenType.Null)
				{
					MetadataProxyAddress = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("LastPingBack", out token) && token.Type != JTokenType.Null)
				{
					LastPingBack = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("Metadata", out token) && token.Type != JTokenType.Null)
				{
					Metadata = (IEnumerable<Metadata>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<Metadata>));
				}
			}
		}
	}
}