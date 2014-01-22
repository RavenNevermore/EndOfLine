using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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

	private static Dictionary<Beacon, long> _availibleBeacons = new Dictionary<Beacon, long>();

	private static long now(){
		DateTime time = DateTime.Now;
		long x = 0;
		x += time.Second;
		x += time.Minute * 60;
		x += time.Hour  * 60 * 60;
		x += time.DayOfYear * 60 * 60 * 24;
		x += time.Year * 60 * 60 * 24 * 356;
		return x;
	}

	public static HashSet<Beacon> availibleBeacons{
		get {
			long t = now();

			lock (_availibleBeacons){
				HashSet<Beacon> toRemove = new HashSet<Beacon>();
				HashSet<Beacon> list = new HashSet<Beacon>();
				int i = 0;

				foreach (KeyValuePair<Beacon, long> beacon in _availibleBeacons){
					if (t - beacon.Value > Beacon.DecayTime){
						Debug.Log("Removing old Beacon: "+beacon);
						toRemove.Add(beacon.Key);
					} else {
						list.Add(beacon.Key);
					}
				}

				foreach (Beacon beacon in toRemove){
					_availibleBeacons.Remove(beacon);
				}

				return list;
			}

		}
	}

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
		if (null != UdpBroadcasting.sailorThread)
			UdpBroadcasting.sailorThread.Join();
		if (plattformIsDevice())
			UdpBroadcasting.close_sailors_ears(UdpBroadcasting.sailorSocket);
	}

	/**
	 * Don't ask. :(
	 */
	private static int[] someNumbers = new int[]{1,2,3,4,5};
	private static int someNumbersIndex = 0;
	private static int aNumber{
		get {
			int x = someNumbers[someNumbersIndex];
			someNumbersIndex++;
			if (someNumbersIndex >= someNumbers.Length)
				someNumbersIndex = 1;
			return x;
		}
	}

	private static Beacon sailorCallout(){
		StringBuilder beaconIp = new StringBuilder();

		if (plattformIsDevice()){

			if (UdpBroadcasting.udp_next_response(UdpBroadcasting.sailorSocket,
			                                      BEACON,
			                                      beaconIp)){
				Debug.Log("Found a beacon at "+beaconIp);
			}
		} else {
			beaconIp.Append("127.0.0.1|Pseudo Device");
			beaconIp.Append(aNumber);
			//Debug.Log("Created random beacon: "+beaconIp);
		}

		string beacon = beaconIp.ToString();
		return Beacon.initBeaconFromSourceString(beacon);
	}

	public static void callAvailibleBeacons(){
		if (UdpBroadcasting.sailorIsActive){
			Debug.Log("There is already a sailor out there!");
			return;
		}

		if (plattformIsDevice()){
			Debug.Log("Attempting to call beacons...");

			UdpBroadcasting.sailorSocket = UdpBroadcasting.udp_callout(1338, 1337, SAILOR);
			Debug.Log("Callout complete at socket "+UdpBroadcasting.sailorSocket);
		} else {
			Debug.Log("Dummy callout!");
		}

		UdpBroadcasting.sailorIsActive = true;

		UdpBroadcasting.sailorThread = Loom.RunAsync(()=>{
			while (UdpBroadcasting.sailorIsActive){
				Thread.Sleep(500);
				//Debug.Log(".:.:.:.:");
				Beacon beacon = UdpBroadcasting.sailorCallout();
				//Beacon y = new Beacon();
				//Debug.Log("Callout returned: "+beacon);

				lock (_availibleBeacons) {
					if (_availibleBeacons.ContainsKey(beacon)){
						_availibleBeacons.Remove(beacon);
					}
					_availibleBeacons.Add(beacon, now());
				}
			}
		});

	}
}
