using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.IO;

using System.Threading.Tasks;
using System.Linq;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;


namespace MultiPlatform
{

    // Static class for network components
    public static class NetworkComponents
    {
        public const string multicastIP = "228.173.63.241";
        public const int gamePort = 24196;		// Port for sending and receiving messages
        public delegate void MessageProc(IGridforceMessage message);	// Delegate vor message-related procedures

        // Message interface
        public interface IGridforceMessage
        {
            string Validate();
        }

        // Exception for deserializing IGridforceMessages
        public class IGridforceMessageDeserializeException : Exception
        {
            // Constructor
            public IGridforceMessageDeserializeException(string message)
                : base(message)
            {
            }
        }

        // Returns local IP address
        public static string GetLocalIPv4()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                            hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

                if (hostname != null)
                {
                    // the ip address
                    return hostname.CanonicalName;
                }
            }

            return null;
        }


        // Class for broadcasting messages via UDP
        public class UDPBroadcaster
        {
            private DatagramSocket sender;      // The socket used for sending data

            private bool closed = false;		// True when connection was closed

            // Constructor
            public UDPBroadcaster()
            {
                Task task = Task.Run(async () => { await this.Connect(); });
                task.Wait();
            }

            // Connect to socket
            async private Task Connect()
            {
                try
                {
                    this.sender = new DatagramSocket();
                    this.sender.MessageReceived += this.MessageReceived;
                    NetworkAdapter networkAdapter = new HostName(NetworkComponents.GetLocalIPv4()).IPInformation.NetworkAdapter;
                    await this.sender.BindServiceNameAsync("", networkAdapter);
                    this.sender.JoinMulticastGroup(new HostName(NetworkComponents.multicastIP));
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            // Destructor
            ~UDPBroadcaster()
            {
                this.Close();
            }

            // Message received
            private void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
            {
                int i = 0;
                i++;
            }

            // Manually close connection
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;
                }
            }

            // Broadcast a message via UDP
            public void Broadcast(IGridforceMessage message)
            {
                Task task = Task.Run(async () => { await this.BroadcastAsync(message); });
                task.Wait();
            }

            async private Task BroadcastAsync(IGridforceMessage message)
            {
                if (this.sender == null)
                    return;

                byte[] data = QuickSerializer.Serialize(message);

                IOutputStream outputStream = await this.sender.GetOutputStreamAsync(new HostName(NetworkComponents.multicastIP), NetworkComponents.gamePort.ToString());

                try
                {
                    await outputStream.WriteAsync(data.AsBuffer());
                    await outputStream.FlushAsync();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }


        // Class for receiving messages via UDP
        public class UDPReceiver
        {
            private DatagramSocket receiver;      // The socket used for receiving data

            private bool closed = false;		// True when connection was closed
            private NetworkComponents.MessageProc messageProc = null;		// Message to call on receiving message

            // Constructor
            public UDPReceiver(NetworkComponents.MessageProc messageProc)
            {
                this.messageProc = messageProc;
                Task task = Task.Run(async () => { await this.Connect(); });
                task.Wait();
            }

            // Connect to socket
            async private Task Connect()
            {
                try
                {
                    this.receiver = new DatagramSocket();
                    this.receiver.MessageReceived += this.MessageReceived;
                    await this.receiver.BindServiceNameAsync(NetworkComponents.gamePort.ToString());
                    this.receiver.JoinMulticastGroup(new HostName(NetworkComponents.multicastIP));
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            // Destructor
            ~UDPReceiver()
            {
                this.Close();
            }

            // Message received
            private void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
            {
                IBuffer buffer = args.GetDataReader().DetachBuffer();
                byte[] data = buffer.ToArray();

                IGridforceMessage message = null;

                try
                {
                    message = QuickSerializer.Deserialize(data);
                }
                catch (IGridforceMessageDeserializeException)
                {
                }

                if (message == null)
                {
                }

                if (message != null && this.messageProc != null)
                    this.messageProc(message);
            }

            // Manually close connection
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;
                }
            }
        }


        // TCP connector class
        public class TCPConnector
        {
        }


        // TCP connection class
        public class TCPConnection
        {
        }
    }



    // Contains information on a host
    public class HostInformation : NetworkComponents.IGridforceMessage
    {
        public string hostIP = null;     // Host's IP address
        public string hostName = null;      // Host's name
        public string instanceID = null;    // Instance ID to differentiate different games from the same host
        public float lastUpdate = 0.0f;     // Time of last received UDP broadcast from this host
        public bool passwordProtected = false;      // Whether game is password protected or not

        // Constructor
        public HostInformation(string instanceID, string hostIP, string hostName, float lastUpdate, bool passwordProtected)
        {
            this.instanceID = instanceID;
            this.hostIP = hostIP;
            this.hostName = hostName;
            this.lastUpdate = lastUpdate;
            this.passwordProtected = passwordProtected;
        }

        // Compares two HostInformation instances
        public override bool Equals(object obj)
        {
            HostInformation hostInformation = obj as HostInformation;
            if (hostInformation == null)
                return false;

            if (this.instanceID != hostInformation.instanceID || !(this.hostIP.Equals(hostInformation.hostIP)))
                return false;

            return true;
        }

        // Returns hash code
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // Converts host information to string
        public override string ToString()
        {
            return this.hostName + " (" + this.hostIP.ToString() + ")" + (this.passwordProtected ? " (p)" : "");
        }

        // Serialize HostInformation
        public static byte[] Serialize(HostInformation message)
        {
            MemoryStream ms = new MemoryStream();

            CustomSerializer.SerializeString(ms, "IGridforceMessage");
            CustomSerializer.SerializeString(ms, "HostInformation");
            CustomSerializer.SerializeString(ms, message.instanceID);
            CustomSerializer.SerializeString(ms, message.hostIP);
            CustomSerializer.SerializeString(ms, message.hostName);
            CustomSerializer.SerializeDouble(ms, (double)(message.lastUpdate));
            CustomSerializer.SerializeBool(ms, message.passwordProtected);

            return ms.ToArray();
        }

        // Deserialize HostInfomration
        public static HostInformation Deserialize(byte[] data)
        {
            int pos = 0;


            string interfaceName = null;
            string className = null;

            try
            {
                interfaceName = CustomSerializer.DeserializeString(data, ref pos);
                className = CustomSerializer.DeserializeString(data, ref pos);
            }
            catch (IGridforceMessageDeserializeException)
            {
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a HostInformation instance");
            }

            if (interfaceName != "IGridforceMessage" || className != "HostInformation")
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a HostInformation instance");


            string instanceID = null;
            string hostIP = null;
            string hostName = null;
            float lastUpdate = 0.0f;
            bool passwordProtected = false;

            try
            {
                instanceID = CustomSerializer.DeserializeString(data, ref pos);
                hostIP = CustomSerializer.DeserializeString(data, ref pos);
                hostName = CustomSerializer.DeserializeString(data, ref pos);
                lastUpdate = (float)(CustomSerializer.DeserializeDouble(data, ref pos));
                passwordProtected = CustomSerializer.DeserializeBool(data, ref pos);
            }
            catch (IGridforceMessageDeserializeException)
            {
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a HostInformation instance");
            }


            return new HostInformation(instanceID, hostIP, hostName, lastUpdate, passwordProtected);
        }

        // Interface method
        public string Validate()
        {
            return "HostInformation";
        }
    }



    public static class QuickSerializer
    {
        public static byte[] Serialize(NetworkComponents.IGridforceMessage message)
        {
            if (message is HostInformation)
                return HostInformation.Serialize((HostInformation)(message));

            return null;
        }

        public static NetworkComponents.IGridforceMessage Deserialize(byte[] data)
        {
            NetworkComponents.IGridforceMessage message = null;

            try
            {
                message = HostInformation.Deserialize(data);
                return message;
            }
            catch (IGridforceMessageDeserializeException)
            {
            }

            throw new IGridforceMessageDeserializeException("The data array doesn't represent an IGridforceMessage");
        }
    }

}