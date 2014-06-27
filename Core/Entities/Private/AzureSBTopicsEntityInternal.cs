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

	public interface IAzureSBTopicsEntityInternal : IEntityBase
	{
		IQuery<AzureSBTopicsResponse> CheckIfTopicExists(Uri url);
		IQuery CreateTopic(Uri url);
		IQuery<ServiceBusEndPointInfo> GetTopicEndPoint(Uri url, string id);
		IQuery RegenerateTopicCredentials(Uri url);
		IQuery DeleteTopic(Uri url);
	}

	public class AzureSBTopicsEntityInternal : EntityBase, IAzureSBTopicsEntityInternal
	{
		public AzureSBTopicsEntityInternal(IShareFileClient client)
			: base (client, "AzureSBTopics")
			
		{

		}

		public IQuery<AzureSBTopicsResponse> CheckIfTopicExists(Uri url)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<AzureSBTopicsResponse>(Client);
			sfApiQuery.Action("CheckIfTopicExists");
			sfApiQuery.Uri(url);
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery CreateTopic(Uri url)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
			sfApiQuery.Action("CreateTopic");
			sfApiQuery.Uri(url);
			sfApiQuery.HttpMethod = "POST";
			return sfApiQuery;
		}

		public IQuery<ServiceBusEndPointInfo> GetTopicEndPoint(Uri url, string id)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query<ServiceBusEndPointInfo>(Client);
			sfApiQuery.Action("GetTopicEndPoint");
			sfApiQuery.Uri(url);
			sfApiQuery.ActionIds(id);
			sfApiQuery.HttpMethod = "GET";
			return sfApiQuery;
		}

		public IQuery RegenerateTopicCredentials(Uri url)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
			sfApiQuery.Action("RegenerateTopicCredentials");
			sfApiQuery.Uri(url);
			sfApiQuery.HttpMethod = "POST";
			return sfApiQuery;
		}

		public IQuery DeleteTopic(Uri url)
		{
			var sfApiQuery = new ShareFile.Api.Client.Requests.Query(Client);
			sfApiQuery.Action("DeleteTopic");
			sfApiQuery.Uri(url);
			sfApiQuery.HttpMethod = "DELETE";
			return sfApiQuery;
		}

	}
#endif
}
