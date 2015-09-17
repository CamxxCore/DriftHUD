using System;
using Ini;
using GTAV_DriftHUD.Structs;

namespace GTAV_DriftHUD
{
    class Config
    {
        private static IniFile ini = new IniFile("scripts\\GTAV_DriftHUD.ini");
        public static UserConfigData UserConfig = LoadUserConfig();

        private static UserConfigData LoadUserConfig()
        {
            var config = new UserConfigData();
            string path = "scripts\\GTAV_DriftHUD.ini";
            string value = null;

            if (!System.IO.File.Exists(path))
                Helpers.CreateConfig();

            value = Config.ReadValue("General", "EnableSound");
            if (!Boolean.TryParse(value, out config.EnableSound))
                config.EnableSound = true;

            value = Config.ReadValue_Safe("Messages", "Message0");
            if (value.Length < 1)
                config.Message0 = "Great!";
            else config.Message0 = value;

            value = Config.ReadValue_Safe("Messages", "Message1");
            if (value.Length < 1)
                config.Message1 = "Awesome!";
            else config.Message1 = value;

            value = Config.ReadValue_Safe("Messages", "Message2");
            if (value.Length < 1)
                config.Message2 = "Amazing!";
            else config.Message2 = value;

            value = Config.ReadValue_Safe("Messages", "Message3");
            if (value.Length < 1)
                config.Message3 = "Insane!";
            else config.Message3 = value;

            value = Config.ReadValue_Safe("Messages", "Message4");
            if (value.Length < 1)
                config.Message4 = "Impossible!";
            else config.Message4 = value;

            value = Config.ReadValue_Safe("Messages", "Message5");
            if (value.Length < 1)
                config.Message5 = "DRIFT KING";
            else config.Message5 = value;

            return config;
        }

        /// <summary>
        /// Invert a config setting and return the new value
        /// </summary>
        /// <param name="section">The section of the config file</param>
        /// <param name="key">Config setting</param>
        /// <returns></returns>
        public static bool ToggleConfigSetting(string section, string key)
        {
            if (ReadValue(section, key) == "true")
            {
                WriteValue(section, key, "false");
                return false;
            }
            else
            {
                WriteValue(section, key, "true");
                return true;
            }
        }

        /// <summary>
        /// Write a string value to the config file at the specified section and key
        /// </summary>
        /// <param name="section">The section in the config file</param>
        /// <param name="key">The key of the config string</param>
        /// <param name="value">The value of the config string</param>
        private static void WriteValue(string section, string key, string value)
        {
            ini.IniWriteValue(section, key, value);
        }

        /// <summary>
        /// Read string from config file with the specified section and key
        /// </summary>
        /// <param name="section">The section in the config file</param>
        /// <param name="key">The key of the config string</param>
        /// <returns></returns>
        private static string ReadValue(string section, string key)
        {
            return ini.IniReadValue(section, key);
        }

        /// <summary>
        /// Read string from config file with the specified section and key and handle exceptions if the file was modified
        /// </summary>
        /// <param name="section">The section in the config file</param>
        /// <param name="key">The key of the config string</param>
        /// <returns></returns>
        private static string ReadValue_Safe(string section, string key)
        {
            try
            {
                return ini.IniReadValue(section, key);
            }

            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a config setting
        /// </summary>
        /// <param name="section">The section of the config file</param>
        /// <param name="key">The config setting</param>
        /// <returns></returns>
        private static T GetConfigSetting<T>(string section, string key)
        {
            System.Type type = typeof(T);

            if (type == typeof(bool))
            {
                object setting = Convert.ToBoolean(Config.ReadValue(section, key));
                return (T)setting;
            }
            else if (type == typeof(int))
            {
                object setting = Convert.ToInt32(Config.ReadValue(section, key));
                return (T)setting;
            }
            else if (type == typeof(uint))
            {
                object setting = Convert.ToUInt32(Config.ReadValue(section, key));
                return (T)setting;
            }
            else if (type == typeof(string))
            {
                object setting = Config.ReadValue(section, key);
                return (T)setting;
            }
            else if (type == typeof(double))
            {
                object setting = Convert.ToDouble(Config.ReadValue(section, key));
                return (T)setting;
            }
            else
                throw new ArgumentException("Not a known type.");
        }
    }
}
