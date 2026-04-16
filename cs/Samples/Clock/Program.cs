using Microsoft.MixedReality.Volumetric;
using System.Text.Json;

namespace CsClock;

public struct ClockSavedState
{
    public string VolumeRestoreId { get; set; }
    public string CurrentTimeZoneId { get; set; }
}

public class ClockVolume : Volume
{
    private readonly string _modelUri = VolumetricApp.GetAssetUri("Clock.glb");

    private readonly string _adaptiveCardTemplate = """
    {
        "type": "AdaptiveCard",
        "body": [
            {
                "id": "time",
                "type": "TextBlock",
                "text": "${currentTime}",
                "horizontalAlignment": "center"
            },
            {
                "id": "timezone",
                "type": "TextBlock",
                "text": "${timezone}",
                "horizontalAlignment": "center"
            }
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "title": "Timezone -",
                "verb": "dec"
            },
            {
                "type": "Action.Execute",
                "title": "Timezone +",
                "verb": "inc"
            }
        ],
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4"
    }
    """;

    private ModelResource? _model;
    private VisualElement? _clockVisual;
    private AdaptiveCard? _adaptiveCard;

    private VisualElement? _hourHand;
    private VisualElement? _minuteHand;
    private VisualElement? _secondHand;

    private TimeZoneInfo _currentTimeZone;
    private List<TimeZoneInfo> _timeZones;
    private int _currentTimeZoneIndex = -1;

    private ClockSavedState _savedState;

    public ClockVolume(VolumetricApp app, ClockSavedState savedState) :
        base(app, isRestorable: true, restoreId: VaUuid.FromString(savedState.VolumeRestoreId))
    {
        _savedState = savedState;
        _savedState.VolumeRestoreId = RestoreId.ToString();

        _timeZones = GetTimeZones();

        if (!string.IsNullOrWhiteSpace(_savedState.CurrentTimeZoneId))
        {
            _currentTimeZoneIndex = _timeZones.FindIndex(tz => tz.Id == _savedState.CurrentTimeZoneId);

            // If a valid index was found, set _currentTimeZone. If not, we'll default to the local below.
            if (_currentTimeZoneIndex != -1)
            {
                _currentTimeZone = _timeZones[_currentTimeZoneIndex];
            }
        }

        if (_currentTimeZoneIndex == -1)
        {
            _currentTimeZone = TimeZoneInfo.Local;
            _currentTimeZoneIndex = _timeZones.FindIndex(tz => tz.Id == _currentTimeZone.Id);
        }

        _savedState.CurrentTimeZoneId = _currentTimeZone!.Id;

        OnReady += HandleOnReady;
        OnUpdate += (_) => HandleOnUpdate();
        OnClose += (_) => HandleOnClose();
        OnRestoreResult += HandleRestoreResult;

        Container.SetDisplayName("Volumetric Clock");
        Container.SetRotationLock(VaVolumeRotationLockFlags.X | VaVolumeRotationLockFlags.Z);
        Container.SetThumbnailIconUri(VolumetricApp.GetAssetUri("ClockThumbnail.png"));
        Container.SetThumbnailModelUri(VolumetricApp.GetAssetUri("ClockThumbnail.glb"));
    }

    private void HandleOnReady(Volume volume)
    {
        _model = new ModelResource(volume, _modelUri);
        _clockVisual = new VisualElement(volume, _model);
        _hourHand = new VisualElement(volume, _clockVisual, "def_hours");
        _minuteHand = new VisualElement(volume, _clockVisual, "def_minutes");
        _secondHand = new VisualElement(volume, _clockVisual, "def_seconds");

        _adaptiveCard = new AdaptiveCard(volume, _adaptiveCardTemplate, FormatAdaptiveCardData(DateTime.UtcNow.ToLocalTime()));
        _adaptiveCard.ActionInvoked += OnAdaptiveCardActionInvoked;

        Program.SaveState(_savedState);

        RequestUpdateAfter(TimeSpan.FromSeconds(1));    // Request an update every 1 second
    }

    private static float DegreesToRadians(float degrees) => (float)(degrees * Math.PI / 180.0);

