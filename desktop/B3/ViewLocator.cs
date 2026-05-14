using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using B3.Services;
using B3.ViewModels;

namespace B3;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        var viewModelType = param.GetType();
        var name = viewModelType.FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = ResolveViewType(viewModelType, name);

        if (type != null)
        {
            try
            {
                if (Activator.CreateInstance(type) is Control control)
                {
                    return control;
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"View 建立失敗: {name}", ex);
                return new TextBlock { Text = "View Create Error: " + name };
            }
        }

        LoggerService.LogWarning($"找不到對應 View: {name}");
        return new TextBlock { Text = "Not Found: " + name };
    }

    private static Type? ResolveViewType(Type viewModelType, string fullViewTypeName)
    {
        // Prefer the same assembly as the ViewModel first.
        var type = viewModelType.Assembly.GetType(fullViewTypeName, throwOnError: false);
        if (type != null)
        {
            return type;
        }

        // Fallback to all loaded assemblies.
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(fullViewTypeName, throwOnError: false))
            .FirstOrDefault(found => found != null);
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
