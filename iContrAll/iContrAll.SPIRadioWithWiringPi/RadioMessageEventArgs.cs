﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iContrAll.SPIRadio
{
    public class RadioMessageEventArgs
    {
        //public string SenderId { get; set; }
        //public string TargetId { get; set; }
        //public string TAG { get; set; }
        //public string Channel { get; set; }
        //public byte[] ChannelVoltage { get; set; }
        //public byte[] ChannelDim { get; set; }

        public byte[] ReceivedBytes { get; set; }

        public RadioMessageEventArgs(byte[] receivedBytes)
        {
            this.ReceivedBytes = receivedBytes;
        }
    }
}
