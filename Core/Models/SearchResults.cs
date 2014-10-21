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
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Models 
{
	public class SearchResults : ODataObject 
	{

		/// <summary>
		/// Are these only partial results?
		/// </summary>
		public bool PartialResults { get; set; }

		/// <summary>
		/// Collection of search result hits
		/// </summary>
		public IEnumerable<SearchResult> Results { get; set; }

		/// <summary>
		/// Did the search query timeout?
		/// </summary>
		public bool TimedOut { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as SearchResults;
			if(typedSource != null)
			{
				PartialResults = typedSource.PartialResults;
				Results = typedSource.Results;
				TimedOut = typedSource.TimedOut;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("PartialResults", out token) && token.Type != JTokenType.Null)
				{
					PartialResults = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
				if(source.TryGetProperty("Results", out token) && token.Type != JTokenType.Null)
				{
					Results = (IEnumerable<SearchResult>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<SearchResult>));
				}
				if(source.TryGetProperty("TimedOut", out token) && token.Type != JTokenType.Null)
				{
					TimedOut = (bool)serializer.Deserialize(token.CreateReader(), typeof(bool));
				}
			}
		}
	}
}