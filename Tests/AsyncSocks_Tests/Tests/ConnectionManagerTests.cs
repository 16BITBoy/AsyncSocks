﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsyncSocks;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace AsyncSocks_Tests.Tests
{
    [TestClass]
    public class ConnectionManagerTests
    {
        private IConnectionManager connManager;
        private Dictionary<IPEndPoint, IPeerConnection> dict;
        private Mock<IMessagePoller> messagePollerMock;

        [TestInitialize]
        public void BeforeEach()
        {
            messagePollerMock = new Mock<IMessagePoller>();
            dict = new Dictionary<IPEndPoint, IPeerConnection>();
            connManager = new ConnectionManager(dict, messagePollerMock.Object);
        }
        
        //[TestMethod]
        //public void AddShouldCreateANewPeerConnectionFromTcpClientAndAddsItToItsList()
        //{
        //    var tcpClientMock = new Mock<ITcpClient>();
        //    var socketMock = new Mock<ISocket>();
        //    var peerConnectionMock = new Mock<IPeerConnection>();
        //    var inboundSpoolerMock = new Mock<IInboundMessageSpooler>();
        //    var outboundSpoolerMock = new Mock<IOutboundMessageSpooler>();
        //    var messagePollerMock = new Mock<IMessagePoller>();
            
        //    var endPoint = new IPEndPoint(IPAddress.Parse("80.80.80.80"), 80);

        //    //connectionFactoryMock.Setup(
        //    //    x => x.Create(
        //    //        inboundSpoolerMock.Object,
        //    //        outboundSpoolerMock.Object,
        //    //        messagePollerMock.Object,
        //    //        tcpClientMock.Object
        //    //    )
        //    //).Returns(peerConnectionMock.Object).Verifiable();

        //    tcpClientMock.Setup(x => x.Client).Returns(socketMock.Object).Verifiable();
        //    socketMock.Setup(x => x.RemoteEndPoint).Returns(endPoint).Verifiable();

        //    peerConnectionMock.Setup(x => x.Start()).Verifiable();
        //    peerConnectionMock.Setup(x => x.RemoteEndPoint).Returns(endPoint);

        //    connManager.Add(inboundSpoolerMock.Object, outboundSpoolerMock.Object, messagePollerMock.Object, tcpClientMock.Object);

        //    tcpClientMock.Verify();
        //    socketMock.Verify();
        //    connectionFactoryMock.Verify();
        //    peerConnectionMock.Verify();

        //    Assert.AreEqual(endPoint, dict[endPoint].RemoteEndPoint); 
        //}

        [TestMethod]
        public void AddShouldAddPeerConnectionObjectToItsDictionary()
        {
            var tcpClientMock = new Mock<ITcpClient>();
            var socketMock = new Mock<ISocket>();
            var peerConnectionMock = new Mock<IPeerConnection>();
            
            var endPoint = new IPEndPoint(IPAddress.Parse("80.80.80.80"), 80);

            peerConnectionMock.Setup(x => x.Start()).Verifiable();
            peerConnectionMock.Setup(x => x.RemoteEndPoint).Returns(endPoint).Verifiable(); 

            connManager.Add(peerConnectionMock.Object);

            peerConnectionMock.Verify();

            Assert.AreEqual(endPoint, dict[endPoint].RemoteEndPoint); 
        }

        [TestMethod]
        public void CloseAllConnectionsShouldCloseConnections()
        {
            var connections = new List<Mock<IPeerConnection>>();
            for(int i = 0; i < 10; i++)
            {
                var conn = new Mock<IPeerConnection>();
                var messagePollerMock = new Mock<IMessagePoller>();
                
                conn.Setup(x => x.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("80.80.80."+i.ToString()), 80));
                conn.Setup(x => x.Close()).Verifiable();

                connections.Add(conn);

                connManager.Add(conn.Object);
            }
            connManager.CloseAllConnetions();

            foreach(Mock<IPeerConnection> conn in connections)
            {
                conn.Verify();
            }
        }

        [TestMethod]
        public void OnNewClientMessageReceivedCallbacksShouldBeCalledWhenEventIsFiredByAPeerConnectionInstance()
        {
            var callbackCalledEvent = new AutoResetEvent(true);
            var peerConnectionMock = new Mock<IPeerConnection>();

            peerConnectionMock.Setup(x => x.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("80.80.80.80"), 80));

            connManager.Add(peerConnectionMock.Object);

            NewClientMessageDelegate callback = delegate(IPeerConnection sender, byte[] message)
            {
                callbackCalledEvent.Set();
            };

            connManager.OnNewClientMessageReceived += callback;

            messagePollerMock.Raise(x => x.OnNewClientMessageReceived += null, peerConnectionMock.Object, Encoding.ASCII.GetBytes("This is a test!"));

            bool called = callbackCalledEvent.WaitOne(2000);

            Assert.IsTrue(called);
        }
    }
}
