using System;
using Kadro;
using System.Xml.Serialization;

namespace Asteroids.Shared
{
    [Serializable]
    public class UserConfig
    {
        #region SerializeFields

        // configuration (specific low level configuration e.g. address for server access)
        public string NetworkName = "Asteroids-dev";

        public int DebugPort = 12345;
        public int DefaultPort = 24816;
        public string DebugServer = "127.0.0.1";
        public string DefaultServer = "rumkuhgel.duckdns.org";
        // end configuration

        // preferences (user preferences e.g. window resolution, volume etc)
        public int ScreenWidth = 1280;  // single setting/option
        public int ScreenHeight = 720;
        public bool Borderless = false;
        // end preferences

        #endregion

        public static UserConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserConfig();
                }

                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private static UserConfig instance;

        private static string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string settingsLocation = System.IO.Path.Combine(roamingPath, "Asteroids-dev");
        private static string settingsFile = System.IO.Path.Combine(settingsLocation, "settings.xml");

        public static UserConfig Load()
        {
            return XmlLoader.Load<UserConfig>(settingsFile);
        }

        public void Save()
        {
            XmlLoader.Save(this, settingsFile);
        }
    }
}
