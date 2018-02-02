using System;
using System.Text;
using BusproService;
using BusproService.Devices;
using BusproService.Enums;

namespace SmartHdlConsoleApp
{
	internal class Program
	{
		private const string Ip = "192.168.1.15";
		private const int Port = 6000;
		private const int SourceSubnetId = 200;
		private const int SourceDeviceId = 200;
		private const DeviceType SourceDeviceType = DeviceType.BusproService;

		private static void Main(string[] args)
		{
			using (var busproController = new BusproController(Ip, Port))
			{
				// listen to events for all commands across bus
				busproController.CommandReceived += BusproController_CommandReceived;

				// listen to broadcast commands across bus
				//busproController.BroadcastCommandReceived += BusproController_BroadcastCommandReceived;

				// sender/source address and devicetype
				busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
				busproController.SourceDeviceType = SourceDeviceType;
				busproController.StartListen();

				Console.WriteLine($"Gateway address set to {busproController.Address}:{busproController.Port}");
				Console.WriteLine("Press enter to close...\n");

				// add devices to controller
				//AddDevices(busproController);

				// return all devices in controller
				//var devices = busproController.Device;

				// get specific device from controller
				//var logic = busproController.GetDevice(new DeviceAddress { SubnetId = 1, DeviceId = 100 });
				//Console.WriteLine(logic == null ? "Did not find Device\n" : "Found Device\n");

				//logic.SendOperationCode()...

				//Thread.Sleep(10000);
				//TurnOffLightMediaroom(busproController);
				//QueryDlp(busproController);

				Console.ReadLine();
			}

		}


		private static void BusproController_CommandReceived(object sender, CommandEventArgs args)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine("Command received:");
			ParseData(result);
		}

