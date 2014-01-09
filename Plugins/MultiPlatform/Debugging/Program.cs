using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiPlatform;

namespace Debugging
{
    class Program
    {
        static void Main(string[] args)
        {
            // UDP testing
            HostInformation hostInformation = new HostInformation("DE98F9", "127.0.0.1", "Markus", 0.0f, true);
            byte[] byteArray = HostInformation.Serialize(hostInformation);
            HostInformation deserialized = HostInformation.Deserialize(byteArray);

            NetworkComponents.UDPBroadcaster broadcaster = new NetworkComponents.UDPBroadcaster();
            NetworkComponents.UDPReceiver receiver = new NetworkComponents.UDPReceiver(null);
            broadcaster.Broadcast(hostInformation);


            // TCP testing
            //NetworkComponents.TCPConnector hostConnector = new NetworkComponents.TCPConnector(OnMesageReceivedFromClient, true);

            //while (!(hostConnector.Listen()))
            //{
            //}

            //hostConnector.Send(hostInformation);

            //hostConnector.Close();

            UserInformation user = new UserInformation("Markus", "127.0.0.1");
            byte[] data = UserInformation.Serialize(user);
            user = UserInformation.Deserialize(data);

            JoinRequest request = new JoinRequest(user, PasswordFunctions.HashPassword("penis1337"));
            data = JoinRequest.Serialize(request);
            request = JoinRequest.Deserialize(data);



            NetworkComponents.TCPConnector hostConnector = new NetworkComponents.TCPConnector(OnMesageReceivedFromClient, false);

            while (!(hostConnector.AcceptConnection("127.0.0.1")))
            {
            }

            hostConnector.Send(hostInformation);

            hostConnector.Close();
        }

        private static void OnMesageReceivedFromClient(NetworkComponents.TCPConnection connection, NetworkComponents.IGridforceMessage message)
        {
            Console.WriteLine("Client sent " + message.GetType().ToString());
        }
    }
}
