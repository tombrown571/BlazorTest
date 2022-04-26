//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
////using FP.Domain.Enum;
////using FP.ExcelReader.Abstract;
////using FP.Extensions;
////using FP.ViewModel.Api.Data;
////using FP.ViewModel.Dto2;


//using FP.MissingLink;


//namespace BlazorWebAssemblyApp.FileHandler;

//public class ReadExcel : IReadExcel
//{
//    private readonly IReferenceDataService _referenceDataService;
//    private IExcelReaderFactory _factory;
//    private const string ZeroTime = "00:00:00";
//    private const string ZeroDate = "31/12/1899";

//    public ReadExcel(IReferenceDataService referenceDataService,
//        IExcelReaderFactory factory)
//    {
//        _referenceDataService = referenceDataService;
//        _factory = factory;
//    }

//    #region Implementation of IReadExcel

//    public async Task<EventFileDetails> IdentifyFile(string filename)
//    {
//        var fileInfo = new FileInfo(filename);
//        if (!fileInfo.Exists)
//        {
//            throw new FileNotFoundException($"Supplied file: {filename} not found");
//        }
//        var fileExtension = fileInfo.Extension.ToLowerInvariant().Replace(".", "");
//        if (fileExtension != SystemWide.CSV &&
//            fileExtension != SystemWide.XLS &&
//            fileExtension != SystemWide.XLSX)
//        {
//            throw new ArgumentException($"File: {filename} is not a type that can be inspected");
//        }
//        if (fileInfo.Name.StartsWith("~$"))
//        {
//            throw new ArgumentException($"Excel Lock File - ignored");
//        }
//        var eventFileDetails = new EventFileDetails
//        {
//            EventFileInfo = fileInfo
//        };
//        var xlReader = _factory.Create(fileInfo.FullName);
//        var headings = xlReader.ReadHeaders();
//        var fileDataContent = await IdentifyDataContent(headings);
//        if (fileDataContent == null) { fileDataContent = EnumFileDataContent.NotRecognised; }
//        eventFileDetails.FileDataContent = EnumFileDataContent.FromId(fileDataContent.Id);
//        if (fileDataContent != EnumFileDataContent.NotSpecified && fileDataContent != EnumFileDataContent.NotRecognised)
//        {
//            var eventData = await GetEventData(fileDataContent, xlReader);
//            eventFileDetails.NumberOfRecords = eventData.DataRows.Count;
//            if (eventFileDetails.FileDataContent.Mnc.HasValue)  // valid Mobile phone operator data
//            {
//                var phone = await GetStatisticPhone(fileDataContent, eventData, eventFileDetails);
//                eventFileDetails.PhoneNumber = phone;
//            }
//        }
//        xlReader.Close();
//        return eventFileDetails;
//    }

//    #endregion

//    private async Task<EnumFileDataContent> IdentifyDataContent(IDictionary<int, string> headings)
//    {
//        var headingsDto = new IdentifyHeadingsDto();
//        foreach (var heading in headings)
//        {
//            headingsDto.HeadingsDictionary.Add(heading.Key, heading.Value);
//        }
//        var serviceResponse = await _referenceDataService.IdentifyHeadings(headingsDto);
//        if (serviceResponse.HasErrors)
//        {
//            return null;
//        }
//        return serviceResponse.ApiResult;
//    }

//    private async Task<IExcelData> GetEventData(EnumFileDataContent fileDataContent, IExcelReader xlReader)
//    {
//        IExcelData eventData;
//        if (fileDataContent.Id > 0)
//        {
//            var serviceResponse = await _referenceDataService.GetHeadingsFromContent(fileDataContent.Id);
//            if (serviceResponse.HasErrors)
//            {
//                eventData = xlReader.ReadSheet();
//            }
//            else
//            {
//                var secondHeaders = new List<string>();
//                var headerDict = serviceResponse.ApiResult.HeadingsDictionary;
//                int key = 0;
//                while (key >= 0)
//                {
//                    if (headerDict.ContainsKey(key))
//                    {
//                        secondHeaders.Add(headerDict[key]);
//                        key++;
//                    }
//                    else
//                    {
//                        key = -1; // exit condition
//                    }
//                }
//                var startRow = xlReader.WorkSheetInfo.IgnoreHeaderRows;
//                if (fileDataContent.Mnc == Constants.MncThree)
//                {
//                    startRow++;
//                }
//                var header2 = xlReader.FindSecondHeaders(startRow, secondHeaders);
//                var split = xlReader.ReadSheetSplitOutput(startRow, header2).ToArray();
//                eventData = split.First();
//            }
//        }
//        else
//        {
//            eventData = xlReader.ReadSheet();
//        }
//        return eventData;
//    }

