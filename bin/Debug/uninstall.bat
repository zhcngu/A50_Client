@echo off 
@title ж��Windows����
path %SystemRoot%\Microsoft.NET\Framework\v4.0.30319
echo==============================================================
echo=
echo          windows����ж��
echo=
echo==============================================================
@echo off 
InstallUtil.exe /u  %~dp0\OPC_UA_Client_A50.exe
pause