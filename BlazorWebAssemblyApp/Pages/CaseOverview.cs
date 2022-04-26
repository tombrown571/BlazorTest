using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FP.MissingLink;
//using FP.Domain.Enum;
//using FP.ViewModel.Api.Data;
//using FP.ViewModel.ClientControls;
//using FP.ViewModel.Dto2;
using Microsoft.AspNetCore.Components;

namespace BlazorWebAssemblyApp.Pages;

public partial class CaseOverview
{
    [Inject]
    public ICaseDataService CaseDataService { get; set; }
    public IEnumerable<CaseDto> Cases { get; set; }

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
        await InitialiseCases();
        //return base.OnInitializedAsync();
    }

    #endregion

    private async Task InitialiseCases()
    {
        try
        {
            var qp = new QueryFullParameters
            {
                SortCriteria = new Dictionary<string, bool> { { "InstructionDate", true } },
                PageNumber = 1,
                PageSize = 20
            };
            var serviceResponse = await CaseDataService.CaseList(qp);
            if (!serviceResponse.HasErrors)
            {
                Cases = serviceResponse.ApiResult;
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
