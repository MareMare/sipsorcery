//-----------------------------------------------------------------------------
// Filename: SctpTransportUnitTest.cs
//
// Description: Unit tests for the SctpTransport class.
//
// Author(s):
// Aaron Clauson (aaron@sipsorcery.com)
// 
// History:
// 22 Mar 2021	Aaron Clauson	Created, Dublin, Ireland.
//
// License: 
// BSD 3-Clause "New" or "Revised" License, see included LICENSE.md file.
//-----------------------------------------------------------------------------

using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SIPSorcery.Sys;
using TinyJson;
using Xunit;

namespace SIPSorcery.Net.UnitTests
{
    public class SctpTransportUnitTest
    {
        private ILogger logger = null;

        public SctpTransportUnitTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            logger = SIPSorcery.UnitTests.TestLogHelper.InitTestLogger(output);
        }

        /// <summary>
        /// Tests getting an INIT ACK packet in response to an INIT packet
        /// works correctly and generates a usable state cookie.
        /// </summary>
        [Fact]
        public void GetInitAckPacket()
        {
            logger.LogDebug("--> " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            logger.BeginScope(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var sctpTransport = new MockSctpTransport();

            uint remoteTag = Crypto.GetRandomUInt();
            uint remoteTSN = Crypto.GetRandomUInt();
            uint remoteARwnd = SctpAssociation.DEFAULT_ADVERTISED_RECEIVE_WINDOW;

            SctpPacket init = new SctpPacket(5000, 5000, 0);
            SctpInitChunk initChunk = new SctpInitChunk(SctpChunkType.INIT, remoteTag, remoteTSN, remoteARwnd);
            init.Chunks.Add(initChunk);

            var initAck = sctpTransport.GetInitAck(init);

            Assert.NotNull(initAck);

            var initAckChunk = initAck.Chunks.Single() as SctpInitChunk;

            Assert.NotNull(initAckChunk);

            var cookieParameter = initAckChunk.VariableParameters.Single(x => x.KnownType == SctpChunkParameterType.StateCookie);
            var cookie = JSONParser.FromJson<SctpTransportCookie>(Encoding.UTF8.GetString(cookieParameter.ParameterValue));

            logger.LogDebug($"Cookie: {cookie.ToJson()}");

            Assert.NotNull(cookie.CreatedAt);
            Assert.NotNull(cookie.HMAC);
            Assert.Equal(cookie.HMAC, sctpTransport.GetCookieHMAC(cookieParameter.ParameterValue));
        }
    }

    /// <summary>
    /// This mock is used to get access to the SctpTransport protected methods.
    /// </summary>
    internal class MockSctpTransport : SctpTransport
    {
        public MockSctpTransport()
        { }

        public SctpPacket GetInitAck(SctpPacket initPacket)
        {
            return base.GetInitAck(initPacket, null);
        }

        public new string GetCookieHMAC(byte[] buffer)
        {
            return base.GetCookieHMAC(buffer);
        }

        public override void Send(string associationID, byte[] buffer, int offset, int length)
        { }
    }
}