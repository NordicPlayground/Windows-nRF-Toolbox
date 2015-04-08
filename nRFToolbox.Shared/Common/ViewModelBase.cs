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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

namespace nRFToolbox.ViewModels
{
	public partial class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (PropertyChanged != null)
			{
				if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public async Task<bool> IsBluetoothSettingOn()
		{
			bool IsBluetoothOn = false;
			try
			{
				Windows.Networking.Proximity.PeerFinder.Start();
				Windows.Networking.Proximity.PeerInformation result = (await Windows.Networking.Proximity.PeerFinder.FindAllPeersAsync()).FirstOrDefault();
				//if(peers != null)
				//	return IsBluetoothOn = false;
				return IsBluetoothOn = true; //boolean variable
			}
			catch (Exception ex)
			{
				if ((uint)ex.HResult == 0x8007048F)
				{
					return IsBluetoothOn = false;
				}
			}
			finally
			{
				Windows.Networking.Proximity.PeerFinder.Stop();
			}
			return IsBluetoothOn;
		}

#if WINDOWS_PHONE_APP
		partial void HideStatusBar();
#endif
		public virtual void ShowMessage(string title, string content, UICommand alternative1, UICommand alternative2)
		{
			var messageDialog = new MessageDialog(content, title);
			messageDialog.Commands.Add(alternative1);
			messageDialog.Commands.Add(alternative2);
			messageDialog.DefaultCommandIndex = 0;
			messageDialog.CancelCommandIndex = 1;
			messageDialog.ShowAsync();
		}
		public virtual void ShowMessage(string title, UICommand alternative1)
		{
			var messageDialog = new MessageDialog(title);
			messageDialog.Commands.Add(alternative1);
			messageDialog.CancelCommandIndex = 0;
			messageDialog.ShowAsync();
		}
	}
}
