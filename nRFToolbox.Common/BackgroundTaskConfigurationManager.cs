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
using Windows.ApplicationModel.Background;
using Windows.Foundation;

//namespace nRFToolboxBackground
//{
//	 public sealed class BackgroundTaskConfigurationManager :IBackgroundTask
//	 {
//		 public void Run(IBackgroundTaskInstance taskInstance)
//		 {
//			 BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

//		 }     
//		 public IAsyncOperation<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition) 
//		 {
//			 return this.SignBackgroundTask(taskEntryPoint, name, trigger, condition).AsAsyncOperation();
//		 }

//		 private async Task<BackgroundTaskRegistration> SignBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
//		 {
//			 if (TaskRequiresBackgroundAccess(name))
//			 {
//				 await BackgroundExecutionManager.RequestAccessAsync();
//			 }

//			 foreach (var cur in BackgroundTaskRegistration.AllTasks)
//			 {

//				 if (cur.Value.Name == name)
//				 {
//					 // 
//					 // The task is already registered.
//					 // 

//					 return (BackgroundTaskRegistration)(cur.Value);
//				 }
//			 }

//			 var builder = new BackgroundTaskBuilder();

//			 builder.Name = name;
//			 builder.TaskEntryPoint = taskEntryPoint;
//			 if(trigger != null)
//			 builder.SetTrigger(trigger);

//			 if (condition != null)
//			 {
//				 builder.AddCondition(condition);

//				 //
//				 // If the condition changes while the background task is executing then it will
//				 // be canceled.
//				 //
//				 builder.CancelOnConditionLoss = true;
//			 }

//			 BackgroundTaskRegistration task = builder.Register();

//			 //
//			 // Remove previous completion status from local settings.
//			 //
//			 //var settings = ApplicationData.Current.LocalSettings;
//			 //settings.Values.Remove(name);
//			 task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
//			 task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
//			 return task;
//		 }

//		 private void OnCompleted(Windows.ApplicationModel.Background.BackgroundTaskRegistration sender, Windows.ApplicationModel.Background.BackgroundTaskCompletedEventArgs args)
//		 {
//			 //
//		 }

//		 private void OnProgress(Windows.ApplicationModel.Background.BackgroundTaskRegistration sender, Windows.ApplicationModel.Background.BackgroundTaskProgressEventArgs args)
//		 {
//			 //
//		 }

//		 public void UnRegisterHeartRateMonitorBackgroundTask()
//		 {
//			 foreach (var cur in BackgroundTaskRegistration.AllTasks)
//			 {
//				 if (cur.Value.Name == "HearRateMonitor")
//				 {
//					 cur.Value.Unregister(true);
//				 }
//			 }
//		 }

//		 private static bool TaskRequiresBackgroundAccess(String name)
//		 {
//#if WINDOWS_PHONE_APP
//			 return true;
//#else
//				if (name == TimeTriggeredTaskName)
//				{
//					 return true;
//				}
//				else
//				{
//					 return false;
//				}
//#endif
//		 }
//	 }
//}
