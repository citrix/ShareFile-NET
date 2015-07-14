using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Extensions;
using System.Linq.Expressions;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class QueryExtensionTests : BaseTests
    {
        private void AssertSelect<T>(IQuery<T> query, params string[] selects) where T : class
        {
            var apiRequest = ApiRequest.FromQuery((QueryBase)query);
            string selectString = String.Join(",", selects.OrderBy(s => s.Length));
            apiRequest.QueryStringCollection.Should().Contain(odataParam => odataParam.Key == "$select" && odataParam.Value == selectString);
        }

        private void AssertExpand<T>(IQuery<T> query, params string[] expands) where T : class
        {
            var apiRequest = ApiRequest.FromQuery((QueryBase)query);
            string expandString = String.Join(",", expands.OrderBy(s => s.Length));
            apiRequest.QueryStringCollection.Should().Contain(odataParam => odataParam.Key == "$expand" && odataParam.Value == expandString);
        }

        private IQuery<Session> BuildSessionQuery<T>(Expression<Func<Session, T>> lambda)
        {
            var query = GetShareFileClient().Sessions.Get();
            query = QueryExtensions.ApplySelectsAndExpands(query, lambda);
            return query;
        }

        [Test]
        public void LambdaQuery_Select()
        {
            var query = BuildSessionQuery(session => session.IsAuthenticated);

            AssertSelect(query, "IsAuthenticated");
        }

        [Test]
        public void LambdaQuery_DontSelect()
        {
            var query = BuildSessionQuery(session => session.Name.Length);

            AssertSelect(query, "Name"); //not Name/Length
        }

        [Test]
        public void LambdaQuery_Expand()
        {
            var query = BuildSessionQuery(session => session.Principal);

            AssertExpand(query, "Principal");
        }

        [Test]
        public void LambdaQuery_SelectMultiple()
        {
            var query = BuildSessionQuery(session => new { Name = session.Name, Authenticated = session.IsAuthenticated });

            AssertSelect(query, "Name", "IsAuthenticated");
        }

        [Test]
        public void LambdaQuery_ExpandMultiple()
        {
            var query = BuildSessionQuery(session => new { Principal = session.Principal, DeviceUser = session.DeviceUser });

            AssertExpand(query, "Principal", "DeviceUser");
        }

        [Test]
        public void LambdaQuery_SelectExpandDeep()
        {
            var query = BuildSessionQuery(session => new
            {
                Email = session.Principal.Email,
                Username = session.Principal.Name,
                DeviceId = session.DeviceUser.Device.Id
            });

            AssertExpand(query,
                "Principal",
                "DeviceUser",
                "DeviceUser/Device");

            AssertSelect(query,
                "Principal/Email",
                "Principal/Name",
                "DeviceUser/Device/Id");
        }

        [Test]
        public void LambdaQuery_Cast()
        {
            var query = BuildSessionQuery(session => ((AccountUser)session.Principal).FullName);

            AssertExpand(query, "Principal");
            AssertSelect(query, "Principal/FullName");
        }

        [Test]
        public void LambdaQuery_As()
        {
            var query = BuildSessionQuery(session => (session.Principal as AccountUser).FullName);
            
            AssertExpand(query, "Principal");
            AssertSelect(query, "Principal/FullName");
        }

        [Test]
        public void LambdaQuery_Coalesce()
        {
            var query = BuildSessionQuery(session => (session.Principal ?? new Principal()).Email);

            AssertExpand(query, "Principal");
            AssertSelect(query, "Principal/Email");
        }

        [Test]
        public void LambdaQuery_Collection()
        {
            var query = BuildSessionQuery(session => (session.Principal as AccountUser).FavoriteFolders);

            AssertExpand(query, "Principal", "Principal/FavoriteFolders");
        }

        [Test]
        public void LambdaQuery_Sublambda()
        {
            var query = BuildSessionQuery(session => (session.Principal as AccountUser).FavoriteFolders.Where(fav => fav.FolderName.Length == 5));

            AssertExpand(query, "Principal", "Principal/FavoriteFolders");
            AssertSelect(query, "Principal/FavoriteFolders/FolderName");
        }

        [Test]
        public void LambdaQuery_MethodArg()
        {
            var query = BuildSessionQuery(session => BogusMethod(session.Principal.Name));

            AssertExpand(query, "Principal");
            AssertSelect(query, "Principal/Name");
        }

        private int BogusMethod(object ignore) { return 2; }

        [Test]
        public void LambdaQuery_ConstructorArg()
        {
            var query = BuildSessionQuery(session => new BogusClass(session.Principal.Name));
            
            AssertExpand(query, "Principal");
            AssertSelect(query, "Principal/Name");
        }

        private class BogusClass { 
            public BogusClass(object ignore) { }
            public object Foo { get; set; }
        }

        [Test]
        public void LambdaQuery_ConstructorInit()
        {
            var query = BuildSessionQuery(session => new BogusClass(session.Principal.Name) { Foo = session.DeviceUser.Device.Id });

            AssertExpand(query,
                "Principal",
                "DeviceUser",
                "DeviceUser/Device");

            AssertSelect(query,
                "Principal/Name",
                "DeviceUser/Device/Id");
        }

        [Test]
        public void LambdaQuery_CollectionInit()
        {
            var query = BuildSessionQuery(session => new List<int> { 
                session.Principal.Name.Length,
                (session.Principal as AccountUser).FavoriteFolders.Count()
            });

            AssertExpand(query, "Principal", "Principal/FavoriteFolders");
            AssertSelect(query, "Principal/Name");
        }

        [Test]
        public void LambdaQuery_Nested()
        {
            var query = BuildSessionQuery(session =>
                session.Principal.As((AccountUser accountUser) => new
                {
                    EnableSync = accountUser.Account.Preferences.EnableSync,
                    CanaryMaxVersion = accountUser.Account.ToolInformation.Single(toolInfo => toolInfo.ToolName == "WinSync").Version 
                },
                null));

            AssertExpand(query,
                "Principal",
                "Principal/Account",
                "Principal/Account/Preferences",
                "Principal/Account/ToolInformation");

            AssertSelect(query,
                "Principal/Account/Preferences/EnableSync",
                "Principal/Account/ToolInformation/ToolName",
                "Principal/Account/ToolInformation/Version");
        }

        private static string StaticField;
        private static string StaticProperty { get { return StaticField;  } }
        private static string StaticMethod() { return StaticField;  }
        
        [Test]
        public void LambdaQuery_StaticField()
        {
            var query = BuildSessionQuery(session => session.Id == QueryExtensionTests.StaticField);
            AssertSelect(query, "Id");
        }

        [Test]
        public void LambdaQuery_StaticProperty()
        {
            var query = BuildSessionQuery(session => session.Id == QueryExtensionTests.StaticProperty);
            AssertSelect(query, "Id");
        }

        [Test]
        public void LambdaQuery_StaticMethod()
        {
            var query = BuildSessionQuery(session => session.Id == QueryExtensionTests.StaticMethod());
            AssertSelect(query, "Id");
        }

        [Test]
        public void LambdaQuery_NonStaticMethod()
        {
            var query = BuildSessionQuery(session => session.Id.ToString());
            AssertSelect(query, "Id");
        }
    }
}
