using UnityEngine;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

public class UdpBroadcasting : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern void udp_create_beacon(int port, 
	                                             string listen_for_token, 
	                                             string answer_with_token);
	[DllImport("__Internal")]
	private static extern void udp_destroy_beacon();


	[DllImport("__Internal")]
	private static extern int udp_callout(int port, int broadcast_port, string call_token);

	[DllImport("__Internal")]
	private static extern void close_sailors_ears(int socket);

	[DllImport("__Internal")]
	private static extern bool udp_next_response(int socket, string beacon_token, 
	                                            StringBuilder responder_ip);


	private static string SAILOR = "EndOfTheLine";
	private static string BEACON = "GridForce";

	private static bool sailorIsActive = false;
	private static int sailorSocket;
	private static Thread sailorThread;

	private static bool plattformIsDevice(){
		#if UNITY_EDITOR
		return false;
		#else
		return true;
		#endif
	}

	public static void createBeacon(){
		if (!plattformIsDevice())
			return;
		
		Loom.RunAsync(()=>{
			Debug.Log("Creating Beacon.");
			UdpBroadcasting.udp_create_beacon(1337, SAILOR, BEACON);
		});
	}

	public static void destroyBeacon(){
		if (!plattformIsDevice())
			return;

		Debug.Log("Attempting to kill beacon!");
		UdpBroadcasting.udp_destroy_beacon();
	}

	public static void killTheSailor(){
		//TODO: still buggy...

		if (!UdpBroadcasting.sailorIsActive)
			return;

		UdpBroadcasting.sailorIsActive = false;
		UdpBroadcasting.sailorThread.Join();
		UdpBroadcasting.close_sailors_ears(UdpBroadcasting.sailorSocket);
	}

	public static void callAvailibleBeacons(){
		if (!plattformIsDevice())
			return;

		if (UdpBroadcasting.sailorIsActive){
			Debug.Log("There is already a sailor out there!");
			return;
		}

		Debug.Log("Attempting to call beacons...");

		UdpBroadcasting.sailorSocket = UdpBroadcasting.udp_callout(1338, 1337, SAILOR);
		Debug.Log("Callout complete at socket "+UdpBroadcasting.sailorSocket);

		UdpBroadcasting.sailorIsActive = true;

		UdpBroadcasting.sailorThread = Loom.RunAsync(()=>{
			while (UdpBroadcasting.sailorIsActive){
				StringBuilder beaconIp = new StringBuilder();

				if (UdpBroadcasting.udp_next_response(UdpBroadcasting.sailorSocket,
				                                      BEACON,
				                                      beaconIp)){
					Debug.Log("Found a beacon at "+beaconIp);
				}

			}
		});
	}
}
