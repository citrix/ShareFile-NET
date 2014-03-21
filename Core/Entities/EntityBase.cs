namespace ShareFile.Api.Client.Entities
{
    public abstract class EntityBase
    {
        protected EntityBase(IShareFileClient client, string entity)
        {
            Client = client;
            Entity = entity;
        }

        public IShareFileClient Client { get; set; }
        public string Entity { get; set; }
    }
}
