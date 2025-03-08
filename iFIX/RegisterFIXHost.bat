
if NOT '%1' == '' cd /d %1

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe GEIP.Orion.ComAdapterInterfaces.dll
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe GEIP.Orion.ComponentBuilder.dll
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe /tlb GEIP.Orion.DataConversion.dll
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\regasm.exe GEIP.Orion.DotNetComAdapters.dll

SET SYSTEM_OS=
FOR /F "tokens=3" %%A IN ('REG.exe query "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" /v "PROCESSOR_ARCHITECTURE"') DO set  SYSTEM_OS=%%A

if %SYSTEM_OS%==x86 (
	%SystemRoot%\System32\regsvr32.exe /S GEIP.Orion.ComponentWrappers.dll
) else (
	%SystemRoot%\SysWOW64\regsvr32.exe /S GEIP.Orion.ComponentWrappers.dll
)
