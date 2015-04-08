using Common.Service.ApplicationException;
/*Copyright (c) 2015, Nordic Semiconductor ASA
 *
 *Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *
 *1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *
 *2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other 
 *materials provided with the distribution.
 *
 *3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific 
 *prior written permission.
 *
 *THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 *PURPOSE ARE DISCLAIMED. *IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF *SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, *DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED *OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace Common.Service.GattService
{
	public interface IBatteryService : IGattService
	{
	   Task<bool> Start(GattDeviceService batteryService);
		event BatteryService.ValueChangeCompletedHandler ValueChangeCompleted;
		Task Stop();
		bool IsServiceStarted { get; }
	}
	public class BatteryService : IBatteryService
	{
		public delegate void ValueChangeCompletedHandler(BatteryLevelCharacteristics batteryLevel);
		public event ValueChangeCompletedHandler ValueChangeCompleted;
		private IBatteryLevelCharacteristics BatteryLevelCharacteristicsHandler { get; set; }

		public BatteryService(IBatteryLevelCharacteristics batteryLevelCharacteristics) 
		{
			this.BatteryLevelCharacteristicsHandler = batteryLevelCharacteristics;
		}

		private object lockObject = new object();

		public async Task<bool> Start(GattDeviceService batteryService)
		{
			if (batteryService == null || batteryService.Uuid != GattServiceUuids.Battery)
				return IsServiceStarted = false;
			DeviceBatteryService = batteryService;
			await SetBatterylevelCharacteristics();
			return IsServiceStarted = true;
		}

		private async Task<bool> SetBatterylevelCharacteristics()
		{
			bool SetValueSuccessed = false;
			batterLevelCharacteristic = DeviceBatteryService.GetCharacteristics(GattCharacteristicUuids.BatteryLevel).FirstOrDefault();
			if (batterLevelCharacteristic != null)
			{
				batterLevelCharacteristic.ValueChanged += batterLevelCharacteristic_ValueChanged;
				var currentDescriptorValue = await batterLevelCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
				if((currentDescriptorValue.Status != GattCommunicationStatus.Success) ||
                    (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify))
				{
					var status = await batterLevelCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
					if (status == GattCommunicationStatus.Unreachable)
					{
						// Register a PnpObjectWatcher to detect when a connection to the device is established,
						// such that the application can retry device configuration.
						//StartDeviceConnectionWatcher();
						return SetValueSuccessed;
					}else if(status == GattCommunicationStatus.Success)
					{
						SetValueSuccessed = true;
						return SetValueSuccessed;
					}			
				}
				else
				{
					SetValueSuccessed = true;
					return SetValueSuccessed;
				}
			}
			return SetValueSuccessed;;
		}

		private async Task<bool> StopReceivBatterylevelNotification() 
		{
			if (!IsServiceStarted || DeviceBatteryService == null)
				throw new ServiceNotInitializedException();
			bool StopSuccessed = false;
			if (batterLevelCharacteristic != null)
			{
				batterLevelCharacteristic.ValueChanged -= batterLevelCharacteristic_ValueChanged;
				var currentDescriptorValue = await batterLevelCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
				if ((currentDescriptorValue.Status == GattCommunicationStatus.Success) &&
						  (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor == GattClientCharacteristicConfigurationDescriptorValue.Notify))
				{
					var status = await batterLevelCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
					if (status == GattCommunicationStatus.Unreachable)
					{
						// Register a PnpObjectWatcher to detect when a connection to the device is established,
						// such that the application can retry device configuration.
						//StartDeviceConnectionWatcher();
						return StopSuccessed;
					}
					else if (status == GattCommunicationStatus.Success)
					{
						StopSuccessed = true;
						return StopSuccessed;
					}
				}
				else
				{
					StopSuccessed = true;
					return StopSuccessed;
				}
			}
			return StopSuccessed;
		}

		private void batterLevelCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			try
			{
				BatteryLevelCharacteristics result;
				var batteryLevelData = new byte[args.CharacteristicValue.Length];
				DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(batteryLevelData);
				result = BatteryLevelCharacteristicsHandler.ProcessDataBatteryLevel(batteryLevelData);
				if(ValueChangeCompleted != null)
				{
					ValueChangeCompleted(result);
				}
			}
			catch (Exception ex)
			{
				var error = ex.StackTrace;
			}
		}

		private GattCharacteristic batterLevelCharacteristic { get; set; }

		public async Task Stop() 
		{
			//Do we need to close on peripheral 
			await StopReceivBatterylevelNotification();
			DeviceBatteryService = null;
			IsServiceStarted = false;
		}

		public bool IsServiceStarted { get; private set; }

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.BATTERY; }
		}

		public GattDeviceService DeviceBatteryService { get; set; }
	}
}
