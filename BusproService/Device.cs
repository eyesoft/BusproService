using System;
using System.Threading.Tasks;
using BusproService.Enums;

namespace BusproService
{
	public class DeviceAddress
	{
		public int SubnetId;
		public int DeviceId;
	}

	public class Device : IDevice
	{
		//public delegate void CommandResponseCallback(Command command);
		
		public delegate void OnCommandReceivedEventHandler(object sender, CommandEventArgs args);
		public event OnCommandReceivedEventHandler CommandReceived;
		internal virtual void OnCommandReceived(CommandEventArgs args)
		{
			CommandReceived?.Invoke(this, args);
		}
		
		public delegate void OnResponseCommandReceivedEventHandler(object sender, CommandEventArgs args);
		public event OnResponseCommandReceivedEventHandler ResponseCommandReceived;
		internal virtual void OnResponseCommandReceived(CommandEventArgs args)
		{
			ResponseCommandReceived?.Invoke(this, args);
		}
		
		internal OperationCode OperationCode { get; set; }
		public readonly DeviceAddress DeviceAddress;
		public readonly DeviceType DeviceType;
		internal readonly BusproController Controller;

		protected Device()
		{
		}

		public Device(int subnetId, int deviceId)
		{
			DeviceAddress = new DeviceAddress { SubnetId = subnetId, DeviceId = deviceId };
		}

		public Device(DeviceAddress deviceAddress)
		{
			DeviceAddress = deviceAddress;
		}

		public Device(DeviceType deviceType, DeviceAddress deviceAddress)
		{
			DeviceAddress = deviceAddress;
			DeviceType = deviceType;
		}

		internal Device(BusproController controller, DeviceType deviceType, DeviceAddress deviceAddress)
		{
			Controller = controller;
			DeviceAddress = deviceAddress;
			DeviceType = deviceType;
		}

		protected bool SingleChannelControl(int channel, int intensity, int secondsRunningtime)
		{
			if (channel == 0) throw new InvalidOperationException("Channel not set");
			if (channel < 1 || channel > 255) throw new InvalidOperationException($"Invalid Channel {channel}");
			if (intensity < 0 || intensity > 100) throw new InvalidOperationException($"Invalid intensity {intensity}");
			if (secondsRunningtime < 0 || secondsRunningtime > 3600) throw new InvalidOperationException($"Invalid secondsRunningtime {secondsRunningtime}");

			var minutes = secondsRunningtime / 256;
			var seconds = secondsRunningtime % 256;

			var additionalContent = new byte[4];
			additionalContent[0] = (byte)channel;
			additionalContent[1] = (byte)intensity;
			additionalContent[2] = (byte)minutes;
			additionalContent[3] = (byte)seconds;

			OperationCode = OperationCode.SingleChannelControl;

			var data = new Command
			{
				AdditionalContent = additionalContent,
				OperationCode = OperationCode.SingleChannelControl,
				SourceAddress = Controller.SourceAddress,
				SourceDeviceType = Controller.SourceDeviceType,
				TargetAddress = DeviceAddress
			};

			var result = Controller.WriteBus(data);
			return result;
		}

		public bool UniversalSwitch(int switchId, ChannelState switchState)
		{
			if (switchId == 0) throw new InvalidOperationException("SwitchId not set");
			if (switchId < 1 || switchId > 255) throw new InvalidOperationException($"Invalid SwitchId {switchId}");

			var additionalContent = new byte[2];
			additionalContent[0] = (byte)switchId;
			additionalContent[1] = (byte)switchState;

			OperationCode = OperationCode.UniversalSwitchControl;

			var data = new Command
			{
				AdditionalContent = additionalContent,
				OperationCode = OperationCode.UniversalSwitchControl,
				SourceAddress = Controller.SourceAddress,
				SourceDeviceType = Controller.SourceDeviceType,
				TargetAddress = DeviceAddress
			};

			var result = Controller.WriteBus(data);
			return result;
		}

		public bool SendOperationCode(OperationCode operationCode, byte[] additionalContent)
		{
			if (additionalContent == null) additionalContent = new byte[0];

			OperationCode = operationCode;

			var data = new Command
			{
				AdditionalContent = additionalContent,
				OperationCode = operationCode,
				SourceAddress = Controller.SourceAddress,
				SourceDeviceType = Controller.SourceDeviceType,
				TargetAddress = DeviceAddress
			};

			var result = Controller.WriteBus(data);
			return result;
		}
















	}













