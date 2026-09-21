using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.SettingItems;

public partial class SettingsPropertyGridViewModel : ObservableObject
{
    public ObservableCollection<PropertyRow> Rows { get; } = new();

    public void Load(object? settingsObject)
    {
        Rows.Clear();
        if (settingsObject == null)
        {
            return;
        }

        foreach (var prop in settingsObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead)
            {
                continue;
            }

            var browsable = prop.GetCustomAttribute<System.ComponentModel.BrowsableAttribute>();
            if (browsable is { Browsable: false })
            {
                continue;
            }

            Rows.Add(new PropertyRow(settingsObject, prop));
        }
    }
}
