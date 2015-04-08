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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Common.Service.ApplicationException;

namespace Common.Service.GattService
{
#if WINDOWS_PHONE_APP
	public interface IImmediateAlertService : IGattService
	{
		Task<bool> Start(GattDeviceService immediateDeviceService);
		Task WriteAlertLevel(AlertLevelEnum level);
		bool IsServiceStarted { get; }
		Task Stop();
	}
	public class ImmediateAlertService : IImmediateAlertService
	{
		private IAlertLevelCharacteristics alertLevelCharacteristics { get; set; }
		public ImmediateAlertService(IAlertLevelCharacteristics alertLevelCharacteristics) 
		{
			this.alertLevelCharacteristics = alertLevelCharacteristics;
		}

		private GattDeviceService immediateDeviceService { get; set; }

		public async Task<bool> Start(GattDeviceService immediateDeviceService)
		{
			if (immediateDeviceService != null && immediateDeviceService.Uuid != GattServiceUuids.ImmediateAlert)
				 return IsServiceStarted = false;
			else
			{
				this.immediateDeviceService = immediateDeviceService;
				return IsServiceStarted = true;
			}
		}

		public async Task WriteAlertLevel(AlertLevelEnum level) 
		{
			if (!IsServiceStarted)
				throw new ServiceNotInitializedException();
			try
			{
				var alertLevel = this.immediateDeviceService.GetCharacteristics(GattCharacteristicUuids.AlertLevel).FirstOrDefault();
				byte[] levelValue = new byte[] { (byte)level };
				var response = await alertLevel.WriteValueAsync(levelValue.AsBuffer(), GattWriteOption.WriteWithoutResponse);
			}
			catch (Exception e)
			{
				string errorMessage = e.Message;
			}
		}

		public bool IsServiceStarted { get; private set; }

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.IMMEDIATE_ALERT; }
		}


		public async Task Stop()
		{
			await WriteAlertLevel(AlertLevelEnum.NoAlert);
			this.immediateDeviceService = null;
			IsServiceStarted = false;
		}
	}
#endif
}
