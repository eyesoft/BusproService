using BusproService;
using BusproService.Enums;

namespace ConsoleApp1
{
    public interface IHdlBus
    {
	    void Dispose();
	    HdlBus.HdlDataStruct? RecvMsg();
	    DeviceType GetDeviceType(string deviceTypeHex);
		/*
	    OperationCode GetOperateCode(string operateCodeHex);
			*/
			
	    bool SendMsg(dynamic action);

    }
}
