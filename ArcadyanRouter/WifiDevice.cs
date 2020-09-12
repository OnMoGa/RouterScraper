using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ArcadyanRouter {


	public enum Connection {
		Wifi2,
		Wifi5,
		Ethernet
	}

	public class WifiDevice {

		public string name { get; set; }
		public PhysicalAddress macAddress { get; set; }
		public IPAddress ipv4Address { get; set; }
		public IPAddress ipv6Address { get; set; }
		public Connection connection { get; set; }
		public int signalStrength { get; set; }
		public int linkRateMbps { get; set; }

	}
}
