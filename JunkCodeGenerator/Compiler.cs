﻿using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JunkCodeGenerator
{
    class Compiler
    {
        public static bool CompileFromSource(string source, string Icon = null, string[] Resources = null)
        {

            // We declare the new compiler parameters variable
            // that will contain all settings for the compilation.
            CompilerParameters CParams = new CompilerParameters();

            // We want an executable file on disk.
            CParams.GenerateExecutable = true;
            CParams.GenerateInMemory = false;
            CParams.OutputAssembly = "hack.dat";


            string options = "/o- /platform:x86 /target:exe /unsafe";
            // If the icon is not null (as we initialize it), add the corresponding option.
            if (Icon != null)
                options += " /win32icon:\"" + Icon + "\"";

            // Set the options.
            CParams.CompilerOptions = options;
            // We don't care about warnings, we don't need them to show as errors.
            CParams.TreatWarningsAsErrors = false;

            // Add the references to the libraries we use so we can have access
            // to their namespaces.
            CParams.ReferencedAssemblies.Add("System.dll");
            CParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            CParams.ReferencedAssemblies.Add("System.Drawing.dll");
            CParams.ReferencedAssemblies.Add("System.Data.dll");
            CParams.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            // Check if the user specified any resource files.
            // If yes, add then to the stub's resources.
            if (Resources != null && Resources.Length > 0)
            {
                // Loop through all resource files specified in the Resources[] array.
                foreach (string res in Resources)
                {
                    // Add each resource file to the compiled stub.
                    CParams.EmbeddedResources.Add(res);
                }
            }

            // Dictionary variable is used to tell the compiler that we want
            // our file to be compiled for .NET v2
            Dictionary<string, string> ProviderOptions = new Dictionary<string, string>();
            ProviderOptions.Add("CompilerVersion", "v4.0");

            // Now, we compile the code and get the result back in the "Results" variable
            CompilerResults Results = new CSharpCodeProvider(ProviderOptions).CompileAssemblyFromSource(CParams, source);
            // Check if any errors occured while compiling.
            if (Results.Errors.Count > 0)
            {
                // Errors occured, notify the user.
                MessageBox.Show(string.Format("The compiler has encountered {0} errors",
                    Results.Errors.Count), "Errors while compiling");
                foreach (CompilerError Err in Results.Errors)
                {
                    MessageBox.Show(string.Format("{0}\nLine: {1} - Column: {2}\nFile: {3}", Err.ErrorText,
                        Err.Line, Err.Column, Err.FileName), "Error");
                }

                return false;

            }
            else
            {
                // No error was found, return true.
                return true;
            }

        }
    }
}
