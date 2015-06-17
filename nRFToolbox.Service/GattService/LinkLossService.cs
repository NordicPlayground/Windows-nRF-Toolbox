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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;
using BackgroundExecutiondManager = Windows.ApplicationModel.Background.BackgroundExecutionManager;
using Common.Service.ApplicationException;
using Common.Utility;


namespace Common.Service.GattService
{
	public interface ILinkLossService : IGattService
	{
		//Key format is PageName,DeviceID
		Dictionary<string, KeyValuePair<GattDeviceService, BackgroundTaskRegistration>> LinkLossServiceTaskRegistrations { get; }
		//Task<BackgroundTaskRegistration> RegisterServiceAsBackgroundTask();
		Task<bool> Start(GattDeviceService linkLossService);
		Task<AlertLevelEnum> ReadAlertLevelCharacteristicAsync();
		void WriteAlertLevelCharacteristicAsync(AlertLevelEnum level);
		bool IsServiceStarted { get;}
		void Stop();
	}
	public class LinkLossService : ILinkLossService
	{
		private IAlertLevelCharacteristics alertLevelCharacteristics { get; set; }
		private GattCharacteristic alertLevel { get; set; }
		public bool IsServiceStarted { get; private set; }
		//Registrations for all proximate devices who use this LinkLoss Service 
		private Dictionary<string, KeyValuePair<GattDeviceService, BackgroundTaskRegistration>> linkLossServiceTaskRegistrations = new Dictionary<string, KeyValuePair<GattDeviceService, BackgroundTaskRegistration>>();
		//Only one is stored, by using LinkLossservice to create an new service instance this code might be changed in future.
		Dictionary<string, KeyValuePair<GattDeviceService, BackgroundTaskRegistration>> ILinkLossService.LinkLossServiceTaskRegistrations
		{
			get
			{
				return linkLossServiceTaskRegistrations;
			}
		}
		//This entry point is defined in appmanifest file
		private string ENTRY_POINT = "nRFToolbox.Background.ProximityBackgroundTask";

		public LinkLossService(IAlertLevelCharacteristics alertLevelCharacteristics) 
		{
			this.alertLevelCharacteristics = alertLevelCharacteristics;
		}

		public async Task<bool> Start(GattDeviceService linkLossDeviceService)
		{
			if (linkLossDeviceService != null && linkLossDeviceService.Uuid != GattServiceUuids.LinkLoss)
				 return IsServiceStarted = false;
			this.linkLossDeviceService = linkLossDeviceService;
			var result = await RegisterDeviceServiceAsBackgroundTask(linkLossDeviceService);
			alertLevel = linkLossDeviceService.GetCharacteristics(GattCharacteristicUuids.AlertLevel).FirstOrDefault();
			if (result != null)
				return IsServiceStarted = true;
			else
				return IsServiceStarted = false;
		}

		public async Task<AlertLevelEnum> ReadAlertLevelCharacteristicAsync()
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException(new Exception());
			if (alertLevel == null)
				throw new NullReferenceException();
			var result = await alertLevel.ReadValueAsync();
			var data = BLEDataReader.ReadToBytes(result);
			var alertLevelSettings = alertLevelCharacteristics.ProcessData(data);
			return alertLevelSettings;
		}

		public async void WriteAlertLevelCharacteristicAsync(AlertLevelEnum level)
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException(new Exception());
			try
			{
				if (linkLossDeviceService == null)
					return;
				byte[] levelValue = new byte[] { (byte)level };
				var response = await alertLevel.WriteValueAsync(levelValue.AsBuffer());
			}
			catch (Exception e)
			{
				string errorMessage = e.Message;
			}
		}

		private async Task<BackgroundTaskRegistration> RegisterDeviceServiceAsBackgroundTask(GattDeviceService deviceService) 
		{
			var deviceAddress = ConvertingTools.GetBLEMacAddress(deviceService.Device.BluetoothAddress);
			StringBuilder name = new StringBuilder(ToolboxIdentifications.PageId.PROXIMITY +",");
			name.Append(deviceService.DeviceId);
			KeyValuePair<GattDeviceService, BackgroundTaskRegistration> task;
			if(this.linkLossServiceTaskRegistrations.TryGetValue(name.ToString(), out task))
			{	
				task.Value.Unregister(true);
				linkLossServiceTaskRegistrations.Remove(name.ToString());
			}
			DeviceConnectionChangeTrigger trigger = await DeviceConnectionChangeTrigger.FromIdAsync(deviceService.DeviceId);
			BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
			trigger.MaintainConnection = true;
			builder.Name = name.ToString();
			builder.TaskEntryPoint = ENTRY_POINT;
			builder.SetTrigger(trigger);
			var newTask = builder.Register();
			linkLossServiceTaskRegistrations.Add(name.ToString(), new KeyValuePair<GattDeviceService, BackgroundTaskRegistration>(deviceService, newTask));
			return newTask;
		}

		public void Stop()
		{
			foreach (var item in this.linkLossServiceTaskRegistrations) 
			{
				WriteAlertLevelCharacteristicAsync(AlertLevelEnum.NoAlert);
				item.Value.Value.Unregister(true);
			}
			linkLossServiceTaskRegistrations.Clear();
			IsServiceStarted = false;
		}

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.LINK_LOSS; }
		}

		private GattDeviceService linkLossDeviceService { get; set; }
	}
}
