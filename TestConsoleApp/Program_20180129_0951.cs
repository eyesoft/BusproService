// https://github.com/genielabs/zwave-lib-dotnet
// https://github.com/caligo-mentis/smart-bus

using System;
using System.Text;
using System.Threading;
using BusproService;
using BusproService.Data;
using BusproService.Enums;

namespace SmartHdlConsoleApp
{
	internal class Program
	{
		private const string Ip = "192.168.1.15";
		private const int Port = 6000;
		private const int SourceSubnetId = 1;
		private const int SourceDeviceId = 255;
		private const DeviceType SourceDeviceType = DeviceType.BusproService;

		private static void Main(string[] args)
		{

			using (var busproController = new BusproController(Ip, Port))
			{
				busproController.ContentReceived += busproController_ContentReceived;
				busproController.DeviceDataContentReceived += busproController_ContentReceivedForDevice;

				busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
				busproController.SourceDeviceType = SourceDeviceType;

				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					while (true)
					{
						// Reads all data on bus
						busproController.ReadBus();
					}
				}).Start();

				Console.WriteLine("Press enter to close...\n");

				ListenToDevice(busproController);

				//Thread.Sleep(10000);
				//TurnOffLightMediaroom(busproController);
				//QueryDlp(busproController);

				Console.ReadLine();
			}




			//var cmd = "read";
			//if (args.Length != 0) cmd = args[0];

			//cmd = "write";

			//switch (cmd)
			//{
			//	case "read":
			//		using (var busproController = new BusproController.BusproController())
			//		{
			//			busproController.ContentReceived += busproController_ContentReceived;

			//			busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
			//			busproController.SourceDeviceType = SourceDeviceType;

			//			StartThread(busproController);

			//			Console.WriteLine("Press enter to close...\n");

			//			Thread.Sleep(5000);
			//			TurnOffLightMediaroom(busproController);

			//			Console.ReadLine();
			//		}
			//		return;
			//	case "write":
			//		using (var busproController = new BusproController.BusproController(Ip, Port))
			//		{
			//			busproController.SourceAddress = new DeviceAddress { DeviceId = SourceDeviceId, SubnetId = SourceSubnetId };
			//			busproController.SourceDeviceType = SourceDeviceType;

			//			TurnOffLightMediaroom(busproController);

