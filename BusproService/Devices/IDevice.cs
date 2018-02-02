using BusproService.Enums;

namespace BusproService
{
	internal interface IDevice
	{
		bool UniversalSwitch(int switchId, Channel.State switchState);
		bool SendOperationCode(OperationCode operationCode, byte[] additionalContent);
	}
}
