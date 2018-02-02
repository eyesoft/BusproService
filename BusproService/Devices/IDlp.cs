using System.Threading.Tasks;
using BusproService.Enums;

namespace BusproService.Devices
{
	internal interface IDlp
	{
		bool ReadAcCurrentState();
		bool ReadFloorHeatingStatus();
		//bool ReadFloorHeatingStatus(Device.CommandResponseCallback taskCompletedCallback);
		Task<bool> ControlFloorHeatingStatus(Temperature.Status? status = null, Temperature.Mode? mode = null, int? temperatureNormal = null, int? temperatureDay = null, int? temperatureNight = null, int? temperatureAway = null);
	}
}
