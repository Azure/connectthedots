:: set up the Visual Studio environment
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\bin\vcvars32.bat"
:: clone library's sources
git clone -b 0.8 https://github.com/apache/qpid-proton.git proton
cd proton
:: generate solutions and projects files
set OPENSSL_ROOT_DIR=C:\Source\openssl\builds_dbg
cmake -DCMAKE_CXX_STANDARD_LIBRARIES:STRING=mincore.lib ^
 -DCMAKE_CXX_FLAGS:STRING=" /arch:IA32 /DWIN32 /D_WINDOWS /W3 /GR /EHsc " ^
 -DCMAKE_C_FLAGS:STRING=" /arch:IA32 /DWIN32 /D_WINDOWS /W3 " ^
 -DCMAKE_C_STANDARD_LIBRARIES:STRING=mincore.lib ^
 -DCMAKE_INSTALL_PREFIX:PATH="C:\Source\proton\builds_dbg" ^
 -DCMAKE_EXE_LINKER_FLAGS:STRING=" /machine:X86 /NODEFAULTLIB:kernel32.lib /NODEFAULTLIB:ws2_32.lib /NODEFAULTLIB:gdi32.lib /NODEFAULTLIB:advapi32.lib /NODEFAULTLIB:crypt32.lib /NODEFAULTLIB:user32.lib "
:: compile sources
msbuild Proton.sln /property:Configuration=Debug
:: install binaries to the builds\ folder
msbuild INSTALL.vcxproj /property:Configuration=Debug
cd ..