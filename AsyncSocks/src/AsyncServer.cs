﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSocks
{
    public delegate void NewPeerConnectionDelegate(IPeerConnection client);
    public class AsyncServer
    {
        private IClientConnectionAgent clientConnectionAgent;
        private IConnectionManager connectionManager;
        private ITcpListener tcpListener;

        public event NewClientMessageDelegate OnNewMessageReceived;
        public event NewPeerConnectionDelegate OnNewClientConnected;

        public AsyncServer(IClientConnectionAgent clientConnectionAgent, IConnectionManager connectionManager, ITcpListener tcpListener)
        {
            // TODO: Complete member initialization
            this.clientConnectionAgent = clientConnectionAgent;
            this.connectionManager = connectionManager;
            this.tcpListener = tcpListener;
            this.connectionManager.OnNewClientMessageReceived += connectionManager_OnNewClientMessageReceived;

            this.clientConnectionAgent.OnNewClientConnection += clientConnectionAgent_OnNewClientConnection;
        }

        void clientConnectionAgent_OnNewClientConnection(ITcpClient client)
        {
            if (OnNewClientConnected != null) 
            {
                OnNewClientConnected(new PeerConnectionFactory().Create(client));
            }
        }

        private void connectionManager_OnNewClientMessageReceived(IPeerConnection sender, byte[] message)
        {
            if (OnNewMessageReceived != null)
            {
                OnNewMessageReceived(sender, message);
            }
        }

        public void Start()
        {
            clientConnectionAgent.Start();
        }

        public void Stop()
        {
            clientConnectionAgent.Stop();
            connectionManager.CloseAllConnetions();
        }
    }
}
