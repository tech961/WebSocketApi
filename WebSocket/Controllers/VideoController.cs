using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using WebSocket.Hubs;
using WebSocket.Options;

namespace WebSocket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly IHubContext<VideoHub> _hubContext;
    private readonly IWebHostEnvironment _environment;
    private readonly StaticVideoOptions _options;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public VideoController(
        IHubContext<VideoHub> hubContext,
        IWebHostEnvironment environment,
        IOptions<StaticVideoOptions> options)
    {
        _hubContext = hubContext;
        _environment = environment;
        _options = options.Value;
    }

    /// <summary>
    /// Broadcasts a static video file from <c>wwwroot</c> to all connected SignalR clients.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload()
    {
        if (string.IsNullOrWhiteSpace(_options.RelativePath))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Static video path is not configured.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }

        var resolvedPath = Path.GetFullPath(Path.Combine(webRootPath, _options.RelativePath));
        var normalizedWebRoot = Path.GetFullPath(webRootPath);

        if (!resolvedPath.StartsWith(normalizedWebRoot, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Configured static video must reside within wwwroot.");
        }

        if (!System.IO.File.Exists(resolvedPath))
        {
            return NotFound($"Static video file not found at '{_options.RelativePath}'.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(resolvedPath);
        var base64Video = Convert.ToBase64String(fileBytes);
        var fileName = Path.GetFileName(resolvedPath);

        if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        await _hubContext.Clients.All.SendAsync("ReceiveVideo", new
        {
            fileName,
            contentType,
            data = base64Video,
            length = fileBytes.LongLength
        });

        return Ok(new
        {
            fileName,
            contentType,
            length = fileBytes.LongLength
        });
    }
}
