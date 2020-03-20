// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//     
//	   Copyright (c) 2018 Citrix ShareFile. All rights reserved.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Client.Models 
{
	/// <summary>
	/// Represents a ShareFile ReportRecord: an individual execution of a ShareFile Report
	/// </summary>
	public class ReportRecord : ODataObject 
	{
		public Report Report { get; set; }
		/// <summary>
		/// The Start Date of the range the ReportRecord will be run against
		/// </summary>
		public DateTime? StartDate { get; set; }
		/// <summary>
		/// The End Date of the range the ReportRecord will be run against
		/// </summary>
		public DateTime? EndDate { get; set; }
		/// <summary>
		/// The Time this ReportRecord began processing
		/// </summary>
		public DateTime? StartRunTime { get; set; }
		/// <summary>
		/// The Time this ReportRecord finished processing
		/// </summary>
		public DateTime? EndRunTime { get; set; }
		public SafeEnum<ReportRunStatus> Status { get; set; }
		public bool? HasData { get; set; }
		public string Message { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as ReportRecord;
			if(typedSource != null)
			{
				Report = typedSource.Report;
				StartDate = typedSource.StartDate;
				EndDate = typedSource.EndDate;
				StartRunTime = typedSource.StartRunTime;
				EndRunTime = typedSource.EndRunTime;
				Status = typedSource.Status;
				HasData = typedSource.HasData;
				Message = typedSource.Message;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("Report", out token) && token.Type != JTokenType.Null)
				{
					Report = (Report)serializer.Deserialize(token.CreateReader(), typeof(Report));
				}
				if(source.TryGetProperty("StartDate", out token) && token.Type != JTokenType.Null)
				{
					StartDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("EndDate", out token) && token.Type != JTokenType.Null)
				{
					EndDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("StartRunTime", out token) && token.Type != JTokenType.Null)
				{
					StartRunTime = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("EndRunTime", out token) && token.Type != JTokenType.Null)
				{
					EndRunTime = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("Status", out token) && token.Type != JTokenType.Null)
				{
					Status = (SafeEnum<ReportRunStatus>)serializer.Deserialize(token.CreateReader(), typeof(SafeEnum<ReportRunStatus>));
				}
				if(source.TryGetProperty("HasData", out token) && token.Type != JTokenType.Null)
				{
					HasData = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("Message", out token) && token.Type != JTokenType.Null)
				{
					Message = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
			}
		}
	}
}