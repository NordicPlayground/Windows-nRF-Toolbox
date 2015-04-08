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
