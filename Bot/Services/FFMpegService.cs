using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Bot.Services;

public class FFMpegService
{
    private readonly FFMpegSettings _settings;
    private readonly ILogger<FFMpegService> _logger;

    public FFMpegService(IOptions<FFMpegSettings> settings, ILogger<FFMpegService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<string> ConvertAsync(string filePath)
    {
        var fileName = $"{Guid.NewGuid()}.webm";
        var outputFilePath = Path.Combine(Path.GetTempPath(), fileName);

        var argumentsParts = new List<string>
        {
            $"-i {filePath}",
            "-filter:v scale='trunc(iw/2)*2:trunc(ih/2)*2'",
            "-max_muxing_queue_size 1024",
            "-c:v libvpx-vp9",
            "-c:a libopus",
            outputFilePath
        };

        var processStartInfo = new ProcessStartInfo
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = _settings.Path,
            Arguments = string.Join(' ', argumentsParts)
        };

        var process = Process.Start(processStartInfo);

        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError(error);
        }

        return fileName;
    }

    public async Task<string> GetThumbnailAsync(string filePath)
    {
        var thumbnailFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        var processStartInfo = new ProcessStartInfo
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = _settings.Path,
            Arguments = $"-i {filePath} -ss 1 -vframes 1 {thumbnailFilePath}"
        };

        var process = Process.Start(processStartInfo);

        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError(error);
        }

        return thumbnailFilePath;
    }
}