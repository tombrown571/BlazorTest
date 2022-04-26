using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
namespace FP.MissingLink
{
    public static class Constants
    {
        public const int MncO2 = 10;
        public const int MncVodafone = 15;
        public const int MncThree = 20;
    }
    public sealed class EnumFileDataContent 
    {
        public const int NotSpecifiedId = 0;

        public const int O2CdrId = 1;
        public const int O2MdeId = 2;
        public const int O2DdrId = 3;
        public const int O2DdrxId = 4;

        public const int VodaCellId = 101;
        public const int VodaGprsId = 102;

        public const int EECdrId = 201;
        public const int EECdrId2 = 202;
        public const int EECdrId3 = 203;  // Legacy EE Call Data only no cell site data 
        public const int EECdrId4 = 204;  // Legacy EE Cell Site Data - accompanies EECdrId3

        public const int H3CdrInId = 301;
        public const int H3CdrOutId = 302;
        public const int H3CellsId = 303;
        public const int H3DataId = 304;

        public const int FicsScheduleCommonId = 900;
        public const int FpSurveyImportId = 901;
        public const int FpSurveyLocationsId = 902;

        public const int NotRecognisedId = -1;

        public static readonly EnumFileDataContent NotSpecified = new EnumFileDataContent(NotSpecifiedId, "Not Specified");

        public static readonly EnumFileDataContent O2Cdr = new EnumFileDataContent(O2CdrId, "O2 Call Data Records", Constants.MncO2);
        public static readonly EnumFileDataContent O2Mde = new EnumFileDataContent(O2MdeId, "O2 Media Data Extract", Constants.MncO2);
        public static readonly EnumFileDataContent O2Ddr = new EnumFileDataContent(O2DdrId, "O2 Digital Data Records", Constants.MncO2);
        public static readonly EnumFileDataContent O2Ddrx = new EnumFileDataContent(O2DdrxId, "O2 Digital Data Records (Extended)", Constants.MncO2);

        public static readonly EnumFileDataContent VodaCell = new EnumFileDataContent(VodaCellId, "Vodafone Voice & SMS", Constants.MncVodafone);
        public static readonly EnumFileDataContent VodaGprs = new EnumFileDataContent(VodaGprsId, "Vodafone GPRS", Constants.MncVodafone);


        public static readonly EnumFileDataContent NotRecognised = new EnumFileDataContent(NotRecognisedId, "Not Recognised");

        public int Id { get; set; }
        private EnumFileDataContent(int id, string name, int? mnc = null)
        {
            Id = id;
            Name = name;
            Mnc = mnc;
        }
        public EnumFileDataContent() { }
        /// <summary>
        /// Property
        /// </summary>
        public string Name { get; private set; }

        public int? Mnc { get; private set; }

        public static IEnumerable<EnumFileDataContent> List()
        {
            return new[] { NotSpecified,
                O2Cdr, O2Mde, O2Ddr, O2Ddrx,
                VodaCell, VodaGprs,
                NotRecognised };
        }

        public EnumFileDataContent FromString(string fileDataContent)
        {
            return List()
                .FirstOrDefault(r => string.Equals(r.Name, fileDataContent, StringComparison.OrdinalIgnoreCase));
        }

        public static EnumFileDataContent FromId(int id)
        {
            return List().FirstOrDefault(r => r.Id == id);
        }
    }

