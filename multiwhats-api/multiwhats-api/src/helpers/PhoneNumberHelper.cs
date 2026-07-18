using System.Text.RegularExpressions;

namespace multiwhats_api.src.helpers;

public static partial class PhoneNumberHelper
{
    public static string Sanitize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        return DigitsOnly().Replace(phoneNumber, "");
    }

    [GeneratedRegex("\\D")]
    private static partial Regex DigitsOnly();
}
