using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRFToolbox.Common
{
	public class GATTServiceIdentification
	{
		public const string ALERT_NOTIFICATION = "AlertNotification";
		public const string BATTERY = "Battery";
		public const string BLOOD_PRESSURE = "BloodPressure";
		public const string CURRENT_TIME = "CurrentTime";
		public const string CYCLING_POWER = "CyclingPower";
		public const string CYCLING_SPEED_AND_CADENCE = "CyclingSpeedAndCadence";
		public const string DEVICE_INFORMATION = "DeviceInformation";
		public const string GENERIC_ACCESS = "GenericAccess";
		public const string GENERIC_ATTRIBUTES = "GenericAttribute";
		public const string GLUCOSE = "Glucose";
		public const string HEALTH_THERMOMETER = "HealthThermometer";
		public const string HEART_RATE = "HeartRate";
		public const string HUMAN_INTERFACE_DEVICE = "HumanInterfaceDevice";
		public const string IMMEDIATE_ALERT = "ImmediateAlert";
		public const string LINK_LOSS = "LinkLoss";
		public const string NEXT_DST_CHANGE = "NextDstChange";
		public const string PHONE_ALERT_STATUS = "PhoneAlertStatus";
		public const string REFERENCE_TIME_UPDATE = "ReferenceTimeUpdate";
		public const string RUNNING_SPEED_AND_CADENCE = "RunningSpeedAndCadence";
		public const string SCAN_PARAMETERS = "ScanParameters";
		public const string TX_POWER = "TxPower";
		public const string DEVICE_FIRMWARE_UPDATE = "DeviceFirmwareUpdate";
		public const string PACKET = "Packet";
		//public const string DEVICE_FIRMWARE_UPDATE = "DeviceFirmwareUpdate";

	}

	public class GATTProfileIdentification
	{
		public const string PROXIMITY = "Proximity";
		public const string DEVICE_FIRMWARE_UPDATE = "DeviceFirmwareUpdate";
	}

	public class ToolboxItemIdentification 
	{
		public const string HEART_RATE = "HRM";
		public const string GLUCOSE = "BGM";
		public const string BLOOD_PRESSURE = "BPM";
		public const string CYCLING_SPEED_AND_CADENCE = "CSC";
		public const string DEVICE_FIRMWARE_UPDATE = "DFU";
		public const string HEALTH_THERMOMETER = "HTM";
		public const string PROXIMITY = "PROXIMITY";
		public const string RUNNING_SPEED_AND_CADENCE = "RSC";
	}
}
