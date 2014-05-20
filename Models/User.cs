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

namespace ShareFile.Api.Models 
{
	public class User : Principal 
	{
		public Account Account { get; set; }

		public string Company { get; set; }

		public int? TotalSharedFiles { get; set; }

		public int? Contacted { get; set; }

		/// <summary>
		/// The first and last name of the user
		/// </summary>
		public string FullName { get; set; }

		public string ReferredBy { get; set; }

		public IEnumerable<Notification> Notifications { get; set; }

		public Zone DefaultZone { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public DateTime? DateCreated { get; set; }

		public string FullNameShort { get; set; }

		public bool? IsConfirmed { get; set; }

		public string Password { get; set; }

		public UserPreferences Preferences { get; set; }

		public UserSecurity Security { get; set; }

		public IEnumerable<FavoriteFolder> FavoriteFolders { get; set; }

		public Folder HomeFolder { get; set; }

		public IEnumerable<DeviceUser> Devices { get; set; }

		public Folder VirtualRoot { get; set; }

		public IEnumerable<SafeEnum<UserRole>> Roles { get; set; }

		public UserInfo Info { get; set; }

	}
}