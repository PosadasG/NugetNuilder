using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
namespace NugetBuilder
{
    public class PackJSON
    {
        public bool createNuspecFromJSON(string JSONPath)
        {     
            try
            {
                Nuget nugetObj = new Nuget();
                string DirectoryOutput = Path.GetDirectoryName(JSONPath);
                dynamic nugetSettings, frameworksList = null;
                Assembly jsonAssembly = Assembly.Load(NugetBuilder.Properties.Resources.Newtonsoft_Json);

                Type reflectionType = jsonAssembly.GetType("Newtonsoft.Json.JsonConvert");
                System.Reflection.MethodInfo _deserializeObjectMethodInfo = reflectionType.GetMethods().SingleOrDefault(m => m.Name == "DeserializeObject" && !m.IsGenericMethod && m.ReturnType == typeof(object) && m.GetParameters().Count() == 1);
                nugetSettings = _deserializeObjectMethodInfo.Invoke(nugetObj as Nuget, new object[] { File.ReadAllText(JSONPath) });
                var f = "";
                  
                frameworksList = _deserializeObjectMethodInfo.Invoke(f, new object[] { nugetSettings.frameworks.Parent.Value.ToString() });
                
                XDocument xmlDoc = XDocument.Parse(Program.nuspecSampleXml);
                //Retrive Nuget class from JSON file
              
                nugetObj.name = nugetSettings.name.Value;
                nugetObj.version = nugetSettings.version.Value;
                nugetObj.description = nugetSettings.description.Value;
                nugetObj.copyright = "Copyright " + nugetSettings.copyright.Value + " Aspen Technology, Inc. ";
                nugetObj.owners = nugetSettings.owners.Value;
              
               
                bool isInterop = nugetSettings.interop.Value;

                //Get namespace to add new elememts in the file as descendants of the root element
                var root = xmlDoc.Root.GetDefaultNamespace();
                IEnumerable<XElement> elements = xmlDoc.DescendantNodes().OfType<XElement>(); 
                
                //Get Folder directory
                //Set values from Nuget class to the XML file
                elements.Where(n => n.Name.LocalName == "id").FirstOrDefault().SetValue(nugetObj.name);
                elements.Where(n => n.Name.LocalName == "version").FirstOrDefault().SetValue(nugetSettings.version);
                elements.Where(n => n.Name.LocalName == "description").FirstOrDefault().SetValue(nugetSettings.description);
                elements.Where(n => n.Name.LocalName == "copyright").FirstOrDefault().SetValue(nugetSettings.copyright);
                elements.Where(n => n.Name.LocalName == "owners").FirstOrDefault().SetValue(nugetSettings.owners);

                XElement dependencies = elements.Where(n => n.Name.LocalName == "dependencies").FirstOrDefault();
                XElement references = elements.Where(n => n.Name.LocalName == "references").FirstOrDefault();
                XElement files = elements.Where(n => n.Name.LocalName == "files").FirstOrDefault();
                int index = 0;
                // "framewors": [ "net48", "net5.0", "netstandard2.0" ]
                foreach (var framework in frameworksList[0])
                {
                    //Create node group to add dependencies
                    dependencies.Add(new XElement(root + "group", new XAttribute("targetFramework", framework.Name)));

                    //Add references for each file in frameworks list 
                    XElement group = new XElement(root + "group");
                    group.Add(new XAttribute("targetFramework", framework.Name));
                    
                    foreach (var file  in framework.Value)
                    {
                      
                        var doc = Path.GetFileName(file.ToString());
                        string[] ext = { ".exe", ".dll", ".pdb", ".pak", ".bin", ".tlb", ".dat" };
                        bool validExtension = Array.Exists(ext, E => E == Path.GetExtension(doc));
                        
                        if (validExtension)
                        {
                            string output = "lib" + @"\" + framework.Name;
                            //if interop flag is sent just pack interop prefix files
                            if ((isInterop && doc.ToLower().Contains("interop")) || (!isInterop && !doc.ToLower().Contains("interop")))
                            {
                                output = doc.ToLower().Contains("interop") ? "lib" : "lib" + @"\" + framework.Name;
                                XElement reference = new XElement(root + "reference");
                                reference.Add(new XAttribute("file", doc));
                                group.Add(reference);
                            }
                            //Add files
                            //Interop files does not need to be copied in each framework folder
                            if ( (isInterop && doc.ToLower().Contains("interop") && index < 1) || (!isInterop  && !doc.ToLower().Contains("interop") && index < 1))

                            {
                                XElement fileInFiles = new XElement(root + "file");
                                fileInFiles.Add(new XAttribute("src", file));
                                fileInFiles.Add(new XAttribute("target", output));
                                files.Add(fileInFiles);
                            }
                            
                        }
                       
                    }

                    references.Add(group);
                    index++;
                }
                //Add targets file
                files.Add(new XElement(root + "file", new XAttribute("src", nugetObj.name + ".targets"), new XAttribute("target", @"\build")));
                Directory.CreateDirectory(DirectoryOutput + "\\"+ Program.outputFolder);
                xmlDoc.Save(DirectoryOutput+ "\\"+Program.outputFolder+"\\" + nugetObj.name + "." + nugetObj.version + ".nuspec");

                //create targetss
                TargetsCreator tc = new TargetsCreator();
                if(isInterop)
                    tc.CreateTargetsFile(Program.targetsSampleInterop, nugetObj.name, isInterop, DirectoryOutput);
                else
                    tc.CreateTargetsFile(Program.targetsSampleManaged, nugetObj.name, isInterop, DirectoryOutput);
                //==========================================
                //Create .npkg file
                //===========================================
                PackageCreator pc = new PackageCreator();
                pc.Pack(nugetObj.name + "." + nugetObj.version, DirectoryOutput);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Program.ShowUsage();
                return false;
            }
        }
    }
}
