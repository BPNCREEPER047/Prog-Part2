using System.Configuration;
using System.Data;
using System.Windows;

namespace ProgPart2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Properties["ApplicationName"] = "SA Cyber Safety Assistant";
            this.Properties["Version"] = "2.0";
        }
    }
}