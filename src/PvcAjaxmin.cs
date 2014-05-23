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
        public override IEnumerable<PvcCore.PvcStream> Execute(IEnumerable<PvcCore.PvcStream> inputStreams)
        {
            var minifyStreams = inputStreams.Where(x => Regex.IsMatch(x.StreamName, @"\.(js|css)$"));
            var resultStreams = new List<PvcStream>();

            foreach (var inputStream in minifyStreams)
            {
                var fileContent = new StreamReader(inputStream).ReadToEnd();
                var minifier = new Minifier();
                var resultContent = inputStream.StreamName.EndsWith(".js") ? minifier.MinifyJavaScript(fileContent) : minifier.MinifyStyleSheet(fileContent);

                foreach (var error in minifier.ErrorList)
                {
                    Console.Error.WriteLine(error.ToString());
                }

                var dirName = Path.GetDirectoryName(inputStream.StreamName);
                var fileName = Path.GetFileNameWithoutExtension(inputStream.StreamName) + ".min" + Path.GetExtension(inputStream.StreamName);

                var resultStream = PvcUtil.StringToStream(resultContent, Path.Combine(dirName, fileName));
                resultStreams.Add(resultStream);
            }

            return inputStreams.Where(x => !minifyStreams.Any(y => y.StreamName == x.StreamName)).Concat(resultStreams);
        }
    }
}
