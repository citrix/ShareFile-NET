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
namespace ShareFile.Api.Client.Models 
{
    public enum CapabilityName
    {
        Thumbnails = 0,
        DocPreview = 1,
        Search = 2,
        AdvancedSearch = 3,
        
        /// <summary>
        /// DEPRECATED: Use CapabilityName.SharingRequest and CapabilityName.SharingSend instead.
        /// </summary>
        Sharing = 4,
        
        /// <summary>
        /// DEPRECATED: Use CapabilityName.AnonymousSharingRequest and CapabilityName.AnonymousSharingSend instead.
        /// </summary>
        AnonymousSharing = 5,
        LastProgenyUpdateDate = 6,
        DirectDownload = 7,
        DirectUpload = 8,
        CheckIn = 9,
        CheckOut = 10,
        FileVersioning = 11,
        RecycleBin = 12,
        FolderTemplates = 13,
        VirusScan = 14,
        ItemLink = 15,
        ItemNote = 16,
        
        /// <summary>
        /// Indicates if Description can be added to items. Currently only ShareFile (sf) supports this.
        /// </summary>
        ItemDescription = 17,
        VersionMax = 18,
        VersionMin = 19,
        
        /// <summary>
        /// Indicates whether items can be favorited. If this capability appears without
        /// only Folders are supported.
        /// </summary>
        Favorites = 20,
        ReusableItemId = 21,
        StandardUploadRaw = 22,
        StandardUploadForms = 23,
        StreamedUploadRaw = 24,
        StreamedUploadForms = 25,
        ThreadedUploadRaw = 26,
        ThreadedUploadForms = 27,
        
        /// <summary>
        /// Supports Right Signature Integration
        /// </summary>
        RightSignature = 28,
        UploadWithRequestParams = 29,
        
        /// <summary>
        /// Provider supports FileLock APIs with SoftLock semantics.
        /// </summary>
        SoftLock = 30,
        
        /// <summary>
        /// Provider supports FileLock APIs with HardLock semantics.
        /// </summary>
        HardLock = 31,
        
        /// <summary>
        /// Provider supports download items by StreamId
        /// </summary>
        DownloadByStream = 32,
        BulkDelete = 33,
        BulkDownload = 34,
        
        /// <summary>
        /// Provider supports downloading Folders
        /// </summary>
        FolderDownload = 35,
        
        /// <summary>
        /// Provider supports sending Folders
        /// </summary>
        FolderSend = 36,
        
        /// <summary>
        /// Provider supports sending Items by StreamId
        /// </summary>
        SendByStream = 37,
        
        /// <summary>
        /// Provider supports requesting Files from authenticated users
        /// </summary>
        SharingRequest = 38,
        
        /// <summary>
        /// Provider supports sending Items to authenticated users
        /// </summary>
        SharingSend = 39,
        
        /// <summary>
        /// Provider supports requesting Files from anonymous users
        /// </summary>
        AnonymousSharingRequest = 40,
        
        /// <summary>
        /// Provider supports sending Items to anonymous users
        /// </summary>
        AnonymousSharingSend = 41,
        
        /// <summary>
        /// Provider supports copying Items
        /// </summary>
        Copy = 42,
        
        /// <summary>
        /// Provider supports moving Items
        /// </summary>
        Move = 43,
        
        /// <summary>
        /// Provider supports unzipping file uploads with file extension .zip
        /// </summary>
        UnZipFileUploads = 44,
        
        /// <summary>
        /// Provider supports Right Signature 4 Integration
        /// </summary>
        RightSignature4 = 45,
        
        /// <summary>
        /// Provider supports Document Approval Workflow Integration
        /// </summary>
        DocumentApprovalWorkflow = 46,
        
        /// <summary>
        /// Provider supports sending list of available ShareAccessRight options
        /// </summary>
        ShareAccessRight = 47,
        
        /// <summary>
        /// Provider supports items pagination ordered by Name asc or desc
        /// </summary>
        ItemSortByName = 48,
        
        /// <summary>
        /// Provider supports items pagination ordered by CreatorNameShort asc or desc
        /// </summary>
        ItemSortByCreatorNameShort = 49,
        
        /// <summary>
        /// Provider supports items pagination ordered by CreationDate asc or desc
        /// </summary>
        ItemSortByCreationDate = 50,
        
        /// <summary>
        /// Provider supports items pagination ordered by FileSizeBytes asc or desc
        /// </summary>
        ItemSortByFileSizeBytes = 51,
        
        /// <summary>
        /// Provider supports items orderingMode FoldersFirst
        /// </summary>
        ItemFoldersFirstGrouping = 52,
        
        /// <summary>
        /// Provider supports returning ItemOperations enum
        /// </summary>
        ItemOperations = 53,
        
        /// <summary>
        /// Supports Simple search with the option of filtering by parent
        /// </summary>
        SearchByParent = 54,
        
        /// <summary>
        /// Indicates Item copy operations are supported for the provided list of host and providers.
        /// </summary>
        ScopedCopy = 55,
        
        /// <summary>
        /// Indicates Item move operations are supported for the provided list of host and providers.
        /// </summary>
        ScopedMove = 56,
        
        /// <summary>
        /// StructuredDownload API that supports client requesting a specific folder structure be returned.
        /// </summary>
        StructuredDownload = 57,
        
        /// <summary>
        /// Indicates that sending Items to anonymous users will result in a third-party Share Uri.
        /// </summary>
        DirectAnonymousSharingSend = 58,
        
        /// <summary>
        /// Provider supports adding tags to an item (Prominent use in Connectors)
        /// </summary>
        MetadataTagging = 59,
        
        /// <summary>
        /// Provider supports updating the Metadata properties of an item
        /// </summary>
        ItemMetadata = 60,
        
        /// <summary>
        /// Provider supports creating a new document and launching editing session.
        /// </summary>
        CreateMicrosoftFiles = 61,
        
        /// <summary>
        /// Provider supports download APIs that will optionally return a DownloadSpecification instead of file bytes.
        /// </summary>
        DownloadSpecification = 62,
        
        /// <summary>
        /// Provider supports the item rename operation
        /// </summary>
        ItemRename = 63,
        
        /// <summary>
        /// Provider supports the Bulk Move operation
        /// </summary>
        BulkMove = 64,
        
        /// <summary>
        /// Provider supports Breadcrumbs
        /// </summary>
        Breadcrumbs = 65
    }
}
