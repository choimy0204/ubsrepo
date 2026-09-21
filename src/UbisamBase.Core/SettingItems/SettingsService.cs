using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace UbisamBase.Core.SettingItems;

public sealed class SettingsService : ISettingsService
{
    private readonly string settingsDir;
    private readonly Dictionary<string, object> items = new();
    // "설정값" 화면(AllSettingsView)에 노출할 키만 따로 추적한다. AddSettingsSub로 등록된 클래스는
    // 자기 전용 화면이 이미 있고, 그 화면의 범용 PropertyGrid는 bool/enum/text만 이해해서
    // ObservableCollection 같은 타입을 넣으면 ToString()으로 잘못 나온다 — 그래서 여기 안 넣는다.
    private readonly HashSet<string> catalogKeys = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SettingsService(string basePath)
    {
        settingsDir = Path.Combine(basePath, "Settings");
        Directory.CreateDirectory(settingsDir);
    }

    public IReadOnlyList<string> RegisteredKeys => catalogKeys.ToList();

    public T Register<T>(string key, T defaultInstance) where T : class, ISettingItem
    {
        var instance = RegisterSettings(key, defaultInstance);
        catalogKeys.Add(key);
        return instance;
    }

    public T RegisterSettings<T>(string key, T defaultInstance) where T : class
    {
        var path = FilePath(key);
        T instance = defaultInstance;

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                instance = JsonSerializer.Deserialize<T>(json, JsonOptions) ?? defaultInstance;
            }
            catch
            {
                instance = defaultInstance;
            }
        }
        else
        {
            // 처음 실행이라 파일이 없으면 기본값으로 바로 만들어둔다.
            File.WriteAllText(path, JsonSerializer.Serialize(defaultInstance, JsonOptions));
        }

        items[key] = instance;
        return instance;
    }

    public T Get<T>(string key) where T : class, ISettingItem => (T)items[key];

    public object GetRaw(string key) => items[key];

    public void Save(string key)
    {
        if (!items.TryGetValue(key, out var obj))
        {
            return;
        }

        var json = JsonSerializer.Serialize(obj, obj.GetType(), JsonOptions);
        File.WriteAllText(FilePath(key), json);
    }

    public void Reload(string key)
    {
        if (!items.TryGetValue(key, out var current))
        {
            return;
        }

        var path = FilePath(key);
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var reloaded = JsonSerializer.Deserialize(json, current.GetType(), JsonOptions);
            if (reloaded == null)
            {
                return;
            }

            // 참조를 바꾸지 않고 값만 옮겨 담는다 — BackupService.Settings처럼 이 인스턴스를
            // 오래 들고 있는 다른 곳이 새로고침 후에도 계속 같은 객체를 보게 하기 위해서다.
            foreach (var prop in current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(current, prop.GetValue(reloaded));
                }
            }
        }
        catch
        {
            // 손상된 파일은 무시하고 현재 값을 유지한다.
        }
    }

    private string FilePath(string key) => Path.Combine(settingsDir, $"{key}.json");
}
