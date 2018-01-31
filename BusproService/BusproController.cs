using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BusproService.Enums;
using BusproService.Helper;

namespace BusproService
{
	public class BusproController : IBusproController, IDisposable
	{

		// Skip sending command to bus
		private const bool SkipSocketSend = false;

		public List<Device> Device { get; private set; }

		// Controller status changed event handler
		public delegate void OnCommandReceivedEventHandler(object sender, CommandEventArgs args);
		public delegate void OnBroadcastCommandReceivedEventHandler(object sender, CommandEventArgs args);

		// Occurs when controller status changed
		public event OnCommandReceivedEventHandler CommandReceived;
		public event OnBroadcastCommandReceivedEventHandler BroadcastCommandReceived;

		// Raises the controller status changed event
		protected virtual void OnCommandReceived(CommandEventArgs args)
		{
			CommandReceived?.Invoke(this, args);
		}
		protected virtual void OnBroadcastCommandReceived(CommandEventArgs args)
		{
			BroadcastCommandReceived?.Invoke(this, args);
		}




		private UdpClient Listener { get; }
		private IPEndPoint EndPoint { get; }

		public DeviceAddress SourceAddress { get; set; }
		public DeviceType SourceDeviceType { get; set; }
		public int Port { get; }
		public IPAddress Address { get; }




		public BusproController() : this(null, 0)
		{
		}

		public BusproController(string ipAddress) : this(ipAddress, 0)
		{
		}

		public BusproController(string ipAddress, int port)
		{
			var address = IPAddress.Any;
			if (!string.IsNullOrWhiteSpace(ipAddress)) address = IPAddress.Parse(ipAddress);

			if (port == 0) port = 6000;

			//http://www.geekpedia.com/Thread13433_Why-Cant-I-Have-Multiple-Listeners-On-A-UDP-Port.html
			Listener = new UdpClient(port);
			EndPoint = new IPEndPoint(address, port);

			Port = port;
			Address = address;

			Device = new List<Device>();
		}




		public Device GetDevice(DeviceAddress deviceAddress)
		{
			var myItem = Device.Find(device => device.DeviceAddress.SubnetId == deviceAddress.SubnetId && device.DeviceAddress.DeviceId == deviceAddress.DeviceId);
			return myItem;
		}


		public Device AddDevice(Device device)
		{
			// Adds the device to the local list of added devices
			// Device in this list will get their messages sent to the event handler OnCommandReceived
			// _devices.Add(device);

			switch (device)
			{
				case Logic logic:
					return AddDeviceToController(new Logic(this, logic.DeviceType, logic.DeviceAddress));
				case Relay relay:
					return AddDeviceToController(new Relay(this, relay.DeviceType, relay.DeviceAddress));
				case Dimmer dimmer:
					return AddDeviceToController(new Dimmer(this, dimmer.DeviceType, dimmer.DeviceAddress));
				case Dlp dlp:
					return AddDeviceToController(new Dlp(this, dlp.DeviceType, dlp.DeviceAddress));
				default:
					return AddDeviceToController(new Device(this, device.DeviceType, device.DeviceAddress));
			}
		}

		private Device AddDeviceToController(Device device)
		{
			// if device is not already added
			var alreadyExists = Device.Any(x => x.DeviceAddress.SubnetId == device.DeviceAddress.SubnetId && x.DeviceAddress.DeviceId == device.DeviceAddress.DeviceId);
			if (!alreadyExists) Device.Add(device);
			return device;
		}





		public void StartListen()
		{
			new Thread(() =>
			{
				Thread.CurrentThread.IsBackground = true;
				while (true)
				{
					// Reads all commands on bus
					ReadBus();
				}
			}).Start();
		}

		private void StopListen()
		{
			// CancellationToken
		}



