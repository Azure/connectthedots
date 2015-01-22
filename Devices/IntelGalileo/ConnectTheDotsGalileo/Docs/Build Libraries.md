##Requirements for OpenSSL
* Install Git (chose option to add Git to the system PATH during installation) - http://git-scm.com/download/win
* Install Perl (chose option to add Git to the system PATH during installation) - https://www.perl.org/get.html
* Install Visual Studio 2013 

##OpenSSL
* Create solder `C:\Source`
* Copy `openssl-debug.bat`, `openssl-release.bat`, `windowsiot.patch`, and `windowsiot_dbg.patch` to `C:\Source`
* Launch `openssl-release.bat` or `openssl-debug.bat` (depend on your needs)
* Find build of `openssl-1.0.2-beta3` in `C:\Source\openssl\builds` (or `C:\Source\openssl\builds_dbg` if Debug configuration was built)

##Requirements for Apache Qpid Proton C
* Build OpenSSL
* Install Cmake (chose option to add Git to the system PATH during installation) - http://www.cmake.org/download/
* Install Python 2.7 (chose option to add Git to the system PATH during installation) - https://www.python.org/downloads/windows/

##Apache Qpid Proton C
* Copy `proton-release.bat` and `proton-debug.bat` to `C:\Source`
* Launch `proton-release.bat` or `proton-debug.bat` (depends on your needs)
* Find build of `qpid-proton 0.8` in `C:\Source\proton\builds` (or `C:\Source\proton\builds_dbg` if Debug configuration was built)

##References 
To understand better, please, read following articles:
* http://pjdecarlo.com/2015/01/porting-open-source-libraries-to-windows-for-iot-mincore.html
* https://code.msdn.microsoft.com/windowsazure/Using-Apache-Qpid-Proton-C-afd76504