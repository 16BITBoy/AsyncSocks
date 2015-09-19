﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocks
{
    public class AsyncServer : IAsyncServer
    {
        private IClientConnectionAgent clientConnectionAgent;
        private IConnectionManager connectionManager;
        private ITcpListener tcpListener;        

        public IConnectionManager ConnectionManager
        {
            get
            {
                return connectionManager;
            }
        }

        public ClientConfig ClientConfig { get; }

        public event NewMessageReceived OnNewMessageReceived;
        public event NewClientConnected OnNewClientConnected;
        public event PeerDisconnected OnPeerDisconnected;

        public AsyncServer(IClientConnectionAgent clientConnectionAgent, IConnectionManager connectionManager, ITcpListener tcpListener, ClientConfig clientConfig)
        {
            ClientConfig = clientConfig;
            this.clientConnectionAgent = clientConnectionAgent;
            this.connectionManager = connectionManager;
            this.tcpListener = tcpListener;
            this.connectionManager.OnNewMessageReceived += connectionManager_OnNewClientMessageReceived;
            this.connectionManager.OnPeerDisconnected += ConnectionManager_OnPeerDisconnected;
            this.clientConnectionAgent.OnNewClientConnection += clientConnectionAgent_OnNewClientConnection;
        }

        private void ConnectionManager_OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            var onPeerDisconnected = OnPeerDisconnected;

            if (onPeerDisconnected != null)
            {
                onPeerDisconnected(this, e);
            }
        }

        void clientConnectionAgent_OnNewClientConnection(object sender, NewClientConnectedEventArgs e)
        {
            connectionManager.Add(e.Client);
            if (OnNewClientConnected != null) 
            {
                OnNewClientConnected(this, e);
            }
        }

        private void connectionManager_OnNewClientMessageReceived(object sender, NewMessageReceivedEventArgs e)
        {
            if (OnNewMessageReceived != null)
            {
                OnNewMessageReceived(this, e);
            }
        }

        public void Start()
        {
            clientConnectionAgent.Start();
        }

        public void Stop()
        {
            clientConnectionAgent.Stop();
            connectionManager.CloseAllConnections();
        }

        public static AsyncServer Create(IPEndPoint localEndPoint)
        {
            var clientConfig = ClientConfig.GetDefault();
            return Create(localEndPoint, clientConfig);
        }

        public static AsyncServer Create(IPEndPoint localEndPoint, ClientConfig clientConfig)
        {
            BaseTcpListener tcpListener = new BaseTcpListener(new TcpListener(localEndPoint));
            ClientConnectionAgent clientConnectionAgent = ClientConnectionAgent.Create(tcpListener, clientConfig);
            ConnectionManager connectionManager = new ConnectionManager(new Dictionary<IPEndPoint, IAsyncClient>());
            return new AsyncServer(clientConnectionAgent, connectionManager, tcpListener, clientConfig);
        }
    }
}
