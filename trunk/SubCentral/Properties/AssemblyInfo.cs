using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SubCentral")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SubCentral")]
[assembly: AssemblyCopyright("Copyright © 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7bd556ad-be7d-489a-894c-bb93fe04b8b6")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
// AssemblyVersion is updated by Pre-Build step
// Assume TortoiseSVN is in enviroment to run: subwcrev.exe
//[assembly: AssemblyVersion("0.9.1.0")]
//[assembly: AssemblyFileVersion("0.9.1.0")]

// MediaPortal plugin version compatibility
[assembly: CompatibleVersion("1.2.100.0")]
[assembly: UsesSubsystem("MP.SkinEngine")]
[assembly: UsesSubsystem("MP.Externals.MediaInfo")]
[assembly: UsesSubsystem("MP.Config")]