    private void HandleOnUpdate()
    {
        if (_clockVisual != null && _secondHand != null && _hourHand != null && _minuteHand != null && _adaptiveCard != null)
        {
            DateTime timeInZone = TimeZoneInfo.ConvertTime(DateTime.UtcNow.ToLocalTime(), _currentTimeZone);
            TimeSpan timeOfDay = timeInZone.TimeOfDay;

            float hourAngle = ((timeOfDay.Hours * 60 + timeOfDay.Minutes) * 0.5f) % 360f;
            float minAngle = ((timeOfDay.Minutes * 60 + timeOfDay.Seconds) * 0.1f) % 360f;
            float secAngle = (timeOfDay.Seconds * 1000 + timeInZone.Millisecond) * 0.006f;

            // The rotation must be consistent with the corresponding gltf node local transform in Glb file.
            // In this case, the clock.glb file contains "def_hours", "def_minutes" and "def_seconds" nodes,
            // and they are rotated 90 degrees around the X axis, and clockwise rotation is around Y axis.
            // One can inspect the gltf node transforms in a gltf viewer, such as https://sandbox.babylonjs.com/
            float XRot = DegreesToRadians(90);
            _hourHand.SetOrientation(VaMath.EulerToQuaternion(XRot, DegreesToRadians(hourAngle), 0));
            _minuteHand.SetOrientation(VaMath.EulerToQuaternion(XRot, DegreesToRadians(minAngle), 0));
            _secondHand.SetOrientation(VaMath.EulerToQuaternion(XRot, DegreesToRadians(secAngle), 0));

            _adaptiveCard.SetData(FormatAdaptiveCardData(timeInZone));

            Console.WriteLine($"Seconds: {(float)FrameState.frameTime * 1e-9f}");
        }

        RequestUpdateAfter(TimeSpan.FromSeconds(1));    // Request an update every 1 second
    }

    private void HandleOnClose()
    {
        _model = null;
        _clockVisual = null;
        _hourHand = null;
        _minuteHand = null;
        _secondHand = null;
        App.RequestExit();
    }

    private void HandleRestoreResult(Volume volume, VaVolumeRestoredResultExt restoreResult)
    {
        if (restoreResult != VaVolumeRestoredResultExt.Success)
        {
            // The restore failed, reset to the local timezone.
            _currentTimeZone = TimeZoneInfo.Local;
            _currentTimeZoneIndex = _timeZones.FindIndex(tz => tz.Id == _currentTimeZone.Id);

            _savedState.CurrentTimeZoneId = _currentTimeZone.Id;
        }
    }

    private void OnAdaptiveCardActionInvoked(object? sender, AdaptiveCard.ActionEventArgs args)
    {
        if (args.Verb == "inc")
        {
            _currentTimeZoneIndex = (_currentTimeZoneIndex + 1) % _timeZones.Count;
        }
        else if (args.Verb == "dec")
        {
            _currentTimeZoneIndex = (_currentTimeZoneIndex - 1 + _timeZones.Count) % _timeZones.Count;
        }

        _currentTimeZone = _timeZones[_currentTimeZoneIndex];

        _savedState.CurrentTimeZoneId = _currentTimeZone.Id;
        Program.SaveState(_savedState);

        // In order to reflect the timezone change we need an update.
        RequestUpdate();
    }

    private string FormatAdaptiveCardData(DateTime time)
    {
        return $$"""
        {
            "currentTime": "{{time:ddd MM/dd/yy H:mm:ss}}",
            "timezone": "(UTC{{_currentTimeZone.BaseUtcOffset}}) {{GetTimezoneDisplay(time)}}",
        }
        """;
    }

    private string GetTimezoneDisplay(DateTime time)
    {
        return _currentTimeZone.SupportsDaylightSavingTime ?
            (_currentTimeZone.IsDaylightSavingTime(time) ? _currentTimeZone.DaylightName : _currentTimeZone.StandardName) // Supports daylight savings, check if the current time is in daylight savings time.
            : _currentTimeZone.StandardName; // Does not support daylight savings.
    }

