﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace AsyncSocks
{
    public class InboundMessageSpoolerRunnable : IInboundMessageSpoolerRunnable
    {
        private INetworkMessageReader networkMessageReader;
        private BlockingCollection<NetworkMessage> queue;
        private AutoResetEvent startedEvent = new AutoResetEvent(false);
        private bool shouldStop;
        private bool running;

        public InboundMessageSpoolerRunnable(INetworkMessageReader networkMessageReader, BlockingCollection<NetworkMessage> queue)
        {
            this.networkMessageReader = networkMessageReader;
            this.queue = queue;
        }

        public void Spool()
        {
            try
            {
                byte[] message = networkMessageReader.Read();
                queue.Add(new NetworkMessage(null, message));
            }
            catch (ThreadInterruptedException)
            {
                
            }
        }

        public void Run()
        {
            running = true;
            startedEvent.Set();
            while (!shouldStop)
            {
                Spool();
            }
            running = false;
        }

        public bool WaitStarted()
        {
            return startedEvent.WaitOne();
        }

        public void Stop()
        {
            shouldStop = true;
        }

        public bool IsRunning
        {
            get { return running; }
        }


        public BlockingCollection<NetworkMessage> Queue
        {
            get
            {
                return queue;
            }
        }
    }
}