		private static void BusproController_BroadcastCommandReceived(object sender, CommandEventArgs args)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine("Broadcast command received:");
			ParseData(result);
		}

		private static void Logic_CommandReceived(object sender, CommandEventArgs args)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine($"Command received for {ParseDeviceAddress(result.SourceAddress)}:");
			ParseData(result);
		}

		private static void Device1_CommandReceived(object sender, CommandEventArgs args)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine($"Command received for {ParseDeviceAddress(result.SourceAddress)}:");
			ParseData(result);
		}

		private static void Device2_CommandReceived(object sender, CommandEventArgs args)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine($"Command received for {ParseDeviceAddress(result.SourceAddress)}:");
			ParseData(result);
		}




		private static void ParseData(Command data)
		{
			if (data == null || !data.Success) return;

			var sb = new StringBuilder("");

			var errorMessage = data.ErrorMessage;
			var success = data.Success;

			var b = data.AdditionalContent;
			var t = BusproController.ByteArrayToText(b);

			var sd = data.SourceAddress.DeviceId;
			var ss = data.SourceAddress.SubnetId;

			var td = data.TargetAddress.DeviceId;
			var ts = data.TargetAddress.SubnetId;

			var o = data.OperationCode;
			var oHex = data.OperationCodeHex;

			var dt = data.SourceDeviceType;
			var dtHex = data.SourceDeviceTypeHex;

			//sb.Append($"Success: \t\t{success}");

			if (success)
			{
				sb.Append($"Operation code: \t{o} ({oHex})\n");
				sb.Append($"Source device type: \t{dt} ({dtHex})\n");
				sb.Append($"Address:\t\t{ss}.{sd} => {ts}.{td}\n");
				sb.Append($"Address:\t\t{ParseDeviceName(ss, sd)} => {ParseDeviceName(ts, td)}\n");
				sb.Append($"Additional content: \t{t}\n");

				if (o == OperationCode.BroadcastSystemDateTime)
				{
					sb.Append($"Current time: \t\t{b[2]}/{b[1]}/{b[0]} {b[3]}:{b[4]}:{b[5]}\n");
				}

				if (o == OperationCode.ReadFloorHeatingStatusResponse)
				{
					sb.Append($"Heat active: \t\t{b[2]}\n");
					sb.Append($"Set temperature: \t{b[4]}\n");
					sb.Append($"Current temperature: \t{b[1]}\n");
				}

				if (o == OperationCode.BroadcastTemperature)
				{
					sb.Append($"Current temperatur: \t{b[1]}\n");
				}
			}
			else
			{
				sb.Append($" ({errorMessage})");
			}

			Console.WriteLine(sb.ToString());
			Console.WriteLine("");
		}

		private static string ParseDeviceAddress(DeviceAddress address)
		{
			return address.SubnetId + "." + address.DeviceId;
		}





		private static void AddDevices(IBusproController busproController)
		{
			// add logic device and listen to events for this device
			var logic = busproController.AddDevice(new Logic(1, 100));
			logic.CommandReceived += Logic_CommandReceived;

			// add generic device and listen to events for this device
			var device1 = busproController.AddDevice(new Device(1, 41));
			device1.CommandReceived += Device1_CommandReceived;

			// add generic device and listen to events for this device
			//var device2 = busproController.AddDevice(new Device(1, 131));
			//device2.CommandReceived += Device2_CommandReceived;
		}

		private static void TurnOffLightMediaroom(IBusproController busproController)
		{
			try
			{
				var dimmerKino = (Dimmer)busproController.AddDevice(new Dimmer(1, 74));
				var ok = dimmerKino.SingleChannelControl(1, 0, 0);
				Console.WriteLine($"Medierom..SingleChannelControl(1, 0, 0): {ok}\n\n");
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				Console.WriteLine(err);
			}
		}










		public static void TestCallback(Command result)
		{
			Console.WriteLine(result.Success);
		}


		private static void QueryDlp(IBusproController busproController)
		{
			try
			{
				//Device.CommandResponseCallback callback = TestCallback;

				var dlpStue = (Dlp)busproController.AddDevice(new Dlp(1, 21));
				dlpStue.CommandReceived += (sender, args) => CommandReceived(sender, args, dlpStue.DeviceAddress);	// Variant 1
				//dlpStue.ResponseCommandReceived += (sender, args) => ResponseCommandReceived(sender, args, dlpStue.DeviceAddress);	// Variant 1
				var ok = dlpStue.ReadFloorHeatingStatus();
				//var ok = dlpStue.ReadFloorHeatingStatus(callback);

				dlpStue.ControlFloorHeatingStatus(temperatureAway: 10, mode: Temperature.Mode.Away);
				//Console.WriteLine("dlpStue.ControlFloorHeatingStatus(heatingStatus) sent...");

				//var dlpTrim = (Dlp)busproController.AddDevice(new Dlp(1, 13));
				//dlpTrim.CommandReceived += dlpTrim_CommandReceived;
				//dlpTrim.CommandReceived += delegate (object sender, CommandEventArgs args) { CommandReceived(sender, args, dlpTrim.DeviceAddress); };		// Variant 2 med lambda
				//ok = dlpTrim.ReadFloorHeatingStatus();
				//Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				//var dlpStue = (Dlp)busproController.AddDevice(new Dlp(1, 21));
				//var ok = dlpStue.ReadAcCurrentState();
				//Console.WriteLine($"Stue..ReadAcCurrentState(): {ok}\n\n");

				//var dlpBad = (Dlp)busproController.AddDevice(new Dlp(1, 17));
				//ok = dlpBad.ReadAcCurrentState();
				//Console.WriteLine($"Bad..ReadAcCurrentState(): {ok}\n\n");
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				Console.WriteLine(err);
			}
		}

		private static void CommandReceived(object sender, CommandEventArgs args, DeviceAddress deviceAddress)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine($"{deviceAddress.SubnetId}.{deviceAddress.DeviceId}:");
			Console.WriteLine($"{ParseDeviceName(deviceAddress.SubnetId, deviceAddress.DeviceId)}:");

			Console.WriteLine($"Command received for DLP {ParseDeviceAddress(result.SourceAddress)}:");
			ParseData(result);
		}

		private static void ResponseCommandReceived(object sender, CommandEventArgs args, DeviceAddress deviceAddress)
		{
			var result = (Command)args;
			if (result == null || !result.Success) return;

			Console.WriteLine($"{deviceAddress.SubnetId}.{deviceAddress.DeviceId}:");
			Console.WriteLine($"{ParseDeviceName(deviceAddress.SubnetId, deviceAddress.DeviceId)}:");

			Console.WriteLine($"Response command received for DLP {ParseDeviceAddress(result.SourceAddress)}:");
			ParseData(result);
		}

		//private static void dlpTrim_CommandReceived(object sender, CommandEventArgs args)
		//{
		//	var result = (Command)args;
		//	if (result == null || !result.Success) return;

		//	Console.WriteLine($"Command received for DLP trim {ParseDeviceAddress(result.SourceAddress)}:");
		//	ParseData(result);
		//}



		private static string ParseDeviceName(int subnetId, int deviceId)
		{
			var deviceName = $"{{Unknown device}} ({subnetId}.{deviceId})";

			if (subnetId == 1)
			{
				switch (deviceId)
				{
					case 5:
						deviceName = "Input1";
						break;
					case 10:
						deviceName = "DLP Hall";
						break;
					case 11:
						deviceName = "DLP Gang";
						break;
					case 12:
						deviceName = "DLP Vaskerom";
						break;
					case 13:
						deviceName = "DLP Trimrom";
						break;
					case 14:
						deviceName = "DLP Kontor";
						break;
					case 15:
						deviceName = "DLP Bad";
						break;
					case 16:
						deviceName = "DLP Garasjebod";
						break;
					case 17:
						deviceName = "DLP Sov2";
						break;
					case 18:
						deviceName = "DLP Sov1";
						break;
					case 19:
						deviceName = "Sov1 høyre";
						break;
					case 21:
						deviceName = "DLP Stue";
						break;
					case 22:
						deviceName = "DLP Kjøkken";
						break;
					case 23:
						deviceName = "DLP Medierom";
						break;
					case 40:
						deviceName = "12i1 Hall";
						break;
					case 41:
						deviceName = "12i1 Gang";
						break;
					case 42:
						deviceName = "8i1 Vaskerom";
						break;
					case 43:
						deviceName = "8i1 Garasjebod";
						break;
					case 44:
						deviceName = "8i1 Garasje";
						break;
					case 45:
						deviceName = "8i1 Bad";
						break;
					case 46:
						deviceName = "8i1 Bod Jonathan";
						break;
					case 49:
						deviceName = "12i1 Stue";
						break;
					case 50:
						deviceName = "12i1 Kjøkken";
						break;
					case 55:
						deviceName = "8i1 Garderobe";
						break;
					case 71:
						deviceName = "Dim1 Bad,kontor,sov";
						break;
					case 72:
						deviceName = "Dim2 Kjøkken";
						break;
					case 73:
						deviceName = "Dim3 Stue,g.bod";
						break;
					case 74:
						deviceName = "Dim4 Medie,gang,vask";
						break;
					case 80:
						deviceName = "Rele1";
						break;
					case 100:
						deviceName = "Logikkmodul";
						break;
					case 110:
						deviceName = "Sikkerhetsmodul";
						break;
					case 120:
						deviceName = "RS232";
						break;
					case 130:
						deviceName = "Rele2 Varme 1-6";
						break;
					case 131:
						deviceName = "Rele2 Varme 7-10";
						break;
					case 165:
						deviceName = "Sov1 venstre";
						break;
					case 255:
						deviceName = "BROADCAST SUBNET " + subnetId;
						break;
				}
			}

			if (subnetId == 3)
			{
				switch (deviceId)
				{
					case 254:
						deviceName = "Setup tool";
						break;
					case 255:
						deviceName = "BROADCAST SUBNET " + subnetId;
						break;
				}
			}

			if (subnetId == 200)
			{
				switch (deviceId)
				{
					case 200:
						deviceName = "BusproService";
						break;
				}
			}

			if (subnetId == 253)
			{
				switch (deviceId)
				{
					case 254:
						deviceName = "HDL Buspro Setup Tool 2";
						break;
				}
			}

			if (subnetId == 255)
			{
				switch (deviceId)
				{
					case 255:
						deviceName = "BROADCAST ALL SUBNETS";
						break;
				}
			}

			return deviceName;
		}



	}
}
