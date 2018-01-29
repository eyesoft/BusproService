using BusproService;
using BusproService.Enums;
using Newtonsoft.Json;

namespace ConsoleApp1
{
	public class HdlData
	{
		private readonly IHdlBus _hdlBus;
		public int SourceSubnetId { get; private set; }
		public int SourceDeviceId { get; private set; }
		public int TargetSubnetId { get; private set; }
		public int TargetDeviceId { get; private set; }
		//public string ContentHex { get; private set; }
		//public int ContentLength { get; private set; }
		//public string OperateCodeHex { get; private set; }
		//public string SourceDeviceTypeHex { get; private set; }
		public string RawData { get; private set; }
		public string Json { get; private set; }

		private string _operateCodeHex = "";
		private string _sourceDeviceTypeHex = "";
		private string _contentHex = "";
		private int _contentLength = 0;

		/// <summary>Initializes a new instance of the HdlData class.</summary>
		/// <param name="hdlBus">The HDL-bus connection to use.</param>
		public HdlData(IHdlBus hdlBus)
		{
			_hdlBus = hdlBus;
		}

		// Edge.js method signature
		//public async Task<object> Foo(dynamic input)
		//{
		//	//var foo = edge.func({
		//	//		assemblyFile: 'HdlBus.dll',
		//	//		typeName: 'HdlBus.Eyesoft.HdlData',
		//	//		methodName: 'Foo'
		//	//});

		//	return null;
		//}

		/// <summary>Receives data from the HDL-bus.</summary>
		/// <returns>True if data received successfully, otherwise false.</returns>
		public bool ReceiveData()
		{
			var result = _hdlBus.RecvMsg();
			if (result == null)
			{
				Json = JsonConvert.SerializeObject("");
				return false;
			}

			SourceDeviceId = ((HdlBus.HdlDataStruct)result).SourceDeviceId;
			SourceSubnetId = ((HdlBus.HdlDataStruct)result).SourceSubnetId;
			TargetDeviceId = ((HdlBus.HdlDataStruct)result).TargetDeviceId;
			TargetSubnetId = ((HdlBus.HdlDataStruct)result).TargetSubnetId;
			_contentHex = ((HdlBus.HdlDataStruct)result).ContentHex;
			_contentLength = ((HdlBus.HdlDataStruct)result).ContentLength;
			_operateCodeHex = ((HdlBus.HdlDataStruct)result).OperateCodeHex;
			RawData = ((HdlBus.HdlDataStruct)result).RawData;
			_sourceDeviceTypeHex = ((HdlBus.HdlDataStruct)result).SourceDeviceTypeHex;
			JsonConvert.SerializeObject(this);

			return true;
		}

		/// <summary>Returnes the DeviceType of the source.</summary>
		public DeviceType SourceDeviceType
		{
			get { return _hdlBus.GetDeviceType(_sourceDeviceTypeHex); }

		}

		/*
		/// <summary>Returnes the OperateCode.</summary>
		public OperationCode OperateCode
		{
			get { return _hdlBus.GetOperateCode(_operateCodeHex); }
		}
		*/


	}
}
