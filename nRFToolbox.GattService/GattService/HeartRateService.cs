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
using nRFToolbox.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration.Pnp;
using Windows.Storage.Streams;

namespace nRFToolbox.Service.GattService
{
	public interface IHeartRateService
	{
		ConcurrentQueue<HeartRateMeasurementCharacteristic> HeartRatioQueue { get; }
	   Task<bool> Start(GattDeviceService heartRateService);
		//To be changed here
		event HeartRateService.HeartRateValueChangeCompletedHandler HeartRateValueChangeCompleted;
		event HeartRateService.SensorLocationValueChangeCompletedHandler SensorLocation_Detected;
		void Stop();
		bool IsServiceStarted { get; }
	}
	public class HeartRateService : IHeartRateService
	{
		//public event nRFToolbox.ViewModel.DeviceSearchViewModel.BLEDeviceConnectionStatusHandler connectionStatusUpdated;
		public IBodySensorLocationCharacteristics bodySensorLocationCharacteristicsHandler { get; set; }
		public IHeartRateMeasurementCharacteristic HeartRateMeasurementCharacteristicHandler { get; set; }
		public IHeartHeartRateControlPointCharacteristics heartHeartRateControlPointCharacteristicsHandler { get; set; }
		public bool IsServiceStarted { get; private set; }

#region Event
		public delegate void HeartRateValueChangeCompletedHandler(HeartRateMeasurementCharacteristic heartRateMeasurementValue);
		public event HeartRateValueChangeCompletedHandler HeartRateValueChangeCompleted;
		public delegate void SensorLocationValueChangeCompletedHandler(string detectedLocation);
		public event SensorLocationValueChangeCompletedHandler SensorLocation_Detected;
#endregion

#region properties
		public bool IsHeartRateServiceInitiated { get; set; }
		private object lockObject = new object();
		private static ConcurrentQueue<HeartRateMeasurementCharacteristic> heartRatioQueue = new ConcurrentQueue<HeartRateMeasurementCharacteristic>();
		public ConcurrentQueue<HeartRateMeasurementCharacteristic> HeartRatioQueue
		{
			get
			{
				return heartRatioQueue;
			}
		}

		private string bodyLocation;
		public string BodyLocation 
		{
			get 
			{
				return bodyLocation;
			}
		}

		private GattDeviceService heartRateServiceInstance { get; set; }
		public GattCharacteristic hrmCharacteristic { get; set; }
		public GattCharacteristic bslCharacteristics { get; set; }
		public GattCharacteristic hrCharacteristics { get; set; }
#endregion

		public HeartRateService(IHeartRateMeasurementCharacteristic heartRateMeasurementCharacteristic, IBodySensorLocationCharacteristics bodySensorLocationCharacteristics)
		{
			this.HeartRateMeasurementCharacteristicHandler = heartRateMeasurementCharacteristic;
			this.bodySensorLocationCharacteristicsHandler = bodySensorLocationCharacteristics;
		}

		public HeartRateService(IHeartRateMeasurementCharacteristic heartRateMeasurementCharacteristic, IBodySensorLocationCharacteristics bodySensorLocationCharacteristics,
			IHeartHeartRateControlPointCharacteristics heartHeartRateControlPointCharacteristics)
		{
			this.HeartRateMeasurementCharacteristicHandler = heartRateMeasurementCharacteristic;
			this.bodySensorLocationCharacteristicsHandler = bodySensorLocationCharacteristics;
			this.heartHeartRateControlPointCharacteristicsHandler = heartHeartRateControlPointCharacteristics;
		}
		public async Task<bool> Start(GattDeviceService hearRateService)
		{
			if (hearRateService == null || !hearRateService.Uuid.Equals(GattServiceUuids.HeartRate))
				return IsServiceStarted = false;
			this.heartRateServiceInstance = hearRateService;
			IsServiceStarted = true;
			await SetHearRateCharacteristic();
			await ReadBodyLocationCharacteristics();
			return IsServiceStarted;
		}

