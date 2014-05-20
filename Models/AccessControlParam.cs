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
	public class AccessControlParam : ODataObject 
	{
		/// <summary>
		/// AccessControl.Item is inherited from AccessControlsBulkParams and cannot be specified here
		/// </summary>
		public AccessControl AccessControl { get; set; }

		/// <summary>
		/// Defines whether this principal should receieve a notice on the permission grant.
		/// If not specified it is inherited AccessControlsBulkParams
		/// </summary>
		public bool? NotifyUser { get; set; }

		/// <summary>
		/// Custom notification message, if any
		/// If not specified it is inherited AccessControlsBulkParams
		/// </summary>
		public string NotifyMessage { get; set; }

		/// <summary>
		/// Defines whether this ACL change should be applied recursively
		/// </summary>
		public bool? Recursive { get; set; }

	}
}