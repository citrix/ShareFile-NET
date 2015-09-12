using System;
// ReSharper disable once CheckNamespace


namespace ShareFile.Api.Client.Entities
{
    public interface IEntityBase
    {
        IShareFileClient Client { get; set; }
        string Entity { get; set; }

        Uri GetEntityUriFromId(string id);
    }

    public abstract class EntityBase : IEntityBase
    {
        protected EntityBase(IShareFileClient client, string entity)
        {
            Client = client;
            Entity = entity;
        }

        public IShareFileClient Client { get; set; }
        public string Entity { get; set; }

        private static readonly char[] TrimChars = { '/' };

        /// <summary>
        /// Will return a composed Uri that will point to <see name="Entity"/>(<see name="id"/>) for the BaseUri
        /// </summary>
        public Uri GetEntityUriFromId(string id)
        {
            string url = Client.BaseUri.ToString();

            return new Uri(url.TrimEnd(TrimChars) + "/" + Entity + "(" + id + ")", UriKind.RelativeOrAbsolute);
        }
    }
}
