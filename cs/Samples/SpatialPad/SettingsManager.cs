using System;
using System.IO;
using System.Text.Json;

namespace Volumetric.Samples.SpatialPad
{
    public class SettingsManager<T> where T : new()
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public T Settings { get; private set; }

        public SettingsManager(string appFolderName = "AppName", string fileName = "settings.json")
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appFolderName);

            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, fileName);
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            Load();
        }
        private void Load()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    Settings = JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
                }
                catch
                {
                    Settings = new T();
                    Save();
                }
            }
            else
            {
                Settings = new T();
                Save();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
