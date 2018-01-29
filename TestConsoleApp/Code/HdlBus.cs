using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BusproService;
using BusproService.Enums;

namespace ConsoleApp1
{
	/// <summary>Represents a connection to the HDL-bus.</summary>
	public class HdlBus : IDisposable
		//, IHdlBus
	{
		//http://smarthousenews.com/filer/hdl-grunnkurs_kabling.pdf

		private readonly string _ipAddress;
		private readonly int _port;
		private UdpClient _listener;
		private IPEndPoint _groupEp;
		private int? _filterOriginalDeviceId;
		//private readonly int? _senderSubnetId;
		//private readonly int? _senderDeviceId;
		//private readonly DeviceType? _senderDeviceType;

		#region IDisposable Members
		public void Dispose()
		{
			//Dispose(true);
			//GC.SuppressFinalize(this);

			_listener?.Close();
		}
		#endregion

		//~HdlBus()
		//{
		//	Dispose(false);
		//}

		//protected virtual void Dispose(bool disposing)
		//{
		//	if (disposing)
		//		ReleaseManagedResources();

		//	ReleaseUnmanagedResources();
		//}

		//void ReleaseManagedResources()
		//{
		//	if (_listener != null)
		//		_listener.Close();
		//}

		//void ReleaseUnmanagedResources()
		//{
		//}





		public int SenderSubnetId { get; set; }
		public int SenderDeviceId { get; set; }
		public DeviceType SenderDeviceType { get; set; }


		/// <summary>Initializes a new connection to the HDL-bus.</summary>
		/// <param name="ipAddress">IP-address of the logic-module.</param>
		/// <param name="port">Portnumber used by the HDL-bus.</param>
		public HdlBus(string ipAddress, int port)
		{
			_ipAddress = ipAddress;
			_port = port;
			//_senderSubnetId = senderSubnetId;
			//_senderDeviceId = senderDeviceId;
			//_senderDeviceType = senderDeviceType;

			//http://www.geekpedia.com/Thread13433_Why-Cant-I-Have-Multiple-Listeners-On-A-UDP-Port.html
			_listener = new UdpClient(_port);
			_groupEp = new IPEndPoint(IPAddress.Any, _port);
		}
		//public HdlBus(string ipAddress, int port)
		//	: this(ipAddress, port, null, null, null)
		//{
		//}

		///// <summary>Initializes a new connection to the HDL-bus.</summary>
		///// <param name="ipAddress">IP-address of the logic-module.</param>
		///// <param name="port">Portnumber used by the HDL-bus.</param>
		///// <param name="senderSubnetId">SubnetID of the sending application.</param>
		///// <param name="senderDeviceId">DeviceID of the sending application.</param>
		///// <param name="senderDeviceType">DeviceType of the sending application.</param>
		//public HdlBus(string ipAddress, int port, int? senderSubnetId, int? senderDeviceId, DeviceType? senderDeviceType)
		//{
		//	_ipAddress = ipAddress;
		//	_port = port;
		//	_senderSubnetId = senderSubnetId;
		//	_senderDeviceId = senderDeviceId;
		//	_senderDeviceType = senderDeviceType;

		//	//http://www.geekpedia.com/Thread13433_Why-Cant-I-Have-Multiple-Listeners-On-A-UDP-Port.html
		//	_listener = new UdpClient(_port);
		//	_groupEp = new IPEndPoint(IPAddress.Any, _port);
		//}











		//private void Close()
		//{
		//	_listener.Close();
		//}

		/// <summary>The filenaneme for the logfile if logging is to be done.</summary>
		public string LogFilename { get; set; }

		internal void SetFilter(int? filterOriginalDeviceId)
		{
			_filterOriginalDeviceId = filterOriginalDeviceId;
		}

		private void WriteToFile(string text)
		{
			if (!LogToFile) return;
			var fileExists = File.Exists(LogFilename);
			var file = new StreamWriter(LogFilename, true);

			if (!fileExists)
				file.WriteLine("Date;SourceSubnetID;SourceDeviceID;SourceDeviceTypeHex;SourceDeviceType;OperateCodeHex;OperateCode;TargetSubnetID;TargetDeviceID;ContentHex;ContentLength;RawData");

			text = DateTime.Now.ToLocalTime() + ";" + text;

			file.WriteLine(text);
			file.Close();
		}

		private bool LogToFile
		{
			get
			{
				return !string.IsNullOrEmpty(LogFilename);
			}
		}

		/*
		private bool SendMsg(int targetSubnetId, int targetDeviceId, byte[] operateCode, byte[] content, int? senderSubnetId, int? senderDeviceId, byte[] senderDeviceType)
		{
			try
			{
				if (senderSubnetId == null) senderSubnetId = 0;
				if (senderDeviceId == null) senderDeviceId = 0;
				if (senderDeviceType == null) senderDeviceType = new byte[2];

				var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				var broadcast = IPAddress.Parse(_ipAddress);
				//byte[] sendbuf = GetTestBufToSend();
				byte[] sendbuf = BuildBufToSend(targetSubnetId, targetDeviceId, operateCode, content, (int)senderSubnetId, (int)senderDeviceId, senderDeviceType);

				if (LogToFile)
				{
					var senderDeviceTypeHex = ByteArrayToString(senderDeviceType, 0, senderDeviceType.Length);
					var operateCodeHex = ByteArrayToString(operateCode, 0, operateCode.Length);
					var contentHex = ByteArrayToString(content, 0, content.Length);
					var contentLength = content.Length;
					var rawDataBytes = StringToByteArray(ByteArrayToString(sendbuf, 0, sendbuf.Length));

					var rawData = "";
					foreach (var t in rawDataBytes)
						rawData += t + " ";

					WriteToFile(senderSubnetId + ";" + senderDeviceId + ";" + senderDeviceTypeHex + ";" +
											GetDeviceType(senderDeviceTypeHex) + ";" +
											operateCodeHex + ";" + GetOperateCode(operateCodeHex) + ";" +
											targetSubnetId + ";" + targetDeviceId + ";" + contentHex + ";" + contentLength + ";" + rawData);
				}

				var ep = new IPEndPoint(broadcast, _port);
				s.SendTo(sendbuf, ep);
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}
		*/

		//private byte[] IntToByteArray(int value)
		//{
		//	var bytes = new byte[2];
		//	bytes[0] = (byte)(value >> 8);
		//	bytes[1] = (byte)(value);

		//	return bytes;
		//}


		/*
	private byte[] OperateCodeToByteArray(OperationCode operateCode)
	{
		var value = (int)operateCode;
		var bytes = new byte[2];
		bytes[0] = (byte)(value >> 8);
		bytes[1] = (byte)(value);

		return bytes;
	}
	*/



		private byte[] DeviceTypeToByteArray(DeviceType? deviceType)
		{
			if (deviceType == null)
				return new byte[2];

			var value = (int)deviceType;
			var bytes = new byte[2];
			bytes[0] = (byte)(value >> 8);
			bytes[1] = (byte)(value);

			return bytes;
		}


		/*
		public bool SendMsg(dynamic action)
		{
			var ok = false;

			try
			{
				string actionName = action.GetType().Name;
				byte[] operateCode = null;
				var senderDeviceType = DeviceTypeToByteArray(SenderDeviceType);
				byte[] content = null;

				switch (actionName)
				{
					case "SingleChannelLightingControl":
						operateCode = OperateCodeToByteArray(OperationCode.SingleChannelControl);

						content = new byte[4];
						content[0] = (byte) action.ChannelNo;
						content[1] = (byte) action.Intensity;
						content[2] = (byte) action.RunningTimeMinutes;
						content[3] = (byte) action.RunningTimeSeconds;

						break;
					case "SceneSwitch":
						break;
					case "UniversalSwitch":
						operateCode = OperateCodeToByteArray(OperationCode.UniversalSwitch);

						content = new byte[2];
						content[0] = (byte) action.SwitchNo;
						content[1] = (byte) action.OnOff;

						break;
				}

				ok = SendMsg(action.TargetSubnetId, action.TargetDeviceId, operateCode, content, SenderSubnetId, SenderDeviceId,
					senderDeviceType);

				// Status må oppdateres asynkront
				//var stop = false;
				//var noTry = 5;
				//while (noTry > 0 & !stop)
				//{
				//	int sourceSubnetId = 0;
				//	int sourceDeviceId = 0;
				//	string sourceDeviceTypeHex = "";
				//	string operateCodeHex = "";
				//	int targetSubnetId = 0;
				//	int targetDeviceId = 0;
				//	string contentHex = "";
				//	int contentLength = 0;
				//	string rawData = "";

				//	RecvMsg(ref sourceSubnetId, ref sourceDeviceId, ref sourceDeviceTypeHex, ref operateCodeHex, ref targetSubnetId, ref targetDeviceId, ref contentHex, ref contentLength, ref rawData);

				//	if (operateCodeHex == "0032" & sourceDeviceId == action.TargetDeviceId)
				//	{
				//		var res = StringToByteArray(contentHex);
				//		if (res[0] == action.ChannelNo)
				//		{
				//			ret = res[2].ToString();

				//			stop = true;

				//			if (res[1].ToString() != "248")
				//				ret = null;
				//		}
				//	}

				//	noTry--;
				//}

			}
			catch (Exception ex)
			{
				var err = ex.Message;
			}

			return ok;
		}
		*/


		public struct HdlDataStruct
		{
			public int SourceSubnetId;
			public int SourceDeviceId;
			public string SourceDeviceTypeHex;
			public string OperateCodeHex;
			public int TargetSubnetId;
			public int TargetDeviceId;
			public string ContentHex;
			public int ContentLength;
			public string RawData;
		}

		/*
		public HdlDataStruct? RecvMsg()
		{
			HdlDataStruct hdlData;
			hdlData.ContentHex = "";
			hdlData.RawData = "";

			if (_listener.Client == null)
				_listener = new UdpClient(_port);

			try
			{
				// Synchronous receive
				var bytes = _listener.Receive(ref _groupEp);
				//var hex = ByteArrayToString(bytes, 0, bytes.Length);

				foreach (var t in bytes)
					hdlData.RawData += t + " ";

				const int indexLengthOfDataPackage = 16;
				const int indexOriginalSubnetId = 17;
				const int indexOriginalDeviceId = 18;
				const int indexOriginalDeviceType = 19;
				const int indexOperateCode = 21;
				const int indexTargetSubnetId = 23;
				const int indexTargetDeviceId = 24;
				const int indexContent = 25;
				int lengthOfDataPackage = bytes[indexLengthOfDataPackage];

				hdlData.SourceDeviceId = bytes[indexOriginalDeviceId];

				if (_filterOriginalDeviceId != null && (_filterOriginalDeviceId != hdlData.SourceDeviceId))
					return null;

				hdlData.ContentLength = lengthOfDataPackage - 1 - 1 - 1 - 2 - 2 - 1 - 1 - 1 - 1;
				hdlData.SourceSubnetId = bytes[indexOriginalSubnetId];
				hdlData.SourceDeviceTypeHex = ByteArrayToString(bytes, indexOriginalDeviceType, 2);
				hdlData.OperateCodeHex = ByteArrayToString(bytes, indexOperateCode, 2);
				hdlData.TargetSubnetId = bytes[indexTargetSubnetId];
				hdlData.TargetDeviceId = bytes[indexTargetDeviceId];
				var contentHex = ByteArrayToString(bytes, indexContent, hdlData.ContentLength);

				var crcH = bytes[bytes.Length - 2].ToString(CultureInfo.InvariantCulture);
				var crcL = bytes[bytes.Length - 1].ToString(CultureInfo.InvariantCulture);

				//TODO SJEKKKE CRC

				if (LogToFile)
				{
					WriteToFile(hdlData.SourceSubnetId + ";" + hdlData.SourceDeviceId + ";" + hdlData.SourceDeviceTypeHex + ";" +
											GetDeviceType(hdlData.SourceDeviceTypeHex) + ";" +
											hdlData.OperateCodeHex + ";" + GetOperateCode(hdlData.OperateCodeHex) + ";" +
											hdlData.TargetSubnetId + ";" + hdlData.TargetDeviceId + ";" + contentHex + ";" + hdlData.ContentLength + ";" + hdlData.RawData);
				}

				return hdlData;
			}
			catch (SocketException socketEx)
			{
				var err = socketEx.Message;
				return null;
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				return null;
			}
		}
		*/

		private string ByteArrayToString(byte[] bytes, int start, int length)
		{
			var newArray = new byte[length];
			for (var i = 0; i < length; i++)
			{
				newArray[i] = bytes[start + i];
			}

			return BitConverter.ToString(newArray).Replace("-", "");
		}

		//private string ByteArrayToString(byte[] bytes)
		//{
		//	return BitConverter.ToString(bytes).Replace("-", "");
		//}

		private byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length / 2)
				.Select(x => Byte.Parse(hex.Substring(2 * x, 2), NumberStyles.HexNumber))
				.ToArray();
		}


		/*
		public OperationCode GetOperateCode(string operateCodeHex)
		{
			if (string.IsNullOrEmpty(operateCodeHex)) return OperationCode.NotSet;

			var operateCodeId = Int32.Parse(operateCodeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var operateCode = (OperationCode)Enum.Parse(typeof(OperationCode), operateCodeId);

			if (!Enum.IsDefined(typeof(OperationCode), operateCode))
				operateCode = OperationCode.NotSet;

			return operateCode;
		}
		*/

		public DeviceType GetDeviceType(string deviceTypeHex)
		{
			if (string.IsNullOrEmpty(deviceTypeHex)) return DeviceType.UnknownDevice;

			var deviceTypeId = Int32.Parse(deviceTypeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), deviceTypeId);

			if (!Enum.IsDefined(typeof(DeviceType), deviceType))
				deviceType = DeviceType.UnknownDevice;

			return deviceType;
		}



		//private string GetHex(int intVal)
		//{
		//	//return intVal.ToString("X");
		//	//return String.Format("{0:X}", intVal);
		//	var hex = String.Format("{0:X}", intVal);
		//	if (hex.Length == 1) hex = "0" + hex;
		//	return hex;
		//}

		//private byte[] GetTestBufToSend()
		//{
		//	var sendbuf = new byte[31];

		//	sendbuf[0] = 192;
		//	sendbuf[1] = 168;
		//	sendbuf[2] = 1;
		//	sendbuf[3] = 15;
		//	sendbuf[4] = 72;
		//	sendbuf[5] = 68;
		//	sendbuf[6] = 76;
		//	sendbuf[7] = 77;
		//	sendbuf[8] = 73;
		//	sendbuf[9] = 82;
		//	sendbuf[10] = 65;
		//	sendbuf[11] = 67;
		//	sendbuf[12] = 76;
		//	sendbuf[13] = 69;

		//	sendbuf[14] = 170;	// Leading code
		//	sendbuf[15] = 170;	// Leading code

		//	sendbuf[16] = 15;		// Length of data package
		//	sendbuf[17] = 1;		// Original subnet id
		//	sendbuf[18] = 23;		// Original device id
		//	sendbuf[19] = 0;		// Original device type
		//	sendbuf[20] = 149;	// Original device type 
		//	sendbuf[21] = 0;		// Operate code
		//	sendbuf[22] = 49;		// Operate code 
		//	sendbuf[23] = 1;		// Target subnet id
		//	sendbuf[24] = 74;		// Target device id

		//	sendbuf[25] = 1;		// content
		//	sendbuf[26] = 40;		// content
		//	sendbuf[27] = 0;		// content
		//	sendbuf[28] = 3;		// content

		//	sendbuf[29] = 191;	// CRC H
		//	sendbuf[30] = 29;		// CRC L

		//	return sendbuf;
		//}

		private byte[] BuildBufToSend(int targetSubnetId, int targetDeviceId, byte[] operateCode, byte[] content,
			int senderSubnetId, int senderDeviceId, byte[] senderDeviceType)
		{
			var length = 25 + content.Length + 2;
			var sendbuf = new byte[length];

			// Fixed header
			sendbuf[0] = 192;   // 192
			sendbuf[1] = 168;   // 168
			sendbuf[2] = 1;     // 1
			sendbuf[3] = 15;    // 15
			sendbuf[4] = 72;    // H
			sendbuf[5] = 68;    // D
			sendbuf[6] = 76;    // L
			sendbuf[7] = 77;    // M
			sendbuf[8] = 73;    // I
			sendbuf[9] = 82;    // R
			sendbuf[10] = 65;   // A
			sendbuf[11] = 67;   // C
			sendbuf[12] = 76;   // L
			sendbuf[13] = 69;   // E

			// Fixed leading code
			sendbuf[14] = 0xAA;
			sendbuf[15] = 0xAA;

			var lengthOfDataPackage = 11 + content.Length;

			sendbuf[16] = (byte)lengthOfDataPackage;  // 15 Length of data package
			sendbuf[17] = (byte)senderSubnetId;       // 1 Original subnet id
			sendbuf[18] = (byte)senderDeviceId;       // 23 Original device id
			sendbuf[19] = senderDeviceType[0];        // 0 Original device type
			sendbuf[20] = senderDeviceType[1];        // 149 Original device type 
			sendbuf[21] = operateCode[0];             // 0 Operate code
			sendbuf[22] = operateCode[1];             // 49 Operate code 
			sendbuf[23] = (byte)targetSubnetId;       // 1 Target subnet id
			sendbuf[24] = (byte)targetDeviceId;       // 74 Target device id

			for (int i = 0; i < content.Length; i++)
				sendbuf[i + 25] = content[i];           // 1 content

			var crcBufLength = lengthOfDataPackage - 2;
			var crcBuf = new byte[crcBufLength];

			for (int i = 0; i < crcBufLength; i++)
				crcBuf[i] = sendbuf[16 + i];

			var crc = new Crc16Ccitt(0);
			var res = crc.ComputeChecksumBytes(crcBuf);

			sendbuf[sendbuf.Length - 2] = res[1];
			sendbuf[sendbuf.Length - 1] = res[0];

			return sendbuf;
		}

	}
}


/*
 * 
 * 15.03.2013 13:38:39;9;99;FFFD;SmartHDLTest;0031;SingleChannelLightingControl;1;74;01280003;4;192 168 1 15 72 68 76 77 73 82 65 67 76 69 170 170 15 9 99 255 253 0 49 1 74 1 40 0 3 242 190 
 * 
 * Sender fra prog "1 74 1 40 0 3": subnetid, deviceid, channel, intensity, minutes, seconds
 * 
 * 15.03.2013 13:38:39;1;74;0260;SB_DN_DT0601;0032;RESPONS_FRA_DIMMER;255;255;01F828;3;192 168 1 15 72 68 76 77 73 82 65 67 76 69 170 170 14 1 74 2 96 0 50 255 255 1 248 40 216 41 
 *
 * Mottar fra dimmer "1 248 40": channel, ???, intensity
*/


// CONVERT TO HEX ????
//receiveData += Convert.ToString(addBuf[i], 16) + " ";
