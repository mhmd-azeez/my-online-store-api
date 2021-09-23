namespace MyOnlineStoreAPI.Helpers
{
    public class CurrencyScoopOptions
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class AuthOptions
    {
        public string Authority { get; set; }
        public string ManagementApiUrl { get; set; }
        public string M2MClientId { get; set; }
        public string M2MClientSecret { get; set; }
        public string SwaggerClientId { get; set; }
        public string Audience { get; set; }
    }
}