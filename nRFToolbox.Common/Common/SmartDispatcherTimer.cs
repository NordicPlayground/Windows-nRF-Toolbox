using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NRFToolbox.Common.Common
{
	public class SmartDispatcherTimer : DispatcherTimer
	{
		public bool IsReentrant { get; set; }
		public bool IsRunning { get; private set; }

		public Action TickTask { get; set; }

		public SmartDispatcherTimer()
		{
			base.Tick += SmartDispatcherTimer_Tick;
		}

		private void SmartDispatcherTimer_Tick(object sender, object e)
		{
			if (TickTask == null)
			{
				//Debug.WriteLine("No task set!");
				return;
			}

			if (IsRunning && !IsReentrant)
			{
				// previous task hasn't completed
				//Debug.WriteLine("Task already running");
				return;
			}

			try
			{
				// we're running it now
				IsRunning = true;
				//Debug.WriteLine("Running Task");
				 TickTask.Invoke();
				//Debug.WriteLine("Task Completed");
			}
			catch (Exception)
			{
				//Debug.WriteLine("Task Failed");
			}
			finally
			{
				// allow it to run again
				IsRunning = false;
			}
		}

	}
}
