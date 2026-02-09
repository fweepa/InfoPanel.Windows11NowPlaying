using InfoPanel.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Windows11NowPlayingPlugin
{
    /// <summary>
    /// Plugin for displaying Windows 11 Now Playing information in InfoPanel
    /// </summary>
    public class Windows11NowPlayingPlugin : BasePlugin
    {
        // Data items for Now Playing information
        private readonly PluginText _title = new("title", "Title", "-");
        private readonly PluginText _artist = new("artist", "Artist", "-");
        private readonly PluginText _album = new("album", "Album", "-");
        private readonly PluginText _appName = new("app-name", "Application", "-");
        private readonly PluginSensor _position = new("position", "Position", 0f, "s");
        private readonly PluginSensor _duration = new("duration", "Duration", 0f, "s");
        private readonly PluginSensor _percentage = new("percentage", "Percentage", 0f, "%");
        private readonly PluginSensor _volume = new("volume", "Volume", 0f, "%");
        private readonly PluginText _status = new("status", "Status", "Stopped");

        public Windows11NowPlayingPlugin() 
            : base("windows11-now-playing", "Windows 11 Now Playing", "Displays currently playing media information from Windows 11")
        {
        }

        public override string? ConfigFilePath => null; // No configuration file needed initially

        public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(1); // Update every second

        private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;

        // Position extrapolation state - for smooth position updates between API polls when Playing
        private double _lastKnownPositionSeconds;
        private DateTime _lastPositionUpdateUtc;
        private double _lastKnownDurationSeconds;
        private bool _hasValidPositionForExtrapolation;

        public override void Initialize()
        {
            try
            {
                // Initialize Windows 11 media session manager
                _sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // If initialization fails, _sessionManager will remain null
                // UpdateAsync will handle this gracefully
                _sessionManager = null;
            }
        }

        public override void Load(List<IPluginContainer> containers)
        {
            // Create the main container for Now Playing data
            var container = new PluginContainer("now-playing", "Now Playing");
            
            // Add all data items to the container
            container.Entries.Add(_title);
            container.Entries.Add(_artist);
            container.Entries.Add(_album);
            container.Entries.Add(_appName);
            container.Entries.Add(_position);
            container.Entries.Add(_duration);
            container.Entries.Add(_percentage);
            container.Entries.Add(_volume);
            container.Entries.Add(_status);
            
            containers.Add(container);
        }

        public override void Update()
        {
            // Synchronous update method - not used, we use UpdateAsync instead
            throw new NotImplementedException();
        }

        public override async Task UpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Check if session manager is available
                if (_sessionManager == null)
                {
                    // Try to initialize if not already done
                    try
                    {
                        _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    }
                    catch
                    {
                        // If we can't get the session manager, set all values to defaults
                        ResetToDefaults();
                        _status.Value = "No Media Session Available";
                        return;
                    }
                }

                // Get all active media sessions
                var sessions = _sessionManager.GetSessions();
                
                if (sessions.Count == 0)
                {
                    // No active media sessions
                    ResetToDefaults();
                    _status.Value = "No Media Playing";
                    return;
                }

                // Get the current media session (first active session)
                // In most cases, the first session is the currently playing one
                var currentSession = sessions.FirstOrDefault(s =>
                {
                    try
                    {
                        var playbackInfo = s.GetPlaybackInfo();
                        return playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing ||
                               playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused;
                    }
                    catch
                    {
                        return false;
                    }
                }) ?? sessions[0]; // Fallback to first session if no playing/paused session found

                // Retrieve media properties
                GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties = null;
                try
                {
                    mediaProperties = await currentSession.TryGetMediaPropertiesAsync();
                }
                catch
                {
                    // If we can't get media properties, continue with defaults
                }

                // Retrieve playback information
                GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo = null;
                try
                {
                    playbackInfo = currentSession.GetPlaybackInfo();
                }
                catch
                {
                    // If we can't get playback info, continue with defaults
                }

                // Retrieve playback controls for position/duration
                GlobalSystemMediaTransportControlsSessionTimelineProperties? timelineProperties = null;
                try
                {
                    var mediaPropertiesControl = currentSession.GetTimelineProperties();
                    timelineProperties = mediaPropertiesControl;
                }
                catch
                {
                    // If we can't get timeline properties, continue without them
                }

                // Update plugin data items - use IsNullOrEmpty for album since many apps (Spotify, etc.) don't provide it via GMTC
                _title.Value = mediaProperties?.Title ?? "-";
                _artist.Value = mediaProperties?.Artist ?? "-";
                _album.Value = string.IsNullOrEmpty(mediaProperties?.AlbumTitle) ? "-" : mediaProperties.AlbumTitle;
                // Note: GMTC only exposes SourceAppUserModelId (browser name, e.g. "Chrome"). Web app/site name (YouTube Music, Spotify, etc.) is not available from the Windows API.
                _appName.Value = currentSession?.SourceAppUserModelId ?? "-";

                // Update playback status
                if (playbackInfo != null)
                {
                    _status.Value = playbackInfo.PlaybackStatus switch
                    {
                        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => "Playing",
                        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => "Paused",
                        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped => "Stopped",
                        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed => "Closed",
                        _ => "Unknown"
                    };
                }
                else
                {
                    _status.Value = "Unknown";
                }

                // Update position and duration
                if (timelineProperties != null)
                {
                    var position = timelineProperties.Position;
                    var duration = timelineProperties.EndTime - timelineProperties.StartTime;
                    var durationSeconds = duration.TotalSeconds;
                    var positionSeconds = position.TotalSeconds;

                    _duration.Value = (float)durationSeconds;

                    var isPlaying = playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

                    if (isPlaying)
                    {
                        // Only update stored position when API returns meaningfully different value
                        // (media app pushed update). Same value = stale, so we extrapolate from previous.
                        const double positionChangeThreshold = 0.5;
                        var positionChanged = !_hasValidPositionForExtrapolation ||
                            Math.Abs(positionSeconds - _lastKnownPositionSeconds) > positionChangeThreshold;

                        if (positionChanged)
                        {
                            _lastKnownPositionSeconds = positionSeconds;
                            _lastPositionUpdateUtc = DateTime.UtcNow;
                            _hasValidPositionForExtrapolation = true;
                        }
                        _lastKnownDurationSeconds = durationSeconds;

                        // Extrapolate: assume playback continued at 1x since we last got a real update
                        var elapsedSinceUpdate = (DateTime.UtcNow - _lastPositionUpdateUtc).TotalSeconds;
                        var extrapolatedPosition = _lastKnownPositionSeconds + elapsedSinceUpdate;

                        // Clamp to [0, duration] to avoid showing position beyond end of track
                        var displayPosition = Math.Clamp(extrapolatedPosition, 0, _lastKnownDurationSeconds);
                        _position.Value = (float)displayPosition;
                    }
                    else
                    {
                        // Paused/Stopped: use raw API position, no extrapolation
                        _hasValidPositionForExtrapolation = false;
                        _position.Value = (float)positionSeconds;
                    }
                }
                else
                {
                    _position.Value = 0f;
                    _duration.Value = 0f;
                    _hasValidPositionForExtrapolation = false;
                }

                // Update volume (if available)
                // Note: Volume is typically not available through GMTC API
                // This would need to be retrieved through other Windows APIs if needed
                _volume.Value = 0f; // Placeholder - GMTC doesn't provide volume info

                // Update percentage (0-100) for gauge display
                _percentage.Value = _duration.Value > 0 ? (float)((_position.Value / _duration.Value) * 100) : 0f;
            }
            catch (Exception ex)
            {
                // Log error and set status to indicate error
                ResetToDefaults();
                _status.Value = $"Error: {ex.Message}";
            }
        }

        private void ResetToDefaults()
        {
            _title.Value = "-";
            _artist.Value = "-";
            _album.Value = "-";
            _appName.Value = "-";
            _position.Value = 0f;
            _duration.Value = 0f;
            _percentage.Value = 0f;
            _volume.Value = 0f;
            _hasValidPositionForExtrapolation = false;
        }

        public override void Close()
        {
            // Clean up any resources
            // Windows Runtime objects don't need explicit disposal
            _sessionManager = null;
        }
    }
}
