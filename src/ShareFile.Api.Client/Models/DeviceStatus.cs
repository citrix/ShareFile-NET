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
	/// Device Status
	/// </summary>
	public class DeviceStatus : ODataObject 
	{
		/// <summary>
		/// Locked users
		/// </summary>
		public IEnumerable<User> LockedUsers { get; set; }
		/// <summary>
		/// Users to wipe
		/// </summary>
		public IEnumerable<DeviceUserWipe> UsersToWipe { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as DeviceStatus;
			if(typedSource != null)
			{
				LockedUsers = typedSource.LockedUsers;
				UsersToWipe = typedSource.UsersToWipe;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("LockedUsers", out token) && token.Type != JTokenType.Null)
				{
					LockedUsers = (IEnumerable<User>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<User>));
				}
				if(source.TryGetProperty("UsersToWipe", out token) && token.Type != JTokenType.Null)
				{
					UsersToWipe = (IEnumerable<DeviceUserWipe>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<DeviceUserWipe>));
				}
			}
		}
	}
}