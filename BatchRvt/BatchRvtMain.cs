﻿//
// Revit Batch Processor
//
// Copyright (c) 2017  Daniel Rumery, BVN
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using BatchRvt.ScriptHost;
using System.Runtime.InteropServices;

namespace BatchRvtCommand
{
    public class BatchRvtMain
    {
        private const string BatchRvtConsoleTitle = "Batch Revit File Processor";

        private const string ScriptsFolderName = "Scripts";
        private const string MonitorScriptFilename = "batch_rvt.py";

        private const int WindowWidth = 160;
        private const int WindowHeight = 60;
        private const int BufferWidth = 320;
        private const int BufferHeight = WindowHeight * 50;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            if (HasConsole())
            {
                InitConsole();
            }

            ExecuteMonitorScript();

            return;
        }

        private static void InitConsole()
        {
            Console.Title = BatchRvtConsoleTitle;

            Console.SetWindowSize(
                    Math.Min(WindowWidth, Console.LargestWindowWidth),
                    Math.Min(WindowHeight, Console.LargestWindowHeight)
                );

            Console.SetBufferSize(BufferWidth, BufferHeight);

            return;
        }

        private static void ExecuteMonitorScript()
        {
            var batchRvtPluginFolderPath = GetExecutableFolderPath();
            var engine = ScriptUtil.CreatePythonEngine();

            var mainModuleScope = ScriptUtil.CreateMainModule(engine);

            var scriptsFolderPath = Path.Combine(batchRvtPluginFolderPath, ScriptsFolderName);

            var monitorScriptFilePath = Path.Combine(
                    scriptsFolderPath,
                    MonitorScriptFilename
                );

            ScriptUtil.AddSearchPaths(
                    engine,
                    new[] {
                        scriptsFolderPath,
                        batchRvtPluginFolderPath
                    }
                );

            ScriptUtil.AddBuiltinVariables(
                    engine,
                    new Dictionary<string, object> {
                        { "__scope__", mainModuleScope }
                    }
                );

            ScriptUtil.AddPythonStandardLibrary(mainModuleScope);

            var scriptSource = ScriptUtil.CreateScriptSourceFromFile(engine, monitorScriptFilePath);

            scriptSource.Execute(mainModuleScope);

            return;
        }

        private static string GetExecutableFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static bool HasConsole()
        {
            return (GetConsoleWindow() != IntPtr.Zero);
        }
    }
}