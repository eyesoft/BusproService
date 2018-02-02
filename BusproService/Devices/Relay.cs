using System;
using BusproService.Enums;

namespace BusproService.Devices
{
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

		public bool SingleChannelControl(int channel, Channel.State channelState)
		{
			var intensity = 0;
			if (channelState == Enums.Channel.State.On) intensity = 100;

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

		public bool SingleChannelControl(Channel.State channelState)
		{
			return SingleChannelControl(Channel, channelState);
		}
	}
}
