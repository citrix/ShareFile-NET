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
using System.IO;
using ShareFile.Api.Models;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Entities
{
    public interface IStorageCentersEntity : IEntityBase
    {
        
        /// <summary>
        /// Get Storage Center
        /// </summary>
        /// <param name="url"></param>
        /// <returns>
        /// A single Storage Center
        /// </returns>
        IQuery<StorageCenter> Get(Uri url);
        
        /// <summary>
        /// Patch Storage Center
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname" }
        /// </example>
        /// <param name="url"></param>
        /// <param name="sc"></param>
        /// <returns>
        /// Modified Storage Center
        /// </returns>
        IQuery<StorageCenter> Update(Uri url, StorageCenter sc);
        
        /// <summary>
        /// Delete Storage Center
        /// </summary>
        /// <param name="url"></param>
        IQuery Delete(Uri url);
        
        /// <summary>
        /// Get List of StorageCenters from Zone
        /// </summary>
        /// <remarks>
        /// Lists Storage Centers of a given Zone
        /// </remarks>
        /// <param name="url"></param>
        /// <returns>
        /// A list of Storage Centers associated with the provided zone
        /// </returns>
        IQuery<ODataFeed<StorageCenter>> GetByZone(Uri url);
        
        /// <summary>
        /// Create StorageCenter
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname"
        /// }
        /// </example>
        /// <remarks>
        /// Creates a new Storage Center associated with a specific zone
        /// </remarks>
        /// <param name="url"></param>
        /// <param name="storageCenter"></param>
        /// <returns>
        /// The new storage center
        /// </returns>
        IQuery<StorageCenter> CreateByZone(Uri url, StorageCenter storageCenter);
        
        /// <summary>
        /// Update StorageCenter
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname"
        /// }
        /// </example>
        /// <remarks>
        /// Updates an existing Storage Center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="storageCenter"></param>
        /// <returns>
        /// the modified storage center
        /// </returns>
        IQuery<StorageCenter> UpdateByZone(Uri zUrl, string scid, StorageCenter storageCenter);
        
        /// <summary>
        /// Delete StorageCenter
        /// </summary>
        /// <remarks>
        /// Removes an existing storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        IQuery DeleteByZone(Uri zUrl, string scid);
        
        /// <summary>
        /// Get StorageCenter Metadata
        /// </summary>
        /// <remarks>
        /// Gets metadata associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <returns>
        /// the storage center metadata feed
        /// </returns>
        IQuery<ODataFeed<Metadata>> GetMetadata(Uri zUrl, string scid);
        
        /// <summary>
        /// Create or update StorageCenter Metadata
        /// </summary>
        /// <example>
        /// [
        /// {"Name":"metadataName1", "Value":"metadataValue1", "IsPublic":"true"},
        /// {"Name":"metadataName2", "Value":"metadataValue2", "IsPublic":"false"},
        /// ...
        /// ]
        /// </example>
        /// <remarks>
        /// Creates or updates Metadata entries associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="metadata"></param>
        /// <returns>
        /// the storage center metadata feed
        /// </returns>
        IQuery<ODataFeed<Metadata>> CreateMetadata(Uri zUrl, string scid, IEnumerable<Metadata> metadata);
        
        /// <summary>
        /// Delete StorageCenter Metadata
        /// </summary>
        /// <remarks>
        /// Delete the Metadata entry associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="name"></param>
        /// <returns>
        /// no data on success
        /// </returns>
        IQuery DeleteMetadata(Uri zUrl, string scid, string name);
    }

    public class StorageCentersEntity : EntityBase, IStorageCentersEntity
    {
        public StorageCentersEntity (IShareFileClient client)
            : base (client, "StorageCenters")
        { }
        
        
        /// <summary>
        /// Get Storage Center
        /// </summary>
        /// <param name="url"></param>
        /// <returns>
        /// A single Storage Center
        /// </returns>
        public IQuery<StorageCenter> Get(Uri url)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<StorageCenter>(Client);
            sfApiQuery.Uri(url);
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Patch Storage Center
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname" }
        /// </example>
        /// <param name="url"></param>
        /// <param name="sc"></param>
        /// <returns>
        /// Modified Storage Center
        /// </returns>
        public IQuery<StorageCenter> Update(Uri url, StorageCenter sc)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<StorageCenter>(Client);
            sfApiQuery.Uri(url);
            sfApiQuery.Body = sc;
            sfApiQuery.HttpMethod = "PATCH";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Delete Storage Center
        /// </summary>
        /// <param name="url"></param>
        public IQuery Delete(Uri url)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
            sfApiQuery.Uri(url);
            sfApiQuery.HttpMethod = "DELETE";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Get List of StorageCenters from Zone
        /// </summary>
        /// <remarks>
        /// Lists Storage Centers of a given Zone
        /// </remarks>
        /// <param name="url"></param>
        /// <returns>
        /// A list of Storage Centers associated with the provided zone
        /// </returns>
        public IQuery<ODataFeed<StorageCenter>> GetByZone(Uri url)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<StorageCenter>>(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(url);
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Create StorageCenter
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname"
        /// }
        /// </example>
        /// <remarks>
        /// Creates a new Storage Center associated with a specific zone
        /// </remarks>
        /// <param name="url"></param>
        /// <param name="storageCenter"></param>
        /// <returns>
        /// The new storage center
        /// </returns>
        public IQuery<StorageCenter> CreateByZone(Uri url, StorageCenter storageCenter)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<StorageCenter>(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(url);
            sfApiQuery.Body = storageCenter;
            sfApiQuery.HttpMethod = "POST";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Update StorageCenter
        /// </summary>
        /// <example>
        /// {
        /// "ExternalAddress":"https://server/",
        /// "Version":"4.12.20",
        /// "HostName":"hostname"
        /// }
        /// </example>
        /// <remarks>
        /// Updates an existing Storage Center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="storageCenter"></param>
        /// <returns>
        /// the modified storage center
        /// </returns>
        public IQuery<StorageCenter> UpdateByZone(Uri zUrl, string scid, StorageCenter storageCenter)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<StorageCenter>(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(zUrl);
            sfApiQuery.ActionIds(scid);
            sfApiQuery.Body = storageCenter;
            sfApiQuery.HttpMethod = "PATCH";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Delete StorageCenter
        /// </summary>
        /// <remarks>
        /// Removes an existing storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        public IQuery DeleteByZone(Uri zUrl, string scid)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(zUrl);
            sfApiQuery.ActionIds(scid);
            sfApiQuery.HttpMethod = "DELETE";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Get StorageCenter Metadata
        /// </summary>
        /// <remarks>
        /// Gets metadata associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <returns>
        /// the storage center metadata feed
        /// </returns>
        public IQuery<ODataFeed<Metadata>> GetMetadata(Uri zUrl, string scid)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<Metadata>>(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(zUrl);
            sfApiQuery.ActionIds(scid);
            sfApiQuery.SubAction("Metadata");
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Create or update StorageCenter Metadata
        /// </summary>
        /// <example>
        /// [
        /// {"Name":"metadataName1", "Value":"metadataValue1", "IsPublic":"true"},
        /// {"Name":"metadataName2", "Value":"metadataValue2", "IsPublic":"false"},
        /// ...
        /// ]
        /// </example>
        /// <remarks>
        /// Creates or updates Metadata entries associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="metadata"></param>
        /// <returns>
        /// the storage center metadata feed
        /// </returns>
        public IQuery<ODataFeed<Metadata>> CreateMetadata(Uri zUrl, string scid, IEnumerable<Metadata> metadata)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<Metadata>>(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(zUrl);
            sfApiQuery.ActionIds(scid);
            sfApiQuery.SubAction("Metadata");
            sfApiQuery.Body = metadata;
            sfApiQuery.HttpMethod = "POST";	
		    return sfApiQuery;
        }
        
        /// <summary>
        /// Delete StorageCenter Metadata
        /// </summary>
        /// <remarks>
        /// Delete the Metadata entry associated with the specified storage center
        /// </remarks>
        /// <param name="zUrl"></param>
        /// <param name="scid"></param>
        /// <param name="name"></param>
        /// <returns>
        /// no data on success
        /// </returns>
        public IQuery DeleteMetadata(Uri zUrl, string scid, string name)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
		    sfApiQuery.Action("StorageCenters");
            sfApiQuery.Uri(zUrl);
            sfApiQuery.ActionIds(scid);
            sfApiQuery.SubAction("Metadata");
            sfApiQuery.QueryString("name", name);
            sfApiQuery.HttpMethod = "DELETE";	
		    return sfApiQuery;
        }
    }
}