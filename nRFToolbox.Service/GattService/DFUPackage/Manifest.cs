using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Service.DFUPackage
{
	public class DFUServiceManifestRoot
	{
		public Manifest manifest { get; set; }
	}

	public class Manifest
	{
		public Application application { get; set; }
		public Bootloader bootloader { get; set; }
		public Softdevice softdevice { get; set; }
		public Softdevice_Bootloader softdevice_bootloader { get; set; }
	}

	public class Application : Firmaware
	{
	}

	public class Bootloader : Firmaware
	{
	}

	public class Softdevice :Firmaware
	{
	}

	public class Softdevice_Bootloader : Firmaware
	{
		public Int32 sd_size { get; set; }
		public Int32 bl_size { get; set; }
	}

	public class Firmaware 
	{
		public string bin_file { get; set; }
		public string dat_file { get; set; }
		public InitPacketData init_packet_data { get; set; }
	}

	public class InitPacketData
	{
		public UInt32 application_version { get; set; }
		public UInt16 compression_type { get; set; }
		public UInt16 device_revision { get; set; }
		public UInt16 device_type { get; set; }
		public int firmware_crc16 { get; set; }
		public string firmware_hash { get; set; }
		public UInt16 packet_version { get; set; }
		public List<UInt16> softdevice_req { get; set; }
	}
}
