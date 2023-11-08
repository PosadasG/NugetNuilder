using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace NugetBuilder
{
   public class PackDir
    {

        public bool CreateNuspectFromDir(string [] args)
        {
            string option = args[1];
            bool isInterop = args[2].Contains("interop");
            string pathDirectory = isInterop ? args[3] : args[2];
          
            XDocument xmlDoc = XDocument.Parse(Program.nuspecSampleXml);
            //Get namespace to add new elememts in the file as descendants of the root element
            var root = xmlDoc.Root.GetDefaultNamespace();
            IEnumerable<XElement> elements = xmlDoc.DescendantNodes().OfType<XElement>();
            //Get Folder directory
            string[]  filesInDir = Directory.GetFiles(pathDirectory);
            //Retrive Nuget class from JSON file
            Nuget nugetObj = new Nuget();

            //==============================================
            //Create nuspec with all files in directory 
            //=============================================
            #region 
            if (  option.Contains("-a"))
            {
                try
                {
                    //Retrieve nuget object from user preferences
                    string name = isInterop ? args[4] : args[3];
                    string version = isInterop ? args[5] : args[4];
                    string description = isInterop ? args[6] : args[5];
                    nugetObj.name = name;
                    nugetObj.version = version;
                    nugetObj.description = description;
                    nugetObj.owners = "Aspen Technology, Inc.";
                    nugetObj.copyright = "Copyright " + DateTime.Today.Year + " Aspen Technology, Inc. ";
                    //Set values from Nuget class to the XML file
                    elements.Where(n => n.Name.LocalName == "id").FirstOrDefault().SetValue(nugetObj.name);
                    elements.Where(n => n.Name.LocalName == "version").FirstOrDefault().SetValue(nugetObj.version);
                    elements.Where(n => n.Name.LocalName == "description").FirstOrDefault().SetValue(nugetObj.description);
                    elements.Where(n => n.Name.LocalName == "copyright").FirstOrDefault().SetValue(nugetObj.copyright);
                    elements.Where(n => n.Name.LocalName == "owners").FirstOrDefault().SetValue(nugetObj.owners);

                    XElement dependencies = elements.Where(n => n.Name.LocalName == "dependencies").FirstOrDefault();
                    XElement references = elements.Where(n => n.Name.LocalName == "references").FirstOrDefault();
                    XElement files = elements.Where(n => n.Name.LocalName == "files").FirstOrDefault();
                    int index = 0;
                    // "framewors": [ "net48", "net5.0", "netstandard2.0" ],
                    foreach (var framework in Program.frameworksList)
                    {
                        //Create node group to add dependencies
                        dependencies.Add(new XElement(root + "group", new XAttribute("targetFramework", framework)));

                        //Add references for each file in frameworks list 
                        XElement group = new XElement(root + "group");
                        group.Add(new XAttribute("targetFramework", framework));
                        
                        foreach (var file in filesInDir)
                        {

                            var doc = Path.GetFileName(file);
                            string[] ext = { ".exe", ".dll", ".pdb", ".pak", ".bin", ".tlb", ".dat" };
                            bool validExtension = Array.Exists(ext, E => E == Path.GetExtension(doc));
                           
                            if (validExtension)
                            {
                                string output = "lib" + @"\" + framework;
                                //if interop flag is sent just pack interop prefix files
                                if ((isInterop && doc.ToLower().Contains("interop")) || (!isInterop && !doc.ToLower().Contains("interop")))

                                {
                                    output = doc.ToLower().Contains("interop") ? "lib" : "lib" + @"\" + framework;
                                    XElement reference = new XElement(root + "reference");
                                    reference.Add(new XAttribute("file", doc));
                                    group.Add(reference);
                                }
                                //Add files
                                //Interop files does not need to be copied in each target framework folder
                                if ((!isInterop && !doc.ToLower().Contains("interop")) || (isInterop && doc.ToLower().Contains("interop") && index < 1) || (isInterop && !doc.ToLower().Contains("interop")))
                                {
                                    XElement fileInFiles = new XElement(root + "file");
                                    fileInFiles.Add(new XAttribute("src", file));
                                    fileInFiles.Add(new XAttribute("target", output));
                                    files.Add(fileInFiles);
                                }
                            }

                            
                        }

                        references.Add(group);
                        index ++;
                    }
                    //Add targets file
                    files.Add(new XElement(root + "file", new XAttribute("src", nugetObj.name + ".targets"), new XAttribute("target", @"\build")));
                    //save nuspec file
                    Directory.CreateDirectory(pathDirectory + "\\"+Program.outputFolder);
                    xmlDoc.Save(pathDirectory + "\\" + Program.outputFolder + "\\" + nugetObj.name + "." + nugetObj.version + ".nuspec");
                    //create targets
                    TargetsCreator tc = new TargetsCreator();
                    if (isInterop)
                        tc.CreateTargetsFile(Program.targetsSampleInterop, nugetObj.name, isInterop, pathDirectory);
                    else
                        tc.CreateTargetsFile(Program.targetsSampleManaged, nugetObj.name, isInterop, pathDirectory);
                    //==========================================
                    //Create .npkg file
                    //===========================================
                    PackageCreator pc = new PackageCreator();
                    pc.Pack(nugetObj.name + "." + nugetObj.version, pathDirectory);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Program.ShowUsage();
                    return false;
                }
            }
            #endregion

            //=============================================
            //Create nuspec file per each file 
            //=============================================
            #region
            if (option.Contains("-s"))
            {
               
                string version = isInterop ? args[4] : args[3];

                try { 
                   
                    nugetObj.version = version;
                    nugetObj.owners = "Aspen Technology, Inc.";
                    nugetObj.copyright = "Copyright " + DateTime.Today.Year + " Aspen Technology, Inc. ";
                 
                    foreach (var file in filesInDir)
                    {
                        
                        var doc = Path.GetFileName(file);
                        var docName = Path.GetFileNameWithoutExtension(doc);


                        if ((isInterop && doc.ToLower().Contains("interop")) || (!isInterop && !doc.ToLower().Contains("interop")))
                       {
                            elements.Where(n => n.Name.LocalName == "id").FirstOrDefault().SetValue(docName);
                            elements.Where(n => n.Name.LocalName == "version").FirstOrDefault().SetValue(nugetObj.version);
                            elements.Where(n => n.Name.LocalName == "description").FirstOrDefault().SetValue(docName + "assembly files");
                            elements.Where(n => n.Name.LocalName == "copyright").FirstOrDefault().SetValue(nugetObj.copyright);
                            elements.Where(n => n.Name.LocalName == "owners").FirstOrDefault().SetValue(nugetObj.owners);

                            XElement dependencies = elements.Where(n => n.Name.LocalName == "dependencies").FirstOrDefault();
                            XElement references = elements.Where(n => n.Name.LocalName == "references").FirstOrDefault();
                            XElement files = elements.Where(n => n.Name.LocalName == "files").FirstOrDefault();
                            int index = 0;
                            // "framewors": [ "net48", "net5.0", "netstandard2.0" ],
                            foreach (var framework in Program.frameworksList)
                            {
                                //Create node group to add dependencies
                                dependencies.Add(new XElement(root + "group", new XAttribute("targetFramework", framework)));

                                //Add references for each file in frameworks list 
                                XElement group = new XElement(root + "group");
                                group.Add(new XAttribute("targetFramework", framework));
                              
                                string[] ext = { ".exe", ".dll", ".pdb" };
                                bool validExtension = Array.Exists(ext, E => E == Path.GetExtension(doc));

                                //if interop flag is sent just pack interop prefix files
                                if (validExtension)
                                {
                                    string output = "lib" + @"\" + framework;
                                    if ((isInterop && doc.ToLower().Contains("interop")) || (!isInterop))
                                    {
                                        output = doc.ToLower().Contains("interop") ? "lib" : "lib" + @"\" + framework;
                                        XElement reference = new XElement(root + "reference");
                                        reference.Add(new XAttribute("file", doc));
                                        group.Add(reference);
                                    }
                                    //Add files
                                    if (index < 1)
                                    {
                                        XElement fileInFiles = new XElement(root + "file");
                                        fileInFiles.Add(new XAttribute("src", file));
                                        fileInFiles.Add(new XAttribute("target", output));
                                        files.Add(fileInFiles);
                                    }
                                    
                                }

                                references.Add(group);
                                index++;
                            }
                            //Add targets file location to nuspec
                            files.Add(new XElement(root + "file", new XAttribute("src", docName + ".targets"), new XAttribute("target", @"\build")));
                            //Save nuspec file
                            System.IO.Directory.CreateDirectory(pathDirectory + "\\"+ Program.outputFolder);
                            xmlDoc.Save(pathDirectory + "\\" + Program.outputFolder + "\\" + docName + "." + nugetObj.version + ".nuspec");

                            //create targets file
                            TargetsCreator tc = new TargetsCreator();

                            if (isInterop)
                                tc.CreateTargetsFile(Program.targetsSampleInterop, docName, isInterop, pathDirectory);
                            else
                                tc.CreateTargetsFile(Program.targetsSampleManaged, docName, isInterop, pathDirectory);

                            //==========================================
                            //Create .npkg file
                            //===========================================                  
                            PackageCreator pc = new PackageCreator();
                            pc.Pack(docName + "." + nugetObj.version, pathDirectory);
                           

                        }//If                   
                      
                    }//foreach filesInDir

                    return true;
                }
                 catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Program.ShowUsage();
                    return false;
                }
            }
            #endregion

            return true;
        }
    }
}
