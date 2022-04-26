using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWebAssemblyApp;

public class StartupOptions
{
    public const string Startup = "Startup";
    public string Filename { get; set; }
    public string FolderName { get; set; }
}
