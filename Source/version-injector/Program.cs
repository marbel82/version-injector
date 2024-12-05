using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace version_injector
{
    public static class Program
    {
        const string REPL_COMMITS_COUNT_FP = "$COMMITS_COUNT_FP$";

        public static int Main(string[] args)
        {
            //new FunctionsTest().RunTests();

            if (args.Length == 0)
            {
                Console.WriteLine("version_injector.exe (2021-2025)");
                Console.WriteLine("    /template=<template_path>");
                Console.WriteLine("    /output=<output_path>");
                Console.WriteLine("");
                Console.WriteLine($"    {REPL_COMMITS_COUNT_FP} => commits number from @ (select first parent in merge) ");
                Console.WriteLine("");
                Console.WriteLine("  By: marbel82");
                return -1;
            }

            Go(args);

            return 0;
        }

        public static void Go(string[] args)
        {
            string templatePath = Functions.GetArgument(args, "/template");
            string outputPath = Functions.GetArgument(args, "/output");

            string template = File.ReadAllText(templatePath);

            bool oldOutputExists = File.Exists(outputPath);
            string oldOutput = oldOutputExists ? File.ReadAllText(outputPath) : "";

            string newOutput = ReplaceSpecialVariables(template);

            if (oldOutput != newOutput)
            {
                File.WriteAllText(outputPath, newOutput);
            }
        }

        public static string ReplaceSpecialVariables(string template)
        {
            var sb = new StringBuilder();
            sb.Append(template);

            if (template.Contains(REPL_COMMITS_COUNT_FP))
            {
                int fpc = ReadGitFirstParentCount();

                sb.Replace(REPL_COMMITS_COUNT_FP, fpc.ToString());
            }

            return sb.ToString();
        }

        public static int ReadGitFirstParentCount()
        {
            var psi = new ProcessStartInfo("git.exe")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                Arguments = "rev-list --count --first-parent @",
            };

            var p = Process.Start(psi);

            string outStr = p.StandardOutput.ReadToEnd();
            //string errStr = p.StandardError.ReadToEnd();

            if (p.ExitCode != 0)
                throw new Exception($"git.exe returned {p.ExitCode}");

            return int.Parse(outStr);
        }

    }

    static class Functions
    {
        public static string GetArgument(string[] args, string argName)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith(argName))
                {
                    var p = arg.IndexOf('=');
                    if (p > 0)
                    {
                        return arg[(p + 1)..];
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            return null;
        }

    }

    class FunctionsTest
    {
        public void RunTests()
        {
            GetArgumentTest();
        }

        public void GetArgumentTest()
        {
            var ret = Functions.GetArgument(new[] { @"/template=c:\dir\file.cs" }, "/template");
            Debug.Assert(ret == @"c:\dir\file.cs");

            ret = Functions.GetArgument(new[] { "/a" }, "/a");
            Debug.Assert(ret == "");

            ret = Functions.GetArgument(new[] { "/z" }, "/a");
            Debug.Assert(ret == null);

            ret = Functions.GetArgument(new[] { @"/template=c:\dir\file.cs" }, "/template");
            Debug.Assert(ret == @"c:\dir\file.cs");
        }
    }
}
