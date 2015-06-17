using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nRFToolbox.ViewModels
{
	public class UartDialogViewModel : ViewModelBase
	{
		private string message = string.Empty;
		public string Message
		{
			get
			{
				return this.message;
			}

			set
			{
				if (this.message != value)
				{
					this.message = value;
					this.OnPropertyChanged("Message");
				}
			}
		}

		private string timeStamp = string.Empty;
		public string TimeStamp
		{
			get
			{
				return this.timeStamp;
			}

			set
			{
				if (this.timeStamp != value)
				{
					this.timeStamp = value;
					this.OnPropertyChanged("TimeStamp");
				}
			}
		}

		private bool isUserSend;
		public bool IsUserSend
		{
			get
			{
				return this.isUserSend;
			}

			set
			{
				if (this.isUserSend != value)
				{
					this.isUserSend = value;
					this.OnPropertyChanged("IsUserSend");
				}
			}
		}

		private string messageBackground;
		public string MessageBackground
		{
			get
			{
				return this.messageBackground;
			}

			set
			{
				if (this.messageBackground != value)
				{
					this.messageBackground = value;
					this.OnPropertyChanged("MessageBackground");
				}
			}
		}


		private string textForeground;
		public string TextForeground
		{
			get
			{
				return this.textForeground;
			}

			set
			{
				if (this.textForeground != value)
				{
					this.textForeground = value;
					this.OnPropertyChanged("TextForeground");
				}
			}
		}

		private string messageMargin;
		public string MessageMargin
		{
			get
			{
				return this.messageMargin;
			}

			set
			{
				if (this.messageMargin != value)
				{
					this.messageMargin = value;
					this.OnPropertyChanged("MessageMargin");
				}
			}
		}

		public UartDialogViewModel(string message, bool isSendByUser)
		{
			this.Message = message;
			this.IsUserSend = isSendByUser;
			if (isUserSend)
			{
				this.MessageBackground = "Transparent";
				//this.MessageMargin = "150,0,0,0";
				this.MessageMargin = "0,0,0,0";
				this.TextForeground = "Green";
			}
			else
			{
				this.MessageBackground = "Transparent";
				//this.MessageMargin = "0,0,150,0";
				this.MessageMargin = "0,0,0,0";
				this.TextForeground = "#DC582A";
			}
			StringBuilder timeStamp = new StringBuilder();
			var hour = DateTime.Now.Hour.ToString();
			var minute = DateTime.Now.Minute.ToString();
			var second = DateTime.Now.Second.ToString();
			var millisecond = DateTime.Now.Millisecond.ToString();
			timeStamp.AppendFormat("{0}:{1}:{2}:{3}", hour, minute, second, millisecond);
			this.timeStamp = timeStamp.ToString();
		}
	}
}
