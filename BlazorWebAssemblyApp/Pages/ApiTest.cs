using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorWebAssemblyApp.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorWebAssemblyApp.Pages
{
    public partial class ApiTest
    {
        [Inject]
        public ITestApiService TestApiService { get; set; }

        public string ApiVersion { get; set; }
        public string ApiResponding { get; set; }
        public string PageError { get; set; }
        #region Overrides of ComponentBase

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await Initialise();
            
        }

        private async Task Initialise()
        {

            try
            {
                ApiResponding = await TestApiService.IsApiResponding();

            }
            catch (Exception e)
            {
                PageError += $"IsApiResponding: Error {e.Message} {e.StackTrace}";
            }
            try
            {
                ApiVersion = await TestApiService.GetApiVersion();

            }
            catch (Exception e)
            {
                PageError += $"Error loading ApiVersion: {e.Message} {e.StackTrace}";
            }
        }

        #endregion
    }
}
