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

namespace ShareFile.Api.Models 
{
	public class Device : ODataObject 
	{

		public SafeEnum<SFTool> Tool { get; set; }

		public string ToolRaw { get; set; }

		public User Owner { get; set; }

		public DateTime? Created { get; set; }

		public string ToolVersion { get; set; }

		public bool JustRegistered { get; set; }

	}
}