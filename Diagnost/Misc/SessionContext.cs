namespace Diagnost.Misc
{
    public static class SessionContext
    {
        public static ApiService ApiService = new ApiService();
        public static long? ResultId { get; set; } = null;
        public static string? AccessCode { get; set; } = null;
    }
}
