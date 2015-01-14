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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Extensions;

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

		public UserUsage UserUsage { get; set; }

		/// <summary>
		/// Maximum disk space for the account in megabtyes
		/// </summary>
		public int? DiskSpaceMax { get; set; }

		public DiskSpace DiskSpace { get; set; }

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

		public BillingInfo BillingInformation { get; set; }

		public override void Copy(ODataObject source, JsonSerializer serializer)
		{
			if(source == null || serializer == null) return;
			base.Copy(source, serializer);

			var typedSource = source as Account;
			if(typedSource != null)
			{
				CompanyName = typedSource.CompanyName;
				BillingContact = typedSource.BillingContact;
				BillingContactId = typedSource.BillingContactId;
				TechnicalContact = typedSource.TechnicalContact;
				TechnicalContactId = typedSource.TechnicalContactId;
				AccountManager = typedSource.AccountManager;
				AccountManagerId = typedSource.AccountManagerId;
				AccountType = typedSource.AccountType;
				PlanName = typedSource.PlanName;
				PlanTrack = typedSource.PlanTrack;
				PlanTrackEnum = typedSource.PlanTrackEnum;
				BillingType = typedSource.BillingType;
				BillingCycle = typedSource.BillingCycle;
				BillingRate = typedSource.BillingRate;
				BaseBillingRate = typedSource.BaseBillingRate;
				BaseBandwidth = typedSource.BaseBandwidth;
				BaseDiskSpace = typedSource.BaseDiskSpace;
				BaseUsers = typedSource.BaseUsers;
				AdditionalBandwidth = typedSource.AdditionalBandwidth;
				AdditionalDiskSpace = typedSource.AdditionalDiskSpace;
				AdditionalUsers = typedSource.AdditionalUsers;
				AdditionalBandwidthRate = typedSource.AdditionalBandwidthRate;
				AdditionalDiskSpaceRate = typedSource.AdditionalDiskSpaceRate;
				AdditionalUserRate = typedSource.AdditionalUserRate;
				UserMax = typedSource.UserMax;
				UserUsage = typedSource.UserUsage;
				DiskSpaceMax = typedSource.DiskSpaceMax;
				DiskSpace = typedSource.DiskSpace;
				BandwidthMax = typedSource.BandwidthMax;
				HasPowerTools = typedSource.HasPowerTools;
				HasEncryption = typedSource.HasEncryption;
				PowerToolsRate = typedSource.PowerToolsRate;
				EncryptionRate = typedSource.EncryptionRate;
				Address1 = typedSource.Address1;
				Address2 = typedSource.Address2;
				City = typedSource.City;
				State = typedSource.State;
				Zip = typedSource.Zip;
				Country = typedSource.Country;
				CreditCardType = typedSource.CreditCardType;
				CreditCardNumber = typedSource.CreditCardNumber;
				CreditCardExpirationMonth = typedSource.CreditCardExpirationMonth;
				CreditCardExpirationYear = typedSource.CreditCardExpirationYear;
				CreditCardFirstName = typedSource.CreditCardFirstName;
				CreditCardLastName = typedSource.CreditCardLastName;
				Phone = typedSource.Phone;
				LastBillingDate = typedSource.LastBillingDate;
				NextBillingDate = typedSource.NextBillingDate;
				UseAdvancedCustomBranding = typedSource.UseAdvancedCustomBranding;
				AdvancedCustomBrandingFolderName = typedSource.AdvancedCustomBrandingFolderName;
				BrandingStyles = typedSource.BrandingStyles;
				LogoURL = typedSource.LogoURL;
				RootItem = typedSource.RootItem;
				RootItemId = typedSource.RootItemId;
				CreationDate = typedSource.CreationDate;
				IsFreeTrial = typedSource.IsFreeTrial;
				IsCancelled = typedSource.IsCancelled;
				CancellationDate = typedSource.CancellationDate;
				SSO = typedSource.SSO;
				Preferences = typedSource.Preferences;
				ProductDefaults = typedSource.ProductDefaults;
				Subdomain = typedSource.Subdomain;
				Subdomains = typedSource.Subdomains;
				MobileSecuritySettings = typedSource.MobileSecuritySettings;
				LoginAccessControlDomains = typedSource.LoginAccessControlDomains;
				FolderAccessControlDomains = typedSource.FolderAccessControlDomains;
				StorageQuotaPerUser = typedSource.StorageQuotaPerUser;
				FreeTrialId = typedSource.FreeTrialId;
				Source = typedSource.Source;
				AttributedSource = typedSource.AttributedSource;
				CompanyURL = typedSource.CompanyURL;
				MarketingOptIn = typedSource.MarketingOptIn;
				CreditCardSecurityCode = typedSource.CreditCardSecurityCode;
				ToolInformation = typedSource.ToolInformation;
				BillingInformation = typedSource.BillingInformation;
			}
			else
			{
				JToken token;
				if(source.TryGetProperty("CompanyName", out token) && token.Type != JTokenType.Null)
				{
					CompanyName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BillingContact", out token) && token.Type != JTokenType.Null)
				{
					BillingContact = (User)serializer.Deserialize(token.CreateReader(), typeof(User));
				}
				if(source.TryGetProperty("BillingContactId", out token) && token.Type != JTokenType.Null)
				{
					BillingContactId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("TechnicalContact", out token) && token.Type != JTokenType.Null)
				{
					TechnicalContact = (User)serializer.Deserialize(token.CreateReader(), typeof(User));
				}
				if(source.TryGetProperty("TechnicalContactId", out token) && token.Type != JTokenType.Null)
				{
					TechnicalContactId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("AccountManager", out token) && token.Type != JTokenType.Null)
				{
					AccountManager = (User)serializer.Deserialize(token.CreateReader(), typeof(User));
				}
				if(source.TryGetProperty("AccountManagerId", out token) && token.Type != JTokenType.Null)
				{
					AccountManagerId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("AccountType", out token) && token.Type != JTokenType.Null)
				{
					AccountType = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("PlanName", out token) && token.Type != JTokenType.Null)
				{
					PlanName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("PlanTrack", out token) && token.Type != JTokenType.Null)
				{
					PlanTrack = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("PlanTrackEnum", out token) && token.Type != JTokenType.Null)
				{
					PlanTrackEnum = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BillingType", out token) && token.Type != JTokenType.Null)
				{
					BillingType = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BillingCycle", out token) && token.Type != JTokenType.Null)
				{
					BillingCycle = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BillingRate", out token) && token.Type != JTokenType.Null)
				{
					BillingRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("BaseBillingRate", out token) && token.Type != JTokenType.Null)
				{
					BaseBillingRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("BaseBandwidth", out token) && token.Type != JTokenType.Null)
				{
					BaseBandwidth = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("BaseDiskSpace", out token) && token.Type != JTokenType.Null)
				{
					BaseDiskSpace = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("BaseUsers", out token) && token.Type != JTokenType.Null)
				{
					BaseUsers = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("AdditionalBandwidth", out token) && token.Type != JTokenType.Null)
				{
					AdditionalBandwidth = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("AdditionalDiskSpace", out token) && token.Type != JTokenType.Null)
				{
					AdditionalDiskSpace = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("AdditionalUsers", out token) && token.Type != JTokenType.Null)
				{
					AdditionalUsers = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("AdditionalBandwidthRate", out token) && token.Type != JTokenType.Null)
				{
					AdditionalBandwidthRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("AdditionalDiskSpaceRate", out token) && token.Type != JTokenType.Null)
				{
					AdditionalDiskSpaceRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("AdditionalUserRate", out token) && token.Type != JTokenType.Null)
				{
					AdditionalUserRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("UserMax", out token) && token.Type != JTokenType.Null)
				{
					UserMax = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("UserUsage", out token) && token.Type != JTokenType.Null)
				{
					UserUsage = (UserUsage)serializer.Deserialize(token.CreateReader(), typeof(UserUsage));
				}
				if(source.TryGetProperty("DiskSpaceMax", out token) && token.Type != JTokenType.Null)
				{
					DiskSpaceMax = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("DiskSpace", out token) && token.Type != JTokenType.Null)
				{
					DiskSpace = (DiskSpace)serializer.Deserialize(token.CreateReader(), typeof(DiskSpace));
				}
				if(source.TryGetProperty("BandwidthMax", out token) && token.Type != JTokenType.Null)
				{
					BandwidthMax = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("HasPowerTools", out token) && token.Type != JTokenType.Null)
				{
					HasPowerTools = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("HasEncryption", out token) && token.Type != JTokenType.Null)
				{
					HasEncryption = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("PowerToolsRate", out token) && token.Type != JTokenType.Null)
				{
					PowerToolsRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("EncryptionRate", out token) && token.Type != JTokenType.Null)
				{
					EncryptionRate = (decimal?)serializer.Deserialize(token.CreateReader(), typeof(decimal?));
				}
				if(source.TryGetProperty("Address1", out token) && token.Type != JTokenType.Null)
				{
					Address1 = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Address2", out token) && token.Type != JTokenType.Null)
				{
					Address2 = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("City", out token) && token.Type != JTokenType.Null)
				{
					City = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("State", out token) && token.Type != JTokenType.Null)
				{
					State = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Zip", out token) && token.Type != JTokenType.Null)
				{
					Zip = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Country", out token) && token.Type != JTokenType.Null)
				{
					Country = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardType", out token) && token.Type != JTokenType.Null)
				{
					CreditCardType = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardNumber", out token) && token.Type != JTokenType.Null)
				{
					CreditCardNumber = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardExpirationMonth", out token) && token.Type != JTokenType.Null)
				{
					CreditCardExpirationMonth = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardExpirationYear", out token) && token.Type != JTokenType.Null)
				{
					CreditCardExpirationYear = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardFirstName", out token) && token.Type != JTokenType.Null)
				{
					CreditCardFirstName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreditCardLastName", out token) && token.Type != JTokenType.Null)
				{
					CreditCardLastName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Phone", out token) && token.Type != JTokenType.Null)
				{
					Phone = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("LastBillingDate", out token) && token.Type != JTokenType.Null)
				{
					LastBillingDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("NextBillingDate", out token) && token.Type != JTokenType.Null)
				{
					NextBillingDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("UseAdvancedCustomBranding", out token) && token.Type != JTokenType.Null)
				{
					UseAdvancedCustomBranding = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("AdvancedCustomBrandingFolderName", out token) && token.Type != JTokenType.Null)
				{
					AdvancedCustomBrandingFolderName = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("BrandingStyles", out token) && token.Type != JTokenType.Null)
				{
					BrandingStyles = (IDictionary<string, string>)serializer.Deserialize(token.CreateReader(), typeof(IDictionary<string, string>));
				}
				if(source.TryGetProperty("LogoURL", out token) && token.Type != JTokenType.Null)
				{
					LogoURL = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("RootItem", out token) && token.Type != JTokenType.Null)
				{
					RootItem = (Item)serializer.Deserialize(token.CreateReader(), typeof(Item));
				}
				if(source.TryGetProperty("RootItemId", out token) && token.Type != JTokenType.Null)
				{
					RootItemId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CreationDate", out token) && token.Type != JTokenType.Null)
				{
					CreationDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("IsFreeTrial", out token) && token.Type != JTokenType.Null)
				{
					IsFreeTrial = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("IsCancelled", out token) && token.Type != JTokenType.Null)
				{
					IsCancelled = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("CancellationDate", out token) && token.Type != JTokenType.Null)
				{
					CancellationDate = (DateTime?)serializer.Deserialize(token.CreateReader(), typeof(DateTime?));
				}
				if(source.TryGetProperty("SSO", out token) && token.Type != JTokenType.Null)
				{
					SSO = (SSOAccountProvider)serializer.Deserialize(token.CreateReader(), typeof(SSOAccountProvider));
				}
				if(source.TryGetProperty("Preferences", out token) && token.Type != JTokenType.Null)
				{
					Preferences = (AccountPreferences)serializer.Deserialize(token.CreateReader(), typeof(AccountPreferences));
				}
				if(source.TryGetProperty("ProductDefaults", out token) && token.Type != JTokenType.Null)
				{
					ProductDefaults = (ProductDefaults)serializer.Deserialize(token.CreateReader(), typeof(ProductDefaults));
				}
				if(source.TryGetProperty("Subdomain", out token) && token.Type != JTokenType.Null)
				{
					Subdomain = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Subdomains", out token) && token.Type != JTokenType.Null)
				{
					Subdomains = (IEnumerable<string>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<string>));
				}
				if(source.TryGetProperty("MobileSecuritySettings", out token) && token.Type != JTokenType.Null)
				{
					MobileSecuritySettings = (MobileSecuritySettings)serializer.Deserialize(token.CreateReader(), typeof(MobileSecuritySettings));
				}
				if(source.TryGetProperty("LoginAccessControlDomains", out token) && token.Type != JTokenType.Null)
				{
					LoginAccessControlDomains = (AccessControlDomains)serializer.Deserialize(token.CreateReader(), typeof(AccessControlDomains));
				}
				if(source.TryGetProperty("FolderAccessControlDomains", out token) && token.Type != JTokenType.Null)
				{
					FolderAccessControlDomains = (AccessControlDomains)serializer.Deserialize(token.CreateReader(), typeof(AccessControlDomains));
				}
				if(source.TryGetProperty("StorageQuotaPerUser", out token) && token.Type != JTokenType.Null)
				{
					StorageQuotaPerUser = (int?)serializer.Deserialize(token.CreateReader(), typeof(int?));
				}
				if(source.TryGetProperty("FreeTrialId", out token) && token.Type != JTokenType.Null)
				{
					FreeTrialId = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("Source", out token) && token.Type != JTokenType.Null)
				{
					Source = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("AttributedSource", out token) && token.Type != JTokenType.Null)
				{
					AttributedSource = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("CompanyURL", out token) && token.Type != JTokenType.Null)
				{
					CompanyURL = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("MarketingOptIn", out token) && token.Type != JTokenType.Null)
				{
					MarketingOptIn = (bool?)serializer.Deserialize(token.CreateReader(), typeof(bool?));
				}
				if(source.TryGetProperty("CreditCardSecurityCode", out token) && token.Type != JTokenType.Null)
				{
					CreditCardSecurityCode = (string)serializer.Deserialize(token.CreateReader(), typeof(string));
				}
				if(source.TryGetProperty("ToolInformation", out token) && token.Type != JTokenType.Null)
				{
					ToolInformation = (IEnumerable<ToolInformation>)serializer.Deserialize(token.CreateReader(), typeof(IEnumerable<ToolInformation>));
				}
				if(source.TryGetProperty("BillingInformation", out token) && token.Type != JTokenType.Null)
				{
					BillingInformation = (BillingInfo)serializer.Deserialize(token.CreateReader(), typeof(BillingInfo));
				}
			}
		}
	}
}