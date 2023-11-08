
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NugetBuilder
{

    public class Nuget
    {
        public string name { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string copyright { get; set; }
        public string owners { get; set; }
        //Array of string it's for files list per framework
        public Dictionary<string, string[]>[] frameworks { get; set; }
        public bool interop { get; set; }
    }

    class Program
    {

        //Show help commands
        public static void ShowUsage()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Usage: NugetBuilder.exe <command> [-options] [/args] ");
            Console.WriteLine("Available commands:");
            Console.WriteLine(" ");
            Console.WriteLine("PackDir:     Creates a package from a provided PATH Directory.");
            Console.WriteLine("             PackDir use 2 options -a (all)  or -s (single).");
            Console.WriteLine("             Option -a creates a package with all the files inside the Directory");
            Console.WriteLine("             (e.g.)  nugetBuilder.exe PackDir -a < PathDir> <'name(Withot blank spaces)'> <version> <'description'>");

            Console.WriteLine("             Option -s creates a package per each file inside the Directory");
            Console.WriteLine("             (e.g.)  nugetBuilder.exe PackDir -s <PathDir> <version> <'description'>");
            Console.WriteLine("             To create a package with COM interop files use the arg /interop ");
            Console.WriteLine("             (e.g.)  NugetBuilder.exe PackDir -a /interop <PathDir> <'name(Withot blank spaces)'> <version> <'description'>");

            Console.WriteLine("PackJSON:    Creates a package from a provided JSON file");
            Console.WriteLine("             (e.g.)  nugetBuilder.exe PackJSON  <JSON file PATH>");
            Console.WriteLine("             To create a package with COM interop files check that the value interop is set up to true in the JSON file");

           

        }

        //XML templates for nuspec and targets files
        public static string nuspecSampleXml = string.Format("<?xml version='1.0' encoding='utf-8'?><package xmlns='http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd'><metadata><id></id><version></version><description></description><copyright>Copyright (YEAR) Aspen Technology, Inc.</copyright><owners>Aspen Technology, Inc.</owners><authors>Aspen Technology, Inc.</authors><requireLicenseAcceptance>false</requireLicenseAcceptance><dependencies></dependencies><references></references></metadata><files></files></package>");
        public static string targetsSampleInterop = string.Format("<?xml version='1.0' encoding='utf-8' ?><Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><Target Name='EmbedReferencedAssemblies' AfterTargets='ResolveAssemblyReferences'><ItemGroup><None Include = '$(MSBuildThisFileDirectory)\\..\\lib\\net48\\*.*'><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None></ItemGroup><ItemGroup><ReferencePath><EmbedInteropTypes>true</EmbedInteropTypes></ReferencePath></ItemGroup></Target></Project>");
        public static string targetsSampleManaged = string.Format("<?xml version='1.0' encoding='utf-8' ?><Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><Target Name='EmbedReferencedAssemblies' AfterTargets='ResolveAssemblyReferences'><ItemGroup><None Include ='$(MSBuildThisFileDirectory)\\..\\lib\\net48\\*.*'><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory ></None></ItemGroup><ItemGroup><ReferencePath><EmbeddedFiles >true</EmbeddedFiles></ReferencePath></ItemGroup></Target></Project>");
       //Folder output of the files
        public static string outputFolder = "nugetPackages";
        //Supported frameworks
        public static string[] frameworksList = { "net48", "net5.0", "netstandard2.0" };
       

        static void Main(string[] args)
        {

          
            if (args.Length == 0) { ShowUsage();  return; }

            //Add time to attach process and test
            //System.Threading.Thread.Sleep(
            //(int)System.TimeSpan.FromSeconds(15).TotalMilliseconds);


            //========================================
            // Create .nuspec files from a directory path provided
            //========================================

            if (args[0].Contains("PackDir"))

            {
                if (args.Length < 2)
                {
                    ShowUsage();
                    return;
                }
                else
                {

                    string option = args[1];
                   

                    if (option != "-a" && option != "-s")
                    {
                        ShowUsage();
                        return;
                    }

                    else
                    {
                        PackDir nuspecFromDir = new PackDir();
                       if(nuspecFromDir.CreateNuspectFromDir(args))
                        {
                            Console.WriteLine("End of the process");
                            return;
                        }
                        
                    }

                }
            }


            //=============================================
            //Create a package from JSON file
            //=============================================
            if (args[0].Contains("PackJSON"))
            {
                if(args.Length < 2)
                {
                    ShowUsage();
                }
                else
                {
                    PackJSON pj = new PackJSON();
                    string path = args[1];
                    bool successJson = true;
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(path);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        //Get all json files inside
                        string[] files = System.IO.Directory.GetFiles(path, "*.json");
                        if(files.Length == 0)
                        {
                            Console.WriteLine("Any JSON file was found");
                        }
                        else
                        {
                            foreach(var file in files)
                            {
                                successJson= pj.createNuspecFromJSON(file);
                            }

                        }
                    }
                    else
                    {

                        successJson= pj.createNuspecFromJSON(path);

                    }

                    if (successJson)
                    {
                        Console.WriteLine("End of the process");
                        return;
                    }

                     
                }
               
            }


                //===================================
                //Pack All json files inDir
                //===================================
                if (args.Length == 0) {
                ShowUsage();
                return;
            }
                 
        }
    }

}
