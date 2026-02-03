using System.Security.Cryptography;

namespace api.Core.Helpers;

public static class CodeGenerator
{
    /// <summary>
    ///     Generates a cryptographically secure, random numeric code of a specified length.
    /// </summary>
    /// <param name="length">The number of digits the code will have, default is 4.</param>
    /// <returns>A string with the generated numeric code.</returns>
    public static string GenerateRandomNumericCode(int length = 4)
    {
        if (length <= 0) throw new ArgumentException(@"Code length must be a positive number.", nameof(length));

        var minValue = (int)Math.Pow(10, length - 1);
        var maxValue = (int)Math.Pow(10, length);

        // Generates a safe number within the range [minValue, maxValue-1]
        return RandomNumberGenerator.GetInt32(minValue, maxValue).ToString();
    }
}