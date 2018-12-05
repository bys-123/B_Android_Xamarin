using System;

using Android.App;
using Android.OS;
using Android.Runtime;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Plugin.CurrentActivity;
using Com.Nostra13.Universalimageloader.Core;
using Microsoft.AppCenter.Distribute;
using Android.Widget;
using System.Text;
using System.Collections.Generic;

namespace MonkeysApp
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }
        // Activity mcontext;
        public override void OnCreate()
        {
            base.OnCreate();

            Crashes.GetErrorAttachments = GetErrorAttachments;
            // Distribute.ReleaseAvailable = OnReleaseAvailable;
            // This should come before MobileCenter.Start() is called
            Push.PushNotificationReceived += (sender, e) =>
            {

                // Add the notification message and title to the message
                var summary = $"Push notification received:" +
                                    $"\n\tNotification title: {e.Title}" +
                                    $"\n\tMessage: {e.Message}";

                // If there is custom data associated with the notification,
                // print the entries
                if (e.CustomData != null)
                {
                    summary += "\n\tCustom data:\n";
                    foreach (var key in e.CustomData.Keys)
                    {
                        summary += $"\t\t{key} : {e.CustomData[key]}\n";
                    }
                }

                // Send the notification summary to debug output
                //System.Diagnostics.Debug.WriteLine(summary);
                Toast.MakeText(this, summary, ToastLength.Short).Show();
            };
            AppCenter.Start("6b55d6ce-52b2-4255-a86f-1dd6a3eb4bad",
                    typeof(Analytics), typeof(Crashes), typeof(Push), typeof(Distribute));
            var config = ImageLoaderConfiguration.CreateDefault(ApplicationContext);
            // Initialize ImageLoader with configuration.
            ImageLoader.Instance.Init(config);
            RegisterActivityLifecycleCallbacks(this);
            Analytics.TrackEvent("sessionStart", new Dictionary<string, string> {
                { "hi", "Music" },
                { "123", "sdfhkwejfwefjsdcnjSALFS1213"}
            });
            var installId = AppCenter.GetInstallIdAsync().Result.ToString();
            System.Diagnostics.Debug.WriteLine("InstallId=" + installId);
            CustomProperties properties = new CustomProperties();
            properties.Set("color", "red2").Set("score", 101).Set("now", DateTime.UtcNow);
            AppCenter.SetCustomProperties(properties);
            try
            {
                int a = 0;
                int b = 1 / a;
                Crashes.GenerateTestCrash();
            }
            catch (Exception exception)
            {
                var properties2 = new Dictionary<string, string> {
                    { "Category", "Music" },
                    { "Wifi", "On" },
                    { "add1", "property1"},
                    { "add2", "property2"}
                };
                Crashes.TrackError(exception, properties2);
            }
            //try
            //{
            //    Crashes.GenerateTestCrash();
            //}
            //catch (Exception exception)
            //{
            //    var properties3 = new Dictionary<string, string> {
            //        { "Appcenter", "generate crash" },
            //    };
            //    Crashes.TrackError(exception, properties3);
            //}
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
            Crashes.ShouldAwaitUserConfirmation = () =>
            {
                var builder = new AlertDialog.Builder(activity);
                builder.SetTitle("Crash detected. Send anonymous crash report?")
               .SetNegativeButton("send", (s, e) =>
               {
                   Crashes.NotifyUserConfirmation(UserConfirmation.Send);
               })
                .SetPositiveButton("Always Send", (s, e) =>
                {
                    Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
                })
                .SetNeutralButton("Don't Send", (s, e) =>
                {
                    Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
                });
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();
                return true;
            };

            Crashes.SendingErrorReport += (sender, e) =>
            {
                Toast.MakeText(this, $"senting crash report", ToastLength.Short).Show();
                // Your code, e.g. to present a custom UI.
            };
            Crashes.SentErrorReport += (sender, e) =>
            {
                Toast.MakeText(this, $"sent crash report successfully", ToastLength.Short).Show();
            };
            Crashes.FailedToSendErrorReport += (sender, e) =>
            {
                Toast.MakeText(this, $"failed to send a crash log", ToastLength.Short).Show();
            };
        }

        public void OnActivityStopped(Activity activity)
        {
        }
        IEnumerable<ErrorAttachmentLog> GetErrorAttachments(ErrorReport report)

        {
            return new ErrorAttachmentLog[]

            {
                ErrorAttachmentLog.AttachmentWithText("Hello world!", "hello.txt"),
                ErrorAttachmentLog.AttachmentWithBinary(Encoding.UTF8.GetBytes("Fake image"), "fake_image.jpeg", "image/jpeg")
            };
        }

        //bool OnReleaseAvailable(ReleaseDetails releaseDetails)
        //{
        //    string versionName = releaseDetails.ShortVersion;
        //    string versionCodeOrBuildNumber = releaseDetails.Version;
        //    string releaseNotes = releaseDetails.ReleaseNotes;
        //    Uri releaseNotesUrl = releaseDetails.ReleaseNotesUrl;

        //    // custom dialog
        //    var title = "Version " + versionName + " available!";
        //    var builder = new AlertDialog.Builder(this.mcontext);
        //    builder.SetTitle(title);
        //    builder.SetMessage(releaseNotes);

        //    //On mandatory update, user cannot postpone
        //    if (releaseDetails.MandatoryUpdate)
        //    {
        //        builder.SetNegativeButton("Download and Install", (s, e) =>
        //        {
        //            Distribute.NotifyUpdateAction(UpdateAction.Update);
        //        });

        //    }
        //    else
        //    {
        //        builder.SetNegativeButton("Download and Install", (s, e) =>
        //        {
        //            Distribute.NotifyUpdateAction(UpdateAction.Update);
        //        });
        //        builder.SetNeutralButton("Maybe tomorrow...", (s, e) =>
        //        {
        //            Distribute.NotifyUpdateAction(UpdateAction.Postpone);
        //        });
        //    }
        //    AlertDialog alertDialog = builder.Create();
        //    alertDialog.Show();
        //    // Return true if you are using your own dialog, false otherwise
        //    return true;
        //}

}
}