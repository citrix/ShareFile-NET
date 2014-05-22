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
	public class Account : ODataObject 
	{

		public string CompanyName { get; set; }

		public User BillingContact { get; set; }

		public string BillingContactId { get; set; }

		public User TechnicalContact { get; set; }

		public string TechnicalContactId { get; set; }

		public User AccountManager { get; set; }

		public string AccountManagerId { get; set; }

		public string AccountType { get; set; }

		/// <summary>
		/// Basic, Professional, Enterprise
		/// </summary>
		public string PlanName { get; set; }

		public string PlanTrack { get; set; }

		public string PlanTrackEnum { get; set; }

		/// <summary>
		/// Credit Card, Invoice, Comp
		/// </summary>
		public string BillingType { get; set; }

		/// <summary>
		/// Monthly, Quarterly, Annually
		/// </summary>
		public string BillingCycle { get; set; }

		public decimal? BillingRate { get; set; }

		public decimal? BaseBillingRate { get; set; }

		/// <summary>
		/// Bandwidth included in plan in megabytes
		/// </summary>
		public int? BaseBandwidth { get; set; }

		/// <summary>
		/// Disk space included in megabytes
		/// </summary>
		public int? BaseDiskSpace { get; set; }

		/// <summary>
		/// Users included in plan
		/// </summary>
		public int? BaseUsers { get; set; }

		/// <summary>
		/// Additional bandwidth purchased for account
		/// </summary>
		public int? AdditionalBandwidth { get; set; }

		/// <summary>
		/// Additional disk space purchased for account
		/// </summary>
		public int? AdditionalDiskSpace { get; set; }

		/// <summary>
		/// Additional users purchased for account
		/// </summary>
		public int? AdditionalUsers { get; set; }

		/// <summary>
		/// Additional rate for extra bandwidth. NOTE: This is specified in gigbytes, not megabytes.
		/// </summary>
		public decimal? AdditionalBandwidthRate { get; set; }

		/// <summary>
		/// Additional rate for extra diskspace. NOTE: This is specified in gigbytes, not megabytes.
		/// </summary>
		public decimal? AdditionalDiskSpaceRate { get; set; }

		/// <summary>
		/// Additional rate for extra users
		/// </summary>
		public decimal? AdditionalUserRate { get; set; }

		public int? UserMax { get; set; }

		/// <summary>
		/// Maximum disk space for the account in megabtyes
		/// </summary>
		public int? DiskSpaceMax { get; set; }

		/// <summary>
		/// Maximum bandwidth for the account in megabtyes
		/// </summary>
		public int? BandwidthMax { get; set; }

		public bool? HasPowerTools { get; set; }

		public bool? HasEncryption { get; set; }

		/// <summary>
		/// Additional rate for adding PowerTools.
		/// </summary>
		public decimal? PowerToolsRate { get; set; }

		/// <summary>
		/// Additional rate for stored file encryption
		/// </summary>
		public decimal? EncryptionRate { get; set; }

		public string Address1 { get; set; }

		public string Address2 { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string Zip { get; set; }

		public string Country { get; set; }

		public string CreditCardType { get; set; }

		public string CreditCardNumber { get; set; }

		public string CreditCardExpirationMonth { get; set; }

		public string CreditCardExpirationYear { get; set; }

		public string CreditCardFirstName { get; set; }

		public string CreditCardLastName { get; set; }

		public string Phone { get; set; }

		public DateTime? LastBillingDate { get; set; }

		public DateTime? NextBillingDate { get; set; }

		public bool? UseAdvancedCustomBranding { get; set; }

		public string AdvancedCustomBrandingFolderName { get; set; }

		public IDictionary<string, string> BrandingStyles { get; set; }

		public string LogoURL { get; set; }

		public Item RootItem { get; set; }

		public string RootItemId { get; set; }

		public DateTime? CreationDate { get; set; }

		public bool? IsFreeTrial { get; set; }

		public bool? IsCancelled { get; set; }

		public DateTime? CancellationDate { get; set; }

		public SSOAccountProvider SSO { get; set; }

		public AccountPreferences Preferences { get; set; }

		public ProductDefaults ProductDefaults { get; set; }

		public string Subdomain { get; set; }

		public IEnumerable<string> Subdomains { get; set; }

		public MobileSecuritySettings MobileSecuritySettings { get; set; }

		public AccessControlDomains LoginAccessControlDomains { get; set; }

		public AccessControlDomains FolderAccessControlDomains { get; set; }

		public int? StorageQuotaPerUser { get; set; }

		public string FreeTrialId { get; set; }

		public string Source { get; set; }

		public string AttributedSource { get; set; }

		public string CompanyURL { get; set; }

		public bool? MarketingOptIn { get; set; }

		public string CreditCardSecurityCode { get; set; }

		public IEnumerable<ToolInformation> ToolInformation { get; set; }

	}
}