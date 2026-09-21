using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.Backup;

public partial class BackupViewModel : ObservableObject
{
    private readonly BackupService service;

    public ObservableCollection<BackupItem> Items { get; }

    [ObservableProperty]
    private string backupTimeText;

    public BackupViewModel(BackupService service)
    {
        this.service = service;
        Items = new ObservableCollection<BackupItem>(service.Settings.Items);
        backupTimeText = service.Settings.BackupTime.ToString(@"hh\:mm");
    }

    [RelayCommand]
    private void AddItem() => Items.Add(new BackupItem());

    [RelayCommand]
    private void RemoveItem(BackupItem? item)
    {
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            ApplyToService();
            service.Save();
            MessageUtil.ShowSavedToast();
        }
        catch
        {
            MessageUtil.ShowErrorToast("저장하지 못했습니다.");
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            service.Reload();
            Items.Clear();
            foreach (var item in service.Settings.Items)
            {
                Items.Add(item);
            }
            BackupTimeText = service.Settings.BackupTime.ToString(@"hh\:mm");
            MessageUtil.ShowSuccessToast("새로고침되었습니다.");
        }
        catch
        {
            MessageUtil.ShowErrorToast("새로고침하지 못했습니다.");
        }
    }

    [RelayCommand]
    private void RunNow()
    {
        // 화면에 보이는 목록 그대로 실행되도록, 실행 전에 서비스 쪽 목록도 동기화한다.
        ApplyToService();
        service.RunBackup();
        MessageUtil.ShowToast("백업을 실행했습니다.");
    }

    private void ApplyToService()
    {
        service.Settings.Items = Items.ToList();
        if (TimeSpan.TryParse(BackupTimeText, out var time))
        {
            service.Settings.BackupTime = time;
        }
    }
}
