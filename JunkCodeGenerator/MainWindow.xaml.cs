using JunkCodeGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JunkCodeGenerator;
using MahApps.Metro.Controls;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace JunkCodeGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            new Thread(Init)
            {
                IsBackground = true
                
            }.Start();
        }

        void Init()
        {
            if(!File.Exists("source.zip"))
            {
                ChangeStatusText("Coudn't find source file!\nMake sure that source.zip is located in same folder.");
                return;
            }
            ChangeStatusText("Loading file...");


            List<string> linesFromAllSources = new List<string>();
            using (FileStream sourceFile = new FileStream("source.zip", FileMode.Open))
            {
                using (ZipArchive zip = new ZipArchive(sourceFile, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        var stream = entry.Open();
                        byte[] bytes;
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            bytes = ms.ToArray();
                        }

                        string result = System.Text.Encoding.UTF8.GetString(bytes);

                        foreach (var line in result.SplitToLines())
                        {
                            linesFromAllSources.Add(line);
                        }
                    }
                }
            }

            ChangeStatusText("Please close steam...");

            while (true)
            {
                Process[] steamProcess = Process.GetProcessesByName("Steam");
                if (steamProcess == null || steamProcess.Length == 0)
                    break;
                Thread.Sleep(200);
            }

            ChangeStatusText("Randomizing code...");

            string source = RandomizeSource(linesFromAllSources);
            ChangeStatusText("Compiling code...");

            if(!Compiler.CompileFromSource(source))
            {
                ChangeStatusText("Please restart the hack, there was an error compiling the source.");
                return;
            }

            ChangeStatusText("Running hack...");

            if(!RunPE.Inject(Process.GetCurrentProcess().MainModule.FileName, File.ReadAllBytes("hack.dat")))
            {
                ChangeStatusText("Please restart the hack as administrator, there was an error executing it.");
                return;
            }
            Environment.Exit(0);
        }

        string RandomizeSource(List<string> linesList)
        {
            JunkCodenz.RemoveComments(ref linesList);
            JunkCodenz.MoveUsing(ref linesList);

            string[] lines = linesList.ToArray();
            JunkCodenz.GenFunc(ref lines);

            string linesToString = "";
            for (int i = 0; i < lines.Length; i++)
            {
                linesToString += lines[i];
            }
            lines = linesToString.Split(new[] { '\r', '\n' });

            JunkCodenz.GenRandomVars(ref lines);

            linesToString = "";
            for (int i = 0; i < lines.Length; i++)
            {
                linesToString += lines[i];
            }
            lines = linesToString.Split(new[] { '\r', '\n' });

            JunkCodenz.GenRandomStaticVars(ref lines);

            linesToString = "";
            for (int i = 0; i < lines.Length; i++)
            {
                linesToString += lines[i];
            }
            lines = linesToString.Split(new[] { '\r', '\n' });

            JunkCodenz.SwapLines(ref lines);

            string source = "";
            for (int i = 0; i < lines.Length; i++)
            {
                source += lines[i] + "\n\r";
            }
            return source;
        }


        void ChangeStatusText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                statusLabel.Content = text;
            });
        }

    }
}