//    private async Task<string> GetStatisticPhone(EnumFileDataContent fileDataContent, IExcelData eventData, EventFileDetails eventFileDetails)
//    {
//        var serviceResponse = await _referenceDataService.RawHeadingsFromContentId(fileDataContent.Id);
//        if (serviceResponse.HasErrors)
//        {
//            return null;
//        }
//        var columnDefs = serviceResponse.ApiResult.RawColumnHeadings;

//        // Note: Column.Sequence is 1-based, and _eventData is zero-based: So subtract 1 from all .Sequence numbers to match with _eventData

//        RawColumnHeadingDto callTimeColumn = null;
//        RawColumnHeadingDto callDateColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.DateTime);
//        if (callDateColumn == null)
//        {
//            // try date only
//            callDateColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.Date);
//            if (callDateColumn != null)
//            {
//                callTimeColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.Time);
//            }
//        }

//        DateTime minDateTime = DateTime.MinValue;
//        DateTime maxDateTime = DateTime.MinValue; ;
//        bool hasTimeInDate = false;
//        //TODO: check actual column headings vs returned column Defs - may require adjustments
//        if (callDateColumn != null)
//        {
//            var callDate = eventData.DataRows[0][callDateColumn.Sequence - 1].Trim();
//            if (DateTime.TryParse(callDate, out minDateTime))
//            {
//                if (callTimeColumn != null)
//                {
//                    if (minDateTime > minDateTime.Date)
//                    {
//                        hasTimeInDate = true;
//                    }
//                    else
//                    {
//                        var callTime = eventData.DataRows[0][callTimeColumn.Sequence - 1].Trim();
//                        if (callTime.StartsWith(ZeroDate))
//                        {
//                            callTime = callTime.Replace(ZeroDate, "").Trim();
//                            // its an excel time, so remove the time portion from callDate
//                            callDate = callDate.Replace(ZeroTime, "").Trim();
//                        }
//                        // construct a full string of date and time
//                        DateTime.TryParse($"{callDate} {callTime}", out DateTime withTime);
//                        if (withTime != DateTime.MinValue)
//                        {
//                            minDateTime = withTime;
//                        }
//                        else if (callDate.EndsWith(ZeroTime))
//                        {
//                            hasTimeInDate = true; // allows for first date is exactly 00:00:00 midnight
//                        }
//                    }
//                }
//            }
//            string lastDate = null;
//            int lastRow = 0;
//            for (int i = eventData.DataRows.Count - 1; i > 1; i--)
//            {
//                lastRow = i;
//                lastDate = eventData.DataRows[i][callDateColumn.Sequence - 1].Trim();
//                if (DateTime.TryParse(lastDate, out maxDateTime))
//                {
//                    break;
//                }
//            }
//            if (maxDateTime != DateTime.MinValue)
//            {
//                if (callTimeColumn != null && !hasTimeInDate)
//                {
//                    var lastTime = eventData.DataRows[lastRow][callTimeColumn.Sequence - 1].Trim();
//                    if (lastTime.StartsWith(ZeroDate))
//                    {
//                        lastTime = lastTime.Replace(ZeroDate, "").Trim();
//                        // its an excel time, so remove the time portion from callDate
//                        lastDate = lastDate.Replace(ZeroTime, "").Trim();
//                    }
//                    DateTime.TryParse($"{lastDate} {lastTime}", out DateTime withTime);
//                    if (withTime != DateTime.MinValue)
//                    {
//                        maxDateTime = withTime;
//                    }
//                }
//            }
//        }
//        eventFileDetails.StartDate = minDateTime;
//        eventFileDetails.EndDate = maxDateTime;

