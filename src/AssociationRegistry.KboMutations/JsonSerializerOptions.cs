using System.Text.Json;

namespace AssocationRegistry.KboMutations;

public class JsonSerializerDefaultOptions
{
    public static JsonSerializerOptions Default => new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}