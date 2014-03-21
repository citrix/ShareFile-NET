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
using System.IO;
using ShareFile.Api.Models;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Requests;

namespace ShareFile.Api.Client.Entities
{
	public class CapabilitiesEntity : EntityBase
	{
		public CapabilitiesEntity(IShareFileClient client)
			: base (client, "Capabilities")
		{

		}

		/// <summary>
		/// Get List of Capabilities
		/// </summary>
		/// <remarks>
		/// Retrieves the capability list of a given provider.
		/// The URL for ShareFile API is of the form Domain/Provider/Version/EntityThe Domain is the server presenting the provider - typically sharefile.com,
		/// but can be any other when using Storage Zones. The Provider represent the kind of data storage connected by the API. Examples
		/// are 'sf' for ShareFile; 'cifs' for CIFS; and 'sp' for SharePoint. Other providers
		/// may be created, clients must not assume any particular string.Version specifies the API version - currently at V3. Any backward incompatible
		/// changes will be performed on a different version identifier, to avoid breaking
		/// existing clients.The Capability document is used to indicate to clients that certain features
		/// are not available on a given provider - allowing the client to suppress UX controls
		/// and avoid "Not Implemented" exceptions to the end-user.
		/// </remarks>
		public IQuery<ODataFeed<Capability>> Get()
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<Capability>>(Client);
			sfApiQuery.From("Capabilities");
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

	}
}
