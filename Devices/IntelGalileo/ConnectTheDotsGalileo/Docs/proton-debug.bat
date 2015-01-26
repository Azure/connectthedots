:: set up the Visual Studio environment
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat"
:: clone library's sources
git clone -b 0.6 https://github.com/apache/qpid-proton.git proton
cd proton
set OPENSSL_ROOT_DIR=C:\Source\openssl\builds_dbg
:: Enable Windows
git apply ..\windowsproton06.patch
:: generate solutions and projects files
cmake -DCMAKE_CXX_STANDARD_LIBRARIES:STRING=mincore.lib ^
 -DCMAKE_CXX_FLAGS:STRING=" /arch:IA32 /DWIN32 /D_WINDOWS /W3 /GR /EHsc /IC:/Source/openssl/builds/include" ^
 -DCMAKE_C_FLAGS:STRING=" /arch:IA32 /DWIN32 /D_WINDOWS /W3 /IC:/Source/openssl/builds/include" ^
 -DCMAKE_C_STANDARD_LIBRARIES:STRING=mincore.lib ^
 -DGEN_JAVA:BOOL=OFF ^
 -DCMAKE_INSTALL_PREFIX:PATH="C:\Source\proton\builds_dbg" ^
 -DCMAKE_EXE_LINKER_FLAGS:STRING=" /debug /machine:X86 /NODEFAULTLIB:kernel32.lib /NODEFAULTLIB:ws2_32.lib /NODEFAULTLIB:gdi32.lib /NODEFAULTLIB:advapi32.lib /NODEFAULTLIB:crypt32.lib /NODEFAULTLIB:user32.lib " ^
 -DCMAKE_MODULE_LINKER_FLAGS:STRING=" /debug /machine:X86 /NODEFAULTLIB:kernel32.lib /NODEFAULTLIB:ws2_32.lib /NODEFAULTLIB:gdi32.lib /NODEFAULTLIB:advapi32.lib /NODEFAULTLIB:crypt32.lib /NODEFAULTLIB:user32.lib " ^
 -DCMAKE_SHARED_LINKER_FLAGS:STRING=" /debug /machine:X86 /NODEFAULTLIB:kernel32.lib /NODEFAULTLIB:ws2_32.lib /NODEFAULTLIB:gdi32.lib /NODEFAULTLIB:advapi32.lib /NODEFAULTLIB:crypt32.lib /NODEFAULTLIB:user32.lib "
:: compile sources
msbuild Proton.sln /property:Configuration=Debug
:: install binaries to the builds\ folder
msbuild INSTALL.vcxproj /property:Configuration=Debug
cd ..