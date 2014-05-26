using Microsoft.Ajax.Utilities;
using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PvcPlugins
{
    public class PvcAjaxmin : PvcPlugin
    {
        private string commandLineSwitches;

        public PvcAjaxmin(string commandLineSwitches = "") 
        {
            this.commandLineSwitches = commandLineSwitches;
        }

        public override string[] SupportedTags
        {
            get
            {
                return new[] { ".css", ".js" };
            }
        }

        public override IEnumerable<PvcCore.PvcStream> Execute(IEnumerable<PvcCore.PvcStream> inputStreams)
        {
            var resultStreams = new List<PvcStream>();
            SwitchParser switchParser = null;

            if (!String.IsNullOrEmpty(this.commandLineSwitches))
            {
                switchParser = new SwitchParser();
                switchParser.Parse(this.commandLineSwitches);
            }

            foreach (var inputStream in inputStreams)
            {
                var fileContent = new StreamReader(inputStream).ReadToEnd();
                var minifier = new Minifier();
                string resultContent = null;

                if (inputStream.StreamName.EndsWith(".js"))
                {
                    resultContent = (switchParser != null) ? minifier.MinifyJavaScript(fileContent, switchParser.JSSettings) : minifier.MinifyJavaScript(fileContent);
                }
                else 
                {
                    resultContent = (switchParser != null) ? minifier.MinifyStyleSheet(fileContent, switchParser.CssSettings) : minifier.MinifyStyleSheet(fileContent);              
                }

                foreach (var error in minifier.ErrorList)
                {
                    Console.Error.WriteLine(error.ToString());
                }

                if (resultContent != null)
                {
                    var dirName = Path.GetDirectoryName(inputStream.StreamName);
                    var fileName = Path.GetFileNameWithoutExtension(inputStream.StreamName) + ".min" + Path.GetExtension(inputStream.StreamName);
                    var resultStream = PvcUtil.StringToStream(resultContent, Path.Combine(dirName, fileName));
                    resultStreams.Add(resultStream);
                }
            }

            return resultStreams;
        }
    }
}
