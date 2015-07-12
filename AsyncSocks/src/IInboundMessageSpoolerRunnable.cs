﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocks
{
    public interface IInboundMessageSpoolerRunnable : IRunnable
    {
        void Spool();
        BlockingCollection<byte[]> Queue { get; }
    }
}
