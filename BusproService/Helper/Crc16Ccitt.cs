using System;

namespace BusproService.Helper
{
	public class Crc16Ccitt
	{
		private const ushort Poly = 4129;
		private readonly ushort[] _table = new ushort[256];
		private readonly ushort _initialValue;

		private ushort ComputeChecksum(byte[] bytes)
		{
			var crc = _initialValue;
			for (var i = 0; i < bytes.Length; ++i)
			{
				crc = (ushort)((crc << 8) ^ _table[((crc >> 8) ^ (0xff & bytes[i]))]);
			}
			return crc;
		}

		public byte[] ComputeChecksumBytes(byte[] bytes)
		{
			var crc = ComputeChecksum(bytes);
			return BitConverter.GetBytes(crc);
		}

		public Crc16Ccitt(ushort initialValue)
		{
			_initialValue = initialValue;
			for (var i = 0; i < _table.Length; ++i)
			{
				ushort temp = 0;
				var a = (ushort)(i << 8);
				for (var j = 0; j < 8; ++j)
				{
					if (((temp ^ a) & 0x8000) != 0)
					{
						temp = (ushort)((temp << 1) ^ Poly);
					}
					else
					{
						temp <<= 1;
					}
					a <<= 1;
				}
				_table[i] = temp;
			}
		}
	}
}
