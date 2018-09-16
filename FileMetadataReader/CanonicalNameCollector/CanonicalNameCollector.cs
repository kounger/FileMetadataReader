using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace CanonicalNameCollector
{
    /// <summary>
    /// This class collects the canonical name of each shell property from 
    /// the SystemProperties.cs file of the Windows API Code Pack and
    /// offers a method to serialize them into an XML file. 
    /// </summary>
    [XmlRoot(ElementName = "CanonicalNameCollection")]
    public class CanonicalNameCollector
    {
        private List<Type> sysPropsTypes = new List<Type>();
        [XmlArray("CanonicalNames")]
        [XmlArrayItem("CanonicalName")]
        public List<string> canonicalNames = new List<string>();

        public CanonicalNameCollector(Type type)
        {
            getSysPropsClasses(type);
            collectCanonicalNames(sysPropsTypes);
        }

        public CanonicalNameCollector()
        {
        }

        /// <summary>
        /// This method collects all nested types of the 
        /// SystemProperties class.
        /// </summary>
        public void getSysPropsClasses(Type type)
        {
            Type[] nestedTypes = type.GetNestedTypes();

            foreach (Type nestedType in nestedTypes)
            {
                sysPropsTypes.Add(nestedType);
                getSysPropsClasses(nestedType);
            }
        }

        /// <summary>
        /// This method collects the canonical names of all properties
        /// that are reflected from the given list of types. 
        /// </summary> 
        public void collectCanonicalNames(List<Type> types)
        {
            foreach (Type type in types)
            {
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo propInfo in properties)
                {
                    PropertyKey key = (PropertyKey)propInfo.GetValue(null);
                    ShellPropertyDescription desc = SystemProperties.GetPropertyDescription(key);
                    string canonicalName = desc.CanonicalName;

                    if ((!canonicalNames.Contains(canonicalName)) && canonicalName != null)
                    {
                        canonicalNames.Add(canonicalName);
                    }
                }
            }
        }

        /// <summary>
        /// This method serializes a list of canonical names into 
        /// an XML file.
        /// </summary>
        public void serializeCanonicalNames(string fileName)
        {
            var serializer = new XmlSerializer(typeof(CanonicalNameCollector));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, this);
            }
        }
    }

    /// <summary>
    /// Each shell property has a canonical name as an identifier. This identifier can 
    /// be used to get a specific property value from a shell container (file or folder).
    /// This program collects all canonical names and saves them in an XML file. 
    /// </summary>
    class CanonicalNameSerializer
    {
        static void Main(string[] args)
        {
            Type sysPropsType = typeof(SystemProperties);
            CanonicalNameCollector collector = new CanonicalNameCollector(sysPropsType);

            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CanonicalNames.xml";
            collector.serializeCanonicalNames(savePath);
        }
    }
}