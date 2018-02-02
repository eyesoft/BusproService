using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BusproService.Enums;

namespace BusproService.Devices
{
	public class Dlp : Device, IDlp
	{
		internal Dlp(BusproController controller, DeviceType deviceType, DeviceAddress deviceAddress) : base(controller, deviceType, deviceAddress)
		{
		}

		public Dlp(int subnetId, int deviceId) : base(subnetId, deviceId)
		{
		}

		public Dlp(DeviceAddress deviceAddress) : base(deviceAddress)
		{
		}

		public Dlp(DeviceType deviceType, DeviceAddress deviceAddress) : base(deviceType, deviceAddress)
		{
		}

		public bool ReadAcCurrentState()
		{
			return base.SendOperationCode(OperationCode.ReadAcCurrentStatus, null);
		}

		public bool ReadFloorHeatingStatus()
		{
			return base.SendOperationCode(OperationCode.ReadFloorHeatingStatus, null);
		}

		//public bool ReadFloorHeatingStatus(CommandResponseCallback commandResponseCallback)
		//{
		//	var response = base.SendOperationCode(OperationCode.ReadFloorHeatingStatus, null);

		//	if (commandResponseCallback != null)
		//		commandResponseCallback(new Command());

		//	return response;
		//}

		private async Task AsyncWait(int millisecondsDelay)
		{
			var wait = Task.Delay(millisecondsDelay);
			await wait;
		}

		public async Task<bool> ControlFloorHeatingStatus(Temperature.Status? status = null, Temperature.Mode? mode = null,
			int? temperatureNormal = null, int? temperatureDay = null, int? temperatureNight = null, int? temperatureAway = null)
		{
			var taskOk = true;
			CommandEventArgs command = null;

			var tcs = new TaskCompletionSource<CommandEventArgs>();
			this.ResponseCommandReceived += (sender, args) => tcs.TrySetResult(args);

			// Først leser vi for å hente siste verdier
			var result = ReadFloorHeatingStatus();

			if (tcs.Task == await Task.WhenAny(tcs.Task, Task.Delay(5000)))
			{
				await tcs.Task;
				command = tcs.Task.Result;
			}
			else
			{
				// Timeout
				taskOk = false;
			}

			if (!taskOk || command == null) return false;

			// Vi bryr oss kun om ReadFloorHeatingStatusResponse sendt fra gjeldende device
			if (command.OperationCode != OperationCode.ReadFloorHeatingStatusResponse ||
			    (this.DeviceAddress.SubnetId != command.SourceAddress.SubnetId && this.DeviceAddress.DeviceId != command.SourceAddress.DeviceId)) return false;

			//Console.WriteLine($"ReadFloorHeatingStatusResponse: \t{BusproController.ByteArrayToText(command.AdditionalContent)}");

			// Henter gjeldende status
			var readFloorHeatingStatusResponse = Parsing.ParseReadFloorHeatingStatusResponse(command);
			if (readFloorHeatingStatusResponse == null) return false;

			//Console.WriteLine($"Current ReadFloorHeatingStatusResponse: \t{Newtonsoft.Json.JsonConvert.SerializeObject(readFloorHeatingStatusResponse)}");

			// Oppretter ny bytearray for oppdatering av status
			var controlFloorHeatingStatus = new byte[7];
			controlFloorHeatingStatus[0] = (byte)readFloorHeatingStatusResponse.TemperatureType;
			controlFloorHeatingStatus[1] = (byte)readFloorHeatingStatusResponse.Status;
			controlFloorHeatingStatus[2] = (byte)readFloorHeatingStatusResponse.Mode;
			controlFloorHeatingStatus[3] = (byte)readFloorHeatingStatusResponse.TemperatureNormal;
			controlFloorHeatingStatus[4] = (byte)readFloorHeatingStatusResponse.TemperatureDay;
			controlFloorHeatingStatus[5] = (byte)readFloorHeatingStatusResponse.TemperatureNight;
			controlFloorHeatingStatus[6] = (byte)readFloorHeatingStatusResponse.TemperatureAway;

			if (status != null) controlFloorHeatingStatus[1] = (byte)status;
			if (mode != null) controlFloorHeatingStatus[2] = (byte)mode;
			if (temperatureNormal != null) controlFloorHeatingStatus[3] = (byte)temperatureNormal;
			if (temperatureDay != null) controlFloorHeatingStatus[4] = (byte)temperatureDay;
			if (temperatureNight != null) controlFloorHeatingStatus[5] = (byte)temperatureNight;
			if (temperatureAway != null) controlFloorHeatingStatus[6] = (byte)temperatureAway;

			//Console.WriteLine($"ControlFloorHeatingStatus: \t\t{BusproController.ByteArrayToText(controlFloorHeatingStatus)}");

			// Oppdaterer status for gjeldende device
			//SendOperationCode(OperationCode.ControlFloorHeatingStatus, controlFloorHeatingStatus);

			return true;
		}



		//private static void CommandReceived(object sender, CommandEventArgs args, DeviceAddress deviceAddress)
		//{
		//	var result = (Command)args;
		//	if (result == null || !result.Success) return;

		//	Console.WriteLine($"{deviceAddress.SubnetId}.{deviceAddress.DeviceId}:");

		//	Console.WriteLine($"Command received for DLP {deviceAddress.SubnetId}.{deviceAddress.DeviceId}:");
		//	Console.WriteLine(BusproController.ByteArrayToText(result.AdditionalContent));
		//}








	}
}
