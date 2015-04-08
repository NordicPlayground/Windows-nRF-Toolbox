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
using Common.Service;
using Common.Service.DFUPackage;
using Common.Service.GattService;
using nRFToolbox.Base;
using nRFToolbox.UI;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace nRFToolbox.ViewModels
{
	public class SettingPivotViewModel : ViewModelBase
	{
		public const string PROXIMITY_SETTING = "ProximitySetting";
		public const string ABOUT_PIVOTITEM = "AboutPage";
		public const string HEART_RATE_MONITOR_SETTING = "HeartRateMonitorSetting";
		public const string DEVICE_FIRMWARE_UPDATE_SETTING = "DeviceFirmwareUpdateSetting";
		public const string UART_SETTING = "UARTSetting";
		public const string BGM_SETTING = "GlucoseMonitorSetting";
		public const string nRFToolboxSetting = "nRFToolboxSetting";
		ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
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


		private SettingPivotViewModel() 
		{
			ApplicationDataContainer container;
			if (!localSettings.Containers.TryGetValue("nRFToolboxSetting", out container))
			{
				localSettings.CreateContainer(nRFToolboxSetting, ApplicationDataCreateDisposition.Always);
			}
		}

		private readonly ObservableDictionary settingPageViewModelDictionary = new ObservableDictionary();
		public ObservableDictionary SettingPageViewModelDictionary
		{
			get { return this.settingPageViewModelDictionary; }
		}

		internal async void SendUsEmail()
		{
			await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Launcher.LaunchUriAsync(new Uri("mailto:[mobileapp@nordicsemi.no]"));
			});
		}

		private AboutPageViewModel CreateAboutPageViewModel()
		{
			var aboutPageViewModel = new AboutPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[ABOUT_PIVOTITEM] = aboutPageViewModel;
			return aboutPageViewModel;
		}

		private UARTSettingPageViewModel CreateUARTSettingViewModel()
		{
			var uARTSettingViewModel = new UARTSettingPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[UART_SETTING] = uARTSettingViewModel;
			return uARTSettingViewModel;
		}

		private GlucoseMonitorSettingPageViewModel CreateGlucoseMonitorSettingViewModel()
		{
			var glucoseMonitorSettingViewModel = new GlucoseMonitorSettingPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[BGM_SETTING] = glucoseMonitorSettingViewModel;
			return glucoseMonitorSettingViewModel;			
		}

		private HeartRateMonitorSettingPageViewModel CreateHeartRateMonitorSettingViewModel()
		{
			var heartRateMonitorSettingViewModel = new HeartRateMonitorSettingPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[HEART_RATE_MONITOR_SETTING] = heartRateMonitorSettingViewModel;
			return heartRateMonitorSettingViewModel;
		}

		private DeviceFirmwareUpdateSettingPageViewModel CreateDFUSettingPageViewModel()
		{
			var deviceFirmwareUpdateSettingPageViewModel = new DeviceFirmwareUpdateSettingPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[DEVICE_FIRMWARE_UPDATE_SETTING] = deviceFirmwareUpdateSettingPageViewModel;
			return deviceFirmwareUpdateSettingPageViewModel;
		}

		private ProximitySettingPageViewModel CreateProximitySettingViewModel()
		{
			var proximitySettingPageViewModel = new ProximitySettingPageViewModel();
			SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary[PROXIMITY_SETTING] = proximitySettingPageViewModel;
			return proximitySettingPageViewModel;
		}

		internal DeviceFirmwareUpdateSettingPageViewModel GetDeviceFirmwareUpdateSettingPageViewModel() 
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(DEVICE_FIRMWARE_UPDATE_SETTING, out viewModel))
				return viewModel as DeviceFirmwareUpdateSettingPageViewModel;
			else
				return CreateDFUSettingPageViewModel();
		}

		internal HeartRateMonitorSettingPageViewModel GetHeartRateMonitorSettingViewModel()
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(HEART_RATE_MONITOR_SETTING, out viewModel))
				return viewModel as HeartRateMonitorSettingPageViewModel;
			else
				return CreateHeartRateMonitorSettingViewModel();
		}

		internal ProximitySettingPageViewModel GetProximitySettingViewModel()
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(PROXIMITY_SETTING, out viewModel))
				return viewModel as ProximitySettingPageViewModel;
			else
				return CreateProximitySettingViewModel();
		}

		internal AboutPageViewModel GetAboutPageViewModel()
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(ABOUT_PIVOTITEM, out viewModel))
				return viewModel as AboutPageViewModel;
			else
				return CreateAboutPageViewModel();
		}

		internal UARTSettingPageViewModel GetUartSettingViewModel()
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(UART_SETTING, out viewModel))
				return viewModel as UARTSettingPageViewModel;
			else
				return CreateUARTSettingViewModel();
		}

		internal GlucoseMonitorSettingPageViewModel GetGlucoseMonitorViewModel()
		{
			object viewModel;
			if (SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary.TryGetValue(BGM_SETTING, out viewModel))
				return viewModel as GlucoseMonitorSettingPageViewModel;
			else
				return CreateGlucoseMonitorSettingViewModel();
		}
	}


	public class DeviceFirmwareUpdateSettingPageViewModel : ViewModelBase 
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
					this.OnPropertyChanged("Description");
				}
			}
		}

		private string favoriteFolderName = NOT_CHOOSEN;
		public string FavoriteFolderName
		{
			get
			{
				return favoriteFolderName;
			}
			set
			{
				if (this.favoriteFolderName != value)
				{
					this.favoriteFolderName = value;
					this.OnPropertyChanged("FavoriteFolderName");
				}
			}
		}

		private string imageFileNames;
		public string ImageFileNames
		{
			get
			{
				return imageFileNames;
			}
			set
			{
				if (this.imageFileNames != value)
				{
					this.imageFileNames = value;
					this.OnPropertyChanged("ImageFileNames");
				}
			}
		}


		public const string FavoriteFolderToken = "nRFToolboxFavoriteFolderToken";

		public const string NOT_CHOOSEN = "Not Choosen";
		public const string ImageFile_Bin = ".bin";
		public const string DataFile_dat = ".dat";
		public const string ZipFile = ".zip";

		public StorageFolder FavoriteFolder { get; set; }
		public string SelectedDeviceFirmwareTypeName { get; set; }

		//<Name, Token>
		public Dictionary<string, string> FileToken = new Dictionary<string, string>();

		public DeviceFirmwareUpdateSettingPageViewModel() 
		{
			Information = "Device Firmware Update(DFU) allows you to update firmware through BLE.";
		}

		public string GetShortFileName() 
		{
			StringBuilder builder = new StringBuilder();
			foreach (var item in FileToken.Keys) 
			{
				builder.Append(GattServiceHelper.TryGetShortFileName(item) + ";");
			}
			return builder.ToString();
		}

		public void ClearDFUSettings() 
		{
			SelectedDeviceFirmwareTypeName = string.Empty;
			ImageFileNames = DeviceFirmwareUpdateSettingPageViewModel.NOT_CHOOSEN;
			foreach(var token in this.FileToken.Values)
			{
				Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(token);
			}
			this.FileToken.Clear();
		}

		public async Task SetExampleApplication() 
		{
			SelectedDeviceFirmwareTypeName = FirmwareTypeEnum.Application.ToString();
			var examples = await GattServiceHelper.GetDFUExampleApplication();
			await SaveFile(examples, FirmwareTypeEnum.Application);
		}

		public async Task<bool> SaveFile(StorageFile file, FirmwareTypeEnum selectedType) 
		{
			try 
			{
				string manifesToken;
				var unZippedFiles = await DFUPackageHandler.UpzipDFUPackage(file);
				if (unZippedFiles.TryGetValue("manifest.json", out manifesToken)) 
				{
					var manifest = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(manifesToken);
					manifestObject = await DFUPackageHandler.GetManifestObject(manifest);
					unZippedFiles.Remove("manifest.json");
					this.FileToken = unZippedFiles;
				}
				return true;
			}
			catch(Exception e)
			{
				return false;
			}
		}

		public void SaveFolder(StorageFolder folder) 
		{
			if (folder != null)
			{
				// Application now has read/write access to all contents in the picked favoriteFolder (including other sub-favoriteFolder contents)
				StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(DeviceFirmwareUpdateSettingPageViewModel.FavoriteFolderToken, folder);
				FavoriteFolder = folder;
				FavoriteFolderName = folder.Name;
			}
			else
			{
				FavoriteFolderName = DeviceFirmwareUpdateSettingPageViewModel.NOT_CHOOSEN;
				StorageApplicationPermissions.MostRecentlyUsedList.Remove(DeviceFirmwareUpdateSettingPageViewModel.FavoriteFolderToken);
			}
		}

		public DFUServiceManifestRoot manifestObject { get; set; }
	}
	public class ProximitySettingPageViewModel : ViewModelBase 
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
					this.OnPropertyChanged("Description");
				}
			}
		}

		private bool isImmediateButtonOn;
		public bool IsImmediateButtonOn
		{
			get
			{
				return isImmediateButtonOn;
			}
			set
			{
				if (this.isImmediateButtonOn != value)
				{
					this.isImmediateButtonOn = value;
					this.OnPropertyChanged("IsImmediateButtonOn");
				}
			}
		}

		private string settingPageErrorMessage;
		public string SettingPageErrorMessage
		{
			get
			{
				return settingPageErrorMessage;
			}
			set
			{
				if (this.settingPageErrorMessage != value)
				{
					this.settingPageErrorMessage = value;
					this.OnPropertyChanged("SettingPageErrorMessage");
				}
			}
		}

		public void SetServiceNotOnErrorMessage()
		{
			this.SettingPageErrorMessage = ServiceNotStart;
		}

		public void UpdateImmediateAlarmGUI(bool onOff)
		{
			this.IsImmediateButtonOn = onOff;
		}

		public string ServiceNotStart = "Start glucoseService by choosing a device.";

		public ProximitySettingPageViewModel()
		{
			Information = "This app connects your phone with a proximity tag. When the connection breaks, the tag will raise an alarm."
				+ " " + "Your phone will receive a notification when the tag is connected and disconnected."
				+ " " + "This app runs in the background. You can only connect one proximity tag at a time.";
			alertToggleText = "Trigger an alarm on the peripheral immediately when connected";
		}
	}
	public class HeartRateMonitorSettingPageViewModel : ViewModelBase
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
					this.OnPropertyChanged("Description");
				}
			}
		}

		public HeartRateMonitorSettingPageViewModel()
		{
			Information = "This app displays your heart rate values, provided by your heart rate monitor." + " " +
				"You can also check other description like the battery level and the position where the sensor should be worn." + " " +
				"(Your data will not be stored.)";
		}
	}
	public class AboutPageViewModel : ViewModelBase
	{
		public string AppID = "e12d2da7-4885-400f-bcd4-6cbd5b8cf62c";
		private string description;
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if (this.description != value)
				{
					this.description = value;
					this.OnPropertyChanged("Description");
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
			description = @"The nRF Toolbox is a container app that stores your Nordic Semiconductor apps for Bluetooth low energy in one typeAndLocation." +
                         "It is based on the Nordic Semiconductor nRF51 Development Kit." +
                         "To use the app, Bluetooth 4.0 is required on both the phone and the peripherals.";
			version = @"1.1.0.3";
			additional = "Only a managed peripheral can be used. To manage the device go to Settings->Bluetooth.";
			nordicSemiconductor = @"Nordic Semiconductor is a fabless semiconductor company specializing in ultra low power (ULP) short-range wireless communication.";
		}
	}
	public class UARTSettingPageViewModel : ViewModelBase 
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
					this.OnPropertyChanged("Description");
				}
			}
		}

		public UARTSettingPageViewModel()
		{
			Information = "The Nordic Universal Asynchronous Receiver/Transmitter (UART) app allows you to remotely control your device by texting commands and, using voice recognition." + " " +
				"This application demostrates communication between your phone and the device in a conversation panel." + " " +
				"Commands are send in string format, encoded into UTF-8. The maximum supported length of a command is 20 bytes at a time." + " " +
				"(Your data will not be stored.)";
		}
	}

	public class GlucoseMonitorSettingPageViewModel : ViewModelBase
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
					this.OnPropertyChanged("Description");
				}
			}
		}

		public GlucoseMonitorSettingPageViewModel()
		{
			Information = "Glucose monitor allows you to connect a glucose sensor." + " " +
				"You can retrieve all stored glucose measurements from your device and pick up the first result or the last one." + " " +
				"Glucose measurement context information is currently not supported." + " " +
				"(Your data will not be stored.)";
		}
	}
}
