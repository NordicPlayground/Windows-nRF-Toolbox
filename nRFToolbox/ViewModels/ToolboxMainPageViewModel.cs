using Common.Service;
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
using Windows.UI.Popups;

namespace nRFToolbox.ViewModels
{
	public class ToolboxMainPageViewModel : ViewModelBase
	{

		public string PageId = ToolboxIdentifications.PageId.MAIN_PAGE;
		public void ShowLeavingToolboxMessage()
		{
			var alternative1 = new UICommand("Leave", new UICommandInvokedHandler(LeaveToolboxClicked), 0);
			var alternative2 = new UICommand("Stay", new UICommandInvokedHandler(StayToolboxClicked), 1);
			ShowMessage(LeavingToolboxMessageTitle, LeavingToolboxMessageContent, alternative1, alternative2);
		}

		private void StayToolboxClicked(IUICommand command)
		{
			if (LeaveOrStayHandler != null)
				LeaveOrStayHandler(true);
		}

		private void LeaveToolboxClicked(IUICommand command)
		{
			if (LeaveOrStayHandler != null)
				LeaveOrStayHandler(false);
		}
		public override void ShowMessage(string title, string content, UICommand alternative1, UICommand alternative2)
		{
			base.ShowMessage(title, content, alternative1, alternative2);
		}

		public string LeavingToolboxMessageTitle = "Leave Toolbox";
		public string LeavingToolboxMessageContent = "You are about to leave Toolbox";

		#region Event
		public delegate void StayOrLeaveToolboxClickedHandler(bool choice);
		public event StayOrLeaveToolboxClickedHandler LeaveOrStayHandler;
		#endregion
	}
}
