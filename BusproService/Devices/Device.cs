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

















}