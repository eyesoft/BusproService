using BusproService.Enums;

namespace BusproService
{
	public interface IBusproController
	{
		void ReadBus();
		void ReadBus(DeviceType filterOnSourceDeviceType);
		Device Device(Device device);
		DeviceAddress SourceAddress { get; set; }
		DeviceType SourceDeviceType { get; set; }
	}
}
