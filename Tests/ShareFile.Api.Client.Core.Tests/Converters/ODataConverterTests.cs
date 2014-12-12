using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Models;
using File = ShareFile.Api.Models.File;

namespace ShareFile.Api.Client.Core.Tests.Converters
{
    [TestFixture]
    public class ODataConverterTests : BaseTests
    {
        protected JsonSerializer GetSerializer()
        {
            return new JsonSerializer
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Converters = { new ODataConverter(), new StringEnumConverter(), new SafeEnumConverter() }
            };
        }

        protected Folder GetFolder()
        {
            var folder = new Folder
            {
                Children = new List<Item>()
                {
                    new File
                    {
                        FileName = "File 1.txt",
                    }
                },
                FileName = "Folder 1"
            };

            return folder;
        }

        [Test]
        public void CreateFolderFromItem()
        {
            var folder = GetFolder();

            folder.MetadataUrl =
                "https://onprem.sharefile.local/sf/v3/$metadata#Items/ShareFile.Api.Models.Folder@Element";
            folder.Children.First().MetadataUrl =
                "https://onprem.sharefile.local/sf/v3/$metadata#Items/ShareFile.Api.Models.File@Element";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);
            
            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));
            
            var item = serializer.Deserialize<ODataObject>(jsonReader) as Item;
            item.Should().NotBeNull();
            item.GetType().Should().Be(typeof (Folder));
        }

        [Test]
        public void CreateFolder_OData_Type()
        {
            var folder = GetFolder();
            folder.__type = "ShareFile.Api.Models.Folder";
            folder.Children.First().__type =
                "ShareFile.Api.Models.File";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataObject>(jsonReader) as Item;
            item.Should().NotBeNull();
            item.GetType().Should().Be(typeof(Folder));
            (item as Folder).Children.FirstOrDefault().GetType().Should().Be(typeof(File));
        }

        [Test]
        public void CreateFolder_MalformedMetadata_ODataTypeFallback()
        {
            var folder = GetFolder();
            folder.__type = "ShareFile.Api.Models.Folder";
            folder.MetadataUrl =
                "https://";
            
            folder.Children.First().__type =
                "ShareFile.Api.Models.File";
            folder.Children.First().MetadataUrl =
                "https://";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataObject>(jsonReader) as Item;
            item.Should().NotBeNull();
            item.GetType().Should().Be(typeof(Folder));
            (item as Folder).Children.FirstOrDefault().GetType().Should().Be(typeof(File));
        }

        [Test]
        public void CreateFolder_Entity_Cast()
        {
            var folder = GetFolder();
            folder.MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#Items/Folder";
            folder.Children.First().MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#Items/File";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataObject>(jsonReader) as Item;
            item.Should().NotBeNull();
            item.GetType().Should().Be(typeof(Folder));
        }

        [Test]
        public void CreateFolder_Cast()
        {
            var folder = GetFolder();
            folder.MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.Folder";
            folder.Children.First().MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.File";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataObject>(jsonReader) as Item;
            item.Should().NotBeNull();
            item.GetType().Should().Be(typeof(Folder));
        }

        [Test]
        public void CreateFeed_Cast()
        {
            var feed = new ODataFeed<Item>();
            feed.Feed = new List<Folder>()
            {
                GetFolder()
            };
            (feed.Feed.First()).MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.Folder";
            ((feed.Feed.First() as Folder).Children.First()).MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.File";
            feed.MetadataUrl = "https://labs.sf-api.com/sf/v3/$metadata#Items";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, feed);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataFeed<Item>>(jsonReader);
            item.Should().NotBeNull();
            item.Feed.First().GetType().Should().Be(typeof (Folder));
        }

        [Test]
        public void CreateFeed_NoMetadata()
        {
            var feed = new ODataFeed<Item>();
            feed.Feed = new List<Folder>()
            {
                GetFolder()
            };
            (feed.Feed.First()).MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.Folder";
            ((feed.Feed.First() as Folder).Children.First()).MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.File";
            feed.MetadataUrl = "";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, feed);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<ODataFeed<Item>>(jsonReader);
            item.Should().NotBeNull();
            item.Feed.First().GetType().Should().Be(typeof(Folder));
        }

        [Test]
        public void CreateFeed_String_NoMetadata()
        {
            var jsonReader =
                new JsonTextReader(
                    new StringReader(
                        "{\"odata.metadata\":\"\",\"odata.count\":1,\"value\":[{\"Name\":\"Shared Documents\",\"FileName\":\"Shared Documents\",\"ProgenyEditDate\":\"0001-01-01T08:00:00Z\",\"Path\":\"/c0cAkAjf7GCKmrm1PbIkf5yHgyrl8dE12wVNBsmmpgID!KT5OQ5n70eFAYRRRz8NpZH6mhSv42fmH7edX3dFsQ__\",\"StreamID\":\"cnNOPm!0FGJcAYd4eAAdU1P31k3hyDHxcmiWfjjpRm4k76FyeXZ-lztHSQuKpj3YTwLS9Ne0Hr6zoQ5hn2kiBllhe0bISQsws-Z678hkXCk_\",\"odata.metadata\":\"https://szqatest2.sharefiletest.com/sp/v3/$metadata#Items/ShareFile.Api.Models.Folder@Element\",\"Id\":\"cnNOPm!0FGJcAYd4eAAdU1P31k3hyDHxcmiWfjjpRm4k76FyeXZ-lztHSQuKpj3YTwLS9Ne0Hr6zoQ5hn2kiBllhe0bISQsws-Z678hkXCk_\",\"url\":\"https://szqatest2.sharefiletest.com/sp/v3/Items(cnNOPm!0FGJcAYd4eAAdU1P31k3hyDHxcmiWfjjpRm4k76FyeXZ-lztHSQuKpj3YTwLS9Ne0Hr6zoQ5hn2kiBllhe0bISQsws-Z678hkXCk_)\"}]}"));

            var serializer = GetSerializer();
            var item = serializer.Deserialize<ODataFeed<Item>>(jsonReader);
            item.Should().NotBeNull();
            item.Feed.First().GetType().Should().Be(typeof(Folder));
        }

        [Test]
        public void VerifyTypeOverrideSupport()
        {
            var test = new ShareFileClient(BaseUriString);
            test.RegisterType<FolderTestClass, Folder>();

            var folder = GetFolder();
            folder.MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.Folder";
            folder.Children.First().MetadataUrl =
                "https://labs.sf-api.com/sf/v3/$metadata#ShareFile.Api.Models.File";

            var serializer = GetSerializer();

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, folder);

            var jsonReader = new JsonTextReader(new StringReader(writer.ToString()));

            var item = serializer.Deserialize<Item>(jsonReader);
            item.GetType().Should().Be(typeof (FolderTestClass));
        }

        public class FolderTestClass : Folder
        {
            public string TestProp1 { get; set; }
        }
    }
}