		private async Task<bool> SetHearRateCharacteristic() 
		{
			bool SetCharacteristicSccueed = false;
			if (!IsServiceStarted || heartRateServiceInstance == null)
				throw new ServiceNotInitializedException();
			hrmCharacteristic = heartRateServiceInstance.GetCharacteristics(GattCharacteristicUuids.HeartRateMeasurement).FirstOrDefault();
			if (hrmCharacteristic != null)
			{
				hrmCharacteristic.ValueChanged += heartRateMeasurement_DataChanged;
				var currentDescriptorValue = await hrmCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync(); 
				if((currentDescriptorValue.Status != GattCommunicationStatus.Success) ||
                    (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify))
				{
					var status = await hrmCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
					if (status == GattCommunicationStatus.Unreachable)
					{
						// Register a PnpObjectWatcher to detect when a connection to the device is established,
						// such that the application can retry device configuration.
						//StartDeviceConnectionWatcher();
						SetCharacteristicSccueed = false;
					}
					SetCharacteristicSccueed = true;
				}
			}
			return SetCharacteristicSccueed;
		}

		private async Task<bool> ReadBodyLocationCharacteristics() 
		{
			bool SetCharacteristicSucceeed = false;
			if (!IsServiceStarted || heartRateServiceInstance == null)
				throw new ServiceNotInitializedException();

			bslCharacteristics = heartRateServiceInstance.GetCharacteristics(GattCharacteristicUuids.BodySensorLocation).FirstOrDefault();
			if (bslCharacteristics != null)
			{
				var result = await bslCharacteristics.ReadValueAsync();
				var bslRaw = new byte[result.Value.Length];
				DataReader.FromBuffer(result.Value).ReadBytes(bslRaw);
				var bslData = bodySensorLocationCharacteristicsHandler.ProcessBodySensorLocationData(bslRaw);
				if(!string.IsNullOrEmpty(bslData.BodySensorLocation))
				{
					SetCharacteristicSucceeed = true;
					this.bodyLocation = bslData.BodySensorLocation;
					if (SensorLocation_Detected != null)
					{
						SensorLocation_Detected(bslData.BodySensorLocation);
					}
				}
			}
			return SetCharacteristicSucceeed;
		}

		private async Task<bool> StopReceiveHeartRateValue() 
		{
			bool SetCharacteristicSucceeed = false;
			if (!IsServiceStarted || heartRateServiceInstance == null)
				throw new ServiceNotInitializedException();
			if (hrmCharacteristic != null)
			{
				hrmCharacteristic.ValueChanged -= heartRateMeasurement_DataChanged;
				var currentDescriptorValue = await hrmCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
				if ((currentDescriptorValue.Status == GattCommunicationStatus.Success) &&
						  (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor == GattClientCharacteristicConfigurationDescriptorValue.Notify))
				{
					var status = await hrmCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
					if (status == GattCommunicationStatus.Success)
					{
						// Register a PnpObjectWatcher to detect when a connection to the device is established,
						// such that the application can retry device configuration.
						//StartDeviceConnectionWatcher();
						SetCharacteristicSucceeed = true;
					}
				}
			}
			return SetCharacteristicSucceeed;
		}

		private void heartRateMeasurement_DataChanged(GattCharacteristic sender, GattValueChangedEventArgs args)   
		{
			var bmpData = new byte[args.CharacteristicValue.Length];
			DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bmpData);
		  	var result = HeartRateMeasurementCharacteristicHandler.ProcessHeartRateMeasurementData(bmpData);
			lock(lockObject)
			{
				heartRatioQueue.Enqueue(result);
			}

			if (HeartRateValueChangeCompleted != null)
			{
				HeartRateValueChangeCompleted(result);
			}
		}

		public void Stop() 
		{
			StopReceiveHeartRateValue();
			this.heartRateServiceInstance = null;
			IsServiceStarted = false;
		}
	}
}
