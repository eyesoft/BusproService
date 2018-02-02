using System.Diagnostics.CodeAnalysis;

namespace BusproService.Enums
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum DeviceType
	{
		UnknownDevice = 0x0,

		RELAY_6B0_10v = 0x0011,			// Rele varme
		SECURITY_SEC250K = 0x0BE9,	// Sikkerhetsmodul
		PIR_12in1 = 0x0134,					// 12i1
		LOGIC_Logic960 = 0x0453,		// Logikkmodul
		DLP_DLP2 = 0x0086,					// DLP
		DLP_DLP = 0x0095,						// DLP
		DLP_DLP_v2 = 0x009C,				// DLPv2
		KEY_WS8M = 0x012B,					// 8 keys panel
		PIR_8in1 = 0x0135,					// 8i1
		DIMMER_DT0601 = 0x0260,			// 6ch Dimmer
		RELAY_R0816 = 0x01AC,				// Rele
		DRY_4Z = 0x0077,						// Input

		BusproService = 0xFFFC,
		SmartHDLTest = 0xFFFD,
		SetupTool = 0xFFFE,

		//SB_DN_DT0601 = 0x009E,	// Universaldimmer 6ch 1A
		//SB_DN_RS232N						// RS232
		//DIMMER_MDT0601 = 0x0260,
		//RELAY_MR0816 = 0x01AC
		//DIMMER_MD0602 = 0x0,
		//RELAY_MFH06 = 0x0
	}
}