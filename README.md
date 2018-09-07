# FileMetadataReader

This little program prints out the metadata / properties which are part of a file in Windows.

---

It asks for a file path and prints out the properties of the file in this format:

>... <br />
>System.GPS.LatitudeDenominator.................. 1; 1; 10000  <br />
>System.GPS.LatitudeNumerator....................... 0; 22; 166799  <br />
>System.GPS.LatitudeRef....................................... S <br />
>...

---

The program is built around the DefaultPropertyCollection property provided by the [Windows API Code Pack](https://github.com/contre/Windows-API-Code-Pack-1.1) library whose description states:
>Gets the collection of all the default properties for this item.

All properties that can be printed can be found inside the [SystemProperties.cs](https://github.com/contre/Windows-API-Code-Pack-1.1/blob/master/source/WindowsAPICodePack/Shell/PropertySystem/SystemProperties.cs) file of the "Windows API Code Pack" library.
