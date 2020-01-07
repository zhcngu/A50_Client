%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe "%~dp0OPC_UA_Client_A50.exe"

Net Start amsService

sc config amsService start= auto

pause