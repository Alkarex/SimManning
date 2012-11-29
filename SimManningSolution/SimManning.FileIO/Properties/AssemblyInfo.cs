using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("File IO (desktop) extensions for DTU SimManning library for simulation of manning."
#if (DEBUG)
 + " (DEBUG version)"
#endif
)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Technical University of Denmark")]
[assembly: AssemblyProduct("SimManning.IO")]
[assembly: AssemblyCopyright("Copyright © DTU 2009-2012, Alexandre Alapetite")]
[assembly: AssemblyTrademark("SimManning.IO")]
[assembly: AssemblyCulture("")]

//Common Language Specification
//http://msdn.microsoft.com/library/ms182156.aspx
[assembly: CLSCompliant(true)]

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
[assembly: AssemblyVersion("1.3.*")]
[assembly: NeutralResourcesLanguageAttribute("en")]
