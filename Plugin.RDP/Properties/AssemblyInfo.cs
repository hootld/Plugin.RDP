using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("a80553e8-cd38-426d-a5a7-2dc08afb8db3")]
[assembly: System.CLSCompliant(false)]

#if NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=90")]
#else

[assembly: AssemblyDescription("Remote Desktop Protocol Client. Original program: RDCMan")]
[assembly: AssemblyCopyright("Copyright © RDCMan authors & Danila Korablin 2011-2025")]
#endif

/*
if $(ConfigurationName) == Release (
..\..\..\..\ILMerge.exe  "/out:$(ProjectDir)..\bin\$(TargetFileName)" "$(TargetDir)$(TargetFileName)" "$(TargetDir)AxMSTSCLib.DLL" "$(TargetDir)MSTSCLib.DLL" "/lib:..\..\..\SAL\bin"
)
 */