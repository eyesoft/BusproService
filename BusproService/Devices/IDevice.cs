using System;
using System.Threading.Tasks;
using BusproService.Enums;

namespace BusproService
{
	internal interface IDevice
	{
		bool UniversalSwitch(int switchId, ChannelState switchState);
		bool SendOperationCode(OperationCode operationCode, byte[] additionalContent);
	}
}
