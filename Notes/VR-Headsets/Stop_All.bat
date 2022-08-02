net stop "VRG Settings"
taskkill /F /IM "vrserver.exe" /T
taskkill /F /IM "vrcompositor.exe" /T
net stop "SVService" 
net stop "svrgui" 
net stop "Tobii Service"
net stop "SRanipalService"
net stop "Tobii VRU02 Runtime" 
net start
pause
