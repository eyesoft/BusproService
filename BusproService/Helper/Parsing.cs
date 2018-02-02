using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using BusproService.Enums;
using BusproService.Helper;

namespace BusproService
{
	public class Parsing : IParsing
	{

		public static OperationCode ParseOperationCode(string operateCodeHex)
		{
			if (string.IsNullOrEmpty(operateCodeHex)) return OperationCode.NotSet;

			operateCodeHex = operateCodeHex.Replace("-", "");
			var operateCodeId = int.Parse(operateCodeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var operateCode = (OperationCode)Enum.Parse(typeof(OperationCode), operateCodeId);

			if (!Enum.IsDefined(typeof(OperationCode), operateCode))
				operateCode = OperationCode.NotSet;

			return operateCode;
		}

		public static DeviceType ParseDeviceType(string deviceTypeHex)
		{
			if (string.IsNullOrEmpty(deviceTypeHex)) return DeviceType.UnknownDevice;

			deviceTypeHex = deviceTypeHex.Replace("-", "");
			var deviceTypeId = int.Parse(deviceTypeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), deviceTypeId);

			if (!Enum.IsDefined(typeof(DeviceType), deviceType))
				deviceType = DeviceType.UnknownDevice;

			return deviceType;
		}

		public static ReadFloorHeatingStatusResponse ParseReadFloorHeatingStatusResponse(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.ReadFloorHeatingStatusResponse) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				var status = new ReadFloorHeatingStatusResponse
				{
					TemperatureType = (Temperature.Type)additionalContent[0],
					CurrentTemperature = additionalContent[1],
					Status = (Temperature.Status)additionalContent[2],
					Mode = (Temperature.Mode)additionalContent[3],
					TemperatureNormal = additionalContent[4],
					TemperatureDay = additionalContent[5],
					TemperatureNight = additionalContent[6],
					TemperatureAway = additionalContent[7]
					//TemperatureTimer = (TemperatureTimer)additionalContent[8]
				};

				return status;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}







		public static SingleChannelControl ParseSingleChannelControl(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.SingleChannelControl) return null;
			if (command.AdditionalContent?.Length < 4) return null;
			var additionalContent = command.AdditionalContent;
			if (additionalContent == null) return null;

			try
			{
				//var seconds = 100;
				//var secondsHigh = seconds / 256;
				//var secondsLow = seconds % 256;
				//var newSeconds = secondsHigh * 256 + secondsLow;

				var secondsHigh = additionalContent[2];
				var secondsLow = additionalContent[3];
				var seconds = secondsHigh * 256 + secondsLow;
				var channelLevel = additionalContent[1];
				if (channelLevel > 100) channelLevel = 100;

				var result = new SingleChannelControl
				{
					ChannelNo = additionalContent[0],
					ChannelLevel = channelLevel,
					RunningTimeSeconds = seconds
				};

				return result;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static SingleChannelControlResponse ParseSingleChannelControlResponse(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.SingleChannelControlResponse) return null;
			if (command.AdditionalContent?.Length < 5) return null;
			var additionalContent = command.AdditionalContent;
			if (additionalContent == null) return null;

			try
			{
				// Limited to 8 channels

				var channelStatus = new Dictionary<int, Channel.Status>();
				var channelStatusBinaryString = Convert.ToString(additionalContent[4], 2).PadLeft(8, '0');
				var noChannels = additionalContent[3];

				var channelNo = 1;
				for (var i = channelStatusBinaryString.Length - 1; i >= 0; i--)
				{
					if (channelNo > noChannels) break;
					channelStatus.Add(channelNo, (Channel.Status)int.Parse(channelStatusBinaryString[i].ToString()));
					channelNo++;
				}
				
				var result = new SingleChannelControlResponse
				{
					ChannelNo = additionalContent[0],
					SuccessOrFail = (SuccessOrFailure)additionalContent[1],
					ChannelLevel = additionalContent[2],
					NoChannels = additionalContent[3],
					ChannelStatus = channelStatus
				};

				return result;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static BroadcastTemperature ParseBroadcastTemperature(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.BroadcastTemperature) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static BroadcastSensorsStatus ParseBroadcastSensorsStatus(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.BroadcastSensorsStatus) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static BroadcastStatusOfScene ParseBroadcastStatusOfScene(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.BroadcastStatusOfScene) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static BroadcastSystemDateTime ParseBroadcastSystemDateTime(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.BroadcastSystemDateTime) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;
			return null;
		}

		public static UniversalSwitchControl ParseUniversalSwitchControl(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.UniversalSwitchControl) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static UniversalSwitchControlResponse ParseUniversalSwitchControlResponse(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.UniversalSwitchControlResponse) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static SceneControl ParseSceneControl(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.SceneControl) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}

		public static SceneControlResponse ParseSceneControlResponse(CommandEventArgs command)
		{
			if (command?.OperationCode != OperationCode.SceneControlResponse) return null;
			if (command.AdditionalContent?.Length < 8) return null;
			var additionalContent = command.AdditionalContent;

			try
			{
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}




	}
}
