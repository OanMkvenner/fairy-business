@echo off
ECHO "%~1"
@echo on
F:\Programming\Libraries\Adroid_SDK\platform-tools\adb connect 192.168.178.37:5555
F:\Programming\Libraries\Adroid_SDK\platform-tools\adb install -r -d "%~1"
pause