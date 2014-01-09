using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MultiPlatform;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Debugging
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        NetworkComponents.UDPBroadcaster broadcaster = null;
        static NetworkComponents.UDPReceiver receiver = null;

        public void MessageReceived(NetworkComponents.IGridforceMessage message)
        {
            if (message is HostInformation)
            {
                HostInformation hostInformation = (HostInformation)(message);
                System.Diagnostics.Debug.WriteLine(hostInformation.ToString());
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            float lastUpdate = 12.481f;
            HostInformation hostInformation = new HostInformation(lastUpdate.GetHashCode().ToString(), NetworkComponents.GetLocalIPv4(), "Your name", lastUpdate, true);
            byte[] byteArray = HostInformation.Serialize(hostInformation);
            HostInformation deserialized = HostInformation.Deserialize(byteArray);

            receiver = new NetworkComponents.UDPReceiver(this.MessageReceived);
            //this.broadcaster = new NetworkComponents.UDPBroadcaster();
            //this.broadcaster.Broadcast(hostInformation);
        }
    }
}
