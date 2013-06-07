using System.Configuration;

namespace TimeRecorder
{
    public class Configuration
    {
        private const string DefaultTimeFormat = "HH:mm";

        public static string TimeFormat
        {
            get
            {
                string name = ConfigurationManager.AppSettings["TimeFormat"];

                if (string.IsNullOrWhiteSpace(name))
                    return DefaultTimeFormat;

                return name;
            }
        }
    }
}