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
using nRFToolbox.Base;
using nRFToolbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace nRFToolbox.ViewModels
{
	public class SettingPivotViewModel : ViewModelBase
	{
		private const string PROXIMITY_SETTING = "ProximitySetting";
		private const string ABOUT_PIVOTITEM = "AboutPage";
		private const string HEART_RATE_MONITOR_SETTING = "HeartRateMonitorSetting";
		private static SettingPivotViewModel proximityViewModelInstance = null;
		public static SettingPivotViewModel GetInstance()
		{
			if (proximityViewModelInstance != null)
			{
				return proximityViewModelInstance;
			}
			else
			{
				proximityViewModelInstance = new SettingPivotViewModel();
				return proximityViewModelInstance;
			}
		}
		private readonly ObservableDictionary settingPageViewModelDictionary = new ObservableDictionary();
		public ObservableDictionary SettingPageViewModelDictionary
		{
			get { return this.settingPageViewModelDictionary; }
		}

		internal void LoadAboutPage()
		{
			var AboutViewModel = new AboutPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[ABOUT_PIVOTITEM] = AboutViewModel;
		}

		internal void LoadProximitySetting()
		{
			var proximitySettingViewModel = new ProximitySettingPage();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[PROXIMITY_SETTING] = proximitySettingViewModel;
		}

		internal async void SendUsEmail()
		{
			await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Launcher.LaunchUriAsync(new Uri("mailto:[jiajun.qiu@nordicsemi.no]"));
			});
		}

		internal void LoadHeartRateMonitorSetting()
		{
			var HeartRateMonitorSettingViewModel = new HeartRateMonitorSettingPage();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[HEART_RATE_MONITOR_SETTING] = HeartRateMonitorSettingViewModel;
		}
	}

	public class ProximitySettingPage : ViewModelBase 
	{
		private string alertToggleText;
		public string AlertToggleText
		{
			get
			{
				return alertToggleText;
			}
			set
			{
				if (this.alertToggleText != value)
				{
					this.alertToggleText = value;
					this.OnPropertyChanged("AlertToggleText");
				}
			}
		}

		private string information;
		public string Information
		{
			get
			{
				return information;
			}
			set
			{
				if (this.information != value)
				{
					this.information = value;
					this.OnPropertyChanged("Information");
				}
			}
		}

		public ProximitySettingPage() 
		{
			Information = "This application links phone with a proximity tag. Once the connection breaks, the tag should start to alarm."
				+ " " + "The phone will receive a notification when it is connected and out of range. (current WindowsPhone OS does not support GATT server.)"
			   + " " + "This application is running on background, at this moment you can only connect one proximity tag at a time.";
			alertToggleText = "Trigger alarm on peripheral immediately when connected";
		}
	}

	public class HeartRateMonitorSettingPage : ViewModelBase
	{
		private string information;
		public string Information
		{
			get
			{
				return information;
			}
			set
			{
				if (this.information != value)
				{
					this.information = value;
					this.OnPropertyChanged("Information");
				}
			}
		}

		public HeartRateMonitorSettingPage()
		{
			Information = "This application presents heart rate values on phone"
				+ " " + "Battery level and detected position are shown if they are available."
				+ " " + "(This application will not running on background)";
		}
	}
	public class AboutPageViewModel : ViewModelBase
	{
		private string information;
		public string Information
		{
			get
			{
				return information;
			}
			set
			{
				if (this.information != value)
				{
					this.information = value;
					this.OnPropertyChanged("Information");
				}
			}
		}

		private string version;
		public string Version
		{
			get
			{
				return version;
			}
			set
			{
				if (this.version != value)
				{
					this.version = value;
					this.OnPropertyChanged("Version");
				}
			}
		}

		private string additional;
		public string Additional
		{
			get
			{
				return additional;
			}
			set
			{
				if (this.additional != value)
				{
					this.additional = value;
					this.OnPropertyChanged("Additional");
				}
			}
		}

		private string nordicSemiconductor;
		public string NordicSemiconductor
		{
			get
			{
				return nordicSemiconductor;
			}
			set
			{
				if (this.nordicSemiconductor != value)
				{
					this.additional = value;
					this.OnPropertyChanged("NordicSemiconductor");
				}
			}
		}


		public AboutPageViewModel() 
		{
			information = @"nRF Toolbox integrate applications support generic attribute profiles. Bluetooth 4.0 is required on both WindowsPhone and peripherals." +
				" " +"This app is designed based on Nordic Semiconductor development kit powered by its nRF51 series chipset.";
			version = @"1.0.0";
			additional = "Only managed peripheral can be used. To manage device go to Settings->Bluetooth.";
			nordicSemiconductor = @"Nordic Semiconductor is a leading company in providing Bluetooth wireless solutions.";
		}
	}
}
