using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace FileMetadataReader
{
    static class FileMetadataReader
    {
        static private List<string> canonicalNameList;
        static private bool restartProgram = false;

        /// <summary>
        /// This method initializes the program and repeats the main procedure after it finished
        /// or had to return after an error.
        /// </summary>
        public static void run()
        {
            Console.WriteLine("################################################################################");
            Console.WriteLine("############################# FILE METADATA READER #############################");
            Console.WriteLine("################################################################################");

            readCanonicalNames();
            while (true) mainProcedure(); 
        }

        /// <summary>
        /// Each property has a canonical name which can be used to request the value of a property.
        /// These canonical names are saved in an xml-file and are loaded into a list during startup.
        /// </summary>
        private static void readCanonicalNames()
        {           
            Assembly asm = Assembly.GetExecutingAssembly();
            XDocument xDoc;

            using (Stream stream = asm.GetManifestResourceStream("FileMetadataReader.CanonicalNames.xml"))
            {
                xDoc = XDocument.Load(stream);
            }

            canonicalNameList = xDoc.Descendants("CanonicalName")
                                    .Select(s => s.Value)
                                        .ToList();
        }

        /// <summary>
        /// This method asks the user for a file path and will return all 
        /// available file property values inside the console. 
        /// </summary>
        private static void mainProcedure()
        {
            //If a method sets this flag to true, this mainProcedure will restart:
            restartProgram = false;
            //Ask the user to enter a file path:
            Console.WriteLine();
            Console.WriteLine("Enter a valid file or folder path:");
            string filePath = Console.ReadLine();
            //Check if the file path is valid:
            checkFilePath(filePath);
            if (restartProgram) return;
            //Create a shell object that allows the retrieval of property values:
            ShellObject item = createShellObject(filePath);
            if (restartProgram) return;
            //Retrieve all properties:
            List<IShellProperty> propertyList = collectProperties(item, canonicalNameList);
            //Print all properties with their canonical names:
            printProperties(propertyList);
        }

        /// <summary>
        /// This method checks if the given file path is valid. If that is not the case it 
        /// will set the restartProgram flag to true. 
        /// </summary>
        private static void checkFilePath(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                Console.WriteLine("This file does not exist. Please enter a path to an existing file or directory.");
                restartProgram = true;
            }            
        }

        /// <summary>
        /// This method creates a shell object from a file path. This shell object will 
        /// allow the retrieval of property values.
        /// </summary>
        private static ShellObject createShellObject(string filePath)
        {
            try
            {
                return ShellContainer.FromParsingName(filePath);
            }
            catch (ShellException)
            {
                Console.WriteLine("Unable to create a ShellObject from this input. Please enter a path to an existing file or directory.");
                restartProgram = true;
            }

            return null;
        }

        /// <summary>
        /// This method uses a list of canonical property names to extract all file properties
        /// whose value isn't null.
        /// </summary>
        private static List<IShellProperty> collectProperties(ShellObject shellItem, List<string> canonicalNames)
        {
            List<IShellProperty> propertyList = new List<IShellProperty>();

            foreach (string canonicalName in canonicalNames)
            {
                IShellProperty property = shellItem.Properties.GetProperty(canonicalName);

                if (property.ValueAsObject != null)
                {
                    propertyList.Add(property);
                }
            }

            return propertyList;
        }
        
        /// <summary>
        /// This method formats each property value and prints them inside the
        /// console with their canonical names.
        /// </summary>
        private static void printProperties(List<IShellProperty> propertyList)
        {
            int sizeMaxCanName = getLongestCanonicalName(propertyList);            
            int countedProperties = 0;

            //Set window width to maximum possible text row width:
            if(sizeMaxCanName + 112 <= Console.LargestWindowWidth)
            {
                Console.SetWindowSize(sizeMaxCanName + 112, Console.WindowHeight);
            }            

            //Prints each property value with its canonical name:
            foreach (IShellProperty property in propertyList)
            {
                string canonicalName = property.CanonicalName;
                string propertyValue = "";
                countedProperties++;

                try
                {
                    //Gets a string representation of the property with a proper format using the installed windows culture:
                    propertyValue = property.FormatForDisplay(PropertyDescriptionFormatOptions.None);
                }
                catch (ShellException)
                {
                    //Return the property value as a string if formatting fails and a ShellException was thrown:
                    propertyValue = getStringPropertyValue(property);
                }

                //Change the property text value to a proper output format when the text is too long:
                propertyValue = FileMetadataReader.insertWordWrap(propertyValue, sizeMaxCanName);

                //Print the canonical name with PadRight to have each property text value on the same width:
                Console.WriteLine(canonicalName.PadRight(sizeMaxCanName + 10, '.') + " " + propertyValue);
            }

            Console.WriteLine("Counted Items: " + countedProperties);
        }

        /// <summary>
        /// This method will return the size of the longest canonical name that will
        /// be displayed inside the console.
        /// </summary> 
        private static int getLongestCanonicalName(List<IShellProperty> propertyList)
        {
            return propertyList
                    .Select(s => s.CanonicalName)
                        .Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur)
                            .Length;
        }

        /// <summary>
        /// This method tries to get the string representation of an property object.
        /// </summary>
        private static string getStringPropertyValue(IShellProperty property)
        {
            try
            {                
                return property.ValueAsObject.ToString();
            }
            catch
            {
                return "Error: Unable to print this property as a text.";
            }
        }

        /// <summary>
        /// This method formats the text value of a property in a way that a new line 
        /// and an indention is inserted after every 100 characters.
        /// </summary>
        private static string insertWordWrap(string propertyText, int sizeMaxCanName)
        {
            if (propertyText.Length >= 100)
            {
                string restOfText = propertyText;
                propertyText = "";

                while (restOfText.Length > 100)
                {
                    //propertyText = already processed text + new line + idention + next 100 characters
                    propertyText = propertyText + restOfText.Substring(0, 100) + "\n" + new string(' ', sizeMaxCanName + 11);
                    restOfText = restOfText.Substring(100);
                }
                propertyText = propertyText + restOfText;
            }

            return propertyText;
        }

        static void Main(string[] args)
        {
            FileMetadataReader.run();
        }
    }
}