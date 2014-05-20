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

namespace ShareFile.Api.Models 
{
	public class Item : ODataObject 
	{
		/// <summary>
		/// Item Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Item File Name. ShareFile allows Items to have different Display and File names: display
		/// names are shown during client navigation, while file names are used when the item is
		/// downloaded.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// User that Created this Item
		/// </summary>
		public User Creator { get; set; }

		/// <summary>
		/// Parent container of the Item. A container is usually a Folder object, with a few exceptions -
		/// the "Account" is the container of top-level folders.
		/// </summary>
		public Item Parent { get; set; }

		/// <summary>
		/// List of Access Controls for this Item. This is not the effective ACL on the Item, just the
		/// ACLs directly attached to this Item. Use the "Info" reference to retrieve effective ACL
		/// </summary>
		public IEnumerable<AccessControl> AccessControls { get; set; }

		/// <summary>
		/// The Storage Zone that contains this Item.
		/// </summary>
		public Zone Zone { get; set; }

		/// <summary>
		/// Item Creation Date.
		/// </summary>
		public DateTime? CreationDate { get; set; }

		/// <summary>
		/// The last modified date of this item and all of its children, recursively. This parameter
		/// is not supported in all ShareFile providers - it is always set in sharefile.com hosting, but
		/// not in some StorageZone connectors. The Capability object of the provider indicates whether
		/// the provider supports this field or not.
		/// </summary>
		public DateTime? ProgenyEditDate { get; set; }

		/// <summary>
		/// Client device filesystem Created Date of this Item.
		/// </summary>
		public DateTime? ClientCreatedDate { get; set; }

		/// <summary>
		/// Client device filesystem last Modified Date of this Item.
		/// </summary>
		public DateTime? ClientModifiedDate { get; set; }

		/// <summary>
		/// Defines the Retention Policy for this Item. After this date, the item is automatically moved
		/// to recycle bin.
		/// </summary>
		public DateTime? ExpirationDate { get; set; }

		/// <summary>
		/// Item description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Disk space limit for the Item. Define the maximum amount of bytes that this container can
		/// hold at any given time.
		/// </summary>
		public int? DiskSpaceLimit { get; set; }

		/// <summary>
		/// Defines whether the Item has a 'hidden' flag.
		/// </summary>
		public bool? IsHidden { get; set; }

		/// <summary>
		/// Bandwidth limit for the Item. Define the maximum amount of bytes that can be added and
		/// retrieved from this item.
		/// </summary>
		public int? BandwidthLimitInMB { get; set; }

		/// <summary>
		/// User Owner of this Item.
		/// </summary>
		public User Owner { get; set; }

		/// <summary>
		/// ShareFile Account containing this item.
		/// </summary>
		public Account Account { get; set; }

		/// <summary>
		/// Item size in Kilobytes. For containers, this field includes all children sizes, recursively.
		/// </summary>
		public int? FileSizeInKB { get; set; }

		/// <summary>
		/// Contains a ItemID path, separated by /, from the virtual root to this given file. Example
		/// /accountID/folderID/folderID/itemID
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// First name of the user that created this item
		/// </summary>
		public string CreatorFirstName { get; set; }

		/// <summary>
		/// Last name of the user that created this item
		/// </summary>
		public string CreatorLastName { get; set; }

		/// <summary>
		/// Amount of days until this item expireses (see ExpirationDate)
		/// </summary>
		public int? ExpirationDays { get; set; }

		/// <summary>
		/// Item size in bytes. For containers, this field will include all children sizes, recursively.
		/// </summary>
		public long? FileSizeBytes { get; set; }

		/// <summary>
		/// Indicates whether a preview image is available for this Item.
		/// 
		/// ShareFile.com always create previews for known file types, although there is a delay from the file
		/// creation until the preview is available. Some Storage Zones Providers do not create previews, depending
		/// on version and deployment options.
		/// 
		/// Previews are not created for unknown file types
		/// </summary>
		public SafeEnum<PreviewStatus> PreviewStatus { get; set; }

		public int? MaxPreviewSize { get; set; }

		/// <summary>
		/// Indicates that the Item is pending for removal. At the next execution of the Cleanup process, the data
		/// blob associated with this item will be removed. This parameter is not used for certain Storage Zone
		/// Providers. For example, in CIFS and SharePoint connectors, removals are performed imediately. The
		/// Capability "HasRecycleBin" indicates whether this field is used or not in the provider.
		/// </summary>
		public bool? HasPendingDeletion { get; set; }

		/// <summary>
		/// Folder Template reference. If set, it indicates that this Item was created from a Folder Template. Modifications
		/// to the folder template are propagated to the associated items.
		/// 
		/// The Capability FolderTemplate indicates whether the provider supports Folder Templates.
		/// </summary>
		public string AssociatedFolderTemplateID { get; set; }

		/// <summary>
		/// Indicates whether the item is owned by a Folder Template. If set, it indicates that this Item was created from a
		/// Folder Template. Modifications to the folder template are propagated to the associated items.
		/// 
		/// The Capability FolderTemplate indicates whether the provider supports Folder Templates.
		/// </summary>
		public bool? IsTemplateOwned { get; set; }

		public bool? HasPermissionInfo { get; set; }

		public int? State { get; set; }

		/// <summary>
		/// Identifier for the Item stream. An Item represents a single version of a file system object. The stream identifies
		/// all versions of the same file system object. For example, when users upload or modify an existing file, a new Item
		/// is created with the same StreamID. All Item enumerations return only the latest version of a given stream. You can
		/// access the previous versions of a file using the StreamID reference.
		/// </summary>
		public string StreamID { get; set; }

		/// <summary>
		/// Short version of items creator's name. E.g., J. Doe.
		/// </summary>
		public string CreatorNameShort { get; set; }

		/// <summary>
		/// Specifies whether there are other versions of this item. Not all providers support file versioning. The
		/// Capability FileVersioning indicates whether the provider supports file versions.
		/// </summary>
		public bool? HasMultipleVersions { get; set; }

		/// <summary>
		/// List of custom metadata object associated with this item
		/// </summary>
		public IEnumerable<Metadata> Metadata { get; set; }

	}
}