using System.Reflection;

[assembly: AssemblyProduct("{assembly_product}")]
[assembly: AssemblyCompany("{assembly_company}")]
[assembly: AssemblyCopyright("{assembly_copyright}")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("{assembly_version}")]
[assembly: AssemblyFileVersion("{assembly_file_version}")]
[assembly: AssemblyInformationalVersion("{assembly_informational_version}")]
