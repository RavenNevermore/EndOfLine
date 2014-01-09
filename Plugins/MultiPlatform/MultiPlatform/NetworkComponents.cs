using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;

using System.Net.Sockets;


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

        // Returns local IP address
        public static string GetLocalIPv4()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address.ToString();
            }

            return null;
        }


        // Class for broadcasting messages via UDP
        public class UDPBroadcaster
        {
            private UdpClient udpClient = null;
            private bool closed = false;
            private IPEndPoint remoteEndPoint = null;

            // Constructor
            public UDPBroadcaster()
            {
                this.udpClient = new UdpClient();

                this.udpClient.JoinMulticastGroup(IPAddress.Parse(NetworkComponents.multicastIP), 50);
                this.remoteEndPoint = new IPEndPoint(IPAddress.Parse(NetworkComponents.multicastIP), NetworkComponents.gamePort);
            }

            // Destructor
            ~UDPBroadcaster()
            {
                this.Close();
            }

            // Manually close connection
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;
                    this.udpClient.Close();
                }
            }

            // Broadcast a message via UDP
            public void Broadcast(IGridforceMessage message)
            {
                byte[] data = QuickSerializer.Serialize(message);

                this.udpClient.Send(data, data.Length, this.remoteEndPoint);
            }
        }


        // Class for receiving messages via UDP
        public class UDPReceiver
        {
            private UdpClient udpClient = null;
            IPEndPoint localEndPoint = null;
            private Thread receiverThread;		// Thread for processing incoming messages

            private bool closed = false;		// True when connection was closed
            private NetworkComponents.MessageProc messageProc = null;		// Message to call on receiving message

            // Constructor
            public UDPReceiver(NetworkComponents.MessageProc messageProc)
            {
                this.messageProc = messageProc;

                this.udpClient = new UdpClient();
                this.udpClient.Client.ReceiveTimeout = 250;
                this.udpClient.ExclusiveAddressUse = false;
                this.localEndPoint = new IPEndPoint(IPAddress.Any, NetworkComponents.gamePort);
                this.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.udpClient.ExclusiveAddressUse = false;
                this.udpClient.Client.Bind(this.localEndPoint);
                this.udpClient.JoinMulticastGroup(IPAddress.Parse(NetworkComponents.multicastIP), 50);

                this.receiverThread = new Thread(this.Receive);
                this.receiverThread.Name = "Gridforce UDP Receiver";
                this.receiverThread.IsBackground = true;
                this.receiverThread.Start();
            }

            // Destructor
            ~UDPReceiver()
            {
                this.Close();
            }

            // Manually close connection
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;
                    this.receiverThread.Abort();
                    this.udpClient.Close();
                }
            }

            // Receive a message via UDP
            private void Receive()
            {
                while (!(this.closed))
                {
                    try
                    {
                        // Get message size
                        this.localEndPoint = new IPEndPoint(IPAddress.Any, NetworkComponents.gamePort);
                        byte[] data = this.udpClient.Receive(ref this.localEndPoint);

                        // Whole message received
                        if (data != null)
                        {
                            try
                            {
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
                            catch (Exception)
                            {
                            }
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (SocketException)
                    {
                    }
                }
            }
        }


        public delegate void MessageReceived(TCPConnection connection, IGridforceMessage message);


        // TCP connector class
        public class TCPConnector
        {
            public bool isHost = false;     // This player is the host
            private TcpListener listener = null;    // Listener to listen for connection requests
            private bool closed = false;    // True when connection was closed
            private List<TCPConnection> connectionList = new List<TCPConnection>();     // List of connections
            public MessageReceived OnMessageReceived = null;       // Method to call on receiving messages

            // Constructor
            public TCPConnector(MessageReceived OnMessageReceived, bool isHost)
            {
                this.isHost = isHost;
                this.OnMessageReceived += OnMessageReceived;

                listener = new TcpListener(IPAddress.Any, NetworkComponents.gamePort);

                if (this.isHost)
                    this.listener.Start();
            }

            // Destructor
            ~TCPConnector()
            {
                this.Close();
            }

            // Close connections
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;

                    for (int i = 0; i < this.connectionList.Count; i++)
                        this.connectionList[i].Close();

                    this.listener.Stop();
                }
            }

            // Listen for connection requests
            public bool Listen()
            {
                if (this.connectionList.Count < 4 && this.listener.Pending() && this.isHost)
                {
                    TCPConnection connection = new TCPConnection(this.listener.AcceptTcpClient(), this.Receive, false);

                    this.connectionList.Add(connection);

                    return true;
                }

                return false;
            }

            // Connect to specific IP address
            public bool ConnectTo(string remoteIP, bool isHost)
            {
                try
                {
                    TcpClient client = new TcpClient(AddressFamily.InterNetwork);
                    client.Connect(IPAddress.Parse(remoteIP), NetworkComponents.gamePort);

                    TCPConnection connection = new TCPConnection(client, this.Receive, isHost);

                    this.connectionList.Add(connection);

                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }

                return false;
            }

            // Accept connection from specific IP address
            public bool AcceptConnection(string remoteIP)
            {
                if (!(this.isHost))
                    this.listener.Start();

                if (this.connectionList.Count < 4 && this.listener.Pending())
                {
                    TcpClient client = this.listener.AcceptTcpClient();
                    IPEndPoint endPoint = client.Client.LocalEndPoint as IPEndPoint;

                    if (endPoint != null && endPoint.Address.Equals(IPAddress.Parse(remoteIP)))
                    {
                        TCPConnection connection = new TCPConnection(client, this.Receive, false);

                        this.connectionList.Add(connection);

                        return true;
                    }
                    else
                    {
                        client.Close();
                    }
                }

                if (!(this.isHost))
                    this.listener.Stop();

                return false;
            }

            // Receive message from TCP connection
            private void Receive(TCPConnection connection, IGridforceMessage message)
            {

                if (this.OnMessageReceived != null)
                    this.OnMessageReceived(connection, message);
            }

            // Send message to all TCP connections
            public void Send(IGridforceMessage message)
            {
                for (int i = 0; i < this.connectionList.Count; i++)
                {
                    if (this.connectionList[i].closed)
                    {

                        this.connectionList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        this.connectionList[i].Send(message);
                    }
                }
            }
        }


        // TCP connection class
        public class TCPConnection
        {
            public TcpClient client = null;     // TCP connection client
            private MessageReceived OnMessageReceived = null;       // Method to call on receiving messages
            private NetworkStream stream = null;        // Network stream for sending and receiving data
            public bool isHost = false;     // This player is the host
            public bool closed = false;    // True when connection was closed
            private Thread receiverThread = null;       // Thread for receiving data

            // Constructor
            public TCPConnection(TcpClient client, MessageReceived OnMessageReceived, bool isHost)
            {
                this.isHost = isHost;
                this.OnMessageReceived += OnMessageReceived;

                this.client = client;
                this.stream = this.client.GetStream();

                this.receiverThread = new Thread(this.Receive);
                this.receiverThread.Start();
            }

           // Destructor
            ~TCPConnection()
            {
                this.Close();
            }

            // Close connection
            public void Close()
            {
                if (!(this.closed))
                {
                    this.closed = true;

                    this.receiverThread.Abort();

                    this.stream.Close();
                    this.client.Close();
                }
            }

            // Send message to TCP connection
            public void Send(IGridforceMessage message)
            {
                byte[] data = QuickSerializer.Serialize(message);
                byte[] size = BitConverter.GetBytes(data.Length);
                if (!(BitConverter.IsLittleEndian))
                    Array.Reverse(size);

                try
                {
                    this.stream.Write(size, 0, 4);
                    this.stream.Write(data, 0, data.Length);
                }
                catch (Exception)
                {
                }
            }

            // Receive message from TCP connection
            public void Receive()
            {
                while (!(this.closed))
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream();
                        byte[] sizeBuffer = new byte[4];

                        // Get message size
                        this.stream.Read(sizeBuffer, 0, 4);

                        if (!(BitConverter.IsLittleEndian))
                            Array.Reverse(sizeBuffer);

                        int totalSize = BitConverter.ToInt32(sizeBuffer, 0);
                        int size = totalSize;

                        while (size > 0)
                        {
                            byte[] data = new byte[1024];
                            int received = this.stream.Read(data, 0, 1024);
                            ms.Write(data, 0, received);
                            size -= received;
                        }

                        // Whole message received
                        if (totalSize > 0)
                        {
                            try
                            {
                                IGridforceMessage message = null;

                                try
                                {
                                    ms.Position = 0;
                                    message = QuickSerializer.Deserialize(ms.ToArray());
                                }
                                catch (IGridforceMessageDeserializeException)
                                {
                                }

                                if (message == null)
                                {
                                }

                                if (message != null && this.OnMessageReceived != null)
                                    this.OnMessageReceived(this, message);
                            }
                            catch (Exception)
                            {
                            }

                            ms.Position = 0;
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                        this.Close();
                    }
                }
            }
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


    // Class for user information
    public class UserInformation : NetworkComponents.IGridforceMessage
    {
        public string name = null;      // User name
        public string ipAddress = null; // User IP

        // Constructor
        public UserInformation(string name, string ipAddress)
        {
            this.name = name;
            this.ipAddress = ipAddress;
        }

        // Serialize
        public static byte[] Serialize(UserInformation message)
        {
            MemoryStream ms = new MemoryStream();

            CustomSerializer.SerializeString(ms, "IGridforceMessage");
            CustomSerializer.SerializeString(ms, "UserInformation");
            CustomSerializer.SerializeString(ms, message.name);
            CustomSerializer.SerializeString(ms, message.ipAddress);

            return ms.ToArray();
        }

        // Deserialize
        public static UserInformation Deserialize(byte[] data)
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
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a UserInformation instance");
            }

            if (interfaceName != "IGridforceMessage" || className != "UserInformation")
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a UserInformation instance");


            string name = null;
            string ipAddress = null;

            try
            {
                name = CustomSerializer.DeserializeString(data, ref pos);
                ipAddress = CustomSerializer.DeserializeString(data, ref pos);
            }
            catch (IGridforceMessageDeserializeException)
            {
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a UserInformation instance");
            }


            return new UserInformation(name, ipAddress);
        }

        // Interface method
        public string Validate()
        {
            return "UserInformation";
        }
    }


    // Class for sending join request
    public class JoinRequest : NetworkComponents.IGridforceMessage
    {
        public UserInformation userInformation = null;
        public byte[] hashedPassword = null;

        // Constructor
        public JoinRequest(UserInformation userInformation, byte[] hashedPassword)
        {
            this.userInformation = userInformation;
            this.hashedPassword = hashedPassword;
        }

        // Serialize
        public static byte[] Serialize(JoinRequest message)
        {
            MemoryStream ms = new MemoryStream();

            CustomSerializer.SerializeString(ms, "IGridforceMessage");
            CustomSerializer.SerializeString(ms, "JoinRequest");
            CustomSerializer.SerializeString(ms, ASCIIEncoding.ASCII.GetString(message.hashedPassword));
            byte[] userInformation = UserInformation.Serialize(message.userInformation);
            ms.Write(userInformation, 0, userInformation.Length);

            return ms.ToArray();
        }

        // Deserialize
        public static JoinRequest Deserialize(byte[] data)
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
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a JoinRequest instance");
            }

            if (interfaceName != "IGridforceMessage" || className != "JoinRequest")
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a JoinRequest instance");


            string hashedPassword = null;
            UserInformation userInformation = null;

            try
            {
                hashedPassword = CustomSerializer.DeserializeString(data, ref pos);
                byte[] userInformationArray = new byte[data.Length - pos];
                Array.Copy(data, pos, userInformationArray, 0, data.Length - pos);
                userInformation = UserInformation.Deserialize(userInformationArray);
            }
            catch (IGridforceMessageDeserializeException)
            {
                throw new IGridforceMessageDeserializeException("The data array doesn't represent a JoinRequest instance");
            }


            return new JoinRequest(userInformation, ASCIIEncoding.ASCII.GetBytes(hashedPassword));
        }

        // Interface method
        public string Validate()
        {
            return "JoinRequest";
        }
    }



    public static class QuickSerializer
    {
        public static byte[] Serialize(NetworkComponents.IGridforceMessage message)
        {
            if (message is HostInformation)
                return HostInformation.Serialize((HostInformation)(message));
            else if (message is UserInformation)
                return UserInformation.Serialize((UserInformation)(message));
            else if (message is JoinRequest)
                return JoinRequest.Serialize((JoinRequest)(message));

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

            try
            {
                message = UserInformation.Deserialize(data);
                return message;
            }
            catch (IGridforceMessageDeserializeException)
            {
            }

            try
            {
                message = JoinRequest.Deserialize(data);
                return message;
            }
            catch (IGridforceMessageDeserializeException)
            {
            }

            throw new IGridforceMessageDeserializeException("The data array doesn't represent an IGridforceMessage");
        }
    }

}