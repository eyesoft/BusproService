using System;
using BusproService.Enums;

namespace ConsoleApp1
{
	public class SingleChannelLightingControl //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int ChannelNo { get; private set; }
		public int Intensity { get; private set; }
		public int RunningTimeMinutes { get; private set; }
		public int RunningTimeSeconds { get; private set; }

		private readonly IHdlBus _hdlBus;

		/// <summary>Initializes a new instance of the Single Channel Lighting Submit command.</summary>
		/// <param name="hdlBus">The HDL-bus connection to use.</param>
		/// <param name="targetSubnetId">SubnetID for recipient of command.</param>
		/// <param name="targetDeviceId">DeviceID for recipient of command.</param>
		/// <param name="channelNo">The channel number for recipient of command.</param>
		/// <param name="intensity">The intensity to set.</param>
		/// <param name="runningTimeMinutes">Minutes running-time for the command.</param>
		/// <param name="runningTimeSeconds">Seconds running-time for the command.</param>
		public SingleChannelLightingControl(IHdlBus hdlBus, int targetSubnetId, int targetDeviceId, int channelNo, int intensity, int runningTimeMinutes, int runningTimeSeconds)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			ChannelNo = channelNo;
			Intensity = intensity;
			RunningTimeMinutes = runningTimeMinutes;
			RunningTimeSeconds = runningTimeSeconds;
		}

		/// <summary>Sends the command to the HDL-bus.</summary>
		/// <returns>True if sending succeded, otherwise false.</returns>
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
	}




	
	public class SceneSwitch //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int AreaNo { get; private set; }
		public int SceneNo { get; private set; }
		private readonly HdlBus _hdlBus;

		public SceneSwitch(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int areaNo, int sceneNo)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			AreaNo = areaNo;
			SceneNo = sceneNo;
		}

		/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
	*/
	}

	public enum OnOff
	{
		Off = 0,
		On = 255
	}


	public class SequenceSwitch //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int AreaNo { get; private set; }
		public int SequenceNo { get; private set; }
		private readonly HdlBus _hdlBus;

		public SequenceSwitch(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int areaNo, int sequenceNo)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			AreaNo = areaNo;
			SequenceNo = sequenceNo;
		}

			/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
	*/
	}

	public class UniversalSwitch //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int SwitchNo { get; private set; }
		public OnOff OnOff { get; private set; }
		private readonly HdlBus _hdlBus;

		public UniversalSwitch(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int switchNo, OnOff onOff)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			SwitchNo = switchNo;
			OnOff = onOff;
		}

				/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
		*/
	}


	public class CurtainSwitch //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int CurtainNo { get; private set; }
		public int StopOpenClose { get; private set; }
		private readonly HdlBus _hdlBus;

		public CurtainSwitch(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int curtainNo, int stopOpenClose)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			CurtainNo = curtainNo;
			StopOpenClose = stopOpenClose;
		}

				/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
		*/
	}

	public class GprsControl //: IControls
	{
		public GprsControl()
		{
			throw new NotImplementedException("Class GprsControl is not yet implemented!");
		}

		public bool Send()
		{
			return false;
		}
	}

			/*
	public class PanelControl : IControls
	{
		public PanelControl()
		{
			throw new NotImplementedException("Class PanelControl is not yet implemented!");
		}

		public bool Send()
		{
			return false;
		}
	}
	*/

	public class BroadcastScene //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int SceneNo { get; private set; }
		private readonly HdlBus _hdlBus;

		public BroadcastScene(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int sceneNo)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			SceneNo = sceneNo;
		}

				/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
		*/
	}

	public class BroadcastChannel// : IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int Intensity { get; private set; }
		public int DelayTimeMinutes { get; private set; }
		public int DelayTimeSeconds { get; private set; }
		private HdlBus _hdlBus;

		public BroadcastChannel(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int intensity, int delayTimeMinutes, int delayTimeSeconds)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			Intensity = intensity;
			DelayTimeMinutes = delayTimeMinutes;
			DelayTimeSeconds = delayTimeSeconds;
		}

				/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
		*/
	}

	public class SecurityControl //: IControls
	{
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		public int AreaNo { get; private set; }
		public int Mode { get; private set; }
		private HdlBus _hdlBus;

		public SecurityControl(HdlBus hdlBus, int targetSubnetId, int targetDeviceId, int areaNo, int mode)
		{
			_hdlBus = hdlBus;
			TargetSubnetId = targetSubnetId;
			TargetDeviceId = targetDeviceId;
			AreaNo = areaNo;
			Mode = mode;
		}

				/*
		public bool Send()
		{
			return _hdlBus.SendMsg(this);
		}
		*/
	}

	public class TTPlayer : IControls
	{
		public TTPlayer()
		{
			throw new NotImplementedException("Class TTPlayer is not yet implemented!");
		}

		public bool Send()
		{
			return false;
		}
	}

	//public class HdlData
	//{
	//	public HdlData(int sourceSubnetId, int sourceDeviceId, string sourceDeviceTypeHex, string operateCodeHex, int targetSubnetId, int targetDeviceId, string contentHex, int contentLength, string rawData)
	//	{
	//		RawData = rawData;
	//		ContentLength = contentLength;
	//		ContentHex = contentHex;
	//		TargetDeviceId = targetDeviceId;
	//		TargetSubnetId = targetSubnetId;
	//		OperateCodeHex = operateCodeHex;
	//		SourceDeviceTypeHex = sourceDeviceTypeHex;
	//		SourceDeviceId = sourceDeviceId;
	//		SourceSubnetId = sourceSubnetId;
	//	}

	//	public int SourceSubnetId { get; private set; }
	//	public int SourceDeviceId { get; private set; }
	//	public string SourceDeviceTypeHex { get; private set; }
	//	public string OperateCodeHex { get; private set; }
	//	public int TargetSubnetId { get; private set; }
	//	public int TargetDeviceId { get; private set; }
	//	public string ContentHex { get; private set; }
	//	public int ContentLength { get; private set; }
	//	public string RawData { get; private set; }
	//}














	//public class HDL
	//{
	//	public string ipAddress { get; set; }
	//	public int port { get; set; }
	//	public int senderSubnetId { get; set; }
	//	public int senderDeviceId { get; set; }
	//}

	//public class SingleChannelLightingControlTEST
	//{
	//	public int TargetSubnetId { get; set; }
	//	public int TargetDeviceId { get; set; }
	//	public int ChannelNo { get; set; }
	//	public int Intensity { get; set; }
	//	public int RunningTimeMinutes { get; set; }
	//	public int RunningTimeSeconds { get; set; }
	//	public HDL myHdl { get; set; }


	//	public void Send()
	//	{
	//		var hdl = new HdlBus(myHdl.ipAddress, myHdl.port, myHdl.senderSubnetId, myHdl.senderDeviceId, null);

	//		var operateCode = hdl.OperateCodeToByteArray(HdlBus.OperateCode.SingleChannelLightingControl);
	//		int senderSubnetId = 0;
	//		if (myHdl.senderSubnetId != null) senderSubnetId = (int)myHdl.senderSubnetId;
	//		int senderDeviceId = 0;
	//		if (myHdl.senderDeviceId != null) senderDeviceId = (int)myHdl.senderDeviceId;
	//		//var senderDeviceType = hdl.DeviceTypeToByteArray(myHdl.senderDeviceType);

	//		var content = new byte[4];
	//		content[0] = (byte)ChannelNo;
	//		content[1] = (byte)Intensity;
	//		content[2] = (byte)RunningTimeMinutes;
	//		content[3] = (byte)RunningTimeSeconds;

	//		var ok = hdl.SendMsg(TargetSubnetId, TargetDeviceId, operateCode, content, senderSubnetId, senderDeviceId, null);


	//	}
	//}

	//public class Data
	//{
	//	public HDL myHdl { get; set; }

	//	public int Read()
	//	{
	//		/*Sender
	//		 * Target
	//		 * Data
	//		 * etc
	//		 */
	//		return 0;
	//	}
	//}

}
