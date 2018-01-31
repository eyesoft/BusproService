using System;
using System.Text;
using BusproService;
using BusproService.Enums;

namespace SmartHdlConsoleApp
{
	internal class Program
	{
		private const string Ip = "192.168.1.15";
		private const int Port = 6000;
		private const int SourceSubnetId = 1;
		private const int SourceDeviceId = 255;
		private const DeviceType SourceDeviceType = DeviceType.BusproService;

		private static void Main(string[] args)
		{
			using (var busproController = new BusproController(Ip, Port))
			{
				// listen to events for all commands across bus
				busproController.CommandReceived += BusproController_CommandReceived;

				// listen to broadcast commands across bus
				busproController.BroadcastCommandReceived += BusproController_BroadcastCommandReceived;

				// sender/source address and devicetype
				busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
				busproController.SourceDeviceType = SourceDeviceType;
				busproController.StartListen();

				Console.WriteLine($"Gateway address set to {busproController.Address}:{busproController.Port}");
				Console.WriteLine("Press enter to close...\n");

				// add devices to controller
				AddDevices(busproController);

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

			sb.Append($"Success: \t\t{success}");

			if (success)
			{
				sb.Append("\n");
				sb.Append($"Operation code: \t{o} ({oHex})\n");
				sb.Append($"Source device type: \t{dt} ({dtHex})\n");
				sb.Append($"Address:\t\t{ss}.{sd} => {ts}.{td}\n");
				sb.Append($"Additional content: \t{t}\n");

				if (o == OperationCode.CurrentDateTime)
				{
					sb.Append($"Current time: \t\t{b[2]}/{b[1]}/{b[0]} {b[3]}:{b[4]}:{b[5]}\n");
				}

				if (o == OperationCode.ReadFloorHeatingStatusResponse)
				{
					sb.Append($"Heat active: \t\t{b[2]}\n");
					sb.Append($"Set temperature: \t{b[4]}\n");
					sb.Append($"Current temperature: \t{b[1]}\n");
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

		private static void QueryDlp(IBusproController busproController)
		{
			try
			{
				var dlpStue = (Dlp)busproController.AddDevice(new Dlp(1, 21));
				var ok = dlpStue.ReadFloorHeatingStatus();
				Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				var dlpTrim = (Dlp)busproController.AddDevice(new Dlp(1, 13));
				ok = dlpTrim.ReadFloorHeatingStatus();
				Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				/*
				ReadFloorHeatingStatus    
				0		temperature.type (0 = Celsius, 1 = Fahrenheit)
				24	temperature.current 
				1		status (av/på)
				1		modus (1 = Normal)
				24	temperature.normal
				20	temperature.day
				20	temperature.night
				20	temperature.away
						temperature.timer
				*/

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



	}
}
