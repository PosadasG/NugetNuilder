using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace NugetBuilder
{
    public class PackageCreator
    {


        public void Pack(string nuspecFile, string DirectoryOutput)
        {

            Process nugetProcess = new Process();

            try
            {

                Assembly nuget = Assembly.Load(NugetBuilder.Properties.Resources.nuget);
             
                string nugetPackageDir = Path.Combine(DirectoryOutput + "\\" + Program.outputFolder);
                

                File.WriteAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + "nuget.exe", NugetBuilder.Properties.Resources.nuget);
                System.Threading.Thread.Sleep(3000);
                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\nuget.exe"))
                {
                    nugetProcess.StartInfo.UseShellExecute = false;
                    nugetProcess.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"\nuget.exe";
                    nugetProcess.StartInfo.Arguments = "pack " + nugetPackageDir + "\\" + nuspecFile + ".nuspec " + "-OutputDirectory " + nugetPackageDir;
                    nugetProcess.StartInfo.CreateNoWindow = true;
                    nugetProcess.EnableRaisingEvents = true;
                    nugetProcess.Start();
                    nugetProcess.WaitForExit();
                    nugetProcess.Close();

                    File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + @"\nuget.exe");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }

        }
    }
}
