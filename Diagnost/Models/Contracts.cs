using System;
using System.Text.Json.Serialization;

namespace Diagnost.Models
{
    // ==========================================
    // /api/Result
    // ==========================================
    public class ResultRequest
    {
        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = string.Empty;

        [JsonPropertyName("studentFullName")]
        public string StudentFullName { get; set; } = string.Empty;

        [JsonPropertyName("sportType")]
        public string SportType { get; set; } = string.Empty;

        [JsonPropertyName("sportQualification")]
        public string SportQualification { get; set; } = string.Empty;

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;
    }

    public class PZMRResultRequest
    {
        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = string.Empty;

        [JsonPropertyName("resultId")]
        public long ResultId { get; set; }

        [JsonPropertyName("pzmRLatet")]
        public double PZMRLatet { get; set; }

        [JsonPropertyName("pzmrVidhil")]
        public double PZMRvidhil { get; set; }

        [JsonPropertyName("pzmr_ErrorsTotal")]
        public int PZMR_ErrorsTotal { get; set; }
    }

    public class PV2_3ResultRequest
    {
        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = string.Empty;

        [JsonPropertyName("resultId")]
        public long ResultId { get; set; }

        [JsonPropertyName("pV2_3Latet")]
        public double PV2_3Latet { get; set; }

        [JsonPropertyName("pV2_StdDev_ms")]
        public double PV2_StdDev_ms { get; set; }

        [JsonPropertyName("pV2_ErrorsTotal")]
        public double PV2_ErrorsTotal { get; set; }

        [JsonPropertyName("pV2_ErrorsMissed")]
        public int PV2_ErrorsMissed { get; set; }

        [JsonPropertyName("pV2_ErrorsWrongButton")]
        public int PV2_ErrorsWrongButton { get; set; }

        [JsonPropertyName("pV2_ErrorsFalseAlarm")]
        public int PV2_ErrorsFalseAlarm { get; set; }
    }

    public class UFPResultRequest
    {
        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = string.Empty;

        [JsonPropertyName("resultId")]
        public long ResultId { get; set; }

        [JsonPropertyName("ufpLatet")]
        public double UFPLatet { get; set; }

        [JsonPropertyName("ufp_StdDev_ms")]
        public double UFP_StdDev_ms { get; set; }

        [JsonPropertyName("ufp_ErrorsTotal")]
        public double UFP_ErrorsTotal { get; set; }
    }

    public class ResultResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = string.Empty;

        [JsonPropertyName("studentFullName")]
        public string StudentFullName { get; set; } = string.Empty;

        [JsonPropertyName("sportType")]
        public string SportType { get; set; } = string.Empty;

        [JsonPropertyName("sportQualification")]
        public string SportQualification { get; set; } = string.Empty;

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("pzmRLatet")]
        public double PZMRLatet { get; set; }

        [JsonPropertyName("pzmrVidhil")]
        public double PZMRvidhil { get; set; }

        [JsonPropertyName("pzmr_ErrorsTotal")]
        public int PZMR_ErrorsTotal { get; set; }

        [JsonPropertyName("pV2_3Latet")]
        public double PV2_3Latet { get; set; }

        [JsonPropertyName("pV2_StdDev_ms")]
        public double PV2_StdDev_ms { get; set; }

        [JsonPropertyName("pV2_ErrorsTotal")]
        public double PV2_ErrorsTotal { get; set; }

        [JsonPropertyName("pV2_ErrorsMissed")]
        public int PV2_ErrorsMissed { get; set; }

        [JsonPropertyName("pV2_ErrorsWrongButton")]
        public int PV2_ErrorsWrongButton { get; set; }

        [JsonPropertyName("pV2_ErrorsFalseAlarm")]
        public int PV2_ErrorsFalseAlarm { get; set; }

        [JsonPropertyName("ufpLatet")]
        public double UFPLatet { get; set; }

        [JsonPropertyName("ufp_StdDev_ms")]
        public double UFP_StdDev_ms { get; set; }

        [JsonPropertyName("ufp_ErrorsTotal")]
        public double UFP_ErrorsTotal { get; set; }
    }

    // ==========================================
    // /api/AccessCode
    // ==========================================
    public class AccessCodeResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    // ==========================================
    // /api/auth
    // ==========================================
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
        
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
    // ==========================================
    // /api/admin
    // ==========================================
    public class RegisterRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    }

    public class RegisterResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";
    }
}