// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="(none)">
//   Copyright © 2013 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace LegoGif
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;

    internal class Program
    {
        private static readonly Lazy<string> LDPath = new Lazy<string>(() =>
        {
            var paths = new string[3];

            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\LDView.exe"))
            {
                paths[0] = key.GetValue("") as string;
            }

            if (paths[0] != null)
            {
                paths[0] = Path.GetDirectoryName(paths[0]);
            }

            paths[1] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LDView");
            paths[2] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LDView");

            return paths.FirstOrDefault(p => p != null && Directory.Exists(p));
        });

        public static void Main(string[] args)
        {
            Console.WriteLine(LDPath.Value);
        }
    }
}
