using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.App;

namespace WildNatureExplorer.Android;

[Service(
	Name = "com.companyname.wildnatureexplorer.android.DangerMonitoringService",
	Exported = false)]
public class DangerMonitoringService : Service
{
	private const string ChannelId = "danger_monitoring_channel";
	private const string OngoingNotificationChannelId = "danger_monitoring_ongoing";
	private const int OngoingNotificationId = 1001;

	private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);
	private static readonly TimeSpan CriticalCooldown = TimeSpan.FromMinutes(30);

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private CancellationTokenSource? _cts;
	private Task? _loopTask;
	private HttpClient? _http;
	private Guid _countryId;

	private string BackendBaseUrl
	{
		get
		{
		// Prefer backend_base_url if provided.
		// For ngrok "single tunnel" tests, backend_base_url can be blank and we will
		// reuse the WebView origin (ngrok frontend) which proxies `/api` to the backend.
		var backend = Preferences.Default.Get("backend_base_url", string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(backend))
		{
			backend = Preferences.Default.Get("web_base_url", "http://localhost:5173/").Trim();
		}
		return backend.TrimEnd('/');
		}
	}

	private string GetLastNotifiedKey()
		=> Preferences.Default.Get("last_notified_key", string.Empty);

	private void SetLastNotifiedKey(string key)
		=> Preferences.Default.Set("last_notified_key", key);

	private long GetLastNotifiedAtTicks()
		=> Preferences.Default.Get("last_notified_at_ticks", 0L);

	private void SetLastNotifiedAtTicks(long ticks)
		=> Preferences.Default.Set("last_notified_at_ticks", ticks);

	public static void Start(Context context, Guid countryId)
	{
		var intent = new Intent(context, typeof(DangerMonitoringService));
		intent.PutExtra("enabled", true);
		intent.PutExtra("country_id", countryId.ToString());

		if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
		{
			context.StartForegroundService(intent);
		}
		else
		{
			context.StartService(intent);
		}
	}

	public static void Stop(Context context)
	{
		var intent = new Intent(context, typeof(DangerMonitoringService));
		intent.PutExtra("enabled", false);
		context.StartService(intent);
	}

	public override IBinder? OnBind(Intent intent) => null;

	public override void OnCreate()
	{
		base.OnCreate();
		CreateNotificationChannels();
		_http = new HttpClient();
	}

	public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
	{
		intent ??= new Intent();

		var enabled = intent.GetBooleanExtra("enabled", false);
		var countryIdStr = intent.GetStringExtra("country_id");

		if (!enabled)
		{
			StopLoop();
			StopSelf();
			return StartCommandResult.Sticky;
		}

		if (string.IsNullOrWhiteSpace(countryIdStr) || !Guid.TryParse(countryIdStr, out var parsed))
		{
			StopSelf();
			return StartCommandResult.Sticky;
		}

		_countryId = parsed;

		EnsureLoop();
		return StartCommandResult.Sticky;
	}

	private void EnsureLoop()
	{
		if (_cts != null) return;

		_cts = new CancellationTokenSource();

		var ongoingNotification = BuildOngoingNotification("Danger monitoring active");
		StartForeground(OngoingNotificationId, ongoingNotification);

		_loopTask = Task.Run(() => LoopAsync(_cts.Token), _cts.Token);
	}

	private void StopLoop()
	{
		try
		{
			_cts?.Cancel();
		}
		catch { /* ignore */ }

		_cts = null;
	}

	private async Task LoopAsync(CancellationToken token)
	{
		var logTag = "DangerMonitoringService";

		while (!token.IsCancellationRequested)
		{
			try
			{
				var location = await Geolocation.GetLocationAsync(new GeolocationRequest
				{
					DesiredAccuracy = GeolocationAccuracy.High,
					Timeout = TimeSpan.FromSeconds(30)
				});

				if (location != null)
				{
					await CheckProximityAndNotifyAsync(location.Latitude, location.Longitude, token);
				}
			}
			catch (Exception ex)
			{
				Log.Warn(logTag, $"Loop failed: {ex.Message}");
			}

			try
			{
				await Task.Delay(PollInterval, token);
			}
			catch (TaskCanceledException)
			{
				break;
			}
		}
	}

	private async Task CheckProximityAndNotifyAsync(double lat, double lon, CancellationToken token)
	{
		if (_http == null) return;

		var url = $"{BackendBaseUrl}/api/map/proximity-check";

		var body = new
		{
			countryId = _countryId,
			userLatitude = lat,
			userLongitude = lon
		};

		using var content = new StringContent(
			JsonSerializer.Serialize(body, JsonOptions),
			Encoding.UTF8,
			"application/json");

		using var res = await _http.PostAsync(url, content, token);
		res.EnsureSuccessStatusCode();

		var json = await res.Content.ReadAsStringAsync(token);
		var parsed = JsonSerializer.Deserialize<ProximityCheckResponse>(json, JsonOptions);
		if (parsed == null) return;

		if (parsed.AlertCount <= 0 || parsed.Alerts == null || parsed.Alerts.Count == 0) return;

		var critical = parsed.Alerts
			.FirstOrDefault(a => string.Equals(a.Warning, "CRITICAL", StringComparison.OrdinalIgnoreCase));

		if (critical == null) return;

		var key = critical.SpeciesId?.ToString() ?? critical.CommonName ?? "unknown";
		var lastKey = GetLastNotifiedKey();
		var lastAtTicks = GetLastNotifiedAtTicks();
		var now = DateTime.UtcNow;

		if (!string.IsNullOrEmpty(lastKey) &&
		    lastKey == key &&
		    lastAtTicks > 0 &&
		    new DateTime(lastAtTicks, DateTimeKind.Utc) + CriticalCooldown > now)
		{
			return; // avoid spamming the same alert
		}

		SetLastNotifiedKey(key);
		SetLastNotifiedAtTicks(now.Ticks);

		ShowCriticalNotification(critical);
	}

	private void ShowCriticalNotification(Alert alert)
	{
		var warningText = alert.CommonName ?? "Unknown animal";
		var distanceText = alert.DistanceKm.HasValue ? $"{alert.DistanceKm.Value:F1} km" : "nearby";

		var title = "Danger zone detected";
		var message = $"{warningText} is nearby ({distanceText}). Stay safe.";

		var builder = new Notification.Builder(this, ChannelId)
			.SetContentTitle(title)
			.SetContentText(message)
			.SetSmallIcon(Resource.Mipmap.appicon)
			.SetAutoCancel(true)
			.SetCategory(Notification.CategoryAlarm);

		var mgr = (NotificationManager)GetSystemService(NotificationService)!;
		mgr.Notify(alert.NotificationId, builder.Build());
	}

	private Notification BuildOngoingNotification(string text)
	{
		// Ongoing notification required for a foreground service.
		return new Notification.Builder(this, OngoingNotificationChannelId)
			.SetContentTitle("WildNatureExplorer")
			.SetContentText(text)
			.SetSmallIcon(Resource.Mipmap.appicon)
			.SetOngoing(true)
			.Build();
	}

	private void CreateNotificationChannels()
	{
		if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

		var channel = new NotificationChannel(ChannelId, "Danger alerts", NotificationImportance.High)
		{
			Description = "Notifications when the user is in or near a danger zone."
		};

		var ongoing = new NotificationChannel(OngoingNotificationChannelId, "Danger monitoring", NotificationImportance.Low)
		{
			Description = "Foreground service notification for background danger monitoring."
		};

		var mgr = (NotificationManager)GetSystemService(NotificationService)!;
		mgr.CreateNotificationChannel(channel);
		mgr.CreateNotificationChannel(ongoing);
	}

	// DTO for /api/map/proximity-check
	private sealed class ProximityCheckResponse
	{
		[JsonPropertyName("alertCount")]
		public int AlertCount { get; set; }

		[JsonPropertyName("alerts")]
		public List<Alert>? Alerts { get; set; }
	}

	private sealed class Alert
	{
		[JsonPropertyName("speciesId")]
		public Guid? SpeciesId { get; set; }

		[JsonPropertyName("commonName")]
		public string? CommonName { get; set; }

		[JsonPropertyName("warning")]
		public string? Warning { get; set; }

		[JsonPropertyName("distanceKm")]
		public double? DistanceKm { get; set; }

		[JsonIgnore]
		public int NotificationId => SpeciesId.HasValue ? SpeciesId.Value.GetHashCode() : CommonName?.GetHashCode() ?? 9999;
	}

	[Obsolete("Stop the foreground service by disabling Danger mode from UI/bridge.")]
	public override void OnDestroy()
	{
		StopLoop();
		base.OnDestroy();
	}
}

