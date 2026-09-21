using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UbisamBase.Core.Setup;

namespace UbisamBase.Core.Bootstrap;

/// <summary>
/// Shell.exe가 실행되는 폴더에서 IAppSetup을 구현한 dll을 찾아 로드한다.
/// </summary>
internal static class AppSetupLoader
{
    public static IAppSetup Discover(string basePath)
    {
        foreach (var dllPath in Directory.GetFiles(basePath, "*.dll"))
        {
            var setupType = TryFindAppSetupType(dllPath);
            if (setupType != null)
            {
                return (IAppSetup)Activator.CreateInstance(setupType)!;
            }
        }

        throw new InvalidOperationException(
            $"IAppSetup을 구현한 dll을 '{basePath}' 폴더에서 찾지 못했습니다. " +
            "프로젝트가 이 Shell과 같은 출력 폴더에 빌드되어 있는지 확인하세요.");
    }

    private static Type? TryFindAppSetupType(string dllPath)
    {
        try
        {
            // 이미 로드되어 있는 어셈블리(Core, CommunityToolkit.Mvvm 등 Shell이 참조하는 것들)를
            // Assembly.LoadFrom으로 다시 로드하면 동일 이름의 "중복 어셈블리"가 생겨서
            // pack://application:,,,/... 리소스 조회가 깨진다. 이미 로드된 게 있으면 그걸 재사용한다.
            var simpleName = Path.GetFileNameWithoutExtension(dllPath);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase))
                ?? Assembly.LoadFrom(dllPath);

            return GetLoadableTypes(assembly)
                .FirstOrDefault(t => typeof(IAppSetup).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false });
        }
        catch
        {
            // 네이티브 dll, 의존성이 안 맞는 dll 등은 후보에서 제외한다.
            return null;
        }
    }

    private static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}
