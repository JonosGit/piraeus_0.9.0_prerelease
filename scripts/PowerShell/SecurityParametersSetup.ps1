#Parameters required for Piraeus PowerShell commands

#URL of <scheme>://<authority> of the Piraeus Web Gateway
$url = "http://piraeus.eastus.cloudapp.azure.com"  #Replace with Host name or IP address of the Piraeus Web Gateway
#$url = "http://localhost:1733" (Use for localhost testing without dockerize flag = false for Web gateway, TCP gateway, and Piraeus.Silo)

#Access key for Piraeus Management API
$key = "12345678"

#Get the security token for the management API to call other PowerShell commands
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key $key

#Use the $url and $token parameters for Piraeus PowerShell commands.