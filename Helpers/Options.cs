namespace MyOnlineStoreAPI.Helpers
{
    public class CurrencyScoopOptions
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class AuthOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
    }
}