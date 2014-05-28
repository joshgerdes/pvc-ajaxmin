using Microsoft.Ajax.Utilities;
using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PvcPlugins
{
    public class PvcAjaxmin : PvcPlugin
    {
        private string commandLineSwitches;
        private bool generateSourceMaps;

        public PvcAjaxmin(string commandLineSwitches = "", bool generateSourceMaps = false) 
        {
            this.commandLineSwitches = commandLineSwitches;
            this.generateSourceMaps = generateSourceMaps;
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
                var dirName = Path.GetDirectoryName(inputStream.StreamName);
                var fileName = Path.GetFileNameWithoutExtension(inputStream.StreamName) + ".min" + Path.GetExtension(inputStream.StreamName);
                var resultName = Path.Combine(dirName, fileName);
                var fileContent = new StreamReader(inputStream).ReadToEnd();
                var minifier = new Minifier();                
                var sourceStream = new MemoryStream();
                var outputWriter = new StreamWriter(sourceStream);

                if (inputStream.StreamName.EndsWith(".js"))
                {                    
                    // Currently AjaxMin only supports JS source maps
                    if (this.generateSourceMaps)
                    {
                        var resultMapName = resultName + ".map";
                        var utf8 = new UTF8Encoding(false);
                        var mapStream = new MemoryStream();               
                        var mapWriter = new SourcemapStreamWriter(mapStream, utf8);
                        var sourceMap = new V3SourceMap(mapWriter);

                        if (sourceMap != null)
                        {
                            if (switchParser == null)
                            {
                                switchParser = new SwitchParser();
                            }

                            switchParser.JSSettings.SymbolsMap = sourceMap;
                            switchParser.JSSettings.TermSemicolons = true;

                            sourceMap.StartPackage(resultName, resultMapName);

                            outputWriter.Write(minifier.MinifyJavaScript(fileContent, switchParser.JSSettings));

                            sourceMap.EndPackage();
                            sourceMap.EndFile(outputWriter, "\r\n");

                            sourceMap.Dispose();
                            mapWriter.Flush();

                            resultStreams.Add(new PvcStream(() => mapStream).As(resultMapName));
                        }
                    }
                    else
                    {
                        CodeSettings settings = new CodeSettings();
                        if (switchParser != null)   settings = switchParser.JSSettings;
                        outputWriter.Write(minifier.MinifyJavaScript(fileContent, settings));                  
                    }
                }
                else 
                {
                    CssSettings settings = new CssSettings();
                    if (switchParser != null) settings = switchParser.CssSettings;
                    outputWriter.Write(minifier.MinifyStyleSheet(fileContent, settings));  
                }

                foreach (var error in minifier.ErrorList)
                {
                    Console.Error.WriteLine(error.ToString());
                }

                outputWriter.Flush(); 
                resultStreams.Add(new PvcStream(() => sourceStream).As(resultName));
            }

            return resultStreams;
        }
    }
}
