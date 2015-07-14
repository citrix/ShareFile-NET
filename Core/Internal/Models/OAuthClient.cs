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
	public class OAuthClient : ODataObject 
	{

		public string ClientSecret { get; set; }

		public string AccountID { get; set; }

		public string Image { get; set; }

		public string ImageSmall { get; set; }

		public SafeEnum<OAuthState> State { get; set; }

		public string Name { get; set; }

		public string CompanyName { get; set; }

		public string ToolUrl { get; set; }

		public DateTime? CreationDate { get; set; }

		public DateTime? LastModifiedDate { get; set; }

		public bool? ServerFlow { get; set; }

		public bool? ClientFlow { get; set; }

		public bool? UsernamePasswordFlow { get; set; }

		public bool? SamlFlow { get; set; }

		public bool? IsQA { get; set; }

		public bool? Impersonation { get; set; }

		public bool? DeviceRegistration { get; set; }

		public bool? CanCreateFreemiumAccount { get; set; }

		public bool? IsInternalAdmin { get; set; }

		public SafeEnum<OAuthClientPermissions> AccessFilesFolders { get; set; }

		public SafeEnum<OAuthClientPermissions> ModifyFilesFolders { get; set; }

		public SafeEnum<OAuthClientPermissions> AdminUsers { get; set; }

		public SafeEnum<OAuthClientPermissions> AdminAccounts { get; set; }

		public SafeEnum<OAuthClientPermissions> ChangeMySettings { get; set; }

		public SafeEnum<OAuthClientPermissions> WebAppLogin { get; set; }

		public SafeEnum<AppCodes> AppCode { get; set; }

		public IEnumerable<string> RedirectUrls { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as OAuthClient;
			if(typedSource != null)
			{
				ClientSecret = typedSource.ClientSecret;
				AccountID = typedSource.AccountID;
				Image = typedSource.Image;
				ImageSmall = typedSource.ImageSmall;
				State = typedSource.State;
				Name = typedSource.Name;
				CompanyName = typedSource.CompanyName;
				ToolUrl = typedSource.ToolUrl;
				CreationDate = typedSource.CreationDate;
				LastModifiedDate = typedSource.LastModifiedDate;
				ServerFlow = typedSource.ServerFlow;
				ClientFlow = typedSource.ClientFlow;
				UsernamePasswordFlow = typedSource.UsernamePasswordFlow;
				SamlFlow = typedSource.SamlFlow;
				IsQA = typedSource.IsQA;
				Impersonation = typedSource.Impersonation;
				DeviceRegistration = typedSource.DeviceRegistration;
				CanCreateFreemiumAccount = typedSource.CanCreateFreemiumAccount;
				IsInternalAdmin = typedSource.IsInternalAdmin;
				AccessFilesFolders = typedSource.AccessFilesFolders;
				ModifyFilesFolders = typedSource.ModifyFilesFolders;
				AdminUsers = typedSource.AdminUsers;
				AdminAccounts = typedSource.AdminAccounts;
				ChangeMySettings = typedSource.ChangeMySettings;
				WebAppLogin = typedSource.WebAppLogin;
				AppCode = typedSource.AppCode;
				RedirectUrls = typedSource.RedirectUrls;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("ClientSecret", out token) && token.Type != JTokenType.Null)
				{
					ClientSecret = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("AccountID", out token) && token.Type != JTokenType.Null)
				{
					AccountID = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Image", out token) && token.Type != JTokenType.Null)
				{
					Image = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ImageSmall", out token) && token.Type != JTokenType.Null)
				{
					ImageSmall = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("State", out token) && token.Type != JTokenType.Null)
				{
					State = (SafeEnum<OAuthState>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthState>));
				}
				if(source.TryGetProperty("Name", out token) && token.Type != JTokenType.Null)
				{
					Name = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CompanyName", out token) && token.Type != JTokenType.Null)
				{
					CompanyName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ToolUrl", out token) && token.Type != JTokenType.Null)
				{
					ToolUrl = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreationDate", out token) && token.Type != JTokenType.Null)
				{
					CreationDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("LastModifiedDate", out token) && token.Type != JTokenType.Null)
				{
					LastModifiedDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("ServerFlow", out token) && token.Type != JTokenType.Null)
				{
					ServerFlow = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("ClientFlow", out token) && token.Type != JTokenType.Null)
				{
					ClientFlow = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("UsernamePasswordFlow", out token) && token.Type != JTokenType.Null)
				{
					UsernamePasswordFlow = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("SamlFlow", out token) && token.Type != JTokenType.Null)
				{
					SamlFlow = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsQA", out token) && token.Type != JTokenType.Null)
				{
					IsQA = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Impersonation", out token) && token.Type != JTokenType.Null)
				{
					Impersonation = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("DeviceRegistration", out token) && token.Type != JTokenType.Null)
				{
					DeviceRegistration = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("CanCreateFreemiumAccount", out token) && token.Type != JTokenType.Null)
				{
					CanCreateFreemiumAccount = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsInternalAdmin", out token) && token.Type != JTokenType.Null)
				{
					IsInternalAdmin = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("AccessFilesFolders", out token) && token.Type != JTokenType.Null)
				{
					AccessFilesFolders = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("ModifyFilesFolders", out token) && token.Type != JTokenType.Null)
				{
					ModifyFilesFolders = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("AdminUsers", out token) && token.Type != JTokenType.Null)
				{
					AdminUsers = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("AdminAccounts", out token) && token.Type != JTokenType.Null)
				{
					AdminAccounts = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("ChangeMySettings", out token) && token.Type != JTokenType.Null)
				{
					ChangeMySettings = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("WebAppLogin", out token) && token.Type != JTokenType.Null)
				{
					WebAppLogin = (SafeEnum<OAuthClientPermissions>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<OAuthClientPermissions>));
				}
				if(source.TryGetProperty("AppCode", out token) && token.Type != JTokenType.Null)
				{
					AppCode = (SafeEnum<AppCodes>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<AppCodes>));
				}
				if(source.TryGetProperty("RedirectUrls", out token) && token.Type != JTokenType.Null)
				{
					RedirectUrls = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
			}
		}
	}
#endif
}