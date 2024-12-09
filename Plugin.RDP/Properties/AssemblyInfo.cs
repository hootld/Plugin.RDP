using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("a80553e8-cd38-426d-a5a7-2dc08afb8db3")]
[assembly: System.CLSCompliant(false)]

#if NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=90")]
#else

[assembly: AssemblyTitle("Plugin.RDP")]
[assembly: AssemblyDescription("Remote Desktop Protocol Client. Original program: RDCMan")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyProduct("Plugin.RDP")]
[assembly: AssemblyCopyright("Copyright © RDCMan authors & Danila Korablin 2011-2024")]
#endif

/*
if $(ConfigurationName) == Release (
..\..\..\..\ILMerge.exe  "/out:$(ProjectDir)..\bin\$(TargetFileName)" "$(TargetDir)$(TargetFileName)" "$(TargetDir)AxMSTSCLib.DLL" "$(TargetDir)MSTSCLib.DLL" "/lib:..\..\..\SAL\bin"
)
 */