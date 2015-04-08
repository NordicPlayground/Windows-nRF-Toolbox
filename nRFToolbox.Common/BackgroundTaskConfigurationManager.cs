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
