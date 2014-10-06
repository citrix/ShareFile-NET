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
using ShareFile.Api.Client.Entities;
using ShareFile.Api.Client.Internal.Models;

namespace ShareFile.Api.Client
{
    public interface IInternalShareFileClient : IShareFileClient
    {
        IBillingEntityInternal Billing { get; }
        IOAuthClientsEntityInternal OAuthClients { get; }
        IPlanAddonsEntityInternal PlanAddons { get; }
        IAzureSBTopicsEntityInternal AzureSBTopics { get; }
        IConfigsEntityInternal Configs { get; }
        IDevicesEntityInternal Devices { get; }
    }

    public class InternalShareFileClient : ShareFileClient, IInternalShareFileClient
    {
        public InternalShareFileClient(string baseUri, Configuration configuration = null)
            : base(baseUri, configuration)
        {
            Billing = new BillingEntityInternal(this);
            OAuthClients = new OAuthClientsEntityInternal(this);
            PlanAddons = new PlanAddonsEntityInternal(this);
            AzureSBTopics = new AzureSBTopicsEntityInternal(this);
            Configs = new ConfigsEntityInternal(this);
            Devices = new DevicesEntityInternal(this);

            EntityTypeMapInternal.RegisterInternalModels();
        }

        public IBillingEntityInternal Billing { get; private set; }
        public IOAuthClientsEntityInternal OAuthClients { get; private set; }
        public IPlanAddonsEntityInternal PlanAddons { get; private set; }
        public IAzureSBTopicsEntityInternal AzureSBTopics { get; private set; }
        public IConfigsEntityInternal Configs { get; private set; }
        public IDevicesEntityInternal Devices { get; private set; }
    }
}