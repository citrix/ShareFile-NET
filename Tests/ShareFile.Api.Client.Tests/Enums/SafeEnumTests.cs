using System;

using FluentAssertions;

using NUnit.Framework;

using ShareFile.Api.Client.Core.Tests;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Tests.Enums
{
    [TestFixture]
    public class SafeEnumTests : BaseTests
    {
        [Test]
        public void SafeEnum_Equals_TwoNulls_True()
        {
            // Arrange
            SafeEnum<UserRole> left = new SafeEnum<UserRole>();
            SafeEnum<UserRole> right = new SafeEnum<UserRole>();

            // Act
            left.Equals(right).Should().BeTrue();
        }

        [Test]
        public void SafeEnum_Equals_LeftNull_False()
        {
            // Arrange
            SafeEnum<UserRole> left = new SafeEnum<UserRole>();
            SafeEnum<UserRole> right = SafeEnum<UserRole>.Create(UserRole.AdminBilling);

            // Act
            left.Equals(right).Should().BeFalse();
        }

        [Test]
        public void SafeEnum_Equals_RightNull_False()
        {
            // Arrange
            SafeEnum<UserRole> left = SafeEnum<UserRole>.Create(UserRole.AdminBilling); 
            SafeEnum<UserRole> right = new SafeEnum<UserRole>();

            // Act
            left.Equals(right).Should().BeFalse();
        }

        [Test]
        public void SafeEnum_Equals_Coerce_LeftNull_False()
        {
            // Arrange
            SafeEnum<UserRole> left = new SafeEnum<UserRole>();

            // Act
            (left == UserRole.AdminConnectors).Should().BeFalse();
        }

        [Test]
        public void SafeEnum_Equals_Coerce_RightNull_False()
        {
            // Arrange
            SafeEnum<UserRole> right = new SafeEnum<UserRole>();

            // Act
            (UserRole.AdminConnectors == right).Should().BeFalse();
        }

        [Test]
        public void SafeEnum_Equals_DifferentValues_False()
        {
            // Arrange
            SafeEnum<UserRole> left = SafeEnum<UserRole>.Create(UserRole.CreateBoxConnectors);
            SafeEnum<UserRole> right = SafeEnum<UserRole>.Create(UserRole.AdminBilling);

            // Act
            (left == right).Should().BeFalse();
        }

        [Test]
        public void SafeEnum_Equals_SameValues_True()
        {
            // Arrange
            SafeEnum<UserRole> left = SafeEnum<UserRole>.Create(UserRole.AdminBilling);
            SafeEnum<UserRole> right = SafeEnum<UserRole>.Create(UserRole.AdminBilling);

            // Act
            (left == right).Should().BeTrue();
        }

        [TestCase("One", "One", ExpectedResult = true)]
        [TestCase("One", "one", ExpectedResult = true)]
        [TestCase("One", "Two", ExpectedResult = false)]
        [TestCase("One", "", ExpectedResult = false)]
        [TestCase("One", null, ExpectedResult = false)]
        [TestCase(null, "one", ExpectedResult = false)]
        public bool SafeEnum_Equals_StringCompare(string leftStr, string rightStr)
        {
            // Arrange
            SafeEnum<UserRole> left = new SafeEnum<UserRole>() { Value = leftStr };
            SafeEnum<UserRole> right = new SafeEnum<UserRole>() { Value = rightStr };

            // Act
            return left == right;
        }

        [Test]
        public void SafeEnum_Equals_OneString_OneEnum_True()
        {
            // Arrange
            SafeEnum<UserRole> left = SafeEnum<UserRole>.Create(UserRole.AdminBilling);
            SafeEnum<UserRole> right = new SafeEnum<UserRole>() { Value = "AdminBilling" };

            // Act
            (left == right).Should().BeTrue();
        }

        [Test]
        public void SafeEnum_Equals_OneString_OneEnum_False()
        {
            // Arrange
            SafeEnum<UserRole> left = SafeEnum<UserRole>.Create(UserRole.AdminBilling);
            SafeEnum<UserRole> right = new SafeEnum<UserRole>() { Value = "NewEnum" };

            // Act
            (left == right).Should().BeFalse();
        }

        [Flags]
        private enum MyFlagsEnum
        {
            Flag1 = 0x1,
            Flag2 = 0x2,
        }

        [Test]
        public void SafeEnum_Flag_Or()
        {
            SafeEnum<MyFlagsEnum> left = MyFlagsEnum.Flag1;
            MyFlagsEnum right = MyFlagsEnum.Flag2;

            MyFlagsEnum both = MyFlagsEnum.Flag1 | MyFlagsEnum.Flag2;
            (left | right).Should().Be(SafeEnum<MyFlagsEnum>.Create(both));
        }

        [Test]
        public void SafeEnum_Flag_And()
        {
            SafeEnum<MyFlagsEnum> value = MyFlagsEnum.Flag1 | MyFlagsEnum.Flag2;
            MyFlagsEnum mask = MyFlagsEnum.Flag2;

            bool hasFlag = (value & mask) == mask;
            hasFlag.Should().BeTrue();
        }
    }
}