    public class ServiceResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Reason { get; set; }
        public string ErrorMessage { get; set; }
        public T ApiResult { get; set; }
        public Dictionary<string, string> ResponseData { get; set; }
        public bool HasErrors => (int)StatusCode >= 400;
    }

    public class CaseDto 
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(10)]
        public string Reference { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(80)]
        public string Name { get; set; }
        public string Description { get; set; }
        [MaxLength(260)] public string Folder { get; set; }
        public DateTime InstructionDate { get; set; }
    }
    public class IdentifyHeadingsDto
    {
        public IdentifyHeadingsDto()
        {
            HeadingsDictionary = new Dictionary<int, string>();
        }
        public Dictionary<int, string> HeadingsDictionary { get; }
    }
    /// <summary>
    /// A List of raw column headings
    /// </summary>
    public class RawColumnHeadingsDto
    {
        public RawColumnHeadingsDto()
        {
            RawColumnHeadings = new List<RawColumnHeadingDto>();
        }
        public List<RawColumnHeadingDto> RawColumnHeadings { get; set; }
    }

    public class RawColumnHeadingDto
    {
        public int EnumFileDataContentId { get; set; }
        public int Sequence { get; set; }
        public string Heading { get; set; }
        public EnumColumnDataType ColumnDataType { get; set; }
    }
    /// <summary>
    /// Used to process some information on files
    /// ahead of a full import.
    /// </summary>
    public enum EnumColumnDataType
    {
        NotSpecified = 0,
        Date = 1,
        Time = 2,
        DateTime = 3,
        CallingNumber = 4,
        ReceivingNumber = 5,
        IMSI = 6, // sometimes used to identify calling number
        MSISDN = 7,
        SubjectNumber = 9,
        Operator = 10

    }
    public interface IReferenceDataService
    {
        Task<ServiceResponse<EnumFileDataContent>> IdentifyHeadings(IdentifyHeadingsDto identifyHeadingsDto);
        Task<ServiceResponse<IdentifyHeadingsDto>> GetHeadingsFromContent(int enumFileDataContentId);
        Task<ServiceResponse<RawColumnHeadingsDto>> RawHeadingsFromContentId(int enumFileDataContentId);

    }
    public interface ICaseDataService
    {
        Task<ServiceResponse<CaseDto>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<CaseDto>>> CaseList(QueryFullParameters queryParameters);

    }
    public class CaseDataService : HttpHelperService,  ICaseDataService
    {

        public CaseDataService(HttpClient httpClientInstance)
            : base(httpClientInstance)
        {
            ControllerBasePath = "Case";
        }

        public async Task<ServiceResponse<IEnumerable<CaseDto>>> CaseList(QueryFullParameters queryParameters)
        {
            var response =
                await HttpClientInstance.GetAsync($"{ControllerBasePath}?{queryParameters.ToQueryString()}");
            var serviceResponse = await SetResponse<IEnumerable<CaseDto>>(response);
            if (!response.IsSuccessStatusCode) return serviceResponse;
            var obj = await response.Content.ReadAsStringAsync();
            serviceResponse.ApiResult = JsonConvert.DeserializeObject<IEnumerable<CaseDto>>(obj);
            serviceResponse.ResponseData = DecodeHeader(response);
            return serviceResponse;
        }
        public async Task<ServiceResponse<CaseDto>> GetByIdAsync(int id)
        {
            var message = CreateGetMessage($"{ControllerBasePath}/{id}", null);
            var response = await HttpClientInstance.SendAsync(message);
            var serviceResponse = await SetResponse<CaseDto>(response);
            if (response.IsSuccessStatusCode)
            {
                var obj = await response.Content.ReadAsStringAsync();
                var caseDto = JsonConvert.DeserializeObject<CaseDto>(obj);
                serviceResponse.ApiResult = caseDto;
            }
            return serviceResponse;
        }
    }
    public abstract class HttpHelperService
    {
        private const string MediaTypeVersion = "application/json;v=1.0";
        protected readonly HttpClient HttpClientInstance;

        protected HttpHelperService(HttpClient httpClientFactory)
        {
            HttpClientInstance = httpClientFactory;
        }

        public string ControllerBasePath { get; set; }
        protected async Task<ServiceResponse<T>> SetResponse<T>(HttpResponseMessage response)
        {
            var serviceResponse = new ServiceResponse<T>
            {
                StatusCode = response.StatusCode,
                Reason = response.ReasonPhrase
            };
            if (!response.IsSuccessStatusCode)
            {
                serviceResponse.ErrorMessage = await response.Content.ReadAsStringAsync();
            }
            return serviceResponse;
        }

        protected HttpRequestMessage CreateGetMessage(string uriFragment, StringContent json)
        {
            var uri = new Uri(HttpClientInstance.BaseAddress + uriFragment);
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Headers =
                {
                    {HttpRequestHeader.Accept.ToString(), MediaTypeVersion}
                },
                Content = json
            };
            return msg;
        }

        protected HttpRequestMessage CreatePostMessage(string uriFragment, StringContent json)
        {
            var uri = new Uri(HttpClientInstance.BaseAddress + uriFragment);
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Headers =
                {
                    {HttpRequestHeader.ContentType.ToString(), MediaTypeVersion}
                },
                Content = json
            };
            return msg;
        }

        protected HttpRequestMessage CreatePutMessage(string uriFragment, StringContent json)
        {
            var uri = new Uri(HttpClientInstance.BaseAddress + uriFragment);
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = uri,
                Headers =
                {
                    {HttpRequestHeader.ContentType.ToString(), MediaTypeVersion}
                },
                Content = json
            };
            return msg;
        }

        protected HttpRequestMessage CreateDeleteMessage(string uriFragment)
        {
            var uri = new Uri(HttpClientInstance.BaseAddress + uriFragment);
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = uri,
                Headers =
                {
                    {HttpRequestHeader.ContentType.ToString(), MediaTypeVersion}
                }
            };
            return msg;
        }

        public Dictionary<string, string> DecodeHeader(HttpResponseMessage response)
        {
            if (response.Headers.Contains("X-Pagination"))
            {
                return DecodePaginationHeader(response);
            }
            return response.Headers.Contains("X-QueryParameters") ? DecodeQueryParameterHeaders(response) : new Dictionary<string, string>();
        }

        private Dictionary<string, string> DecodePaginationHeader(HttpResponseMessage response)
        {
            var paginationValues = new Dictionary<string, string>();
            var paginationJson = new
            {
                totalCount = "",
                pageSize = "",
                currentPage = "",
                totalPages = "",
                prevPageLink = "",
                nextPageLink = ""
            };
            if (!response.Headers.TryGetValues("X-Pagination", out var headerValue)) return paginationValues;

            var headerValues = JsonConvert.DeserializeAnonymousType(headerValue.First(), paginationJson);
            paginationValues.Add("totalCount", headerValues.totalCount);
            paginationValues.Add("pageSize", headerValues.pageSize);
            paginationValues.Add("currentPage", headerValues.currentPage);
            paginationValues.Add("totalPages", headerValues.totalPages);
            paginationValues.Add("prevPageLink", headerValues.prevPageLink);
            paginationValues.Add("nextPageLink", headerValues.nextPageLink);
            return paginationValues;
        }

        private Dictionary<string, string> DecodeQueryParameterHeaders(HttpResponseMessage response)
        {
            var queryParameterValues = new Dictionary<string, string>();
            var queryParamJson = new
            {
                totalCount = "",
                pageSize = "",
                currentPage = "",
                totalPages = "",
                prevPageLink = "",
                nextPageLink = "",
                currentPageLink = "",
            };
            if (!response.Headers.TryGetValues("X-QueryParameters", out var headerValue)) return queryParameterValues;

            var headerValues = JsonConvert.DeserializeAnonymousType(headerValue.First(), queryParamJson);
            queryParameterValues.Add("totalCount", headerValues.totalCount);
            queryParameterValues.Add("pageSize", headerValues.pageSize);
            queryParameterValues.Add("currentPage", headerValues.currentPage);
            queryParameterValues.Add("totalPages", headerValues.totalPages);
            queryParameterValues.Add("prevPageLink", headerValues.prevPageLink);
            queryParameterValues.Add("nextPageLink", headerValues.nextPageLink);
            queryParameterValues.Add("currentPageLink", headerValues.currentPageLink);
            return queryParameterValues;
        }
    }

    public interface ITypedClientConfig
    {
        Uri ApiUrl { get; set; }
        int HttpTimeout { get; set; }
        string ApiVersion { get; set; }
    }
    public class TypedClientConfig : ITypedClientConfig
    {
        public TypedClientConfig(IOptions<HttpClientSettings> options)
        {
            HttpClientSettings settings = options.Value;
            ApiUrl = settings.ApiUrl;
            HttpTimeout = settings.HttpTimeout;
            ApiVersion = settings.ApiVersion;
        }

        #region Implementation of ITypedClientConfig

        public Uri ApiUrl { get; set; }
        public int HttpTimeout { get; set; }
        public string ApiVersion { get; set; }

        #endregion   
    }
    public class HttpClientSettings
    {
        public Uri ApiUrl { get; set; }

        public int HttpTimeout { get; set; }

        public string ApiVersion { get; set; }
    }

    public class QueryFullParameters
    {
        const int MaxPageSize = 250;
        const int DefaultPageSize = 50;

        public QueryFullParameters()
        {
            SortCriteria = new Dictionary<string, bool>();
        }

        public int PageNumber { get; set; } = 1;

        private int _pageSize = DefaultPageSize;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1) ? DefaultPageSize : value;
        }
        public IDictionary<string, bool> SortCriteria { get; set; }
        public string ToQueryString()
        {
            StringBuilder queryString = new StringBuilder($"pageNumber={PageNumber}&pageSize={PageSize}");
            if (SortCriteria.Count > 0)
            {
                queryString.Append("&orderBy=");
                foreach (var criterion in SortCriteria)
                {
                    queryString.Append($"{criterion.Key}{(criterion.Value ? " desc" : "")},");
                }
            }
            return queryString.ToString();
        }
    }

    public interface IExcelReaderFactory
    {
        IExcelReader Create(string fileName);
        IExcelReader Create(string fileName, int ignoreHeadingRows);
        IExcelReader Create();
        IExcelReader Create(Stream stream, string fileExtension, int ignoreHeadingRows);
    }

    public interface IExcelReader
    {
        /// <summary>
        /// Find the first row of Excel file which appears to be column headers
        /// </summary>
        /// <returns>Dictionary of Header strings</returns>
        IDictionary<int, string> ReadHeaders();

        /// <summary>
        /// Close any open files/streams
        /// </summary>
        void Close();

        /// <summary>
        /// Find headers of a specific worksheet
        /// </summary>
        /// <param name="worksheetName"></param>
        /// <returns>Dictionary of Header strings</returns>
        IDictionary<int, string> ReadHeaders(string worksheetName);

        /// <summary>
        /// Read first row of Excel file into list of row headers
        /// </summary>
        /// <param name="minColumns">Minimnum number of columns with data</param>
        /// <param name="maxRows">Maximum rows to read looking for headers</param>
        /// <param name="worksheetName"></param>
        /// <returns></returns>
        IDictionary<int, string> ReadHeaders(int minColumns, int maxRows, string worksheetName = null);

        /// <summary>
        /// Excel Headers, ignoring the minimum columns stipulation
        /// </summary>
        /// <param name="worksheetName"></param>
        /// <param name="ignoreMinColumns"></param>
        /// <returns></returns>
        IDictionary<int, string> ReadHeaders(string worksheetName, bool ignoreMinColumns);

        /// <summary>
        /// Read the worksheet
        /// </summary>
        /// <returns></returns>
        IExcelData ReadSheet();


        /// <summary>
        /// Read a single worksheet into two output IExcelDataStructures
        /// </summary>
        /// <param name="headerStart1"></param>
        /// <param name="headerStart2"></param>
        /// <returns></returns>
        IEnumerable<IExcelData> ReadSheetSplitOutput(int headerStart1, int headerStart2);

        string FileName { get; }

        DateTime FileLastWriteTime { get; }

        string WorksheetName { get; set; }

        bool IsEmpty { get; }
    }
    public interface IExcelData
    {
        string FileName { get; set; }
        string SheetName { get; set; }
        IDictionary<int, string> Headers { get; set; }
        IDictionary<string, int> LowerNoSpaceHeaders { get; set; }
        IList<IList<string>> DataRows { get; set; }
        int SheetRowCount { get; set; }

        // Sorted interface
        ImmutableSortedDictionary<string, int> SortedLowerNoSpaceHeaders { get; set; }
        ImmutableSortedDictionary<int, string> SortedHeaders { get; set; }
        SortedList<int, List<string>> SortedDataRows { get; set; }
    }
}