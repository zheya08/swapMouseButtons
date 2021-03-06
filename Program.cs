﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwapMouseButton
{
    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwapMouseButton([param: MarshalAs(UnmanagedType.Bool)] bool fSwap);

        enum MouseButtonsSetting
        {
            RightHanded = 0,
            LeftHanded = 1
        }        

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            if (args.Length > 0 && (Regex.IsMatch(args[0], @"[-/][\?h]") || Regex.IsMatch(args[0], @"/[^lrc]")))
            {
                // pattern @"[-/][\?h]" can also be written as @"(-|/)(\?|h)"
                ShowUsage();
            }
            else if (args.Length > 0 && String.Compare(args[0], "/l", true) == 0)
            {
                // set to left handed regardless of current persisted and runtime state
                SetMouseButtonsSetting(MouseButtonsSetting.LeftHanded);
            }
            else if (args.Length > 0 && String.Compare(args[0], "/r", true) == 0)
            {
                // set to right handed regardless of current persisted and runtime state
                SetMouseButtonsSetting(MouseButtonsSetting.RightHanded);
            }
            else if (args.Length > 0 && String.Compare(args[0], "/c", true) == 0)
            {
                // get and display current persisted setting, no runtime setting lookup option
                var currentSetting = GetMouseButtonsSetting();
                if (currentSetting == MouseButtonsSetting.RightHanded) Console.WriteLine("the currently persisted setting is right handed");
                else /* (currentSetting == MouseButtonSettings.LeftHanded) */ Console.WriteLine("the currently persisted setting is left handed");
            }
            else // lookup current persisted setting, no runtime setting lookup option, and swap it
            {
                var currentSetting = GetMouseButtonsSetting();
                if (currentSetting == MouseButtonsSetting.RightHanded) SetMouseButtonsSetting(MouseButtonsSetting.LeftHanded);
                else /* (currentSetting == MouseButtonSettings.LeftHanded) */ SetMouseButtonsSetting(MouseButtonsSetting.RightHanded);
            }
        }

        /// <summary>
        /// gets the mouse buttons setting currently in effect
        /// </summary>
        /// <returns>value indicating whether right or left handed setting is in place</returns>
        static MouseButtonsSetting GetMouseButtonsSetting()
        {
            var mouseButtonsSetting = MouseButtonsSetting.RightHanded;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Mouse"))
            {
                if (key is null) throw new ApplicationException("unable to open mouse settings registry key");
                var kv = key.GetValue("SwapMouseButtons");
                if (kv is null) throw new ApplicationException("unable to open mouse settings registry key value");
                if (Convert.ToInt16(kv) == 1) mouseButtonsSetting = MouseButtonsSetting.LeftHanded;
            }

            return mouseButtonsSetting;
        }

        /// <summary>
        /// sets the mouse buttons setting currently in effect
        /// </summary>
        /// <returns>value indicating whether right or left handed setting is in place</returns>
        static void SetMouseButtonsSetting(MouseButtonsSetting mouseButtonsSetting)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Mouse", true)) // open writable
            {
                if (key is null) throw new ApplicationException("unable to open mouse settings registry key");
                if (mouseButtonsSetting == MouseButtonsSetting.LeftHanded)
                {
                    Console.WriteLine("swapping mouse buttons settings to left handed");
                    SwapMouseButton(true); // change runtime setting
                    try { key.SetValue("SwapMouseButtons", "1", RegistryValueKind.String); } // change persisted setting
                    catch (UnauthorizedAccessException) { Console.WriteLine(
                        "unable to persist change execute from \"run as administrator\" environment"); }
                }
                else /* (mouseButtonsSetting == MouseButtonSettings.RightHanded) */
                {
                    Console.WriteLine("swapping mouse buttons settings to right handed");
                    SwapMouseButton(false); // change runtime setting
                    try { key.SetValue("SwapMouseButtons", "0", RegistryValueKind.String); } // change persisted setting
                    catch (UnauthorizedAccessException) { Console.WriteLine(
                        "unable to persist change execute from \"run as administrator\" environment"); }
                }
            }
        }

        static void ShowUsage()
        {
            var asm = Assembly.GetExecutingAssembly();
            var asmVersion = asm.GetName().Version.ToString();
            // see https://stackoverflow.com/questions/45652783/regex-including-what-is-supposed-to-be-non-capturing-group-in-result
            // for details on differences between @"[^\\]+(?:\.exe)" vs @"[^\\]+(?=\.exe)" vs @"([^\\]+)\.exe" matching
            //var asmName = Regex.Match(Environment.CommandLine, @"[^\\]+(?:\.exe)", RegexOptions.IgnoreCase).Value; // ".exe" included
            var asmName = Regex.Match(Environment.CommandLine, @"[^\\]+(?=\.exe)", RegexOptions.IgnoreCase).Value;
            //var asmName = Regex.Match(Environment.CommandLine, @"([^\\]+)\.exe", RegexOptions.IgnoreCase).Groups[1].Value; 
            //var asmName = Regex.Match(Environment.CommandLine, @"(?<fnm>[^\\]+)(?<ext>\.exe)", RegexOptions.IgnoreCase).Groups["fnm"].Value;
            //var asmName = Path.GetFileName(Environment.CommandLine); // "Illegal characters in path." unless outer quotes and args stripped

            /* const string Status = "in progress"; */ const string Version = "12aug17";
            //Console.WriteLine("\nstatus = " + Status + ", version = " + Version + "\n");  
            Console.WriteLine("\nversion = " + Version + "\n");
            Console.WriteLine("description");
            Console.WriteLine("  command line utility to switch primary and secondary mouse buttons\n");
            Console.WriteLine("usage");
            Console.WriteLine("  " + asmName + " [/l | /r | /c | /h]\n");
            Console.WriteLine("where");
            Console.WriteLine("     = no arguments swaps whatever currently persisted setting is");
            Console.WriteLine("  /l = switches to left handed regardless of current setting");
            Console.WriteLine("  /r = switches to right handed regardless of current setting");
            Console.WriteLine("  /c = displays currently persisted mouse setting");
            Console.WriteLine("  /h = [ | /? | unsupported argument ] shows this usage info\n");
            Console.WriteLine("examples");
            Console.WriteLine("  " + asmName + " ");
            Console.WriteLine("  " + asmName + " /l");
            Console.WriteLine("  " + asmName + " /r");
            Console.WriteLine("");
        }
    }
}
