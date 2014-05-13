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
	public class AccessControlsBulkParams : ODataObject 
	{
		/// <summary>
		/// Defines whether the principal should receieve a notice on the permission grant.
		/// If an AccessControlParam doesn't specify the property it is inherited from here.
		/// </summary>
		public bool? NotifyUser { get; set; }

		/// <summary>
		/// Custom notification message, if any
		/// If an AccessControlParam doesn't specify the property it is inherited from here.
		/// </summary>
		public string NotifyMessage { get; set; }

		/// <summary>
		/// AccessControlParams
		/// </summary>
		public IEnumerable<AccessControlParam> AccessControlParams { get; set; }

	}
}