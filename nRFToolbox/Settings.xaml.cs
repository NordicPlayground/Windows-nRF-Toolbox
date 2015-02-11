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
using nRFToolbox.Service;
using Windows.Phone.UI.Input;


namespace nRFToolbox
{
	public sealed partial class Settings : Page
	{

		public Settings()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
			}
		}
		public NavigationHelper navigationHelper { get; set; }

		private void AlertOnDevice_Toggled(object sender, RoutedEventArgs e)
		{
			var x = sender as ToggleSwitch;
			if (x.IsOn)
			{
				if(!ProximityViewModel.GetInstance().SetImmediateAlertOn())
				{
					x.IsOn = false;
				}
			}
			else
			{
				ProximityViewModel.GetInstance().SetImmediateAlertOff();
			}
				
		}

		public SettingPivotViewModel settingPageViewModel { get; set; }

		private void AboutPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().LoadAboutPage();
		}

		private void ProximityPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().LoadProximitySetting();
		}

		private void HeartRateMonitorSettingPage_Loaded(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().LoadHeartRateMonitorSetting();
		}

		private void WriteEmail_Clicked(object sender, RoutedEventArgs e)
		{
			SettingPivotViewModel.GetInstance().SendUsEmail();
		}
	}
}
