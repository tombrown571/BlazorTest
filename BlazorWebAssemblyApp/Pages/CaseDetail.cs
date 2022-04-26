using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using FP.Domain.Enum;
//using FP.ViewModel.Api.Data;
//using FP.ViewModel.Dto2;
using Microsoft.AspNetCore.Components;
using FP.MissingLink;

namespace BlazorWebAssemblyApp.Pages;

public partial class CaseDetail
{
    [Parameter]
    public string FpCaseId { get; set; }
    [Inject]
    public ICaseDataService CaseDataService { get; set; }

    public CaseDto Case { get; set; } = new CaseDto();

    public string ApiError { get; set; }


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
        await InitialiseCase();
    }

    #endregion

    private async Task InitialiseCase()
    {
        try
        {
            var serviceResponse = await CaseDataService.GetByIdAsync(Int32.Parse(FpCaseId));
            if (!serviceResponse.HasErrors)
            {
                Case = serviceResponse.ApiResult;
            }
            else
            {
                ApiError = $"Error calling API: {serviceResponse.ErrorMessage} {serviceResponse.Reason}";
            }
        }
        catch (Exception ex)
        {
            ApiError += $"Error loading cases: {ex.Message} {ex.StackTrace}";
        }
    }
}
