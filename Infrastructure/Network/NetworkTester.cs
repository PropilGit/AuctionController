using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace AuctionController.Infrastructure.Network
{
    class NetworkTester
    {
        Ping myPing = new Ping();

        byte[] buffer = new byte[32];
        int timeout = 1000;

        PingOptions pingOptions = new PingOptions();

        bool Ping(string host = "google.com")
        {
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            if (reply.Status == IPStatus.Success) return true;
            else return false;
        }
    }
}
