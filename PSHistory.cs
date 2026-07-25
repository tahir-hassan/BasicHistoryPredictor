using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicHistoryPredictor
{
    public static class PSHistory
    {
        public static string[] GetHistoryLines(string historySavePath)
        {
            var text = File.ReadAllText(historySavePath);
            var lines = text.Split(["\r\n"], StringSplitOptions.None);
            return lines;
        }

        public static void SetHistoryLines(string historySavePath, string[] lines)
        {
            var text = String.Join("\r\n", lines);
            File.WriteAllText(historySavePath, text);
        }
    }
}
