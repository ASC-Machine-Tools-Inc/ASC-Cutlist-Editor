using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;

namespace AscCutlistEditor.Models.MQTT
{
    // Wrapper class over settings for SQL connection string.
    public class UserSqlSettings : ISettings
    {
        public string DataSource
        {
            get => (string)Settings.Default["DataSource"];
            set => Settings.Default["DataSource"] = value;
        }

        public string DatabaseName
        {
            get => (string)Settings.Default["DatabaseName"];
            set => Settings.Default["DatabaseName"] = value;
        }

        public string Username
        {
            get => (string)Settings.Default["Username"];
            set => Settings.Default["Username"] = value;
        }

        public string Password
        {
            get => (string)Settings.Default["Password"];
            set => Settings.Default["Password"] = value;
        }

        // TODO: add customizable table & column settings
    }
}