		private void ReadBus()
		{
			var cmd = Receive();

			foreach (var device in Device)
			{
				var subnetId = device.DeviceAddress.SubnetId;
				var deviceId = device.DeviceAddress.DeviceId;

				if ((subnetId == cmd.SourceAddress.SubnetId && deviceId == cmd.SourceAddress.DeviceId) ||
					(subnetId == cmd.TargetAddress.SubnetId && deviceId == cmd.TargetAddress.DeviceId))
				{
					device.OnCommandReceived(cmd);
				}
			}

			const int broadcastSubnetId = 255;
			const int broadcastDeviceId = 255;

			if ((broadcastSubnetId == cmd.SourceAddress.SubnetId && broadcastDeviceId == cmd.SourceAddress.DeviceId) ||
					(broadcastSubnetId == cmd.TargetAddress.SubnetId && broadcastDeviceId == cmd.TargetAddress.DeviceId))
			{
				OnBroadcastCommandReceived(cmd);
			}

			OnCommandReceived(cmd);
		}

		//public void ReadBus(DeviceType filterOnSourceDeviceType)
		//{
		//	var command = Receive(filterOnSourceDeviceType);
		//	if (command != null) OnCommandReceived(command);
		//}




		internal bool WriteBus(Command command)
		{
			var result = Send(command);
			return result.Success;
		}









		//public BusproData ReadBus(DeviceType deviceType, OperationCode operationCode)
		//{
		//	var msg = RecvMsg();
		//	//var oc = GetOperationCode(msg.OperationCode);
		//	var oc = msg.OperationCode;
		//	var dt = msg.SourceDeviceType;

		//	//if (oc == operationCode && dt == deviceType) return msg;
		//	return null;
		//}
		//public BusproData ReadBus(OperationCode operationCode)
		//{
		//	var msg = RecvMsg();
		//	//var oc = GetOperationCode(msg.OperationCode);
		//	var oc = msg.OperationCode;

		//	//if (oc == operationCode) return msg;
		//	return null;
		//}
		//public BusproData ReadBus(DeviceType deviceType)
		//{
		//	var msg = RecvMsg();
		//	var dt = msg.SourceDeviceType;

		//	//if (dt == deviceType) return msg;
		//	return null;
		//}
		//public BusproData ReadBus(DeviceAddress deviceAddress)
		//{
		//	var msg = RecvMsg();

		//	var da = deviceAddress;
		//	var ta = msg.TargetAddress;
		//	var sa = msg.SourceAddress;


		//	if (da.SubnetId == ta.SubnetId && da.DeviceId == ta.DeviceId) return msg;
		//	if (da.SubnetId == sa.SubnetId && da.DeviceId == sa.DeviceId) return msg;
		//	return null;
		//}


















		private SendResult Send(Command commandToSend)
		{
			var result = new SendResult();

			try
			{
				if (commandToSend == null) //throw new InvalidOperationException("No command to send");
					return new SendResult { Success = false, ErrorMessageSpecified = true, ErrorMessage = "No command to send" };

				if (commandToSend.SourceAddress == null) commandToSend.SourceAddress = new DeviceAddress { SubnetId = 0, DeviceId = 0 };

				var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				//var sendbuf = GetTestBufToSend();
				var sendbuf = BuildBufToSend(commandToSend);

				if (!SkipSocketSend)
					s.SendTo(sendbuf, EndPoint);

				//var hex = ByteArrayToHex(sendbuf);
				result.CommandSent = sendbuf;
				result.Success = true;
			}
			catch (Exception ex)
			{
				return new SendResult { Success = false, ErrorMessageSpecified = true, ErrorMessage = ex.Message };
			}

			return result;
		}



