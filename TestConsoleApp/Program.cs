// https://github.com/genielabs/zwave-lib-dotnet
// https://github.com/caligo-mentis/smart-bus

using System;
using System.Text;
using System.Threading;
using BusproService;
using BusproService.Data;
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
				busproController.ContentReceived += busproController_ContentReceived;
				busproController.DeviceDataContentReceived += busproController_ContentReceivedForDevice;

				busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
				busproController.SourceDeviceType = SourceDeviceType;

				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					while (true)
					{
						// Reads all data on bus
						busproController.ReadBus();
					}
				}).Start();

				Console.WriteLine("Press enter to close...\n");

				ListenToDevice(busproController);

				//Thread.Sleep(10000);
				//TurnOffLightMediaroom(busproController);
				//QueryDlp(busproController);
				
				Console.ReadLine();
			}

		}

		private static void busproController_ContentReceivedForDevice(object sender, ContentEventArgs args)
		{
			var result = args;

			if (result == null || !result.Success) return;

			Console.WriteLine("Received data for " + result.SourceAddress.SubnetId + "."  + result.SourceAddress.DeviceId);
		}


		private static void busproController_ContentReceived(object sender, ContentEventArgs args)
		{
			var result = args;

			if (result == null || !result.Success) return;

			var sb = new StringBuilder("");

			var errorMessage = result.ErrorMessage;
			var success = result.Success;

			var b = result.AdditionalContent;
			var t = BusproController.ByteArrayToText(b);

			var sd = result.SourceAddress.DeviceId;
			var ss = result.SourceAddress.SubnetId;

			var td = result.TargetAddress.DeviceId;
			var ts = result.TargetAddress.SubnetId;

			var o = result.OperationCode;
			var oHex = result.OperationCodeHex;

			var dt = result.SourceDeviceType;
			var dtHex = result.SourceDeviceTypeHex;

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


		public static void ListenToDevice(IBusproController busproController)
		{
			var device = new Logic(1, 100);
			// tag for eventhandling
			// device.tag
			var logic = (Logic)busproController.Device(device);
		}


		public static void TurnOffLightMediaroom(IBusproController busproController)
		{
			try
			{
				var dimmerKino = (Dimmer)busproController.Device(new Dimmer(1, 74));
				var ok = dimmerKino.SingleChannelControl(1, 0, 0);
				Console.WriteLine($"Medierom..SingleChannelControl(1, 0, 0): {ok}\n\n");
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				Console.WriteLine(err);
			}
		}


		public static void QueryDlp(IBusproController busproController)
		{
			try
			{
				var dlpStue = (Dlp)busproController.Device(new Dlp(1, 21));
				var ok = dlpStue.ReadFloorHeatingStatus();
				Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				var dlpTrim = (Dlp)busproController.Device(new Dlp(1, 13));
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
									??temperature.timer
				*/



				//var dlpStue = (Dlp)busproController.Device(new Dlp(1, 21));
				//var ok = dlpStue.ReadAcCurrentState();
				//Console.WriteLine($"Stue..ReadAcCurrentState(): {ok}\n\n");

				//var dlpBad = (Dlp)busproController.Device(new Dlp(1, 17));
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
