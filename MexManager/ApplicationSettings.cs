using PropertyModels.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace MexManager
{
    public class ApplicationSettings
    {
        private static readonly string FileName = "config.json";

        private static string FilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); } }

        [DisplayName("Melee (v1.02) ISO Path")]
        [Description("Path to Melee ISO Note: only vanilla (USA) (v1.02) is supported")]
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
            string configPath = FilePath;
            if (File.Exists(configPath))
            {
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true, // For pretty-printing
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // For camelCase naming
                };
                string jsonString = File.ReadAllText(configPath);
                ApplicationSettings? file = JsonSerializer.Deserialize<ApplicationSettings>(jsonString, options);

                if (file != null)
                    return file;
            }

            ApplicationSettings settings = new();
            settings.Save();
            return new ApplicationSettings();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true, // For pretty-printing
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // For camelCase naming
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText("config.json", jsonString);
        }
    }
}