    private List<TimeZoneInfo> GetTimeZones()
    {
        var windowsTimeZones = new List<string>
        {
            "Hawaiian Standard Time",      // Pacific/Honolulu UTC-10:00
            "Alaskan Standard Time",       // America/Anchorage UTC-09:00
            "Pacific Standard Time",       // America/Los_Angeles UTC-08:00
            "Mountain Standard Time",      // America/Denver UTC-07:00
            "Central Standard Time",       // America/Chicago UTC-06:00
            "Eastern Standard Time",       // America/New_York UTC-05:00
            "Pacific SA Standard Time",    // America/Santiago UTC-04:00
            "Argentina Standard Time",     // America/Buenos_Aires UTC-03:00
            "GMT Standard Time",           // Europe/London UTC±00:00
            "W. Europe Standard Time",     // Europe/Berlin UTC+01:00
            "GTB Standard Time",           // Europe/Athens UTC+02:00
            "Arab Standard Time",          // Asia/Riyadh UTC+03:00
            "Arabian Standard Time",       // Asia/Dubai UTC+04:00
            "Pakistan Standard Time",      // Asia/Karachi UTC+05:00
            "Bangladesh Standard Time",    // Asia/Dhaka UTC+06:00
            "SE Asia Standard Time",       // Asia/Bangkok UTC+07:00
            "China Standard Time",         // Asia/Shanghai UTC+08:00
            "Tokyo Standard Time",         // Asia/Tokyo UTC+09:00
            "AUS Eastern Standard Time",   // Australia/Sydney UTC+10:00
            "New Zealand Standard Time"    // Pacific/Auckland UTC+12:00
        };

        var timeZones = new List<TimeZoneInfo>();

        foreach (var windowsTimeZone in windowsTimeZones)
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(windowsTimeZone, out var timeZone))
            {
                timeZones.Add(timeZone);
            }
        }

        return timeZones;
    }
}

internal sealed class Program
{
    static int Main()
    {
        var app = new VolumetricApp("CsClockSample",
            requiredExtensions: new[] {
                Extensions.VA_EXT_gltf2_model_resource,
                Extensions.VA_EXT_adaptive_card_element,
                Extensions.VA_EXT_volume_restore,
                Extensions.VA_EXT_volume_container_thumbnail,
            },
            volumeRestoreBehavior: VaVolumeRestoreBehaviorExt.ByApp);
        app.OnStart += OnStart;
        app.OnVolumeRestoreIdInvalidated += OnVolumeRestoreIdInvalidated;
        return app.Run();
    }

    private static void OnStart(VolumetricApp app)
    {
        ClockSavedState? savedState = LoadState();

        if (savedState is null)
        {
            savedState = new ClockSavedState
            {
                VolumeRestoreId = VaUuid.Empty.ToString(),
                CurrentTimeZoneId = ""
            };
        }
        else
        {
            Console.WriteLine($"Restoring state for volume {savedState.Value.VolumeRestoreId}");
        }

        _ = new ClockVolume(app, savedState.Value);
    }

    private static void OnVolumeRestoreIdInvalidated(VolumetricApp app, VaUuid volumeRestoreId)
    {
        Console.WriteLine($"OnVolumeRestoreIdInvalidated, deleting state. VolumeRestoreId: {volumeRestoreId}");
        DeleteStateFile();
    }

    private static string StateFilePath => Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA")!, "CsClockSample_state.json");
    private static JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static void SaveState(ClockSavedState state)
    {
        try
        {
            string jsonText = JsonSerializer.Serialize(state, SerializerOptions);
            File.WriteAllText(StateFilePath, jsonText);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error saving state file: {ex.Message}");
        }
    }

    public static ClockSavedState? LoadState()
    {
        if (!File.Exists(StateFilePath))
        {
            return null;
        }

        try
        {
            string jsonText = File.ReadAllText(StateFilePath);

            return JsonSerializer.Deserialize<ClockSavedState>(jsonText);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error loading state file: {ex.Message}");
            File.Delete(StateFilePath);
            return null;
        }
    }

    public static void DeleteStateFile()
    {
        if (File.Exists(StateFilePath))
        {
            try
            {
                File.Delete(StateFilePath);
            }
            catch (IOException e)
            {
                Console.WriteLine($"Error deleting state file: {e.Message}");
            }
        }
    }
}
