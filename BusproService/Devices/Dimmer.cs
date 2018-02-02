using System;
using System.Collections.Generic;
using System.Text;
using BusproService.Enums;

namespace BusproService.Devices
{
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
}
