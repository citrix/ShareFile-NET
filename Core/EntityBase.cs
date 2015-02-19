// ReSharper disable once CheckNamespace
namespace ShareFile.Api.Client.Entities
{
    public interface IEntityBase
    {
        IShareFileClient Client { get; set; }
        string Entity { get; set; }
    }

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
