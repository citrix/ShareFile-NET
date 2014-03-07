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
using ShareFile.Api.Models;
using ShareFile.Api.Client;

namespace ShareFile.Api.Client
{
#if ShareFile
	public class ZonesEntityInternal : ZonesEntity
	{
		/// <summary>
		/// Get List of Zones
		/// </summary>
		/// <remarks>
		/// Retrieve the list of Zones accessible to the authenticated user
		/// This method will concatenate the list of private zones in the user's account and the
		/// list of public zones accessible to this account. Any user can see the list of zones.
		/// </remarks>
		/// <param name="services"></param>
		/// <param name="includeDisabled"></param>
		/// <returns>
		/// The list of public and private zones accessible to this user
		/// </returns>
		public IQuery<ODataFeed<Zone>> Get(ZoneService services = StorageZone, bool includeDisabled = false)
		{
			IQuery<ODataFeed<Zone>> query = new IQuery<ODataFeed<Zone>>(Client);
			query.From("Zones");
			query.QueryString("services", services);
			query.QueryString("includeDisabled", includeDisabled);
			query.HttpMethod = "GET";
			return query;
		}

		/// <summary>
		/// Get List of Zones
		/// </summary>
		/// <remarks>
		/// Retrieve the list of Zones accessible to the authenticated user
		/// This method will concatenate the list of private zones in the user's account and the
		/// list of public zones accessible to this account. Any user can see the list of zones.
		/// </remarks>
		/// <param name="services"></param>
		/// <param name="includeDisabled"></param>
		/// <returns>
		/// The list of public and private zones accessible to this user
		/// </returns>
		public IQuery<Zone> Get(string id, bool secret = false)
		{
			IQuery<Zone> query = new IQuery<Zone>(Client);
			query.From("Zones");
			query.Ids(id);
			query.QueryString("secret", secret);
			query.HttpMethod = "GET";
			return query;
		}

		/// <summary>
		/// Create Zone
		/// </summary>
		/// <example>
		/// {
		/// "Name":"Name",
		/// "HeartbeatTolerance":10,
		/// "ZoneServices":"StorageZone, SharepointConnector, NetworkShareConnector"
		/// }
		/// </example>
		/// <remarks>
		/// Creates a new Zone.
		/// </remarks>
		/// <returns>
		/// the created zone
		/// </returns>
		public IQuery<Zone> Create(Zone zone)
		{
			IQuery<Zone> query = new IQuery<Zone>(Client);
			query.From("Zones");
			query.Body = zone;
			query.HttpMethod = "GET";
			return query;
		}

		/// <summary>
		/// Update Zone
		/// </summary>
		/// <example>
		/// {
		/// "Name":"Name",
		/// "HeartbeatTolerance":10,
		/// "ZoneServices":"StorageZone, SharepointConnector, NetworkShareConnector"
		/// }
		/// </example>
		/// <remarks>
		/// Updates an existing zone
		/// </remarks>
		/// <param name="id"></param>
		/// <param name="zone"></param>
		/// <returns>
		/// The modified zone
		/// </returns>
		public IQuery<Zone> Update(string id, Zone zone)
		{
			IQuery<Zone> query = new IQuery<Zone>(Client);
			query.From("Zones");
			query.Ids(id);
			query.Body = zone;
			query.HttpMethod = "PATCH";
			return query;
		}

		/// <summary>
		/// Delete Zone
		/// </summary>
		/// <remarks>
		/// Removes an existing zone
		/// </remarks>
		/// <param name="id"></param>
		/// <param name="force"></param>
		public IQuery Delete(string id, bool force = false)
		{
			IQuery query = new IQuery(Client);
			query.From("Zones");
			query.Ids(id);
			query.QueryString("force", force);
			query.HttpMethod = "DELETE";
			return query;
		}

		/// <summary>
		/// Reset Zone Secret
		/// </summary>
		/// <remarks>
		/// Resets the current Zone Secret to a new Random value
		/// Caution! This Call will invalidate all Storage Center communications until the Storage Center Zone secret
		/// is also updated.
		/// User must be a Zone admin to perform this action
		/// </remarks>
		/// <param name="id"></param>
		/// <returns>
		/// The modified Zone object
		/// </returns>
		public IQuery<Zone> ResetSecret( id = , string parentid)
		{
			IQuery<Zone> query = new IQuery<Zone>(Client);
			query.From("Zones");
			query.Action("ResetSecret");
			query.Ids(id);
			query.QueryString("id", parentid);
			query.HttpMethod = "POST";
			return query;
		}

		/// <summary>
		/// Get Zone Metadata
		/// </summary>
		/// <remarks>
		/// Gets metadata associated with the specified zone
		/// </remarks>
		/// <param name="id"></param>
		/// <returns>
		/// the zone metadata feed
		/// </returns>
		public IQuery<ODataFeed<Metadata>> GetMetadata(string id)
		{
			IQuery<ODataFeed<Metadata>> query = new IQuery<ODataFeed<Metadata>>(Client);
			query.From("Zones");
			query.Action("Metadata");
			query.Ids(id);
			query.HttpMethod = "GET";
			return query;
		}

		/// <summary>
		/// Create or update Zone Metadata
		/// </summary>
		/// <example>
		/// [
		/// {"Name":"metadataName1", "Value":"metadataValue1", "IsPublic":"true"},
		/// {"Name":"metadataName2", "Value":"metadataValue2", "IsPublic":"false"},
		/// ...
		/// ]
		/// </example>
		/// <remarks>
		/// Creates or updates Metadata entries associated with the specified zone
		/// </remarks>
		/// <param name="id"></param>
		/// <param name="metadata"></param>
		/// <returns>
		/// the zone metadata feed
		/// </returns>
		public IQuery<ODataFeed<Metadata>> CreateMetadata(string id, IEnumerable<Metadata> metadata)
		{
			IQuery<ODataFeed<Metadata>> query = new IQuery<ODataFeed<Metadata>>(Client);
			query.From("Zones");
			query.Action("Metadata");
			query.Ids(id);
			query.Body = metadata;
			query.HttpMethod = "POST";
			return query;
		}

		/// <summary>
		/// Delete Zone Metadata
		/// </summary>
		/// <remarks>
		/// Delete the Metadata entry associated with the specified zone
		/// </remarks>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <returns>
		/// no data on success
		/// </returns>
		public IQuery DeleteMetadata(string id, string name)
		{
			IQuery query = new IQuery(Client);
			query.From("Zones");
			query.Action("Metadata");
			query.Ids(id);
			query.QueryString("name", name);
			query.HttpMethod = "DELETE";
			return query;
		}

	}
#endif
}
