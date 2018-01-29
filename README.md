## BusproService

C# .NET Core 2 implementation of HDL SmartBus protocol http://smarthomebus.com/

### Initialization

Create instance of Buspro controller and start listening on the bus

```c#
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
      busproController.ReadBus();
    }
  }).Start();

  Console.WriteLine("Press enter to close...\n");
  Console.ReadLine();
}
```
