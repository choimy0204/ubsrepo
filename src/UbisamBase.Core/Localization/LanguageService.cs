using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UbisamBase.Core.Localization;

/// <summary>
/// "Lang/{culture}.json" (key-value) 파일을 읽어 런타임에 언어를 전환한다.
/// XAML에서는 인덱서 바인딩으로 쓴다: {Binding [KeyName], Source={StaticResource Lang}}
/// </summary>
public sealed class LanguageService : INotifyPropertyChanged
{
    private readonly string langDir;
    private Dictionary<string, string> current = new();
    private string currentCulture;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> AvailableCultures { get; private set; } = Array.Empty<string>();

    public string CurrentCulture
    {
        get => currentCulture;
        set
        {
            if (currentCulture == value)
            {
                return;
            }

            currentCulture = value;
            Load(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
            // 인덱서 값이 통째로 바뀌었다는 WPF 표준 표기: 바인딩 엔진이 이걸 보고 모든 [key] 바인딩을 다시 읽는다.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    public string this[string key] => current.TryGetValue(key, out var value) ? value : key;

    public LanguageService(string basePath, string defaultCulture = "ko-KR")
    {
        langDir = Path.Combine(basePath, "Lang");
        currentCulture = defaultCulture;

        try
        {
            Directory.CreateDirectory(langDir);
            AvailableCultures = Directory.GetFiles(langDir, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name != null)
                .Select(name => name!)
                .ToList();
        }
        catch
        {
            AvailableCultures = Array.Empty<string>();
        }

        Load(currentCulture);
    }

    private void Load(string culture)
    {
        var path = Path.Combine(langDir, $"{culture}.json");
        if (!File.Exists(path))
        {
            current = new Dictionary<string, string>();
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            current = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch
        {
            current = new Dictionary<string, string>();
        }
    }
}
