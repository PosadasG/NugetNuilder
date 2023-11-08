using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NugetBuilder
{
    public class TargetsCreator
    {
        public void CreateTargetsFile(string targetsSample,string packageName,bool isInterop,string pathDirectory)
        {
            try
            {
                XDocument xmlDocTargets = XDocument.Parse(targetsSample);
               
                IEnumerable<XElement> elementsTarget = xmlDocTargets.DescendantNodes().OfType<XElement>();
                XElement ReferencePath ;
                XElement NameTargets;
                NameTargets= elementsTarget.Where(n => n.Name.LocalName == "Target").FirstOrDefault();
                NameTargets.SetAttributeValue("Name", "EmbedReferencedAssemblies_" + packageName);
                if (isInterop)
                    ReferencePath= elementsTarget.Where(n => n.Name.LocalName == "ReferencePath").FirstOrDefault();
                else {
                    ReferencePath = elementsTarget.Where(n => n.Name.LocalName == "ReferencePath").FirstOrDefault();
                }
                if (isInterop)
                    ReferencePath.Add(new XAttribute("Condition", "'%(ReferencePath.NuGetPackageId)' == '" + packageName + "'" + " AND '%(Extension)' == '.dll' "));
                else
                {
                    ReferencePath.Add(new XAttribute("Condition", "'%(ReferencePath.NuGetPackageId)' == '" + packageName + "'" + " AND '%(Extension)' == '.dll' "));

                }
                xmlDocTargets.Save(pathDirectory + "\\"+Program.outputFolder+"\\" + packageName + ".targets");
            }
            catch (Exception e)
            {
                Program.ShowUsage();
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }
        }
    }
}