//        string phoneNumber = null;
//        // if date/time columns combined, but RawColumns has a time column, shift phone columns (because actual data has no time column)
//        int colOffset = hasTimeInDate ? 2 : 1;
//        // most common sending & receiving number assumed to be subject number
//        RawColumnHeadingDto phoneColumn = null;
//        // test subject number first - this will be easiest
//        phoneColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.SubjectNumber);
//        if (phoneColumn != null)
//        {
//            var mostCommonSubject = eventData.DataRows.GroupBy(x => x[phoneColumn.Sequence - colOffset].Trim())
//                .OrderByDescending(g => g.Count())
//                .FirstOrDefault();
//            if (mostCommonSubject != null)
//            {
//                phoneNumber = mostCommonSubject.Key;
//            }
//        }
//        else
//        {
//            var callingColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.CallingNumber);
//            if (callingColumn != null)
//            {
//                // 10 most common calling numbers
//                var mostCommonCalling = eventData.DataRows.GroupBy(x => x[callingColumn.Sequence - colOffset])
//                    .OrderByDescending(g => g.Count())
//                    .Select(s => new
//                    {
//                        Number = s.Key,
//                        Count = s.Count()
//                    })
//                    .Where(w => !string.IsNullOrWhiteSpace(w.Number))
//                    .Take(10);
//                var receivingColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.ReceivingNumber);
//                if (receivingColumn != null)
//                {
//                    var mostCommonReceiving = eventData.DataRows.GroupBy(x => x[receivingColumn.Sequence - colOffset])
//                        .OrderByDescending(g => g.Count())
//                        .Select(s => new
//                        {
//                            Number = s.Key,
//                            Count = s.Count()
//                        })
//                        .Where(w => !string.IsNullOrWhiteSpace(w.Number))
//                        .Take(10);
//                    // combine with calling
//                    mostCommonCalling = mostCommonCalling.Concat(mostCommonReceiving).OrderByDescending(o => o.Count);
//                }
//                // decypher the result to get the phone number
//                //foreach (var resValue in mostCommonCalling)
//                //{
//                //    Debug.WriteLine($"{resValue.Count}  {resValue.Number}");
//                //}
//                var mostCommonNumber = mostCommonCalling
//                    .GroupBy(x => x.Number)
//                    .Select(ph => new
//                    {
//                        Number = ph.Key,
//                        Count = ph.Sum(x => x.Count)
//                    })
//                    .OrderByDescending(x => x.Count)
//                    .FirstOrDefault();
//                if (mostCommonNumber != null)
//                {
//                    phoneNumber = mostCommonNumber.Number;
//                }
//            }
//            else
//            {
//                // try MSISDN column, used in some Data formats
//                var msisdnColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.MSISDN);
//                if (msisdnColumn != null)
//                {
//                    var mostCommonMsisdn = eventData.DataRows.GroupBy(x => x[msisdnColumn.Sequence - colOffset].Trim())
//                        .OrderByDescending(g => g.Count())
//                        .FirstOrDefault();
//                    if (mostCommonMsisdn != null)
//                    {
//                        phoneNumber = mostCommonMsisdn.Key;
//                    }
//                }
//                else
//                {
//                    var imsiColumn = columnDefs.FirstOrDefault(x => x.ColumnDataType == EnumColumnDataType.IMSI);
//                    if (imsiColumn != null)
//                    {
//                        var mostCommonImsi = eventData.DataRows.GroupBy(x => x[imsiColumn.Sequence - colOffset].Trim())
//                            .OrderByDescending(g => g.Count())
//                            .FirstOrDefault();
//                        if (mostCommonImsi != null)
//                        {
//                            phoneNumber = mostCommonImsi.Key;
//                        }
//                    }
//                }

//            }
//        }
//        return phoneNumber;
//    }
//}
