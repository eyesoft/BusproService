using System;
using System.Collections.Generic;
using System.Text;
using BusproService.Enums;

namespace BusproService.Devices
{
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
}