		private Command Receive(DeviceType? filterOnSourceDeviceType = null)
		{
			//if (_listener.Client == null) _listener = new UdpClient(_port);

			try
			{
				// Synchronous receive
				var ipEndPoint = EndPoint;
				var bytes = Listener.Receive(ref ipEndPoint);

				const int indexLengthOfDataPackage = 16;
				const int indexOriginalSubnetId = 17;
				const int indexOriginalDeviceId = 18;
				const int indexOriginalDeviceType = 19;
				const int indexOperateCode = 21;
				const int indexTargetSubnetId = 23;
				const int indexTargetDeviceId = 24;
				const int indexAdditionalContent = 25;
				int lengthOfDataPackage = bytes[indexLengthOfDataPackage];
				var additionalContentLength = lengthOfDataPackage - 1 - 1 - 1 - 2 - 2 - 1 - 1 - 1 - 1;

				//if (_filterOriginalDeviceId != null && (_filterOriginalDeviceId != hdlData.SourceDeviceId))
				//	return null;

				var cmd = new Command
				{
					RawData = bytes,
					SourceAddress = new DeviceAddress { DeviceId = bytes[indexOriginalDeviceId], SubnetId = bytes[indexOriginalSubnetId] },
					SourceDeviceTypeBytes = GetByteArray(bytes, indexOriginalDeviceType, 2),
					OperationCodeBytes = GetByteArray(bytes, indexOperateCode, 2),
					TargetAddress = new DeviceAddress { DeviceId = bytes[indexTargetDeviceId], SubnetId = bytes[indexTargetSubnetId] },
					AdditionalContent = GetByteArray(bytes, indexAdditionalContent, additionalContentLength),
					Success = true
				};

				if (filterOnSourceDeviceType != null && filterOnSourceDeviceType != cmd.SourceDeviceType)
					return null;

				// CRC verification code
				var crcH = bytes[bytes.Length - 2];
				var crcL = bytes[bytes.Length - 1];
				var calculatedChecksum = CalculateChecksum(bytes);

				if (crcH == calculatedChecksum.CrcHigh && crcL == calculatedChecksum.CrcLow) return cmd;

				cmd.Success = false;
				cmd.ErrorMessageSpecified = true;
				cmd.ErrorMessage = "Checksum error";
				return cmd;
			}
			catch (SocketException socketEx)
			{
				return new Command { Success = false, ErrorMessage = socketEx.Message, ErrorMessageSpecified = true };
			}
			catch (Exception ex)
			{
				return new Command { Success = false, ErrorMessage = ex.Message, ErrorMessageSpecified = true };
			}
		}









		private byte[] BuildBufToSend(Command commandToSend)
		{
			var content = commandToSend.AdditionalContent;
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

			var senderSubnetId = commandToSend.SourceAddress.SubnetId;
			var senderDeviceId = commandToSend.SourceAddress.DeviceId;
			var senderDeviceType = commandToSend.SourceDeviceTypeBytes ?? DeviceTypeToByteArray(commandToSend.SourceDeviceType);
			var operationCode = commandToSend.OperationCodeBytes ?? OperationCodeToByteArray(commandToSend.OperationCode);
			var targetSubnetId = commandToSend.TargetAddress.SubnetId;
			var targetDeviceId = commandToSend.TargetAddress.DeviceId;

			// Start command
			sendbuf[16] = (byte)lengthOfDataPackage;  // 15 Length of command package
			sendbuf[17] = (byte)senderSubnetId;       // 1 Original subnet id
			sendbuf[18] = (byte)senderDeviceId;       // 23 Original device id
			sendbuf[19] = senderDeviceType[0];        // 0 Original device type
			sendbuf[20] = senderDeviceType[1];        // 149 Original device type 
			sendbuf[21] = operationCode[0];             // 0 Operate code
			sendbuf[22] = operationCode[1];             // 49 Operate code 
			sendbuf[23] = (byte)targetSubnetId;       // 1 Target subnet id
			sendbuf[24] = (byte)targetDeviceId;       // 74 Target device id

			for (var i = 0; i < content.Length; i++)  // 1 content
				sendbuf[i + 25] = content[i];
			// End command

			var calculatedChecksum = CalculateChecksum(sendbuf);
			sendbuf[sendbuf.Length - 2] = calculatedChecksum.CrcHigh;
			sendbuf[sendbuf.Length - 1] = calculatedChecksum.CrcLow;

			return sendbuf;
		}


