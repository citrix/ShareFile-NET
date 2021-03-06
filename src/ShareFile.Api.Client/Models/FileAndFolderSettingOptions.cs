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
	/// Represents the options applicable to a File and Folder Setting definition for a File and Folder Policy.
	/// This setting can either be locked to users with a default value or allow users to select from a range.
	/// </summary>
	public class FileAndFolderSettingOptions : ODataObject 
	{
		/// <summary>
		/// Represents the setting option type selected: Locked or Range
		/// </summary>
		public bool? LockedSelection { get; set; }
		/// <summary>
		/// Default value for the locked setting option
		/// </summary>
		public int? LockedDefault { get; set; }
		/// <summary>
		/// Maximum value for the range setting option
		/// </summary>
		public int? RangeMax { get; set; }
		/// <summary>
		/// Minimum value for the range setting option
		/// </summary>
		public int? RangeMin { get; set; }
		/// <summary>
		/// Default value for the range setting option
		/// </summary>
		public int? RangeDefault { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as FileAndFolderSettingOptions;
			if(typedSource != null)
			{
				LockedSelection = typedSource.LockedSelection;
				LockedDefault = typedSource.LockedDefault;
				RangeMax = typedSource.RangeMax;
				RangeMin = typedSource.RangeMin;
				RangeDefault = typedSource.RangeDefault;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("LockedSelection", out token) && token.Type != JTokenType.Null)
				{
					LockedSelection = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("LockedDefault", out token) && token.Type != JTokenType.Null)
				{
					LockedDefault = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("RangeMax", out token) && token.Type != JTokenType.Null)
				{
					RangeMax = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("RangeMin", out token) && token.Type != JTokenType.Null)
				{
					RangeMin = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("RangeDefault", out token) && token.Type != JTokenType.Null)
				{
					RangeDefault = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
			}
		}
	}
}