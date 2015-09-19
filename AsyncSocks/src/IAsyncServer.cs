﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocks
{
    public interface IAsyncServer
    {
        IConnectionManager ConnectionManager { get; }
        ClientConfig ClientConfig { get; }

        event NewMessageReceived OnNewMessageReceived;
        event PeerDisconnected OnPeerDisconnected;
        event NewClientConnected OnNewClientConnected;

        void Start();
        void Stop();
    }
}
