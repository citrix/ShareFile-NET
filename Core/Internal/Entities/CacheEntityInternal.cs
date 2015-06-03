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
    public interface ICacheEntityInternal : IEntityBase
    {
        IQuery Stats();
        IQuery TestInsertGet(string cacheid);
        IQuery TestRemove(string cacheid);
    }

    public class CacheEntityInternal : EntityBase, ICacheEntityInternal
    {
        public CacheEntityInternal (IShareFileClient client)
            : base (client, "Cache")
        { }
        
        public IQuery Stats()
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
		    sfApiQuery.From("Cache");
		    sfApiQuery.Action("Stats");
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
        public IQuery TestInsertGet(string cacheid)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
		    sfApiQuery.From("Cache");
		    sfApiQuery.Action("TestInsertGet");
            sfApiQuery.QueryString("cacheid", cacheid);
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
        public IQuery TestRemove(string cacheid)
        {
            var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
		    sfApiQuery.From("Cache");
		    sfApiQuery.Action("TestRemove");
            sfApiQuery.QueryString("cacheid", cacheid);
            sfApiQuery.HttpMethod = "GET";	
		    return sfApiQuery;
        }
    }
}