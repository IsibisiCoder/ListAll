using MediaInfo;
using Microsoft.Extensions.Logging;

namespace ListAll.Plugin.MediaInfo;

public class MediaInfo : IMediaPlugin
{
    private readonly ILogger _logger;

    public MediaInfo(ILogger<MediaInfo> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get the specified properties of the file
    /// </summary>
    /// <param name="filePath">Fullpathname of the file</param>
    /// <returns>Dictionary with properties and values of the file</returns>
    public Dictionary<string, string> GetProperties(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException($"{nameof(filePath)}");

        Dictionary<string, string> properties = new Dictionary<string, string>();

        try
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"{nameof(filePath)} does not exists!");
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var media = new MediaInfoWrapper(stream, _logger);
                if (media.Success)
                {
                    if (media.HasVideo)
                    {
                        var duration = media.Duration;
                        properties.Add(nameof(duration), duration.ToString());

                        duration = duration / 1000;

                        var inhours = (duration > 3600) ? (duration / 60) : 0;
                        var inminutes = (duration > 60) ? ((duration - (inhours * 60)) / 60) : 0;
                        var inseconds = duration - (inhours * 60) - (inminutes * 60);
                        properties.Add(nameof(inhours), inhours.ToString("D2"));
                        properties.Add(nameof(inminutes), inminutes.ToString("D2"));
                        properties.Add(nameof(inseconds), inseconds.ToString("D2"));

                        var height = media.Height;
                        properties.Add(nameof(height), height.ToString());

                        var width = media.Width;
                        properties.Add(nameof(width), width.ToString());

                        var framerate = media.Framerate;
                        properties.Add(nameof(framerate), framerate.ToString());

                        var aspectRatio = media.AspectRatio;
                        properties.Add(nameof(aspectRatio), aspectRatio.ToString());

                        var codec = media.Codec;
                        properties.Add(nameof(codec), codec.ToString());

                        var videoCodec = media.VideoCodec;
                        properties.Add(nameof(videoCodec), videoCodec);

                        var videoRate = media.VideoRate;
                        properties.Add(nameof(videoRate), videoRate.ToString());

                        var videoResolution = media.VideoResolution;
                        properties.Add(nameof(videoResolution), videoResolution);

                        var format = media.Format;
                        properties.Add(nameof(format), format.ToString());

                        var audioCodec = media.AudioCodec;
                        properties.Add(nameof(audioCodec), audioCodec.ToString());

                        var audioSampleRate = media.AudioSampleRate;
                        properties.Add(nameof(audioSampleRate), audioSampleRate.ToString());

                        var audioChannelsFriendly = media.AudioChannelsFriendly;
                        properties.Add(nameof(audioChannelsFriendly), audioChannelsFriendly);

                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
        }

        return properties;
    }
}
