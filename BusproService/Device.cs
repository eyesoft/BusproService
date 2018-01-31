using System;
using BusproService;
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


		public delegate void OnCommandReceivedEventHandler(object sender, CommandEventArgs args);
		public event OnCommandReceivedEventHandler CommandReceived;

		internal virtual void OnCommandReceived(CommandEventArgs args)
		{
			CommandReceived?.Invoke(this, args);
		}





		internal readonly DeviceAddress DeviceAddress;
		internal readonly DeviceType DeviceType;
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

			var data = new Command
			{
				AdditionalContent = additionalContent,
				OperationCode = OperationCode.UniversalSwitch,
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