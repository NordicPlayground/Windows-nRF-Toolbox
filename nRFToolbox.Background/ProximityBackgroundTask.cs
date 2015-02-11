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
using Windows.Data.Xml.Dom;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.Notifications;

namespace nRFToolbox.Background
{
	public sealed class ProximityBackgroundTask : IBackgroundTask
	{
		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
			try 
			{
				DeviceConnectionChangeTriggerDetails details = (DeviceConnectionChangeTriggerDetails)taskInstance.TriggerDetails;
				BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(details.DeviceId);
				if(bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
				{
					XmlDocument xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
					xml.SelectSingleNode("/toast/visual/binding/text").InnerText = string.Format("Proximity tag {0} is connect.", bleDevice.Name);
					ToastNotification toast = new ToastNotification(xml);
					ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
					notifier.Show(toast);
				}else
				{
					XmlDocument xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
					xml.SelectSingleNode("/toast/visual/binding/text").InnerText = string.Format("Proximity tag {0} is out of range.", bleDevice.Name);
					ToastNotification toast = new ToastNotification(xml);
					ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
					notifier.Show(toast);
				}
			}
			catch(Exception e)
			{

			}
			finally
			{
				deferral.Complete();
			}
		}
	}
}
