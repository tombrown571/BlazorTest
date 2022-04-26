using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWebAssemblyApp.Services
{
    public interface ITestApiService
    {
        Task<string> GetApiVersion();
        Task<string> IsApiResponding();
    }
}
