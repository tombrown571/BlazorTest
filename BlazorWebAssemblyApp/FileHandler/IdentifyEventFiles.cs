//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FP.MissingLink;

//namespace BlazorWebAssemblyApp.FileHandler
//{
//    public class IdentifyEventFiles : IIdentifyEventFiles
//    {
//        private readonly IReadExcel _readExcel;

//        public IdentifyEventFiles(IReadExcel readExcel)
//        {
//            _readExcel = readExcel;
//            //EventFiles = new List<EventFileDetails>();
//            EventFiles = new ObservableCollection<EventFileDetails>();
//        }
//        //public List<EventFileDetails> EventFiles { get; }

//        public ObservableCollection<EventFileDetails> EventFiles { get; }

//        public async Task<int> Identify(string startFolder)
//        {
//            if (!Directory.Exists(startFolder))
//            {
//                throw new ArgumentException($"Cannot find folder {startFolder}");
//            }
//            var filesToProcess = SystemWide.EnumerateFilesRecursive(startFolder, SystemWide.AllowedFileTypes);
//            int filesProcessedCount = 0;
//            foreach (var procssFile in filesToProcess)
//            {
//                EventFileDetails eventFile = null;
//                try
//                {
//                    eventFile = await _readExcel.IdentifyFile(procssFile);
//                }
//                catch (Exception ex)
//                {
//                    var notes = ex.Message;
//                    if (ex.Message == "You uploaded an empty file")
//                    {
//                        notes = "Empty file";
//                    }
//                    else if (ex.GetType() == typeof(IOException))
//                    {
//                        notes = $"Error: {ex.Message}";
//                    }
//                    eventFile = new EventFileDetails
//                    {
//                        EventFileInfo = new FileInfo(procssFile),
//                        FileDataContent = EnumFileDataContent.NotRecognised,
//                        Notes = notes
//                    };
//                }
//                EventFiles.Add(eventFile);
//                filesProcessedCount++;
//            }
//            return filesProcessedCount;
//        }

//        public async Task<int> Identify(string startFolder, ObservableCollection<EventFileDetails> eventFiles)
//        {
//            if (!Directory.Exists(startFolder))
//            {
//                throw new ArgumentException($"Cannot find folder {startFolder}");
//            }
//            var filesToProcess = SystemWide.EnumerateFilesRecursive(startFolder, SystemWide.AllowedFileTypes);
//            int filesProcessedCount = 0;
//            foreach (var procssFile in filesToProcess)
//            {
//                EventFileDetails eventFile = null;
//                try
//                {
//                    eventFile = await _readExcel.IdentifyFile(procssFile);
//                }
//                catch (Exception ex)
//                {
//                    var notes = ex.Message;
//                    if (ex.Message == "You uploaded an empty file")
//                    {
//                        notes = "Empty file";
//                    }
//                    else if (ex.GetType() == typeof(IOException))
//                    {
//                        notes = $"Error: {ex.Message}";
//                    }
//                    eventFile = new EventFileDetails
//                    {
//                        EventFileInfo = new FileInfo(procssFile),
//                        FileDataContent = EnumFileDataContent.NotRecognised,
//                        Notes = notes
//                    };
//                }
//                eventFiles.Add(eventFile);
//                filesProcessedCount++;
//            }
//            return filesProcessedCount;
//        }
//    }
//}
