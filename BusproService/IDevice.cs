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

	internal interface ILogic
	{
	}

	internal interface IDimmer
	{
	}

	internal interface IRelay
	{
	}

	internal interface IDlp
	{
		bool ReadAcCurrentState();
		bool ReadFloorHeatingStatus();
		//bool ReadFloorHeatingStatus(Device.CommandResponseCallback taskCompletedCallback);
		Task<bool> ControlFloorHeatingStatus(Temperature.Status? status = null, Temperature.Mode? mode = null, int? temperatureNormal = null, int? temperatureDay = null, int? temperatureNight = null, int? temperatureAway = null);
	}

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