	public class Logic : Device, ILogic
	{
		internal Logic(BusproController controller, DeviceType deviceType, DeviceAddress deviceAddress) : base(controller, deviceType, deviceAddress)
		{
		}

		public Logic(int subnetId, int deviceId) : base(subnetId, deviceId)
		{
		}

		public Logic(DeviceAddress deviceAddress) : base(deviceAddress)
		{
		}

		public Logic(DeviceType deviceType, DeviceAddress deviceAddress) : base(deviceType, deviceAddress)
		{
		}
	}

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
			var readFloorHeatingStatusResponse = ParseReadFloorHeatingStatusResponse(command);
			if (readFloorHeatingStatusResponse == null) return false;

			//Console.WriteLine($"Current FloorHeatingStatus: \t{Newtonsoft.Json.JsonConvert.SerializeObject(readFloorHeatingStatusResponse)}");

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

		private FloorHeatingStatus ParseReadFloorHeatingStatusResponse(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.ReadFloorHeatingStatusResponse) return null;

			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				var status = new FloorHeatingStatus
				{
					TemperatureType = (Temperature.Type)additionalContent[0],
					CurrentTemperature = additionalContent[1],
					Status = (Temperature.Status)additionalContent[2],
					Mode = (Temperature.Mode)additionalContent[3],
					TemperatureNormal = additionalContent[4],
					TemperatureDay = additionalContent[5],
					TemperatureNight = additionalContent[6],
					TemperatureAway = additionalContent[7]
					//TemperatureTimer = (TemperatureTimer)additionalContent[8]
				};

				return status;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
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

	public class Dimmer : Device, IDimmer, IDimmerRelay
	{
		internal Dimmer(BusproController controller, DeviceType deviceType, DeviceAddress deviceAddress) : base(controller, deviceType, deviceAddress)
		{
		}

		public Dimmer(int subnetId, int deviceId) : base(subnetId, deviceId)
		{
		}

		public Dimmer(DeviceAddress deviceAddress) : base(deviceAddress)
		{
		}

		public Dimmer(DeviceType deviceType, DeviceAddress deviceAddress) : base(deviceType, deviceAddress)
		{
		}

		public int Channel { get; set; }

		public new bool SingleChannelControl(int channel, int intensity, int secondsRunningtime)
		{
			return base.SingleChannelControl(channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(int channel, int intensity, TimeSpan secondsRunningtime)
		{
			var seconds = (int)secondsRunningtime.TotalSeconds;
			return SingleChannelControl(channel, intensity, seconds);
		}

		public bool SingleChannelControl(int channel, ChannelState channelState)
		{
			var intensity = 0;
			if (channelState == ChannelState.On) intensity = 100;
			return SingleChannelControl(channel, intensity, 0);
		}

		public bool SingleChannelControl(int intensity, int secondsRunningtime)
		{
			return SingleChannelControl(Channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(int intensity, TimeSpan secondsRunningtime)
		{
			return SingleChannelControl(Channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(ChannelState channelState)
		{
			return SingleChannelControl(Channel, channelState);
		}
	}

	public class Relay : Device, IRelay, IDimmerRelay
	{
		internal Relay(BusproController controller, DeviceType deviceType, DeviceAddress deviceAddress) : base(controller, deviceType, deviceAddress)
		{
		}

		public Relay(int subnetId, int deviceId) : base(subnetId, deviceId)
		{
		}

		public Relay(DeviceAddress deviceAddress) : base(deviceAddress)
		{
		}

		public Relay(DeviceType deviceType, DeviceAddress deviceAddress) : base(deviceType, deviceAddress)
		{
		}

		public int Channel { get; set; }

		public new bool SingleChannelControl(int channel, int intensity, int secondsRunningtime)
		{
			return base.SingleChannelControl(channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(int channel, int intensity, TimeSpan secondsRunningtime)
		{
			var seconds = (int)secondsRunningtime.TotalSeconds;

			return SingleChannelControl(channel, intensity, seconds);
		}

		public bool SingleChannelControl(int channel, ChannelState channelState)
		{
			var intensity = 0;
			if (channelState == ChannelState.On) intensity = 100;

			return SingleChannelControl(channel, intensity, 0);
		}

		public bool SingleChannelControl(int intensity, int secondsRunningtime)
		{
			return SingleChannelControl(Channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(int intensity, TimeSpan secondsRunningtime)
		{
			return SingleChannelControl(Channel, intensity, secondsRunningtime);
		}

		public bool SingleChannelControl(ChannelState channelState)
		{
			return SingleChannelControl(Channel, channelState);
		}
	}

}