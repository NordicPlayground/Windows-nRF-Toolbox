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
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using nRFToolbox.Base;
using nRFToolbox.Service.GattService;
using nRFToolbox.Service;

namespace nRFToolbox
{
	public sealed class GattServiceManager : GattServiceManagerBase
	{
		private void RegisterServices()
		{
		   container = new UnityContainer();
			container.RegisterType<IHeartRateMeasurementCharacteristic, HeartRateMeasurementCharacteristic>();
			container.RegisterType<IBodySensorLocationCharacteristics, BodySensorLocationCharacteristics>();
			container.RegisterType<IBatteryLevelCharacteristics, BatteryLevelCharacteristics>();
			container.RegisterType<IAlertLevelCharacteristics, AlertLevelCharacteristics>();
			container.RegisterType<IDeviceFirmwareUpdatePacketCharacteristics, DeviceFirmwareUpdatePacketCharacteristics>();
			container.RegisterType<IDeviceFirmwareUpdateControlPointCharacteristics, DeviceFirmwareUpdateControlPointCharacteristics>();
			container.RegisterType<HeartRateService>(new InjectionConstructor(container.Resolve<HeartRateMeasurementCharacteristic>(), container.Resolve<BodySensorLocationCharacteristics>()));
			container.RegisterType<BatteryService>(new InjectionConstructor(container.Resolve<BatteryLevelCharacteristics>()));
			container.RegisterType<LinkLossService>(new InjectionConstructor(container.Resolve<AlertLevelCharacteristics>()));
			container.RegisterType<ImmediateAlertService>(new InjectionConstructor(container.Resolve<AlertLevelCharacteristics>()));
			container.RegisterType<DeviceFirmwareUpdateService>(new InjectionConstructor(container.Resolve<DeviceFirmwareUpdatePacketCharacteristics>(), container.Resolve<DeviceFirmwareUpdateControlPointCharacteristics>()));
		}

		public GattServiceManager()
			:base()
		{
			RegisterServices();
		}

		public override void Dispose() 
		{
			this.container.Dispose();
		}

		private UnityContainer container { get; set; }
		private static GattServiceManager managerInstance = null;
		public static GattServiceManager GetGATTServiceManager()
		{
			if (managerInstance != null)
				return managerInstance;
			else
			{
				managerInstance = new GattServiceManager();
				return managerInstance;
			}
		}


		public IHeartRateService GetHeartRateService() 
		{
			return container.Resolve<HeartRateService>();
		}

		public IBatteryService GetBatteryService() 
		{
			return container.Resolve<BatteryService>();
		}

		public ILinkLossService GetLinkLossService()
		{
			return container.Resolve<LinkLossService>();
		}

		public IDeviceFirmwareUpdateService GetDeviceFirmwareUpdateService() 
		{
			return container.Resolve<DeviceFirmwareUpdateService>();
		}

		public List<IGattService> GetServicesForProximityMonitor() 
		{
			List<IGattService> existingService;
			if (inUsedServices.TryGetValue(nRFToolbox.Service.ToolboxIdentifications.PageId.PROXIMITY, out existingService))
			{
				// services will live before app be killed
				return existingService;
			}
			else
			{
				var requiredServices = new List<IGattService>();
				requiredServices.Add(container.Resolve<LinkLossService>());
				requiredServices.Add(container.Resolve<BatteryService>());
				requiredServices.Add(container.Resolve<ImmediateAlertService>());
				inUsedServices.Add(ToolboxIdentifications.PageId.PROXIMITY, requiredServices);
				return requiredServices;
			}
		}

		private Dictionary<string, List<IGattService>> inUsedServices = new Dictionary<string, List<IGattService>>();
		public Dictionary<string, List<IGattService>> InUsedServices 
		{
			get
			{
				return inUsedServices;
			}
		}
	}
}
