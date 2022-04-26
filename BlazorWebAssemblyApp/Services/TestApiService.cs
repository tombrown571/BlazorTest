using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorWebAssemblyApp.Services
{
    public class TestApiService : ITestApiService
    {
        private readonly HttpClient _httpClient;

        public TestApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Implementation of ITestApiService

        public async Task<string> GetApiVersion()
        {
            var response = await _httpClient.GetAsync($"api/Basic/ApiVersion");
            if (response.IsSuccessStatusCode)
            {
                var versionString = await response.Content.ReadAsStringAsync();
                return versionString;
            }
            throw new Exception($"API Call returned :{response.StatusCode}, {response.ReasonPhrase}");
        }

        public async Task<string> IsApiResponding()
        {
            var response = await _httpClient.GetAsync($"api/Basic/IsApiResponding");
            if (response.IsSuccessStatusCode)
            {
                var versionString = await response.Content.ReadAsStringAsync();
                return versionString;
            }
            throw new Exception($"API Call returned :{response.StatusCode}, {response.ReasonPhrase}");
        }

        #endregion
    }
}
