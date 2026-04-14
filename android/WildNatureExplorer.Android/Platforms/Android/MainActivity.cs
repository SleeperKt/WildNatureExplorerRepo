using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

namespace WildNatureExplorer.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	private const int NotificationsRequestCode = 5123;
	private static TaskCompletionSource<bool>? _notificationsPermissionTcs;

	public static Task<bool> RequestPostNotificationsPermissionAsync()
	{
		#if ANDROID
		if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Tiramisu)
			return Task.FromResult(true);

		var activity = Platform.CurrentActivity as MainActivity
		               ?? throw new InvalidOperationException("MainActivity not available.");

		if (activity.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) ==
		    Permission.Denied)
		{
			_notificationsPermissionTcs = new TaskCompletionSource<bool>();

			activity.RequestPermissions(
				new[] { global::Android.Manifest.Permission.PostNotifications },
				NotificationsRequestCode);

			return _notificationsPermissionTcs.Task;
		}

		return Task.FromResult(true);
		#else
		return Task.FromResult(true);
		#endif
	}

	public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
	{
		base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		if (requestCode != NotificationsRequestCode) return;

		if (_notificationsPermissionTcs == null) return;

		var granted = grantResults.Length > 0 && grantResults[0] == Permission.Granted;
		_notificationsPermissionTcs.TrySetResult(granted);
		_notificationsPermissionTcs = null;
	}
}
