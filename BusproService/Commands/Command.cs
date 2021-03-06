﻿using System;
using System.Collections.Generic;
using BusproService.Enums;

namespace BusproService
{
	public class Command : CommandEventArgs
	{
	}

	public class CommandEventArgs : EventArgs
	{
		public bool Success;
		public bool ErrorMessageSpecified;
		public string ErrorMessage;

		public DeviceAddress SourceAddress;
		public DeviceAddress TargetAddress;

		private DeviceType _sourceDeviceType;
		public DeviceType SourceDeviceType
		{
			get
			{
				if (_sourceDeviceType == DeviceType.UnknownDevice && SourceDeviceTypeBytes != null) return BusproController.GetDeviceType(SourceDeviceTypeBytes);
				return _sourceDeviceType;
			}
			set => _sourceDeviceType = value;
		}
		private string _sourceDeviceTypeHex;
		public string SourceDeviceTypeHex
		{
			get
			{
				if (_sourceDeviceTypeHex == null && SourceDeviceTypeBytes != null) return BusproController.ByteArrayToHex(SourceDeviceTypeBytes);
				return _sourceDeviceTypeHex;
			}
			internal set => _sourceDeviceTypeHex = value;
		}

		private OperationCode _operationCode;
		public OperationCode OperationCode
		{
			get
			{
				if (_operationCode == OperationCode.NotSet && OperationCodeBytes != null) return BusproController.GetOperationCode(OperationCodeBytes);
				return _operationCode;
			}
			set => _operationCode = value;
		}
		private string _operationCodeHex;
		public string OperationCodeHex
		{
			get
			{
				if (_operationCodeHex == null && OperationCodeBytes != null) return BusproController.ByteArrayToHex(OperationCodeBytes);
				return _operationCodeHex;
			}
			internal set => _operationCodeHex = value;
		}

		public byte[] AdditionalContent;

		internal byte[] RawData;
		internal byte[] OperationCodeBytes;
		internal byte[] SourceDeviceTypeBytes;
	}

	public class ReadFloorHeatingStatusResponse
	{
		public Temperature.Type TemperatureType { get; set; }
		public int CurrentTemperature { get; internal set; }
		public Temperature.Status Status { get; set; }
		public Temperature.Mode Mode { get; set; }
		public int TemperatureNormal { get; set; }
		public int TemperatureDay { get; set; }
		public int TemperatureNight { get; set; }
		public int TemperatureAway { get; set; }
		//public TemperatureTimer TemperatureTimer { get; internal set; }
	}

	public class SingleChannelControl
	{
		public int ChannelNo;
		public int ChannelLevel;
		public int RunningTimeSeconds;
	}

	public class SingleChannelControlResponse
	{
		public int ChannelNo { get; internal set; }
		public SuccessOrFailure SuccessOrFail { get; internal set; }
		public int ChannelLevel { get; internal set; }
		public int NoChannels { get; internal set; }
		public Dictionary<int, Channel.Status> ChannelStatus { get; internal set; }
	}

	public class BroadcastTemperature
	{

	}

	public class BroadcastSensorsStatus
	{

	}

	public class BroadcastStatusOfScene
	{

	}

	public class BroadcastSystemDateTime
	{

	}

	public class UniversalSwitchControl
	{

	}

	public class UniversalSwitchControlResponse
	{

	}

	public class SceneControl
	{

	}

	public class SceneControlResponse
	{

	}

}
