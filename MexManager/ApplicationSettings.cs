using System.Text.Json;
using System.IO;
using System;
using System.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;

namespace MexManager
{
    public class ApplicationSettings
    {
        private static readonly string FileName = "config.json";

        private static string FilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); } }

        [DisplayName("Melee ISO Path")]
        [Description("Path to Melee ISO")]
        [PathBrowsable(InitialFileName = "", Filters = "Gamecube ISO (*.iso)|*.iso")]
        public string MeleePath { get; set; } = "";

        [DisplayName("Dolphin Path")]
        [Description("Path to Dolphin emulator")]
        [PathBrowsable(InitialFileName = "Dolphin.exe", Filters = "Executable(*.exe)|*.exe")]
        public string DolphinPath { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ApplicationSettings TryOpen()
        {
            var configPath = FilePath;
            if (File.Exists(configPath))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // For pretty-printing
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // For camelCase naming
                };
                string jsonString = File.ReadAllText(configPath);
                var file = JsonSerializer.Deserialize<ApplicationSettings>(jsonString, options);

                if (file != null)
                    return file;
            }

            var settings = new ApplicationSettings();
            settings.Save();
            return new ApplicationSettings();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // For pretty-printing
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // For camelCase naming
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText("config.json", jsonString);
        }
    }
}
