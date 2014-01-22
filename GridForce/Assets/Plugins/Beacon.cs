using System;
using System.Collections;

public class Beacon {

	public static long DecayTime = 5;



	private string _ip;
	private string _name;

	public static Beacon initBeaconFromSourceString(string source){
		int devident = source.IndexOf('|');

		if (devident > -1){
			string ip = source.Substring(0, devident);
			string name = source.Substring(devident+1);
			return new Beacon(ip, name);
		} else {
			return new Beacon(source, "unnamed");
		}
	}

	public Beacon(){
	}

	public Beacon(string ip, string name){
		this._ip = ip;
		this._name = name;
	}





	public string ip {
		get {
			return this._ip;
		}
	}

	public string name {
		get {
			return this._name;
		}
	}

	public override int GetHashCode(){
		return this._ip.GetHashCode() + this._name.GetHashCode();
	}

	public override bool Equals (object obj){
		if (!(obj is Beacon))
			return false;
		Beacon other = (Beacon) obj;
		return this._ip.Equals(other._ip) && this._name.Equals(other._name);
	}

	public override string ToString (){
		return string.Format ("[Beacon: ip={0}, name={1}]", ip, name);
	}

}
