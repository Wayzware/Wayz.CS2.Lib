using Newtonsoft.Json;
using System.IO;
using System.Text;
using UnityEngine;

namespace Wayz.CS2;

public interface IWayzSettingsManager
{
    public bool TryGetSettings<T>(string modIdentifier, string settingName, out T settings);

    public T? GetSettings<T>(string modIdentifier, string settingName);

    public void SaveSettings<T>(string modIdentifier, string settingName, T settings);

    public T GetOrInitializeSettings<T>(string modIdentifier, string settingName) where T : new();
}

public class WayzSettingsManager : IWayzSettingsManager
{
    private readonly string _baseSettingsFolder;

    public WayzSettingsManager()
    {
        _baseSettingsFolder = Path.Combine(Application.persistentDataPath, "ModSettings");
    }

    public T? GetSettings<T>(string modIdentifier, string settingName)
    {
        var settingsPath = Path.Combine(_baseSettingsFolder, modIdentifier, $"{settingName}.json");
        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException($"Settings file not found at {settingsPath}");
        }
        var settingsJson = File.ReadAllText(settingsPath, Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(settingsJson);
    }

    public void SaveSettings<T>(string modIdentifier, string settingName, T settings)
    {
        if(!Directory.Exists(Path.Combine(_baseSettingsFolder, modIdentifier)))
        {
            Directory.CreateDirectory(Path.Combine(_baseSettingsFolder, modIdentifier));
        }
        var settingsPath = Path.Combine(_baseSettingsFolder, modIdentifier, $"{settingName}.json");
        var settingsJson = JsonConvert.SerializeObject(settings);
        File.WriteAllText(settingsPath, settingsJson, Encoding.UTF8);
    }

    public bool TryGetSettings<T>(string modIdentifier, string settingName, out T settings)
    {
        try
        {
            settings = GetSettings<T>(modIdentifier, settingName)!;
            return settings != null;
        }
        catch (FileNotFoundException)
        {
            settings = default!;
            return false;
        }
    }

    public T GetOrInitializeSettings<T>(string modIdentifier, string settingName) where T : new()
    {
        if (TryGetSettings<T>(modIdentifier, settingName, out var settings))
        {
            return settings;
        }
        else
        {
            var defaultSettings = new T();
            SaveSettings(modIdentifier, settingName, defaultSettings);
            return defaultSettings;
        }
    }
}
