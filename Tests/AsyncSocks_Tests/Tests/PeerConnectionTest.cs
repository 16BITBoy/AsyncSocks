﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsyncSocks;
using Moq;
using System.Text;

namespace AsyncSocks_Tests.Tests
{
    [TestClass]
    public class PeerConnectionTest
    {
        private Mock<IInboundMessageSpooler> inboundSpoolerMock;
        private Mock<IOutboundMessageSpooler> outboundSpoolerMock;
        private Mock<ITcpClient> tcpClientMock;
        private PeerConnection connection;

        [TestInitialize]
        public void BeforeEach()
        {
            inboundSpoolerMock = new Mock<IInboundMessageSpooler>();
            outboundSpoolerMock = new Mock<IOutboundMessageSpooler>();
            tcpClientMock = new Mock<ITcpClient>();

            var inboundSpooler = inboundSpoolerMock.Object;
            var outboundSpooler = outboundSpoolerMock.Object;
            var tcpClient = tcpClientMock.Object;

            connection = new PeerConnection(inboundSpooler, outboundSpooler, tcpClient);
        }

        [TestMethod]
        public void SendMessageShouldTellOutboundMessageSpoolerToEnqueueTheMessage()
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes("This is a test message");

            outboundSpoolerMock.Setup(x => x.Enqueue(messageBytes)).Verifiable();
            
            connection.SendMessage(messageBytes);

            outboundSpoolerMock.Verify();
        }

        [TestMethod]
        public void StartSpoolersShouldStartInboundAndOutboundMessageSpooling()
        {
            inboundSpoolerMock.Setup(x => x.Start()).Verifiable();
            outboundSpoolerMock.Setup(x => x.Start()).Verifiable();
            
            connection.StartSpoolers();
            
            inboundSpoolerMock.Verify();
            outboundSpoolerMock.Verify();
        }

        [TestMethod]
        public void CloseShouldStopSpoolersAndCloseConnectionWithPeer()
        {
            inboundSpoolerMock.Setup(x => x.Stop()).Verifiable();
            outboundSpoolerMock.Setup(x => x.Stop()).Verifiable();
            tcpClientMock.Setup(x => x.Close()).Verifiable();

            connection.Close();

            inboundSpoolerMock.Verify();
            outboundSpoolerMock.Verify();
            tcpClientMock.Verify();
        }
    }
}
