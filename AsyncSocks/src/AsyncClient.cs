﻿using AsyncSocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocks
{
    namespace Exceptions
    {
        [Serializable]
        public class MessageTooBigException : Exception
        {
            public MessageTooBigException(string message) : base(message)
            {
            }
        }
    }

    public class AsyncClient : IAsyncClient
    {
        private IInboundMessageSpooler inboundSpooler;
        private IOutboundMessageSpooler outboundSpooler;
        private ITcpClient tcpClient;
        private IMessagePoller poller;
        private IOutboundMessageFactory messageFactory;
        private bool isClosing;

        public event NewMessageReceived OnNewMessageReceived;
        public event PeerDisconnected OnPeerDisconnected;

        public AsyncClient(IInboundMessageSpooler inboundSpooler, IOutboundMessageSpooler outboundSpooler, IMessagePoller poller, IOutboundMessageFactory messageFactory, ITcpClient tcpClient, ClientConfig clientConfig)
        {
            this.inboundSpooler = inboundSpooler;
            this.outboundSpooler = outboundSpooler;
            this.poller = poller;
            this.tcpClient = tcpClient;
            this.messageFactory = messageFactory;
            ClientConfig = clientConfig;

            setupEvents();
            if (tcpClient.Connected) saveEndPoints();
        }

        private void setupEvents()
        {
            poller.OnNewClientMessageReceived += poller_OnNewClientMessageReceived;
            inboundSpooler.OnPeerDisconnected += InboundSpooler_OnPeerDisconnected;
        }

        private void saveEndPoints()
        {
            LocalEndPoint = tcpClient.Socket.LocalEndPoint;
            RemoteEndPoint = tcpClient.Socket.RemoteEndPoint;
        }

        private void InboundSpooler_OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            Task.Run(() =>
            {
                var onPeerDisconnected = OnPeerDisconnected;
                if (onPeerDisconnected != null)
                {
                    var ev = new PeerDisconnectedEventArgs(this);
                    onPeerDisconnected(this, ev);
                }

                if (!isClosing) Close();
            });
            
        }

        private void poller_OnNewClientMessageReceived(object sender, NewMessageReceivedEventArgs e)
        {
            if (OnNewMessageReceived != null)
            {
                var newE = new NewMessageReceivedEventArgs(this, e.Message);
                OnNewMessageReceived(this, newE);
            }
        }

        public void SendMessage(byte[] messageBytes)
        {
            //OutboundMessage msg = messageFactory.Create(messageBytes, null);
            //outboundSpooler.Enqueue(msg);
            SendMessage(messageBytes, null);
        }

        public void SendMessage(byte[] messageBytes, Action<bool, SocketException> callback)
        {
            if (messageBytes.Length > ClientConfig.MaxMessageSize)
            {
                int maxSize = ClientConfig.MaxMessageSize;
                int msgSize = messageBytes.Length;
                throw new MessageTooBigException("Max size expected for outgoing messages is: " + maxSize.ToString() + " Received message of size: " + msgSize.ToString());
            }

            OutboundMessage msg = messageFactory.Create(messageBytes, callback);
            outboundSpooler.Enqueue(msg);
        }

        public void Start()
        {
            inboundSpooler.Start();
            outboundSpooler.Start();
            poller.Start();
        }

        public void Close()
        {
            isClosing = true;
            tcpClient.Close();
            poller.Stop();
            inboundSpooler.Stop();
            outboundSpooler.Stop();
        }

        public EndPoint RemoteEndPoint { get; private set; }
        
        public EndPoint LocalEndPoint { get; private set; }

        public bool IsActive()
        {
            return (inboundSpooler.IsRunning() && outboundSpooler.IsRunning() && poller.IsRunning());
        }

        public ITcpClient TcpClient
        {
            get { return tcpClient; }
        }

        public ClientConfig ClientConfig { get; }

        public static AsyncClient Create(IPEndPoint remoteIpAddress, ClientConfig config)
        {
            TcpClient tcpClient;
            BaseTcpClient client;

            if (remoteIpAddress == null)
            {
                tcpClient = new TcpClient();
                client = new BaseTcpClient(tcpClient);
            }
            else
            {
                tcpClient = new TcpClient(remoteIpAddress.Address.ToString(), remoteIpAddress.Port);
                client = new BaseTcpClient(tcpClient);
            }

            if (config == null)
            {
                return (AsyncClient)new AsyncClientFactory().Create(client);
            }
            else
            {
                return (AsyncClient)new AsyncClientFactory(config).Create(client);
            }

        }

        public static AsyncClient Create(ClientConfig config)
        {
            return Create(null, config);
        }

        public static AsyncClient Create(IPEndPoint remoteIpAddress)
        {
            return Create(remoteIpAddress, null);
        }

        public static AsyncClient Create()
        {
            return Create(null, null);
        }

        public void Connect()
        {
            Connect(null);
            Start();
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null)
            {
                tcpClient.Connect();
            }
            else
            {
                tcpClient.Connect(remoteEndPoint);
            }
            
            saveEndPoints();
            Start();
        }
        
    }
}
