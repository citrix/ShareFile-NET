namespace ShareFile.Api.Client.Entities
{
    public abstract class EntityBase
    {
        protected EntityBase(ShareFileClient client)
        {
            Client = client;
        }

        public ShareFileClient Client { get; set; }
    }
}
