using System.Text.RegularExpressions;

namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing an image URL
/// </summary>
public record ImageUrl
{
    private static readonly Regex UrlValidationRegex = new(
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public ImageUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Image URL cannot be null or empty", nameof(value));

        var trimmedValue = value.Trim();
        
        if (!IsValidUrl(trimmedValue))
            throw new ArgumentException("Invalid URL format", nameof(value));

        Value = trimmedValue;
    }

    private static bool IsValidUrl(string url)
    {
        // Allow both HTTP/HTTPS URLs and data URLs (for base64 images)
        if (url.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return true;

        return UrlValidationRegex.IsMatch(url);
    }

    public bool IsDataUrl() => Value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);
    
    public bool IsHttpUrl() => Value.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    public string GetFileExtension()
    {
        if (IsDataUrl())
        {
            // Extract format from data URL like "data:image/jpeg;base64,..."
            var semicolonIndex = Value.IndexOf(';');
            if (semicolonIndex > 0)
            {
                var mimeType = Value[11..semicolonIndex]; // Skip "data:image/"
                return $".{mimeType}";
            }
            return ".unknown";
        }

        try
        {
            var uri = new Uri(Value);
            var path = uri.AbsolutePath;
            var extensionIndex = path.LastIndexOf('.');
            return extensionIndex >= 0 ? path[extensionIndex..] : ".unknown";
        }
        catch
        {
            return ".unknown";
        }
    }

    public override string ToString() => Value;

    public static implicit operator string(ImageUrl imageUrl) => imageUrl.Value;
    public static implicit operator ImageUrl(string value) => new(value);
}