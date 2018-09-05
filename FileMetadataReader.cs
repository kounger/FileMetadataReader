using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace FileMetadataReader
{
    class FileMetadataReader
    {
        static void Main(string[] args)
        {
            Console.WriteLine("############################################################################");
            Console.WriteLine("########################### FILE METADATA READER ###########################");
            Console.WriteLine("############################################################################");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Enter a valid file path:");

                string filePath = Console.ReadLine();

                //Return with an error message if the given file path doesn't exist:
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("This file does not exist. Please enter a valid file path.");
                    continue;
                }

                ShellFile file = ShellFile.FromFilePath(filePath);

                ShellPropertyCollection propertyCollection = file.Properties.DefaultPropertyCollection;

                //Properties of type System.PropertyName should be listed on top.
                IEnumerable<IShellProperty> sortedPropertiesSystem =    from c in propertyCollection
                                                                        where c.CanonicalName != null && c.ValueAsObject != null
                                                                        let cat = c.CanonicalName.Split('.')
                                                                        where cat.Length == 2
                                                                        orderby c.CanonicalName
                                                                        select c;

                //The rest of the properties of type System.SomeOtherClass.PropertyName should be listed next.
                IEnumerable<IShellProperty> sortedPropertiesRest =      from c in propertyCollection
                                                                        where c.CanonicalName != null && c.ValueAsObject != null
                                                                        let cat = c.CanonicalName.Split('.')
                                                                        where cat.Length > 2
                                                                        orderby c.CanonicalName
                                                                        select c;   

                IEnumerable<IShellProperty> sortedProperties = sortedPropertiesSystem.Concat(sortedPropertiesRest);

                //Size of the longest canonical name that gets displayed inside the console.
                int sizeMaxCanName = propertyCollection
                                        .Where(s => s.CanonicalName != null)
                                            .OrderByDescending(s => s.CanonicalName.Length)
                                                .First()
                                                    .CanonicalName.Length;

                int counter = 0;

                try
                {
                    //Print properties using the installed windows culture:
                    foreach (var property in sortedProperties)
                    {
                        string canonicalName = property.CanonicalName;
                        string propertyValue = property.FormatForDisplay(PropertyDescriptionFormatOptions.None);
                        counter++;

                        Console.WriteLine(canonicalName.PadRight(sizeMaxCanName + 10, '.') + " " + propertyValue);
                    }
                }
                catch (ShellException)
                {
                    Console.WriteLine("A ShellException occured. Please enter the file path again.");
                    continue;
                }                

                Console.WriteLine("Counted Items: " + counter);              
            }
        }
    }
}