$curFolder = $PSScriptRoot
New-Service -Name "@ServiceName" -BinaryPathName ($curFolder+"/SDK_Interface.exe")
