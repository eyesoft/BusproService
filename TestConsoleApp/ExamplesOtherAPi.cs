
using System;
using System.Threading;

namespace BusproServiceExamplesOtherApi
{
	internal enum StatusEnum
	{
		Connected,
		Disconnected,
		Initializing,
		Ready,
		Error
	}

	internal class BusproData
	{
		public StatusEnum Status;
	}

	internal class BusproDataEventArgs : EventArgs
	{
		public BusproData DataStatus;

		public BusproDataEventArgs(BusproData controllerDataStatus)
		{
			DataStatus = controllerDataStatus;
		}
		public BusproDataEventArgs() { }
	}


	internal class BusproController
	{
		// Controller status changed event handler
		public delegate void BusproDataReceivedEventHandler(object sender, BusproDataEventArgs args);

		// Occurs when controller status changed
		public event BusproDataReceivedEventHandler ControllerStatusChanged;

		public void PrivateMetodThatInvokeEvent()
		{
			var dataStatus = new BusproData();

			dataStatus.Status = StatusEnum.Ready;
			var args = new BusproDataEventArgs { DataStatus = dataStatus };
			OnControllerStatusChanged(args);

			System.Threading.Thread.Sleep(1000);

			dataStatus.Status = StatusEnum.Initializing;
			OnControllerStatusChanged(new BusproDataEventArgs(dataStatus));

			System.Threading.Thread.Sleep(1000);

			dataStatus.Status = StatusEnum.Connected;
			OnControllerStatusChanged(new BusproDataEventArgs(dataStatus));

		}

		// Raises the controller status changed event
		protected virtual void OnControllerStatusChanged(BusproDataEventArgs args)
		{
			ControllerStatusChanged?.Invoke(this, args);
		}
	}












	public class ExamplesOtherAPi
	{

		public void DoWork()
		{
			var controller = new BusproController();
			controller.ControllerStatusChanged += Controller_ControllerStatusChanged;

			Console.WriteLine("\nType ! to exit\n");

			StartThread(controller);

			var command = "";
			while (command != "!")
			{
				command = Console.ReadLine();
				switch (command)
				{
					case "0":
						break;
				}
			}
			Console.WriteLine("\nCiao!\n");
		}

		private void StartThread(BusproController controller)
		{
			new Thread(() =>
			{
				Thread.CurrentThread.IsBackground = true;
				while (true)
				{
					controller.PrivateMetodThatInvokeEvent();
				}
			}).Start();
		}

		private void Controller_ControllerStatusChanged(object sender, BusproDataEventArgs args)
		{
			Console.WriteLine("ControllerStatusChange {0}", args.DataStatus.Status);
		}
	}
}



//https://docs.microsoft.com/en-us/dotnet/standard/events/how-to-raise-and-consume-events
