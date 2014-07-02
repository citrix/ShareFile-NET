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
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Entities
{
#if ShareFile

	public interface IOAuthClientsEntityInternal : IEntityBase
	{
		IQuery<ODataFeed<OAuthClient>> Get();
		IQuery<OAuthClient> Get(Uri url);
		IQuery<ODataFeed<OAuthClient>> ByAccount(string accountId);
		IQuery<OAuthClient> Create(OAuthClient oauthClient, bool singlePlane = false);
		IQuery<OAuthClient> Update(Uri url, OAuthClient oauthClient, bool singlePlane = false);
		IQuery Delete(Uri url, bool singlePlane = false);
	}

	public class OAuthClientsEntityInternal : EntityBase, IOAuthClientsEntityInternal
	{
		public OAuthClientsEntityInternal(IShareFileClient client)
			: base (client, "OAuthClients")
			
		{

		}

		public IQuery<ODataFeed<OAuthClient>> Get()
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<OAuthClient>>(Client);
			sfApiQuery.From("OAuthClients");
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery<OAuthClient> Get(Uri url)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<OAuthClient>(Client);
			sfApiQuery.Uri(url);
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery<ODataFeed<OAuthClient>> ByAccount(string accountId)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ODataFeed<OAuthClient>>(Client);
			sfApiQuery.From("OAuthClients");
			sfApiQuery.Action("ByAccount");
			sfApiQuery.QueryString("accountId", accountId);
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery<OAuthClient> Create(OAuthClient oauthClient, bool singlePlane = false)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<OAuthClient>(Client);
			sfApiQuery.From("OAuthClients");
			sfApiQuery.QueryString("singlePlane", singlePlane);
			sfApiQuery.Body = oauthClient;
			sfApiQuery.HttpMethod = "POST";
			return sfApiQuery;
		}

		public IQuery<OAuthClient> Update(Uri url, OAuthClient oauthClient, bool singlePlane = false)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<OAuthClient>(Client);
			sfApiQuery.Uri(url);
			sfApiQuery.QueryString("singlePlane", singlePlane);
			sfApiQuery.Body = oauthClient;
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery Delete(Uri url, bool singlePlane = false)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
			sfApiQuery.Uri(url);
			sfApiQuery.QueryString("singlePlane", singlePlane);
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

	}
#endif
}
