using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiPlatform;

namespace DebuggingClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // TCP testing
            NetworkComponents.TCPConnector clientConnector = new NetworkComponents.TCPConnector(OnMesageReceivedFromHost, false);
            while (!(clientConnector.ConnectTo("127.0.0.1", true)))
            {
            }

            clientConnector.Send(new HostInformation("DE98F9", "127.0.0.1", "Markus", 0.0f, true));

            clientConnector.Close();
        }

        private static void OnMesageReceivedFromHost(NetworkComponents.TCPConnection connection, NetworkComponents.IGridforceMessage message)
        {
            Console.WriteLine("Host sent " + message.GetType().ToString());
        }
    }
}
