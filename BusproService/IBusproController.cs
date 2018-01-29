using BusproService.Enums;

namespace BusproService
{
	public interface IBusproController
	{
		void ReadBus();
		void ReadBus(DeviceType filterOnSourceDeviceType);
		Device AddDevice(Device device);
		Device GetDevice(DeviceAddress deviceAddress);
		DeviceAddress SourceAddress { get; set; }
		DeviceType SourceDeviceType { get; set; }
	}
}
