using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace nRFToolbox
{
	public sealed partial class ToolboxProfilesGridView : UserControl
	{
		private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
		public ToolboxProfilesGridView()
		{
			this.InitializeComponent();
			//StoreData items = new StoreData();
			//ToolboxItemGridView.ItemsSource = items.Collection;
		}

		//private async void ToolboxItemClicked(object sender, ItemClickEventArgs e)
		//{
		//	if (! await DeviceFirmwareUpdateViewModel.IsBluetoothSettingOn())
		//	{
		//		this.deviceFirmwareUpdateViewModel.ShowToastMessage("Please turn bluetooth on at settingButton page.");
		//		return;
		//	}
		//	Type page;
		//	var chosenItem = (ProfileViewModel)e.ClickedItem;
		//	switch (chosenItem.PageId) 
		//	{
		//		case ToolboxIdentifications.PageId.HEART_RATE:
		//			page = typeof(nRFToolbox);
		//			break;
		//		case ToolboxIdentifications.PageId.DEVICE_FIRMWARE_UPDATE:
		//			page = typeof(DeviceFirmwareUpdate);
		//			break;
		//		default:
		//			page = typeof(MainPage);
		//			break;
		//	}
		//	if (!((Frame)Window.Current.Content).Navigate(page, chosenItem))
		//	{
		//		throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
		//	}
		//}
	}
}
