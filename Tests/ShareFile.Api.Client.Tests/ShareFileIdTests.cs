using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ShareFile.Api.Client.Primitives;

namespace ShareFile.Api.Client.Tests
{
    [TestFixture]
    public class ShareFileIdTests
    {
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("fohb21a1d0fd4f7e8b100139fa41ea30")]
        [TestCase("fi823181-2731-2741-7247-247712472174")]
        [TestCase("ste01968-1e99-4e1c-9573-de953fe5a87d")]
        [TestCase("96e6d6d2-594e-465b-83f4-2a4c9d17ae8e")]
        [TestCase("96e6d6d2594e465b83f42a4c9d17ae8e")]
        [TestCase("ac6cef9d-c870-4081-b8b8-bd8f0c6c3739")]
        [TestCase("gsac6cef9d-c870-4081-b8b8-bd8f0c6c37")]
        [TestCase("gsac6cef9dc8704081b8b8bd8f0c6c37")]
        [TestCase("zpfed2b3f5-fbbf-4ed5-9a58-f1bd888f01")]
        [TestCase("s-123456789")]
        [TestCase("s123456789")]
        [TestCase("s-s123456789")]
        [TestCase("abc_0123456789")]
        [TestCase("top")]
        [TestCase("allshared")]
        [TestCase("favorites")]
        [TestCase("connectors")]
        [TestCase("c-sp")]
        [TestCase("c-pcc")]
        [TestCase("c-cifs")]
        [TestCase("c-o365")]
        [TestCase("c-documentum")]
        [TestCase("c-shareconnect")]
        [TestCase("box")]
        [TestCase("")]
        [TestCase(null)]
        public void SFID_Parsed_Equal(string input)
        {
            var sfId = new ShareFileId(input, mustParse: true);
            var output = sfId.ToString();
            input.Should().Be(output);
        }

        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Unparsed_Equal(string input)
        {
            var sfId = new ShareFileId(input, mustParse: false);
            var output = sfId.ToString();
            bool eq = string.Equals(input, output);
            eq.Should().BeTrue();
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("allshared")]
        public void SFID_UpperToString_Lower(string input)
        {
            input = input.ToUpper();
            var sfId = new ShareFileId(input);
            var output = sfId.ToString();
            output.Should().Be(input.ToLower());
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Equality(string input)
        {
            var sfId1 = new ShareFileId(input);
            var sfId2 = new ShareFileId(input);
            bool eq = sfId1.Equals(sfId2);
            eq.Should().BeTrue();
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        public void SFID_Equality_IgnoreHyphens(string input)
        {
            var sfId1 = new ShareFileId(input);
            var sfId2 = new ShareFileId(input.Replace("-", ""));
            bool eq = sfId1.Equals(sfId2);
            eq.Should().BeTrue();
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Equality_IgnoreCase(string input)
        {
            var sfId1 = new ShareFileId(input.ToLower());
            var sfId2 = new ShareFileId(input.ToUpper());
            bool eq = sfId1.Equals(sfId2);
            eq.Should().BeTrue();
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Hashcode(string input)
        {
            var sfId1 = new ShareFileId(input);
            var sfId2 = new ShareFileId(input);
            int h1 = sfId1.GetHashCode();
            int h2 = sfId2.GetHashCode();
            h1.Should().Be(h2);
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        public void SFID_Hashcode_IgnoreHyphens(string input)
        {
            var sfId1 = new ShareFileId(input);
            var sfId2 = new ShareFileId(input.Replace("-", ""));
            int h1 = sfId1.GetHashCode();
            int h2 = sfId2.GetHashCode();
            h1.Should().Be(h2);
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Hashcode_IgnoreCase(string input)
        {
            var sfId1 = new ShareFileId(input.ToLower());
            var sfId2 = new ShareFileId(input.ToUpper());
            int h1 = sfId1.GetHashCode();
            int h2 = sfId2.GetHashCode();
            h1.Should().Be(h2);
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase(null, null)]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", "MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Conversion_EqualityOperator_Equals(string left, string right)
        {
            ShareFileId sfId1 = left;
            ShareFileId sfId2 = right;
            bool eq = sfId1 == sfId2;
            eq.Should().BeTrue();
        }
        
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "fi823181-2731-2741-7247-247712472174")]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", null)]
        [TestCase(null, "fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", "MbpfNUHdTAdbPqG1-5V***DIFFERENT+==QILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        [TestCase("", null)]
        public void SFID_Conversion_EqualityOperator_NotEquals(string left, string right)
        {
            ShareFileId sfId1 = left;
            ShareFileId sfId2 = right;
            bool neq = sfId1 != sfId2;
            neq.Should().BeTrue();
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "fi823181-2731-2741-7247-247712472174")]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", null)]
        [TestCase(null, "fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", "MbpfNUHdTAdbPqG1-5V***DIFFERENT+==QILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        [TestCase("", null)]
        public void SFID_Conversion_MixedOperator_NotEquals(string left, string right)
        {
            ShareFileId sfId = left;
            bool eq1 = sfId == right;
            bool eq2 = right == sfId;
            eq1.Should().BeFalse();
            eq2.Should().BeFalse();
        }
        
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "foh")]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "fo")]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", "fohb21")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", "MbpfNUH")]
        public void SFID_StartsWith(string input, string prefix)
        {
            bool ok = input.StartsWith(prefix);
            ok.Should().BeTrue();
        }
        
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", 0, 3)]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", 0, 6)]
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", 6, 3)]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", 0, 6)]
        public void SFID_Substring(string input, int offset, int length)
        {
            string expected = input.Substring(offset, length);
            var sfId = new ShareFileId(input);
            string actual = input.Substring(offset, length);
            actual.Should().Be(expected);
        }

        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", false)]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", false)]
        [TestCase(null, true)]
        [TestCase("", false)]
        public void SFID_NullLiteral_Eq(string input, bool expectedEq)
        {
            var sfId = new ShareFileId(input);
            bool actualEq1 = sfId == null;
            bool actualEq2 = null == sfId;
            actualEq1.Should().Be(expectedEq);
            actualEq2.Should().Be(expectedEq);
        }
        
        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30", true)]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl", true)]
        [TestCase(null, false)]
        [TestCase("", true)]
        public void SFID_NullLiteral_Neq(string input, bool expectedNeq)
        {
            var sfId = new ShareFileId(input);
            bool actualNeq1 = sfId != null;
            bool actualNeq2 = null != sfId;
            actualNeq1.Should().Be(expectedNeq);
            actualNeq2.Should().Be(expectedNeq);
        }


        [TestCase("fohb21a1-d0fd-4f7e-8b10-0139fa41ea30")]
        [TestCase("fohb21a1d0fd4f7e8b100139fa41ea30")]
        [TestCase("top")]
        [TestCase("allshared")]
        [TestCase("")]
        [TestCase("MbpfNUHdTAdbPqG1-5V6tGEUfS10DcQxmVQILwLwkfbgUFeuo4zT9Rs34nKryiMl")]
        public void SFID_Length(string input)
        {
            var sfId = new ShareFileId(input);
            sfId.Length.Should().Be(input.Length);
        }
    }
}
