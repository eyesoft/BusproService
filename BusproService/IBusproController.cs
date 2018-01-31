using System.Net;
using BusproService.Enums;

namespace BusproService
{
	public interface IBusproController
	{
		void StartListen();

		Device AddDevice(Device device);
		Device GetDevice(DeviceAddress deviceAddress);

		DeviceAddress SourceAddress { get; set; }
		DeviceType SourceDeviceType { get; set; }
		int Port { get; }
		IPAddress Address { get; }
	}
}
