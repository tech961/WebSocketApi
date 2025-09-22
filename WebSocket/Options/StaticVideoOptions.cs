namespace WebSocket.Options;

public class StaticVideoOptions
{
    public const string SectionName = "StaticVideo";

    /// <summary>
    /// Relative path to the file inside the application's <c>wwwroot</c> directory
    /// that should be broadcast to connected clients.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;
}
