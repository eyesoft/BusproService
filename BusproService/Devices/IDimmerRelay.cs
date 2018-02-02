using System;
using System.Collections.Generic;
using System.Text;
using BusproService.Enums;

namespace BusproService.Devices
{
	internal interface IDimmerRelay
	{
		bool SingleChannelControl(int channel, int intensity, int secondsRunningtime);
		bool SingleChannelControl(int channel, int intensity, TimeSpan secondsRunningtime);
		bool SingleChannelControl(int channel, ChannelState channelState);
		bool SingleChannelControl(int intensity, int secondsRunningtime);
		bool SingleChannelControl(int intensity, TimeSpan secondsRunningtime);
		bool SingleChannelControl(ChannelState channelState);
		int Channel { get; set; }
	}
}
