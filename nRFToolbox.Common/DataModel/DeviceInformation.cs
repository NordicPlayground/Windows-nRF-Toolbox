using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage;

namespace NRFToolbox.Common
{
	public class DeviceInformationItem 
	{
		public BluetoothLEDevice Device { get; set; }
		public string DeviceAddress { get; set; }
		public List<Boolean> Flags { get; set; }
		public string UUID { get; set; }
		public string CompleteDeviceName { get; set; }
		public bool AlertOnPhone { get; set; }
		public bool AlertOnDevice { get; set; }
		public string DeviceID { get; set; }
		public Dictionary<string, GattDeviceService> Services = new Dictionary<string, GattDeviceService>();
		private BluetoothConnectionStatus _connectionStatus;
		public BluetoothConnectionStatus ConnectionStatus
		{
			get
			{
				return _connectionStatus;
			}
			set
			{
				if (_connectionStatus != value)
				{
					this._connectionStatus = value;
				}
			}
		}

		public delegate void BLEDeviceConnectionStatusHandler(bool isConnected);

		private static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

		public DeviceInformationItem(string deviceAddress, List<Boolean> flags, string uUID, string completeName)
		{
			this.DeviceAddress = deviceAddress;
			this.Flags = flags;
			this.UUID = uUID;
			this.CompleteDeviceName = completeName;
		}

		public DeviceInformationItem(BluetoothLEDevice device)
		{
			this.Device = device;
			this.DeviceAddress = ConvertingTools.GetBLEMacAddress(device.BluetoothAddress);
			try
			{
				foreach (var service in device.GattServices)
				{
					switch (service.Uuid.ToString())
					{
						case "00001811-0000-1000-8000-00805f9b34fb":
							Services.Add("AlertNotification", service);
							break;
						case "0000180f-0000-1000-8000-00805f9b34fb":
							Services.Add("Battery", service);
							break;
						case "00001810-0000-1000-8000-00805f9b34fb":
							Services.Add("BloodPressure", service);
							break;
						case "00001805-0000-1000-8000-00805f9b34fb":
							Services.Add("CurrentTime", service);
							break;
						case "00001818-0000-1000-8000-00805f9b34fb":
							Services.Add("CyclingPower", service);
							break;
						case "00001816-0000-1000-8000-00805f9b34fb":
							Services.Add("CyclingSpeedAndCadence", service);
							break;
						case "0000180a-0000-1000-8000-00805f9b34fb":
							Services.Add("DeviceInformation", service);
							break;
						case "00001800-0000-1000-8000-00805f9b34fb":
							Services.Add("GenericAccess", service);
							break;
						case "00001801-0000-1000-8000-00805f9b34fb":
							Services.Add("GenericAttribute", service);
							break;
						case "00001808-0000-1000-8000-00805f9b34fb":
							Services.Add("Glucose", service);
							break;
						case "00001809-0000-1000-8000-00805f9b34fb":
							Services.Add("HealthThermometer", service);
							break;
						case "0000180d-0000-1000-8000-00805f9b34fb":
							Services.Add("HeartRate", service);
							break;
						case "00001812-0000-1000-8000-00805f9b34fb":
							Services.Add("HumanInterfaceDevice", service);
							break;
						case "00001802-0000-1000-8000-00805f9b34fb":
							Services.Add("ImmediateAlert", service);
							break;
						case "00001803-0000-1000-8000-00805f9b34fb":
							Services.Add("LinkLoss", service);
							break;
						case "00001819-0000-1000-8000-00805f9b34fb":
							Services.Add("LocationAndNavigation", service);
							break;
						case "00001807-0000-1000-8000-00805f9b34fb":
							Services.Add("NextDstChange", service);
							break;
						case "0000180e-0000-1000-8000-00805f9b34fb":
							Services.Add("PhoneAlertStatus", service);
							break;
						case "00001806-0000-1000-8000-00805f9b34fb":
							Services.Add("ReferenceTimeUpdate", service);
							break;
						case "00001814-0000-1000-8000-00805f9b34fb":
							Services.Add("RunningSpeedAndCadence", service);
							break;
						case "00001813-0000-1000-8000-00805f9b34fb":
							Services.Add("ScanParameters", service);
							break;
						case "00001804-0000-1000-8000-00805f9b34fb":
							Services.Add("TxPower", service);
							break;
						case "00001530-1212-efde-1523-785feabcd123":
							Services.Add(GATTServiceIdentification.DEVICE_FIRMWARE_UPDATE, service);
							break;
						//case "00001531-1212-efde-1523-785feabcd123":
						//	Services.Add("Packet", service);
						//	break;
						//case "00001532-1212-efde-1523-785feabcd123":
						//	Services.Add("ControlPoint", service);
						//	break;
						//case "00001534-1212-efde-1523-785feabcd123":
						//	Services.Add("DFUVersion", service);
						//	break;
						default:
							break;
					}

				}
			}
			catch (Exception e)
			{
				//Catch any exceptions and make alert to phone or user
			}
			if (LocalSettings.Values.ContainsKey(DeviceAddress))
			{
				string[] values = ((string)LocalSettings.Values[DeviceAddress]).Split(',');
				AlertOnPhone = bool.Parse(values[0]);
				AlertOnDevice = bool.Parse(values[1]);
				//alertLevel = (AlertLevel)Enum.Parse(typeof(AlertLevel), values[2]);
			}
			this.CompleteDeviceName = device.Name;
			this.DeviceID = device.DeviceId;
			this.ConnectionStatus = device.ConnectionStatus;
		}
	}
}
