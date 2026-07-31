using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public int PasswordMinLength { get; set; } = 6;
    public bool RequireRegistrationCode { get; set; } = false;
    public int RegistrationCodeExpiryHours { get; set; } = 48;
    public string DefaultUserRole { get; set; } = "Support";
}

public class OccurrenceOptions
{
    public const string SectionName = "Occurrence";

    public List<string> StatusFlow { get; set; } = new()
    {
        "Open", "InProgress", "Resolved", "Closed"
    };
}

public class MediaOptions
{
    public const string SectionName = "Media";

    public List<string> AllowedTypes { get; set; } = new()
    {
        "Image", "Audio", "Video", "Document", "Sticker"
    };
    public int MaxSizeMB { get; set; } = 50;
}