		private (byte CrcLow, byte CrcHigh) CalculateChecksum(byte[] sendbuf)
		{
			const int indexLengthOfDataPackage = 16;
			int lengthOfDataPackage = sendbuf[indexLengthOfDataPackage];
			var crcBufLength = lengthOfDataPackage - 2;

			var crcBuf = new byte[crcBufLength];
			for (var i = 0; i < crcBufLength; i++) crcBuf[i] = sendbuf[16 + i];
			var crc = new Crc16Ccitt(0);
			var checksum = crc.ComputeChecksumBytes(crcBuf);

			var crcHigh = checksum[1];
			var crcLow = checksum[0];

			return (CrcLow: crcLow, CrcHigh: crcHigh);
		}






		private byte[] GetTestBufToSend()
		{
			//Setter lys medierom til 40% på 3 sek

			var sendbuf = new byte[31];

			sendbuf[0] = 192;
			sendbuf[1] = 168;
			sendbuf[2] = 1;
			sendbuf[3] = 15;
			sendbuf[4] = 72;
			sendbuf[5] = 68;
			sendbuf[6] = 76;
			sendbuf[7] = 77;
			sendbuf[8] = 73;
			sendbuf[9] = 82;
			sendbuf[10] = 65;
			sendbuf[11] = 67;
			sendbuf[12] = 76;
			sendbuf[13] = 69;

			sendbuf[14] = 170;  // Leading code
			sendbuf[15] = 170;  // Leading code

			sendbuf[16] = 15;   // Length of command package
			sendbuf[17] = 1;    // Original subnet id
			sendbuf[18] = 23;   // Original device id
			sendbuf[19] = 0;    // Original device type
			sendbuf[20] = 149;  // Original device type 
			sendbuf[21] = 0;    // Operate code
			sendbuf[22] = 49;   // Operate code 
			sendbuf[23] = 1;    // Target subnet id
			sendbuf[24] = 74;   // Target device id

			sendbuf[25] = 1;    // content
			sendbuf[26] = 40;   // content
			sendbuf[27] = 0;    // content
			sendbuf[28] = 3;    // content

			sendbuf[29] = 191;  // CRC H
			sendbuf[30] = 29;   // CRC L

			return sendbuf;
		}







		private static OperationCode ParseOperationCode(string operateCodeHex)
		{
			if (string.IsNullOrEmpty(operateCodeHex)) return OperationCode.NotSet;

			operateCodeHex = operateCodeHex.Replace("-", "");
			var operateCodeId = int.Parse(operateCodeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var operateCode = (OperationCode)Enum.Parse(typeof(OperationCode), operateCodeId);

			if (!Enum.IsDefined(typeof(OperationCode), operateCode))
				operateCode = OperationCode.NotSet;

			return operateCode;
		}

		private static DeviceType ParseDeviceType(string deviceTypeHex)
		{
			if (string.IsNullOrEmpty(deviceTypeHex)) return DeviceType.UnknownDevice;

			deviceTypeHex = deviceTypeHex.Replace("-", "");
			var deviceTypeId = int.Parse(deviceTypeHex, NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture);
			var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), deviceTypeId);

			if (!Enum.IsDefined(typeof(DeviceType), deviceType))
				deviceType = DeviceType.UnknownDevice;

			return deviceType;
		}

		private byte[] GetByteArray(byte[] bytes, int start, int length)
		{
			var newArray = new byte[length];
			for (var i = 0; i < length; i++)
			{
				newArray[i] = bytes[start + i];
			}

			return newArray;
		}

		private string ByteArrayToString(byte[] bytes, int start, int length)
		{
			var newArray = new byte[length];
			for (var i = 0; i < length; i++)
			{
				newArray[i] = bytes[start + i];
			}

			return BitConverter.ToString(newArray);
		}

		private byte[] OperationCodeToByteArray(OperationCode operationCode)
		{
			var value = (int)operationCode;
			return IntToByteArray(value);
		}

