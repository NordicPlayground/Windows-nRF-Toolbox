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
	public interface IBloodPressureService : IGattService
	{
		event Common.Service.GattService.BloodPressureService.BloodPressureMeasurementNotification MeasurementNotification;
		event Common.Service.GattService.BloodPressureService.BloodPressureFeatureNotification FeaturesNotification;
		Task<bool> Start(GattDeviceService bloodPressureService);
		Task EnableBloodPressureMeasurementIndication();
		Task GetBloodPressureFeature();
		Task Stop();
	}
	public class BloodPressureService : IBloodPressureService
	{

		#region Events
		public delegate void BloodPressureMeasurementNotification(BloodPressureMeasurementValue measurement);
		public event BloodPressureMeasurementNotification MeasurementNotification;
		public delegate void BloodPressureFeatureNotification(BloodPressureFeatureValue features);
		public event BloodPressureFeatureNotification FeaturesNotification;
		#endregion
		public BloodPressureService(IBloodPressureMeasurementCharacteristic bloodPressureMeasurementCharacteristic, IBloodPressureFeatureCharacterisctic bloodPressureFeature) 
		{
			this.BloodPressureMeasurementCharacteristicHandler = bloodPressureMeasurementCharacteristic;
			this.BloodPressureFeatureCharacteristicHandler = bloodPressureFeature;
		}

		public BloodPressureService(IBloodPressureMeasurementCharacteristic bloodPressureMeasurementCharacteristic, IBloodPressureFeatureCharacterisctic bloodPressureFeature, IIntermediateCuffPressureCharacteristic IntermediateCuffPressureCharacteristic)
		{
			this.BloodPressureMeasurementCharacteristicHandler = bloodPressureMeasurementCharacteristic;
			this.BloodPressureFeatureCharacteristicHandler = bloodPressureFeature;
			this.IntermediateCuffPressureCharacteristicHandler = IntermediateCuffPressureCharacteristic;
		}

		public async Task<bool> Start(GattDeviceService bloodPressureService)
		{
			if (bloodPressureService == null || bloodPressureService.Uuid != GattServiceUuids.BloodPressure)
				return IsServiceStarted = false;
			this.BloodPressureDeviceService = bloodPressureService;
			await EnableBloodPressureMeasurementIndication();
			return IsServiceStarted = true;
		}

		public async Task EnableBloodPressureMeasurementIndication() 
		{
		   this.bloodPressureMeasurementCharacteristic = this.BloodPressureDeviceService.GetCharacteristics(GattCharacteristicUuids.BloodPressureMeasurement).FirstOrDefault();
			if (bloodPressureMeasurementCharacteristic != null)
			{
				bloodPressureMeasurementCharacteristic.ValueChanged += bloodPressureMeasurementCharacteristic_ValueChanged;
				var result = await bloodPressureMeasurementCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
				if (result != null && result.Status == GattCommunicationStatus.Success &&
					result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
					await bloodPressureMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
			}
		}

		void bloodPressureMeasurementCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			var result = this.BloodPressureMeasurementCharacteristicHandler.ProcessData(File.ToBytes(args.CharacteristicValue));
			if (MeasurementNotification != null)
				MeasurementNotification(result);
		}

		public async Task GetBloodPressureFeature()
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			bloodPressureFeatureCharacteristic = this.BloodPressureDeviceService.GetCharacteristics(GattCharacteristicUuids.BloodPressureFeature).FirstOrDefault();
			if (bloodPressureFeatureCharacteristic != null)
			{
				var result = await bloodPressureFeatureCharacteristic.ReadValueAsync();
				var features = this.BloodPressureFeatureCharacteristicHandler.ProcessData(File.ToBytes(result.Value));
				if (FeaturesNotification != null)
					FeaturesNotification(features);
			}
		}

		public async Task DisableBloodPressureMeasurementIndication() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			this.bloodPressureMeasurementCharacteristic = this.BloodPressureDeviceService.GetCharacteristics(GattCharacteristicUuids.BloodPressureMeasurement).FirstOrDefault();
			if (bloodPressureMeasurementCharacteristic != null)
			{
				var result = await bloodPressureMeasurementCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
				if (result != null && result.Status == GattCommunicationStatus.Success &&
					result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.None)
				{ 
					await bloodPressureMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
					bloodPressureMeasurementCharacteristic.ValueChanged -= bloodPressureMeasurementCharacteristic_ValueChanged;
				}
			}
		}

		public async Task Stop() 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			await DisableBloodPressureMeasurementIndication();
			IsServiceStarted = false;
		}

		private IBloodPressureMeasurementCharacteristic BloodPressureMeasurementCharacteristicHandler { get; set; }
		private IBloodPressureFeatureCharacterisctic BloodPressureFeatureCharacteristicHandler { get; set; }
		private IIntermediateCuffPressureCharacteristic IntermediateCuffPressureCharacteristicHandler { get; set; }
		private GattCharacteristic bloodPressureMeasurementCharacteristic { get; set; }
		private GattCharacteristic bloodPressureFeatureCharacteristic { get; set; }
		private GattDeviceService BloodPressureDeviceService { get; set; }
		public bool IsServiceStarted { get; set; }
		public string Name
		{
			get 
			{
				return ToolboxIdentifications.GattServiceNames.BLOOD_PRESSURE;
			}
		}


	}
}
