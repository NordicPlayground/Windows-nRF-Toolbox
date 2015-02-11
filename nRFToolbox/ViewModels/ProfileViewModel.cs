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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections;
using nRFToolbox.UI;
using nRFToolbox.Base;
using nRFToolbox.Service;

namespace nRFToolbox.ViewModels
{
	public class ProfileViewModel : ViewModelBase
	{
		private string _Title = string.Empty;
		public string Title
		{
			get
			{
				return this._Title;
			}

			set
			{
				if (this._Title != value)
				{
					this._Title = value;
					this.OnPropertyChanged("Title");
				}
			}
		}

		private ImageSource _Image = null;
		public ImageSource Image
		{
			get
			{
				return this._Image;
			}

			set
			{
				if (this._Image != value)
				{
					this._Image = value;
					this.OnPropertyChanged("Image");
				}
			}
		}

		private string pageId = null;
		public string PageId
		{
			get 
			{
				return pageId;
			}
			set 
			{
				if (this.pageId != value)
				{
					this.pageId = value;
				}
			}
		}

		public void SetLocalImage(Uri baseUri, String path)
		{
			Image = new BitmapImage(new Uri(baseUri, path));
		}
	}

	public class StoreData
	{
		private GridViewItemCollection<ProfileViewModel> _Collection = new GridViewItemCollection<ProfileViewModel>();
		public GridViewItemCollection<ProfileViewModel> Collection
		{
			get
			{
				return this._Collection;
			}
		}


		public StoreData()
		{
			ProfileViewModel item;
			Uri _baseUri = new Uri("ms-appx:///Assets/image/");
			item = new ProfileViewModel();
			item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.HEART_RATE;
			item.PageId = ToolboxIdentifications.PageId.HEART_RATE;
			item.SetLocalImage(_baseUri, "ic_hrs_feature.png");
			_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.GLUCOSE;
			//item.SetLocalImage(_baseUri, "ic_glucose_feature.png");
			//item.PageId = ToolboxIdentifications.PageId.GLUCOSE;
			//_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.BLOOD_PRESSURE;
			//item.PageId = ToolboxIdentifications.PageId.BLOOD_PRESSURE;
			//item.SetLocalImage(_baseUri, "ic_bpm_feature.png");
			//_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.CYCLING_SPEED_AND_CADENCE;
			//item.PageId = ToolboxIdentifications.PageId.CYCLING_SPEED_AND_CADENCE;
			//item.SetLocalImage(_baseUri, "ic_csc_feature.png");
			//_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.DEVICE_FIRMWARE_UPDATE;
			//item.PageId = ToolboxIdentifications.PageId.DEVICE_FIRMWARE_UPDATE;
			//item.SetLocalImage(_baseUri, "ic_dfu_feature.png");
			//_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.HEALTH_THERMOMETER;
			//item.PageId = ToolboxIdentifications.PageId.HEALTH_THERMOMETER;
			//item.SetLocalImage(_baseUri, "ic_hts_feature.png");
			//_Collection.Add(item);

			item = new ProfileViewModel();
			item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.PROXIMITY;
			item.PageId = ToolboxIdentifications.PageId.PROXIMITY;
			item.SetLocalImage(_baseUri, "ic_proximity_feature.png");
			_Collection.Add(item);

			//item = new ProfileViewModel();
			//item.Title = ToolboxIdentifications.ToolboxProfilesShortNames.RUNNING_SPEED_AND_CADENCE;
			//item.PageId = ToolboxIdentifications.PageId.RUNNING_SPEED_AND_CADENCE;
			//item.SetLocalImage(_baseUri, "ic_rsc_feature.png");
			//_Collection.Add(item);
		}
	}
}
