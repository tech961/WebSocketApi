using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebSocket.Hubs;

namespace WebSocket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly IHubContext<VideoHub> _hubContext;

    public VideoController(IHubContext<VideoHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Accepts a video file and broadcasts it to all connected SignalR clients.
    /// </summary>
    /// <param name="video">The uploaded video file.</param>
    [HttpPost("upload")]
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? video)
    {
        if (video is null || video.Length == 0)
        {
            return BadRequest("No video file was uploaded.");
        }

        await using var memoryStream = new MemoryStream();
        await video.CopyToAsync(memoryStream);
        var base64Video = Convert.ToBase64String(memoryStream.ToArray());

        await _hubContext.Clients.All.SendAsync("ReceiveVideo", new
        {
            fileName = video.FileName,
            contentType = video.ContentType,
            data = base64Video,
            length = video.Length
        });

        return Ok(new
        {
            video.FileName,
            video.ContentType,
            video.Length
        });
    }
}
