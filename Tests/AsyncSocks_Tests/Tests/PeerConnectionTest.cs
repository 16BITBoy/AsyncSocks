﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsyncSocks;
using Moq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AsyncSocks_Tests.Tests
{
    [TestClass]
    public class PeerConnectionTest
    {
        private Mock<IInboundMessageSpooler> inboundSpoolerMock;
        private Mock<IOutboundMessageSpooler> outboundSpoolerMock;
        private Mock<IMessagePoller> messagePollerMock;
        private Mock<ITcpClient> tcpClientMock;
        private IPeerConnection connection;

        [TestInitialize]
        public void BeforeEach()
        {
            inboundSpoolerMock = new Mock<IInboundMessageSpooler>();
            outboundSpoolerMock = new Mock<IOutboundMessageSpooler>();
            messagePollerMock = new Mock<IMessagePoller>();
            tcpClientMock = new Mock<ITcpClient>();

            var inboundSpooler = inboundSpoolerMock.Object;
            var outboundSpooler = outboundSpoolerMock.Object;
            var tcpClient = tcpClientMock.Object;
            var messagePoller = messagePollerMock.Object;

            connection = new PeerConnection(inboundSpooler, outboundSpooler, messagePoller, tcpClient);
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
        public void StartShouldStartInboundAndOutboundMessageSpoolingAndMessagePolling()
        {
            inboundSpoolerMock.Setup(x => x.Start()).Verifiable();
            outboundSpoolerMock.Setup(x => x.Start()).Verifiable();
            messagePollerMock.Setup(x => x.Start()).Verifiable();
            
            connection.Start();
            
            inboundSpoolerMock.Verify();
            outboundSpoolerMock.Verify();
            messagePollerMock.Verify();
        }

        [TestMethod]
        public void CloseShouldStopSpoolersAndCloseConnectionWithPeer()
        {
            inboundSpoolerMock.Setup(x => x.Stop()).Verifiable();
            outboundSpoolerMock.Setup(x => x.Stop()).Verifiable();
            tcpClientMock.Setup(x => x.Close()).Verifiable();
            messagePollerMock.Setup(x => x.Stop()).Verifiable();

            connection.Close();

            inboundSpoolerMock.Verify();
            outboundSpoolerMock.Verify();
            tcpClientMock.Verify();
            messagePollerMock.Verify();
        }

        [TestMethod]
        public void RemoteEndPointPropertyShouldReturnValuesAccordingToTcpClientObject()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("80.80.80.80"), 80);
            var clientMock = new Mock<ISocket>();
            tcpClientMock.Setup(x => x.Client).Returns(clientMock.Object).Verifiable();
            clientMock.Setup(x => x.RemoteEndPoint).Returns(endPoint).Verifiable();
            Assert.AreEqual(endPoint, connection.RemoteEndPoint);
        }

        [TestMethod]
        public void IsActiveShouldReturnTrueIfBothSpoolersAreRunning()
        {
            bool inboundSpoolerStarted = false;
            bool outboundSpoolerStarted = false;

            inboundSpoolerMock.Setup(x => x.Start()).Callback(() => inboundSpoolerStarted = true).Verifiable();
            outboundSpoolerMock.Setup(x => x.Start()).Callback(() => outboundSpoolerStarted = true).Verifiable();

            inboundSpoolerMock.Setup(x => x.IsRunning()).Returns(() => { return inboundSpoolerStarted;}).Verifiable();
            outboundSpoolerMock.Setup(x => x.IsRunning()).Returns(() => { return outboundSpoolerStarted;}).Verifiable();

            connection.Start();

            bool active = connection.IsActive();
            
            inboundSpoolerMock.Verify();
            outboundSpoolerMock.Verify();

            Assert.IsTrue(active);
            
        }

        
    }
}
