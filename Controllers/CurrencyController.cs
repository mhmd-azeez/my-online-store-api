using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOnlineStoreAPI.Helpers;

namespace MyOnlineStoreAPI.Controllers
{
    [Authorize (Roles = "admin")]
    [Route("[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly CurrencyService _currencyService;

        public CurrencyController(CurrencyService currencyService)
        {
            this._currencyService = currencyService;
        }

        [HttpGet]
        public async Task<CurrencyResponse> Get()
        {
            var response = await _currencyService.GetLatestRatesAsync();
            var rate = response.Rates["IQD"];

            return new CurrencyResponse
            {
                Timestamp = response.Date,
                USDRate = rate
            };
        }
    }

    public class CurrencyResponse
    {
        public DateTime Timestamp { get; set; }
        public decimal USDRate { get; set; }
    }
}