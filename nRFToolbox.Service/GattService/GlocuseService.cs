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
using Common.Service.ApplicationException;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Common.Service.GattService
{
	public interface IGlocuseService : IGattService
	{
		event Common.Service.GattService.GlocuseService.GlucoseMeasurementNotification MeasurementNotification;
		Task<bool> Start(GattDeviceService glucoseService);
		Task GetAllStoredRecord();
		Task DeleteAllStoredRecord();
		Task GetFirstRecord();
		Task GetLastRecord();
		Task Stop();
	}

	public class GlocuseService : IGlocuseService
	{
		private IGlucoseMeasurementCharacteristic IGlucoseMeasurementCharacteristic { get; set; }
		private IGlucoseMeasurementContextCharacteristic IGlucoseMeasurementContextCharacteristic { get; set; }
		private IGlucoseFeatureCharacteristic IGlucoseFeatureCharacteristic { get; set; }
		private IRecordAccessControlPointCharacteristic IRecordAccessControlPointCharacteristic { get; set; }

		#region Events
		public delegate void GlucoseMeasurementNotification(GlucoseMeasurementValue measurement);
		public event GlucoseMeasurementNotification MeasurementNotification;
		#endregion

		public GlocuseService(IGlucoseMeasurementCharacteristic iGlucoseMeasurementCharacteristic, IGlucoseFeatureCharacteristic iGlucoseFeatureCharacteristic, 
			IGlucoseMeasurementContextCharacteristic iGlucoseMeasurementContextCharacteristic, IRecordAccessControlPointCharacteristic iRecordAccessControlPointCharacteristic) 
		{
			this.IGlucoseMeasurementCharacteristic = iGlucoseMeasurementCharacteristic;
			this.IGlucoseFeatureCharacteristic = iGlucoseFeatureCharacteristic;
			this.IGlucoseMeasurementContextCharacteristic = iGlucoseMeasurementContextCharacteristic;
			this.IRecordAccessControlPointCharacteristic = iRecordAccessControlPointCharacteristic;
		}

		public GlocuseService(IGlucoseMeasurementCharacteristic iGlucoseMeasurementCharacteristic, IGlucoseFeatureCharacteristic iGlucoseFeatureCharacteristic,
			IRecordAccessControlPointCharacteristic iRecordAccessControlPointCharacteristic)
		{
			this.IGlucoseMeasurementCharacteristic = iGlucoseMeasurementCharacteristic;
			this.IGlucoseFeatureCharacteristic = iGlucoseFeatureCharacteristic;
			this.IRecordAccessControlPointCharacteristic = iRecordAccessControlPointCharacteristic;
		}

		public async Task<bool> Start(GattDeviceService glucoseService) 
		{
			if (glucoseService == null || glucoseService.Uuid != GattServiceUuids.Glucose)
				return IsServiceStarted = false;
			this.glucoseService = glucoseService;
			await EnableGlucoseMeasurementNotification();
			await EnableRecordAccessControlPointIndication();
			return IsServiceStarted = true;
		}

		private async Task EnableGlucoseMeasurementNotification() 
		{
			glucoseMeasurement = this.glucoseService.GetCharacteristics(GattCharacteristicUuids.GlucoseMeasurement).FirstOrDefault();
			if (glucoseMeasurement != null)
			{
				glucoseMeasurement.ValueChanged += glucoseMeasurementNotification;
				var result = await glucoseMeasurement.ReadClientCharacteristicConfigurationDescriptorAsync();
				if (result.Status == GattCommunicationStatus.Success
					&& result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
					await glucoseMeasurement.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
			}
		}

		private async Task DisableGlucoseMeasurementNotification()
		{
			var result = await glucoseMeasurement.ReadClientCharacteristicConfigurationDescriptorAsync();
			if (result.Status == GattCommunicationStatus.Success
				&& result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.None)
				await glucoseMeasurement.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
			glucoseMeasurement.ValueChanged -= glucoseMeasurementNotification;
		}

		private async Task EnableRecordAccessControlPointIndication() 
		{
			recordAccessControlPoint = this.glucoseService.GetCharacteristics(GattCharacteristicUuids.RecordAccessControlPoint).FirstOrDefault();
			if (glucoseMeasurement != null)
			{
				recordAccessControlPoint.ValueChanged += recordAccessControlPoint_ValueChanged;
				var result = await recordAccessControlPoint.ReadClientCharacteristicConfigurationDescriptorAsync();
				if (result.Status == GattCommunicationStatus.Success &&
					result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
					await recordAccessControlPoint.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
			}
		}

		private async Task DisableRecordAccessControlPointIndication()
		{
			var result = await recordAccessControlPoint.ReadClientCharacteristicConfigurationDescriptorAsync();
			if (result.Status == GattCommunicationStatus.Success &&
				result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.None)
				await recordAccessControlPoint.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
			recordAccessControlPoint.ValueChanged -= recordAccessControlPoint_ValueChanged;
		}

		private void glucoseMeasurementNotification(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			var measurementObject = (GlucoseMeasurementValue)this.IGlucoseMeasurementCharacteristic.ProcessData(args.CharacteristicValue);
			if (MeasurementNotification != null)
				MeasurementNotification(measurementObject);
		}

		public async Task GetAllStoredRecord() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await this.recordAccessControlPoint.WriteValueAsync(this.IRecordAccessControlPointCharacteristic.GetAllStoredRecord());
		}

		public async Task GetFirstRecord() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await this.recordAccessControlPoint.WriteValueAsync(this.IRecordAccessControlPointCharacteristic.GetFirstRecord());
		}

		public async Task GetLastRecord() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await this.recordAccessControlPoint.WriteValueAsync(this.IRecordAccessControlPointCharacteristic.GetLastRecord());
		}

		public async Task DeleteAllStoredRecord() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await this.recordAccessControlPoint.WriteValueAsync(this.IRecordAccessControlPointCharacteristic.DeleteAllStoredRecord());
		}

		void recordAccessControlPoint_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			this.IRecordAccessControlPointCharacteristic.ProcessData(args.CharacteristicValue);
		}

		private bool IsServiceStarted { get; set; }
		private GattDeviceService glucoseService { get; set; }

		private GattCharacteristic recordAccessControlPoint { get; set; }

		private GattCharacteristic glucoseMeasurement { get; set; }

		public async Task Stop()
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await DisableGlucoseMeasurementNotification();
			await DisableRecordAccessControlPointIndication();
		}

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.GLUCOSE; }
		}
	}
}
