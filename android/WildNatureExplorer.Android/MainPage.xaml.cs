using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace WildNatureExplorer.Android;

public partial class MainPage : ContentPage
{
	private bool _bridgeAdded;

	public MainPage()
	{
		InitializeComponent();

		AppWebView.IsVisible = false;
        SetupPanel.IsVisible = true;

		// Load URLs for ngrok testing (or local dev).
		WebUrlEntry.Text = Preferences.Default.Get("web_base_url", "http://localhost:5173/");
		ApiUrlEntry.Text = Preferences.Default.Get("backend_base_url", "http://localhost:5000");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await RefreshPermissionStatusAsync();
		TryEnableWebView();
	}

	private async void OnRequestPermissionsClicked(object? sender, EventArgs e)
	{
		// Persist URLs so both WebView and background service use the same endpoints.
		Preferences.Default.Set("web_base_url", WebUrlEntry.Text?.Trim() ?? string.Empty);
		Preferences.Default.Set("backend_base_url", ApiUrlEntry.Text?.Trim() ?? string.Empty);

		await RequestAllPermissionsAsync();
		await RefreshPermissionStatusAsync();
		TryEnableWebView();
	}

	private void TryEnableWebView()
	{
		var allGranted = IsNotificationsGranted() && IsLocationGranted() && IsBackgroundLocationGranted();
		if (!allGranted) return;

		if (AppWebView.Source == null)
		{
			var webBaseUrl = Preferences.Default.Get("web_base_url", string.Empty);
			if (string.IsNullOrWhiteSpace(webBaseUrl)) return;
			AppWebView.Source = webBaseUrl.EndsWith("/") ? webBaseUrl : webBaseUrl + "/";
		}

		AppWebView.IsVisible = true;
        SetupPanel.IsVisible = false;
	}

	private bool IsNotificationsGranted()
	{
		#if ANDROID
		if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Tiramisu)
			return true;

		return global::Android.App.Application.Context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications)
		       == global::Android.Content.PM.Permission.Granted;
		#else
		return true;
		#endif
	}

	private bool IsLocationGranted()
	{
		return Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>()
			.ConfigureAwait(false).GetAwaiter().GetResult() == PermissionStatus.Granted;
	}

	private bool IsBackgroundLocationGranted()
	{
		return Permissions.CheckStatusAsync<Permissions.LocationAlways>()
			.ConfigureAwait(false).GetAwaiter().GetResult() == PermissionStatus.Granted;
	}

	private async Task RefreshPermissionStatusAsync()
	{
		var locInUse = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
		var locAlways = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

		NotificationsStatusLabel.Text = $"Notifications: {(IsNotificationsGranted() ? "Granted" : "Denied")}";
		LocationStatusLabel.Text = $"Location: {locInUse}";
		BackgroundLocationStatusLabel.Text = $"Background location: {locAlways}";
	}

	private async Task RequestAllPermissionsAsync()
	{
		#if ANDROID
		// Android 13+ requires POST_NOTIFICATIONS runtime permission for showing local alerts.
		if (!IsNotificationsGranted())
			await MainActivity.RequestPostNotificationsPermissionAsync();
		#endif

		await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		await Permissions.RequestAsync<Permissions.LocationAlways>();
	}

	private void OnUrlTextChanged(object? sender, TextChangedEventArgs e)
	{
		// Intentionally no-op: we persist URLs when the user clicks the permission button.
	}

	private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
	{
		if (_bridgeAdded) return;

#if ANDROID
		// Attach a JS bridge so the React "Danger mode" toggle can start/stop
		// native background monitoring.
		if (AppWebView.Handler?.PlatformView is global::Android.Webkit.WebView nativeWeb)
		{
			nativeWeb.Settings.JavaScriptEnabled = true;
			nativeWeb.AddJavascriptInterface(
				new DangerJsBridge(global::Android.App.Application.Context),
				"AndroidBridge");
			_bridgeAdded = true;
		}
#endif
		await Task.CompletedTask;
	}

#if ANDROID
	private sealed class DangerJsBridge : global::Java.Lang.Object
	{
		private readonly global::Android.Content.Context _context;

		public DangerJsBridge(global::Android.Content.Context context)
		{
			_context = context;
		}

		[global::Android.Webkit.JavascriptInterface]
		public void setDangerModeEnabled(bool enabled, string countryId)
		{
			if (enabled)
			{
				if (Guid.TryParse(countryId, out var guid))
				{
					DangerMonitoringService.Start(_context, guid);
				}
			}
			else
			{
				DangerMonitoringService.Stop(_context);
			}
		}
	}
#endif
}