			//			Console.WriteLine("Press enter to close...\n");
			//			Console.ReadLine();
			//		}
			//		return;
			//}
		}

		private static void busproController_ContentReceivedForDevice(object sender, ContentEventArgs args)
		{
			var result = args;

			if (result == null || !result.Success) return;

			Console.WriteLine("Received data for " + result.SourceAddress.SubnetId + "."  + result.SourceAddress.DeviceId);
		}


		private static void busproController_ContentReceived(object sender, ContentEventArgs args)
		{
			var result = args;

			if (result == null || !result.Success) return;

			var sb = new StringBuilder("");

			var errorMessage = result.ErrorMessage;
			var success = result.Success;

			var b = result.AdditionalContent;
			var t = BusproService.BusproController.ByteArrayToText(b);

			var sd = result.SourceAddress.DeviceId;
			var ss = result.SourceAddress.SubnetId;

			var td = result.TargetAddress.DeviceId;
			var ts = result.TargetAddress.SubnetId;

			var o = result.OperationCode;
			var oHex = result.OperationCodeHex;

			var dt = result.SourceDeviceType;
			var dtHex = result.SourceDeviceTypeHex;

			sb.Append($"Success: \t\t{success}");

			if (success)
			{
				sb.Append("\n");
				sb.Append($"Operation code: \t{o} ({oHex})\n");
				sb.Append($"Source device type: \t{dt} ({dtHex})\n");
				sb.Append($"Address:\t\t{ss}.{sd} => {ts}.{td}\n");
				sb.Append($"Additional content: \t{t}\n");

				if (o == OperationCode.CurrentDateTime)
				{
					sb.Append($"Current time: \t\t{b[2]}/{b[1]}/{b[0]} {b[3]}:{b[4]}:{b[5]}\n");
				}

				if (o == OperationCode.ReadFloorHeatingStatusResponse)
				{
					sb.Append($"Heat active: \t\t{b[2]}\n");
					sb.Append($"Set temperature: \t{b[4]}\n");
					sb.Append($"Current temperature: \t{b[1]}\n");
				}


			}
			else
			{
				sb.Append($" ({errorMessage})");
			}

			Console.WriteLine(sb.ToString());
			Console.WriteLine("");

		}


		public static void ListenToDevice(IBusproController busproController)
		{
			var device = new Logic(1, 100);
			// tag for eventhandling
			// device.tag
			var logic = (Logic)busproController.Device(device);
		}
		private static void logic_ContentReceived(object sender, ContentEventArgs args)
		{
			var result = args;
		}


		public static void TurnOffLightMediaroom(IBusproController busproController)
		{
			try
			{
				var dimmerKino = (Dimmer)busproController.Device(new Dimmer(1, 74));
				var ok = dimmerKino.SingleChannelControl(1, 0, 0);
				Console.WriteLine($"Medierom..SingleChannelControl(1, 0, 0): {ok}\n\n");
			}
			catch (Exception ex)
			{
				var err = ex.Message;
				Console.WriteLine(err);
			}
		}


		public static void QueryDlp(IBusproController busproController)
		{
			try
			{
				var dlpStue = (Dlp)busproController.Device(new Dlp(1, 21));
				var ok = dlpStue.ReadFloorHeatingStatus();
				Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				var dlpTrim = (Dlp)busproController.Device(new Dlp(1, 13));
				ok = dlpTrim.ReadFloorHeatingStatus();
				Console.WriteLine($"ReadFloorHeatingStatus(): {ok}\n\n");

				/*
				ReadFloorHeatingStatus    
				0		temperature.type (0 = Celsius, 1 = Fahrenheit)
				24	temperature.current 
				1		status (av/på)
				1		modus (1 = Normal)
				24	temperature.normal
				20	temperature.day
				20	temperature.night
				20	temperature.away
									??temperature.timer
				*/



				//var dlpStue = (Dlp)busproController.Device(new Dlp(1, 21));
				//var ok = dlpStue.ReadAcCurrentState();
				//Console.WriteLine($"Stue..ReadAcCurrentState(): {ok}\n\n");

				//var dlpBad = (Dlp)busproController.Device(new Dlp(1, 17));
				//ok = dlpBad.ReadAcCurrentState();
				//Console.WriteLine($"Bad..ReadAcCurrentState(): {ok}\n\n");

			}
			catch (Exception ex)
			{
				var err = ex.Message;
				Console.WriteLine(err);
			}
		}















		//public static void WriteBuspro(IBusproService busproController)
		//{
		//	try
		//	{
		//		bool commandOk;

		//		var dimmerKino = (Dimmer)busproController.Device(new Dimmer(1, 74));
		//		commandOk = dimmerKino.SingleChannelControl(1, 0, 0);
		//		Console.WriteLine(commandOk);

		//		var logic = (Logic)busproController.Device(new Logic(1, 100));
		//		commandOk = logic.UniversalSwitch(150, ChannelState.On);
		//		Console.WriteLine(commandOk);

		//		var d = (Dimmer)busproController.Device(new Dimmer(1, 74));
		//		commandOk = d.SingleChannelControl(1, 0, 0);
		//		//commandOk = d.UniversalSwitch(123, ChannelState.Off);
		//		Console.WriteLine(commandOk);

		//		var l = (Logic)busproController.Device(new Logic(1, 100));
		//		commandOk = l.UniversalSwitch(150, ChannelState.On);
		//		Console.WriteLine(commandOk);


		//		//dimmerKino.Channel = 1;
		//		//var commandOk = dimmerKino.SingleChannelControl(75, 3);
		//		//commandOk = dimmerKino.SingleChannelControl(15, 10);
		//	}
		//	catch (Exception ex)
		//	{
		//		var err = ex.Message;
		//		Console.WriteLine(err);
		//	}


		//	//try
		//	//{
		//	//	var dimmer = busproController.Device(new Dimmer(1, 100));
		//	//	commandOk = dimmer.SingleChannelControl(2, 75, 3);
		//	//}
		//	//catch (Exception ex)
		//	//{
		//	//	var err = ex.Message;
		//	//}


		//	//var commandOk = false;


		//	//var relayDevice = new Relay(1, 130);
		//	//var relay = busproController.Device(new Relay(1, 130));

		//	//commandOk = relay.SingleChannelControl(3, ChannelState.On);

		//	//relay.Channel = 3;
		//	//commandOk = relay.SingleChannelControl(ChannelState.Off);
		//}







		//private static void Run(IBusproService busproController)
		//{
		//	while (true)
		//	{
		//		ReadBuspro(busproController);
		//	}
		//}






		//private static void ReadBuspro(IBusproService busproController)
		//{

		//	try
		//	{

		//		var result = busproController.ReadBus();
		//		//var result = busproController.ReadBus(new DeviceAddress { DeviceId = 40, SubnetId = 1 });
		//		//var result = busproController.ReadBus(DeviceType.PIR_12in1);
		//		//var result = busproController.ReadBus(OperationCode.CurrentDateTime);
		//		//var result = busproController.ReadBus(DeviceType.LOGIC_Logic960, OperationCode.CurrentDateTime);
		//		//var result = busproController.ReadBus(new Dimmer(DeviceType.LOGIC_Logic960, new DeviceAddress { SubnetId = 1, DeviceId = 100 }, 5));

		//		if (result == null || !result.Success) return;

		//		var sb = new StringBuilder("");

		//		var errorMessage = result.ErrorMessage;
		//		var success = result.Success;

		//		var b = result.AdditionalContent;
		//		var t = busproController.ByteArrayToText(b);

		//		var sd = result.SourceAddress.DeviceId;
		//		var ss = result.SourceAddress.SubnetId;

		//		var td = result.TargetAddress.DeviceId;
		//		var ts = result.TargetAddress.SubnetId;

		//		var o = result.OperationCode;
		//		var oHex = result.OperationCodeHex;

		//		var dt = result.SourceDeviceType;
		//		var dtHex = result.SourceDeviceTypeHex;

		//		sb.Append($"Success: \t\t{success}");

		//		if (success)
		//		{
		//			sb.Append("\n");
		//			sb.Append($"Operation code: \t{o} ({oHex})\n");
		//			sb.Append($"Source device type: \t{dt} ({dtHex})\n");
		//			sb.Append($"Address:\t\t{ss}.{sd} => {ts}.{td}\n");
		//			sb.Append($"Additional content: \t{t}\n");

		//			if (o == OperationCode.CurrentDateTime)
		//			{
		//				sb.Append($"Current time: \t\t{b[2]}/{b[1]}/{b[0]} {b[3]}:{b[4]}:{b[5]}\n");
		//			}
		//		}
		//		else
		//		{
		//			sb.Append($" ({errorMessage})");
		//		}

		//		Console.WriteLine(sb.ToString());
		//		Console.WriteLine("");
		//	}
		//	catch (Exception ex)
		//	{
		//		var err = ex.Message;
		//		Console.WriteLine(err);
		//	}
		//}










		//private static void SendToBus()
		//{
		//	//IBusproServiceLogic busproController = new BusproController.BusproServiceLogic("192.168.1.15");
		//	//var channelNo = 5;
		//	//var spotBad = new Dimmer(DeviceType.DIMMER_DT0601, new DeviceAddress { SubnetId = 1, DeviceId = 100 }, channelNo);

		//	//spotBad.SingleChannelControl(50, 5);
		//	//busproController.Submit(spotBad);

		//	//spotBad.SingleChannelControl(20, 5);
		//	//busproController.Submit(spotBad);

		//	//spotBad.SingleChannelControl(70, 5);
		//	//busproController.Submit(spotBad);


		//	//var additionalContent = new byte[] { (byte)channelNo, 60, 0, 5 };
		//	//busproController.Submit(spotBad, OperationCode.SingleChannelControl, additionalContent);

		//	//var relay = new Relay(DeviceType.RELAY_R0816, new DeviceAddress { SubnetId = 1, DeviceId = 100 }, 4);

		//	//relay.SingleChannelControl(ChannelState.Off);
		//	//busproController.Submit(relay);


		//	Console.ReadKey();
		//}







		//private static void SendBuspro(IBusproService busproController)
		//{
		//	var operationCode = OperationCode.NotSet;
		//	byte[] additionalContent;
		//	var sourceDeviceType = DeviceType.UnknownDevice;

		//	var data = new BusproData
		//	{
		//		TargetAddress = new DeviceAddress { SubnetId = 0, DeviceId = 0 },
		//		SourceAddress = new DeviceAddress { SubnetId = 0, DeviceId = 0 },
		//		//OperationCode = operationCode,
		//		//AdditionalContent = additionalContent,
		//		SourceDeviceType = sourceDeviceType
		//	};



		//}









		//private static void ReadDirectly()
		//{

		//	using (var udpClient = new UdpClient(6000))
		//	{
		//		while (true)
		//		{
		//			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		//			var receivedResult = udpClient.Receive(ref remoteEndPoint);
		//			Console.WriteLine(Encoding.ASCII.GetString(receivedResult));
		//		}
		//	}

		//}







		//private static void ReadBus()
		//{
		//	//_hdl.LogFilename = null;

		//	var hdlData = new HdlData(_hdl);
		//	var ok = hdlData.ReceiveData();

		//	Console.WriteLine("Data receivd OK: \t" + ok);
		//	Console.WriteLine("Source: \t\t" + GetDeviceName(hdlData.SourceSubnetId, hdlData.SourceDeviceId));
		//	Console.WriteLine("Target: \t\t" + GetDeviceName(hdlData.TargetSubnetId, hdlData.TargetDeviceId));
		//	Console.WriteLine("DeviceType: \t\t" + hdlData.SourceDeviceType);
		//	Console.WriteLine("OperateCode: \t\t" + hdlData.OperateCode);
		//	Console.WriteLine("JSON: \t\t" + hdlData.Json);
		//	Console.WriteLine("");
		//}

		//private static void LightMediaroomOn()
		//{
		//	var lysMedierom = new SingleChannelLightingControl(_hdl, 1, 74, 1, 40, 0, 3);
		//	lysMedierom.Send();

		//	Console.WriteLine(lysMedierom.Intensity);
		//}


		//private enum Devices
		//{
		//	Input1 = 5,
		//	DlpHall = 10
		//};








		//private static string ParseDeviceName(int subnetId, int deviceId)
		//{
		//	var deviceName = $"{{Unknown device}} ({subnetId}.{deviceId})";

		//	if (subnetId == 1)
		//	{
		//		switch (deviceId)
		//		{
		//			case 5:
		//				deviceName = "Input1";
		//				break;
		//			case 10:
		//				deviceName = "DLP Hall";
		//				break;
		//			case 11:
		//				deviceName = "DLP Gang";
		//				break;
		//			case 12:
		//				deviceName = "DLP Vaskerom";
		//				break;
		//			case 13:
		//				deviceName = "DLP Trimrom";
		//				break;
		//			case 14:
		//				deviceName = "DLP Kontor";
		//				break;
		//			case 15:
		//				deviceName = "DLP Bad";
		//				break;
		//			case 16:
		//				deviceName = "DLP Garasjebod";
		//				break;
		//			case 17:
		//				deviceName = "DLP Sov2";
		//				break;
		//			case 18:
		//				deviceName = "DLP Sov1";
		//				break;
		//			case 19:
		//				deviceName = "Sov1 høyre";
		//				break;
		//			case 21:
		//				deviceName = "DLP Stue";
		//				break;
		//			case 22:
		//				deviceName = "DLP Kjøkken";
		//				break;
		//			case 23:
		//				deviceName = "DLP Medierom";
		//				break;
		//			case 40:
		//				deviceName = "12i1 Hall";
		//				break;
		//			case 41:
		//				deviceName = "12i1 Gang";
		//				break;
		//			case 42:
		//				deviceName = "8i1 Vaskerom";
		//				break;
		//			case 43:
		//				deviceName = "8i1 Garasjebod";
		//				break;
		//			case 44:
		//				deviceName = "8i1 Garasje";
		//				break;
		//			case 45:
		//				deviceName = "8i1 Bad";
		//				break;
		//			case 46:
		//				deviceName = "8i1 Bod Jonathan";
		//				break;
		//			case 49:
		//				deviceName = "12i1 Stue";
		//				break;
		//			case 50:
		//				deviceName = "12i1 Kjøkken";
		//				break;
		//			case 55:
		//				deviceName = "8i1 Garderobe";
		//				break;
		//			case 71:
		//				deviceName = "Dim1 Bad,kontor,sov";
		//				break;
		//			case 72:
		//				deviceName = "Dim2 Kjøkken";
		//				break;
		//			case 73:
		//				deviceName = "Dim3 Stue,g.bod";
		//				break;
		//			case 74:
		//				deviceName = "Dim4 Medie,gang,vask";
		//				break;
		//			case 80:
		//				deviceName = "Rele1";
		//				break;
		//			case 100:
		//				deviceName = "Logikkmodul";
		//				break;
		//			case 110:
		//				deviceName = "Sikkerhetsmodul";
		//				break;
		//			case 120:
		//				deviceName = "RS232";
		//				break;
		//			case 130:
		//				deviceName = "Rele2 Varme 1-6";
		//				break;
		//			case 131:
		//				deviceName = "Rele2 Varme 7-10";
		//				break;
		//			case 165:
		//				deviceName = "Sov1 venstre";
		//				break;
		//			case 255:
		//				deviceName = "BROADCAST SUBNET " + subnetId;
		//				break;
		//		}
		//	}

		//	if (subnetId == 3)
		//	{
		//		switch (deviceId)
		//		{
		//			case 254:
		//				deviceName = "Setup tool";
		//				break;
		//			case 255:
		//				deviceName = "BROADCAST SUBNET " + subnetId;
		//				break;
		//		}
		//	}

		//	if (subnetId == 253)
		//	{
		//		switch (deviceId)
		//		{
		//			case 254:
		//				deviceName = "HDL Buspro Setup Tool 2";
		//				break;
		//		}
		//	}

		//	if (subnetId == 255)
		//	{
		//		switch (deviceId)
		//		{
		//			case 255:
		//				deviceName = "BROADCAST ALL SUBNETS";
		//				break;
		//		}
		//	}

		//	return deviceName;
		//}







		//private byte[] StringToByteArray(string hex)
		//{
		//	return Enumerable.Range(0, hex.Length / 2)
		//									 .Select(x => Byte.Parse(hex.Substring(2 * x, 2), NumberStyles.HexNumber))
		//									 .ToArray();
		//}


		/*
	private static void NotImplementedExample()
	{
		try
		{
			var gprs = new GprsControl();
			gprs.Send();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
	*/

		//private void Example()
		//{
		//	// Instantiate connection to the HDL-bus
		//	// Optionally log to file
		//	var hdlBus = new HdlBus("", 0);
		//	hdlBus.LogFilename = "hdl.log";


		//	/*
		//	// Send SingleChannelLightingControl command
		//	var sclc = new SingleChannelLightingControl(hdlBus, 0, 0, 0, 0, 0, 0);
		//	sclc.Send();
		//	var channelNo = sclc.ChannelNo;
		//	var intensity = sclc.Intensity;
		//	var runningTimeMinutes = sclc.RunningTimeMinutes;
		//	var runningTimeSeconds = sclc.RunningTimeSeconds;
		//	var targetDeviceId = sclc.TargetDeviceId;
		//	var targetSubnetId = sclc.TargetSubnetId;

		//	// Receive data from the HDL-bus
		//	var hdlData = new HdlData(hdlBus);
		//	hdlData.ReceiveData();
		//	var operateCode = hdlData.OperateCode;
		//	var rawData = hdlData.RawData;
		//	var sourceDeviceId = hdlData.SourceDeviceId;
		//	var sourceDeviceType = hdlData.SourceDeviceType;
		//	var sourceSubnetId = hdlData.SourceSubnetId;
		//	targetDeviceId = hdlData.TargetDeviceId;
		//	targetSubnetId = hdlData.TargetSubnetId;
		//	*/


		//	// Dispose the connection to the HDL-bus
		//	hdlBus.Dispose();
		//}





		//private void UpdateStatus(string originalSubnetId, string originalDeviceId, string originalDeviceName, string originalDeviceTypeHex, string originalDeviceType,
		//											string targetSubnetId, string targetDeviceId, string targetDeviceName, string operateCodeHex, string operateCode,
		//											string contentHex, byte[] Content, string rawData)
		//{
		//	textBoxOriginalSubnetID.Text = originalSubnetId;
		//	textBoxOriginalDeviceID.Text = originalDeviceId;
		//	textBoxOriginalDevice.Text = originalDeviceName;
		//	textBoxOriginalDeviceType.Text = originalDeviceType;
		//	textBoxOriginalDeviceTypeHex.Text = "0x" + originalDeviceTypeHex;

		//	textBoxTargetSubnetID.Text = targetSubnetId;
		//	textBoxTargetDeviceID.Text = targetDeviceId;
		//	textBoxTargetDevice.Text = targetDeviceName;

		//	textBoxOperateCodeHex.Text = "0x" + operateCodeHex;
		//	textBoxOperateCode.Text = operateCode;

		//	textBoxContentHex.Text = "0x" + contentHex;

		//	byte[] content = Content;
		//	var cont = "";
		//	for (int i = 0; i < content.Length; i++)
		//		cont += content[i] + " ";
		//	textBoxContent.Text = cont;

		//	textBoxData.Text = rawData;

		//	if (operateCodeHex == "0032" & originalDeviceId == "74")
		//	{
		//		if (Content[0].ToString(CultureInfo.InvariantCulture) == "1")
		//		{
		//			//		trackBar1.Value = int.Parse(Content[2].ToString());
		//			labelIntensity.Text = int.Parse(Content[2].ToString(CultureInfo.InvariantCulture)) + "%";

		//		}
		//	}
		//}

		//private void buttonStart_Click(object sender, EventArgs e)
		//{
		//	textBoxOriginalSubnetID.Text = "";
		//	textBoxOriginalDeviceID.Text = "";
		//	textBoxOriginalDevice.Text = "";
		//	textBoxOriginalDeviceType.Text = "";
		//	textBoxTargetSubnetID.Text = "";
		//	textBoxTargetDeviceID.Text = "";
		//	textBoxTargetDevice.Text = "";
		//	textBoxOperateCodeHex.Text = "";
		//	textBoxOperateCode.Text = "";
		//	textBoxContentHex.Text = "";
		//	textBoxContent.Text = "";
		//	textBoxData.Text = "";

		//	stopProcess = false;
		//	labelRunning.Text = "RUNNING";
		//	workerThread = new Thread(DoThings);
		//	workerThread.Start();
		//}

		//private void buttonSend_Click(object sender, EventArgs e)
		//{
		//	const int targetSubnetID = 1;
		//	const int targetDeviceID = 74; //Dim

		//	var operateCode = new byte[] { 0, 49 };			// Single channel lighting control
		//	var content = new byte[] { 1, 40, 0, 3 };		// channel 1, 40%, 0:3

		//	const int senderSubnetID = 1;
		//	const int senderDeviceID = 23; //DLP medie
		//	var senderDeviceType = new byte[] { 0, 149 }; //DLP

		//	_hdl.SendMsg(targetSubnetID, targetDeviceID, operateCode, content, senderSubnetID, senderDeviceID, senderDeviceType);
		//	_hdl.Close();
		//}

		//private void buttonStop_Click(object sender, EventArgs e)
		//{
		//	stopProcess = true;
		//	labelRunning.Text = "STOPPED";
		//	_hdl.Close();
		//}

		//private void buttonLysMedieromAv_Click(object sender, EventArgs e)
		//{
		//	var lysMedierom = new SingleChannelLightingControl
		//	{
		//		TargetSubnetId = 1,
		//		TargetDeviceId = 74,
		//		ChannelNo = 1,
		//		Intensity = 0,
		//		RunningTimeMinutes = 0,
		//		RunningTimeSeconds = 3
		//	};

		//	_hdl.SendMsg(lysMedierom);
		//	trackBar1.Value = lysMedierom.Intensity;
		//}

		//private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		//{
		//	var lysMedierom = new SingleChannelLightingControl
		//	{
		//		TargetSubnetId = 1,
		//		TargetDeviceId = 74,
		//		ChannelNo = 1,
		//		Intensity = (int)numericUpDown1.Value,
		//		RunningTimeMinutes = 0,
		//		RunningTimeSeconds = 3
		//	};

		//	_hdl.SendMsg(lysMedierom);
		//	trackBar1.Value = lysMedierom.Intensity;
		//}

		//private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	var lysMedierom = new SingleChannelLightingControl
		//	{
		//		TargetSubnetId = 1,
		//		TargetDeviceId = 74,
		//		ChannelNo = 1,
		//		Intensity = int.Parse(comboBox1.SelectedItem.ToString()),
		//		RunningTimeMinutes = 0,
		//		RunningTimeSeconds = 3
		//	};

		//	_hdl.SendMsg(lysMedierom);
		//	trackBar1.Value = lysMedierom.Intensity;
		//}

		//private void trackBar1_Scroll(object sender, EventArgs e)
		//{
		//	var lysMedierom = new SingleChannelLightingControl
		//	{
		//		TargetSubnetId = 1,
		//		TargetDeviceId = 74,
		//		ChannelNo = 1,
		//		Intensity = int.Parse(trackBar1.Value.ToString(CultureInfo.InvariantCulture)),
		//		RunningTimeMinutes = 0,
		//		RunningTimeSeconds = 3
		//	};

		//	_hdl.SendMsg(lysMedierom);
		//	trackBar1.Value = lysMedierom.Intensity;
		//}

		//private void buttonBorte_Click(object sender, EventArgs e)
		//{
		//	var uvSwitchBorte = new UniversalSwitch
		//	{
		//		TargetSubnetId = 1,
		//		TargetDeviceId = 100,
		//		SwitchNo = 102,
		//		OnOff = HdlBus.HdlBus.OnOff.On
		//	};

		//	_hdl.SendMsg(uvSwitchBorte);
		//}







		//private static void TestCrc()
		//{
		//	var crcBuf = new byte[] { 15, 1, 23, 0, 149, 0, 49, 1, 74, 1, 40, 0, 3 };

		//	var crc = new Crc16Ccitt(0);
		//	var res = crc.ComputeChecksumBytes(crcBuf);

		//	var a = res[0];
		//	var b = res[1];

		//	//sendbuf[29] = 191;  // CRC H
		//	//sendbuf[30] = 29;   // CRC L

		//	return;
		//	//return sendbuf;
		//}



	}
}
