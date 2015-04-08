using Common.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace nRFToolbox.Common
{
    public class VoiceControlHandler
    {
#if WINDOWS_PHONE_APP
		 public static void HandleCommand(IActivatedEventArgs args, Frame rootFrame) 
		 {
			 if (args.Kind == ActivationKind.VoiceCommand)
			 {
				 VoiceCommandActivatedEventArgs voiceArgs = (VoiceCommandActivatedEventArgs)args;
				 if (!((Frame)Window.Current.Content).Navigate(typeof(NordicUART), ToolboxIdentifications.PageId.NORDIC_UART))
				 {
				 }
			 }
		 }
#endif
    }
}
