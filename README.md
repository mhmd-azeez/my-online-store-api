# My Online Store API
This is project is part of a crash course on learning ASP.NET Core API

## Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "CurrencyScoop": {
    "BaseUrl": "https://api.currencyscoop.com/v1/",
      "ApiKey": "***"
  },
  "Auth": {
    "Issuer": "https://example.com",
    "Audience": "https://example.com",
    "Secret": "super-strong-secret-2134656" // Should be 16 characters or more
  }
}
```

