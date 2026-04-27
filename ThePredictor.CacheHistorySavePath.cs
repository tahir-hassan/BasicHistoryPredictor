using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace BasicHistoryPredictor
{
    public partial class ThePredictor
    {
        private string? _historySavePath;
        public void CacheHistorySavePath()
        {
            using var ps = PowerShell.Create();
            ps.Runspace = _runspace;
            _historySavePath = 
                ps.AddCommand("Get-PSReadLineOption")
                .AddCommand("Select-Object").AddParameter("ExpandProperty", "HistorySavePath")
                .InvokeAndCleanup<string>()?.FirstOrDefault();
        }
    }
}
