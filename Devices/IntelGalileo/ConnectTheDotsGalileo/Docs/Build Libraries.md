##Requirements for OpenSSL
* Install Git (chose option to add Git to the system PATH during installation) - http://git-scm.com/download/win
* Install Perl (chose option to add Perl to the system PATH during installation) - https://www.perl.org/get.html
* Install Visual Studio 2013 

##OpenSSL
* Create a folder `C:\Source`
* Copy `openssl-debug.bat`, `openssl-release.bat`, `windowsiot.patch`, and `windowsiot_dbg.patch` to `C:\Source`
* Launch `openssl-release.bat` or `openssl-debug.bat` (depend on your needs)
* Find the build of `openssl-1.0.2-beta3` in `C:\Source\openssl\builds` (or `C:\Source\openssl\builds_dbg` if it was built in the Debug configuration)

##Requirements for Apache Qpid Proton C
* Build OpenSSL
* Install Cmake (chose option to add Cmake to the system PATH during installation) - http://www.cmake.org/download/
* Install Python 2.7 (chose option to add Python to the system PATH during installation) - https://www.python.org/downloads/windows/

##Apache Qpid Proton C
* Copy `proton-release.bat`, `proton-debug.bat`, `windowsproton06.patch` to `C:\Source`
* Launch `proton-release.bat` or `proton-debug.bat` (depends on your needs)
* Find the build of `qpid-proton 0.6` in `C:\Source\proton\builds` (or `C:\Source\proton\builds_dbg` if it was built in the Debug configuration)

##References 
To understand better, please, read following articles:
* http://pjdecarlo.com/2015/01/porting-open-source-libraries-to-windows-for-iot-mincore.html
* https://code.msdn.microsoft.com/windowsazure/Using-Apache-Qpid-Proton-C-afd76504

##Product Versions
Scripts have been tested on Windows 8.1 in environment with: 
* CMake 3.1.0
* git 1.9.5
* Perl 5.20.1
* Python 2.7.9
* Visual Studio Ultimate 2013 Update 4