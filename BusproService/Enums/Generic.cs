namespace BusproService.Enums
{

	public enum SuccessOrFailure
	{
		Success = 0xF8,
		Failure = 0xF5
	}



	public class Channel
	{
		public enum State
		{
			Off = 0,
			On = 100
		}

		public enum Status
		{
			Off = 0,
			On = 1
		}
	}


	public class Temperature
	{
		public enum Type
		{
			Celsius = 0,
			Fahrenheit = 1
		}

		public enum Status
		{
			Off = 0,
			On = 1
		}

		public enum Mode
		{
			Normal = 1,
			Day = 2,
			Night = 3,
			Away = 4,
			Timer = 5
		}

		public enum Timer
		{
			Day = 0,
			Night = 1
		}
	}

}