		private byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length / 2)
				.Select(x => Byte.Parse(hex.Substring(2 * x, 2), NumberStyles.HexNumber))
				.ToArray();
		}

		private byte[] IntToByteArray(int value)
		{
			var bytes = new byte[2];
			bytes[0] = (byte)(value >> 8);
			bytes[1] = (byte)(value);

			return bytes;
		}

		private byte[] DeviceTypeToByteArray(DeviceType deviceType)
		{
			var value = (int)deviceType;
			var bytes = new byte[2];
			bytes[0] = (byte)(value >> 8);
			bytes[1] = (byte)(value);

			return bytes;
		}

		internal static OperationCode GetOperationCode(byte[] operationCode)
		{
			var operationCodeHex = ByteArrayToString(operationCode);
			return ParseOperationCode(operationCodeHex);
		}

		public static string ByteArrayToText(byte[] bytes)
		{
			var text = "";
			foreach (var t in bytes)
				text += t + " ";

			return text;
		}

		internal static string ByteArrayToHex(byte[] bytes)
		{
			var hex = ByteArrayToString(bytes);
			hex = "0x" + hex.Replace("-", "");
			return hex;
		}

		private static string ByteArrayToString(byte[] bytes)
		{
			return BitConverter.ToString(bytes);
		}

		internal static DeviceType GetDeviceType(byte[] deviceType)
		{
			var deviceTypeHex = ByteArrayToString(deviceType);
			return ParseDeviceType(deviceTypeHex);
		}

















		~BusproController()
		{
			Dispose();
		}

		#region IDisposable Members
		public void Dispose()
		{
			Listener?.Close();
		}
		#endregion





















































		//public bool Submit(AddDevice device)
		//{
		//	//return Submit(device, OperationCode.NotSet, null);
		//	return false;
		//}



		//public bool Submit(AddDevice device, OperationCode operationCode, byte[] additionalContent)
		//{
		//	var commandToSend = new BusproData();
		//	var deviceType = device.DeviceType;
		//	var deviceAddress = device.DeviceAddress;

		//	if (device is Relay relay)
		//	{
		//		//Console.WriteLine("Relay");

		//		//if (relay.OperationCode == OperationCode.SingleChannelControl)
		//		//{
		//		//	additionalContent = new byte[2];
		//		//	additionalContent[0] = (byte)relay.ChannelNo;
		//		//	additionalContent[1] = (byte)relay.State;
		//		//	additionalContent[2] = 0;
		//		//	additionalContent[3] = 0;

		//		//	operationCode = relay.OperationCode;
		//		//}
		//	}

		//	if (device is Dimmer dimmer)
		//	{
		//		//Console.WriteLine("Dimmer");

		//		//if (dimmer.OperationCode == OperationCode.SingleChannelControl)
		//		//{
		//		//	var secondsHigh = dimmer.SecondsRunningTime / 256;
		//		//	var secondsLow = dimmer.SecondsRunningTime % 256;

		//		//	additionalContent = new byte[4];
		//		//	additionalContent[0] = (byte)dimmer.ChannelNo;
		//		//	additionalContent[1] = (byte)dimmer.Intensity;
		//		//	additionalContent[2] = (byte)secondsHigh;
		//		//	additionalContent[3] = (byte)secondsLow;

		//		//	operationCode = dimmer.OperationCode;
		//		//}
		//	}

		//	commandToSend.OperationCode = OperationCodeToByteArray(operationCode);
		//	commandToSend.AdditionalContent = additionalContent;
		//	commandToSend.SourceDeviceType = deviceType;
		//	commandToSend.TargetAddress = deviceAddress;

		//	commandToSend = null;
		//	var result = Send(commandToSend);

		//	if (result.Success)
		//	{
		//		//AddDevice = null;
		//		return true;
		//	}

		//	//var err = result.ErrorMessage;
		//	return false;
		//}



	}


	internal class SendResult
	{
		public bool Success;
		public string ErrorMessage;
		public bool ErrorMessageSpecified;
		public byte[] CommandSent;
	}

}
