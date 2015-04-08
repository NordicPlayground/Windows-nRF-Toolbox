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
using nRFToolbox.DataModel;
using nRFToolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using nRFToolbox.UI;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Media.Imaging;
using Common.Service;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
		public NavigationHelper navigationHelper { get; set; }
		public ToolboxDeviceSelectionViewModel deviceSelectionVM { get; set; }
		public AppBarButton AboutButton { get; set; }
		public static MainPage Current;
		public MainPage()
		{
			this.InitializeComponent();
			Current = this;
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
			App.Current.Resuming += Current_Resuming;
			Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);
			//HardwareButtons.BackPressed += HardwareButtons_BackPressed;
		}

		void Current_Resuming(object sender, object e)
		{
			//
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

		private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
		{
			
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			SuspensionManager.RegisterFrame(RootFrame, "RootFrame");
			RegisterDeviceSelectionControl();
			RegisterProfileView();
			toolboxMainPageViewModel = new ToolboxMainPageViewModel();
			var br = StatusBar.GetForCurrentView();
			br.ForegroundColor = Color.FromArgb(1, 0, 156, 222);
		}

		void toolboxMainPageViewModel_LeaveOrStayHandler(bool choice)
		{
			Frame frame = Window.Current.Content as Frame;
		}

		private void RegisterProfileView()
		{
			profileGridView = ToolboxProfiles.FindName(ToolboxProfilesGridView.ProfileGridView) as GridView;
			GattServiceProfile items = new GattServiceProfile();
			profileGridView.ItemsSource = items.Collection;
			profileGridView.ItemClick += profileGridView_ItemClick;
		}


		 void profileGridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			Type page;
			var chosenItem = (ProfileViewModel)e.ClickedItem;
			switch (chosenItem.PageId)
			{
				case ToolboxIdentifications.PageId.HEART_RATE:
					page = typeof(HeartRateMonitor);
					break;
				case ToolboxIdentifications.PageId.DEVICE_FIRMWARE_UPDATE:
					page = typeof(DeviceFirmwareUpdate);
					break;
				case ToolboxIdentifications.PageId.PROXIMITY:
					page = typeof(ProximityMonitor);
					break;
				case ToolboxIdentifications.PageId.NORDIC_UART:
					page = typeof(NordicUART);
					break;
				case ToolboxIdentifications.PageId.GLUCOSE:
					page = typeof(GlucoseMonitor);
					break;
				default:
					page = null;
					break;
		  }	
			if (page != null && !((Frame)Window.Current.Content).Navigate(page, chosenItem.PageId))
			{
				throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
			}
		}

		private void RegisterDeviceSelectionControl()
		{
			var controlPage = new AppBarControl();
			deviceSelectionVM = new ToolboxDeviceSelectionViewModel();
			this.BottomAppBar = controlPage.BottomAppBar;
			this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
			AboutButton = (AppBarButton)controlPage.FindName(AppBarControl.TOOLBOX_ABOUT);
			AboutButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			AboutButton.Click += AboutSettingButton_Click;
			var settingButton = (AppBarButton)controlPage.FindName(AppBarControl.TOOLBOX_SETTING);
			settingButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		void AboutSettingButton_Click(object sender, RoutedEventArgs e)
		{
			if (!((Frame)Window.Current.Content).Navigate(typeof(Settings), toolboxMainPageViewModel.PageId))
			{
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

		private void ToolboxItemView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{

		}

		private ToolboxMainPageViewModel toolboxMainPageViewModel { get; set; }

		public GridView profileGridView { get; set; }
	}
}
