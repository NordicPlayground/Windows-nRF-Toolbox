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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace nRFToolbox.ViewModels
{
	public class ControlButtonViewModel : ViewModelBase
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

		private string buttonId = null;
		public string ButtonId
		{
			get
			{
				return buttonId;
			}
			set
			{
				if (this.buttonId != value)
				{
					this.buttonId = value;
				}
			}
		}

		private string positionId = null;
		public string PositionId
		{
			get
			{
				return positionId;
			}
			set
			{
				if (this.positionId != value)
				{
					this.positionId = value;
				}
			}
		}

		private string background = null;
		public string Background
		{
			get
			{
				return background;
			}
			set
			{
				if (this.background != value)
				{
					this.background = value;
					this.OnPropertyChanged("Background");
				}
			}
		}

		public void SetLocalImage(Uri baseUri, String path)
		{
			Image = new BitmapImage(new Uri(baseUri, path));
		}
	}
}
