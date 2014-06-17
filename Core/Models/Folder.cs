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
	public class Folder : Item 
	{

		/// <summary>
		/// Number of Items defined under this Folder, including sub-folder counts.
		/// </summary>
		public int? FileCount { get; set; }

		/// <summary>
		/// List of Children defined under this folder.
		/// </summary>
		public IEnumerable<Item> Children { get; set; }

		/// <summary>
		/// Defines whether the request to retreive Children is to be navigated to a remote endpoint.
		/// </summary>
		public bool? HasRemoteChildren { get; set; }

		/// <summary>
		/// Effective Access Control Permissions for this Folder
		/// </summary>
		public ItemInfo Info { get; set; }

		/// <summary>
		/// Redirection endpoint for this Item.
		/// </summary>
		public Redirection Redirection { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as Folder;
			if(typedSource != null)
			{
				FileCount = typedSource.FileCount;
				Children = typedSource.Children;
				HasRemoteChildren = typedSource.HasRemoteChildren;
				Info = typedSource.Info;
				Redirection = typedSource.Redirection;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("FileCount", out token) && token.Type != JTokenType.Null)
				{
					FileCount = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("Children", out token) && token.Type != JTokenType.Null)
				{
					Children = (IEnumerable<Item>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<Item>));
				}
				if(source.TryGetProperty("HasRemoteChildren", out token) && token.Type != JTokenType.Null)
				{
					HasRemoteChildren = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Info", out token) && token.Type != JTokenType.Null)
				{
					Info = (ItemInfo)serializer.Deserialize(token.CreateReader(), typeof(ItemInfo));
				}
				if(source.TryGetProperty("Redirection", out token) && token.Type != JTokenType.Null)
				{
					Redirection = (Redirection)serializer.Deserialize(token.CreateReader(), typeof(Redirection));
				}
			}
		}
	}
}