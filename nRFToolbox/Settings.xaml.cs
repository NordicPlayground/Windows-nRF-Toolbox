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
using nRFToolbox.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using nRFToolbox.ViewModels;
using Windows.Phone.UI.Input;
using Windows.Storage.Pickers;
using nRFToolbox.Common;
using Windows.Storage;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;
using Common.Service.GattService;
using Common.Service;
using Windows.ApplicationModel.Store;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Settings : Page, IFileOpenPickerContinuable, IFolderPickerContinuable
	{

		public Settings()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
			//HardwareButtons.BackPressed += HardwareButtons_BackPressed;
		}

		private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			if (frame == null)
			{
				return;
			}

			if (frame.CanGoBack)
			{
				frame.GoBack();
				e.Handled = true;
			}
		}
		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event receivedBytes that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
			//Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			//var toolboxSettingContainerKey = SettingPivotViewModel.nRFToolboxSetting;
			//var dfuSettingViewModel = SettingPivotViewModel.GetInstance().GetDeviceFirmwareUpdateSettingPageViewModel();
			//var chosenFolder = dfuSettingViewModel.FavoriteFolderName;
			//if (localSettings.Containers.ContainsKey(toolboxSettingContainerKey) && chosenFolder != null)
			//{
			//	localSettings.Containers[toolboxSettingContainerKey].Values[dfuSettingViewModel.FavoriteFolderToken] = dfuSettingViewModel.FavoriteFolder;
			//}
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			this.DataContext = SettingPivotViewModel.GetInstance().SettingPageViewModelDictionary;
			ResolvePageHeader(e.NavigationParameter.ToString());
		}

		private void ResolvePageHeader(string pageName)
		{
			switch (pageName) 
			{ 
				case ToolboxIdentifications.PageId.HEART_RATE:
					this.SettingPivot.SelectedItem = this.HeartRateMonitorSettingPage;
					break;
				case ToolboxIdentifications.PageId.MAIN_PAGE:
					this.SettingPivot.SelectedItem = this.AboutPage;
					break;
				case ToolboxIdentifications.PageId.PROXIMITY:
					this.SettingPivot.SelectedItem = this.ProximitySettingPage;
					break;
				case ToolboxIdentifications.PageId.DEVICE_FIRMWARE_UPDATE:
					this.SettingPivot.SelectedItem = this.DFUSettingPage;
					break;
				case ToolboxIdentifications.PageId.NORDIC_UART:
					this.SettingPivot.SelectedItem = this.UartSettingPage;
					break;
				case ToolboxIdentifications.PageId.GLUCOSE:
					this.SettingPivot.SelectedItem = this.GlucoseSettingPage;
					break;
				case ToolboxIdentifications.PageId.BLOOD_PRESSURE:
					this.SettingPivot.SelectedItem = this.BloodPressureSettingPage;
					break;
				default:
					this.SettingPivot.SelectedItem = this.AboutPage;
					break;
			}
		}
		public NavigationHelper navigationHelper { get; set; }

		#region Proximity Settings
		private void AlertOnDevice_Toggled(object sender, RoutedEventArgs e)
		{
			var alarmToggle = sender as ToggleSwitch;
			if (alarmToggle.IsOn)
			{
				if (ProximityViewModel.GetInstance().SetImmediateAlertOn())
					proximitySettingViewModel.UpdateImmediateAlarmGUI(true);
				else 
				{
					SettingPivotViewModel.GetInstance().GetProximitySettingViewModel().SetServiceNotOnErrorMessage();
					FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);  
				}
			}
			else
			{
				ProximityViewModel.GetInstance().SetImmediateAlertOff();
				proximitySettingViewModel.UpdateImmediateAlarmGUI(false);
			}
		}

		private void ProximityPage_Loaded(object sender, RoutedEventArgs e)
		{
			proximitySettingViewModel = SettingPivotViewModel.GetInstance().GetProximitySettingViewModel();
			if (ProximityViewModel.GetInstance().IsImmediateServiceStarted())
				this.AlertOnDeviceToggle.IsOn = proximitySettingViewModel.IsImmediateButtonOn;
			else
				this.AlertOnDeviceToggle.IsEnabled = false;
		}

		ProximitySettingPageViewModel proximitySettingViewModel { get; set; }

		#endregion

		#region HeartRate Settings
		private void HeartRateMonitorSettingPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().GetHeartRateMonitorSettingViewModel();
		}
		#endregion

		#region About
		private void AboutPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().GetAboutPageViewModel();
		}
		private void WriteEmail_Clicked(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().SendUsEmail();
		}
		private async void WriteReview_Clicked(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(
				new Uri("ms-windows-store:reviewapp?appid=" + SettingPivotViewModel.GetInstance().GetAboutPageViewModel().AppID));
		}
		#endregion

   	#region DFU Settings
		private async void DFUSettingPage_Loaded(object sender, RoutedEventArgs e)
		{
			dfuSettingViewModel = SettingPivotViewModel.GetInstance().GetDeviceFirmwareUpdateSettingPageViewModel();
			if (DeviceFirmwareUpdateSettingPageViewModel.FavoriteFolderToken != null
				 && StorageApplicationPermissions.FutureAccessList.ContainsItem(DeviceFirmwareUpdateSettingPageViewModel.FavoriteFolderToken))
			{
				var cachedFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(DeviceFirmwareUpdateSettingPageViewModel.FavoriteFolderToken, AccessCacheOptions.None) as StorageFolder;
				dfuSettingViewModel.FavoriteFolder = cachedFolder;
			}
			if (dfuSettingViewModel.FileToken.Count > 0) 
			{
				dfuSettingViewModel.ImageFileNames = dfuSettingViewModel.GetShortFileName();
			}
		}

		private void ClearSelectedFileButton_Clicked(object sender, RoutedEventArgs e)
		{
			dfuSettingViewModel.ClearDFUSettings();
		}

		private void SaveImageAs_Clicked(object sender, RoutedEventArgs e)
		{
			FolderPicker folderPicker = new FolderPicker();
			folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
			folderPicker.PickFolderAndContinue();
		}

		public void ContinueFolderPicker(FolderPickerContinuationEventArgs args)
		{
			StorageFolder folder = args.Folder;
			dfuSettingViewModel.SaveFolder(folder);
		}

		public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
		{
		   foreach(StorageFile pickedFile in args.Files)
			{
				await dfuSettingViewModel.SaveFile(pickedFile);
			}
			dfuSettingViewModel.ImageFileNames = dfuSettingViewModel.GetShortFileName();
		}

		private async void TryExample_Clicked(object sender, RoutedEventArgs e)
		{
			await dfuSettingViewModel.SetExampleApplication();
			dfuSettingViewModel.ImageFileNames = dfuSettingViewModel.GetShortFileName();
		}

		private void SelectFilesButton_Clicked(object sender, RoutedEventArgs e)
		{
			FileOpenPicker folderPicker = new FileOpenPicker();
			folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
			folderPicker.FileTypeFilter.Add(DeviceFirmwareUpdateSettingPageViewModel.ZipFile);
			folderPicker.PickMultipleFilesAndContinue();
		}

		private DeviceFirmwareUpdateSettingPageViewModel dfuSettingViewModel { get; set; }
		#endregion

		#region Uart Settings
		private void UartSettingPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().GetUartSettingViewModel();
		}
		#endregion

		#region BGM
		private void GlucoseSettingPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().GetGlucoseMonitorViewModel();
		} 
		#endregion

		#region BPM
		private void BPMSettingPage_Loaded(object sender, RoutedEventArgs e) 
		{
			SettingPivotViewModel.GetInstance().GetBloodPressureMeasurementViewModel();
		}
      #endregion

	}
}
