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

namespace ShareFile.Api.Models 
{
	public class SSOInfo : ODataObject 
	{
		public IEnumerable<SSOInfoEntry> Info { get; set; }

		public string AppControlPlane { get; set; }

		public string ApiControlPlane { get; set; }

	}
}