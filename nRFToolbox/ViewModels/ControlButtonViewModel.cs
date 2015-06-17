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
