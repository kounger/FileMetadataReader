# FileMetadataReader

This little program prints out the metadata / properties which are part of a file or directory in Windows.

---

It asks for a path to a file or directory and prints out the properties of the file in this format:

>... <br />
>System.GPS.LatitudeDenominator.................. 1; 1; 10000  <br />
>System.GPS.LatitudeNumerator....................... 0; 22; 166799  <br />
>System.GPS.LatitudeRef....................................... S <br />
>...

---

The program uses the [SystemProperties.cs](https://github.com/contre/Windows-API-Code-Pack-1.1/blob/master/source/WindowsAPICodePack/Shell/PropertySystem/SystemProperties.cs) class of the [Windows API Code Pack](https://github.com/contre/Windows-API-Code-Pack-1.1) library to get each property.

A list of all properties that can be printed can also be found inside this [CanonicalNames.xml](https://github.com/kounger/FileMetadataReader/blob/master/FileMetadataReader/FileMetadataReader/CanonicalNames.xml) file.
