using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace BasicHistoryPredictor;

internal static class PowerShellExtensions
{
    internal static Collection<T> InvokeAndCleanup<T>(this PowerShell ps)
    {
        var results = ps.Invoke<T>();
        ps.Commands.Clear();

        return results;
    }

    internal static void InvokeAndCleanup(this PowerShell ps)
    {
        ps.Invoke();
        ps.Commands.Clear();
    }
}
