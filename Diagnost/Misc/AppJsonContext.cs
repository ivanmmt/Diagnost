using Diagnost.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Diagnost.Misc
{
    
    [JsonSourceGenerationOptions(
        WriteIndented = true, 
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        GenerationMode = JsonSourceGenerationMode.Default)]
    // ==========================================
    // /api/Result
    [JsonSerializable(typeof(ResultRequest))]
    [JsonSerializable(typeof(ResultResponse))]
    [JsonSerializable(typeof(ResultResponse[]))]
    [JsonSerializable(typeof(List<ResultResponse>))]
    [JsonSerializable(typeof(long?))] // ID for created Result
    [JsonSerializable(typeof(PZMRResultRequest))]
    [JsonSerializable(typeof(PV2_3ResultRequest))]
    [JsonSerializable(typeof(UFPResultRequest))]

    // ==========================================
    // /api/AccessCode
    [JsonSerializable(typeof(AccessCodeResponse))]
    [JsonSerializable(typeof(List<AccessCodeResponse>))]

    // ==========================================
    // /api/auth
    [JsonSerializable(typeof(LoginRequest))]
    [JsonSerializable(typeof(LoginResponse))]

    // ==========================================
    // /api/admin
    [JsonSerializable(typeof(RegisterRequest))]
    [JsonSerializable(typeof(RegisterResponse))]
    [JsonSerializable(typeof(List<RegisterResponse>))]
    
    [JsonSerializable(typeof(ResultRequest))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }

    public class LoginResponse
    {
    }
}