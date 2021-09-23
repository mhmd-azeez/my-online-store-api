namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static T GetOptions<T>(this IConfiguration configuration, string section) where T : class, new()
        {
            var options = new T();
            configuration.GetSection(section).Bind(options);
            
            return options;
        }
    }
}