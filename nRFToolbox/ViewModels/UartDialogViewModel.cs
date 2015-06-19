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
