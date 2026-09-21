using System;
using System.Collections.Generic;

namespace UbisamBase.Core.Modules;

internal sealed class MainTabRegistration
{
    public string Key { get; }
    public string Title { get; }
    public Icon Icon { get; }
    public Func<IServiceProvider, object> ContentFactory { get; }
    public int Order { get; }
    public List<SubTabRegistration> SubTabs { get; } = new();

    public MainTabRegistration(string key, string title, Icon icon, Func<IServiceProvider, object> contentFactory, int order)
    {
        Key = key;
        Title = title;
        Icon = icon;
        ContentFactory = contentFactory;
        Order = order;
    }
}

internal sealed class SubTabRegistration
{
    public string Title { get; }
    public Func<IServiceProvider, object> ContentFactory { get; }
    public int Order { get; }

    public SubTabRegistration(string title, Func<IServiceProvider, object> contentFactory, int order)
    {
        Title = title;
        ContentFactory = contentFactory;
        Order = order;
    }
}
