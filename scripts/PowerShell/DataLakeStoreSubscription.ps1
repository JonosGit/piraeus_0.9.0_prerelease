#Data Lake Store Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.azuredatalakestore.net
#Domain = Azure Active Directory (AAD) domain, e.g., microsoft.onmicrosoft.com
#AppId = Application ID created when registering application in AAD
#ClientSecret = Secret Key created in when registering application in AAD
#Folder = Folder in ADLS to write file(s)
#Filename = (Optional) Use only when intending to append messages to a single file.
#                      If omitted, each message will write a distinct file.
#NumClients = (Optional) Number of data lake store clients to use to write messages.  Default is 1.
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$domain = ""
$appId = ""
$secret = ""
$folder = ""
$filename = ""
$numClients = 1
$description = ""



Add-PiraeusDataLakeSubscription -ServiceUrl $url -SecurityToken $token `
                                -ResourceUriString $resource `
                                -Account $account `
                                -Domain $domain `
                                -AppId $appId `
                                -ClientSecret $secret `
                                -Folder $folder `
                                -Filename $filename `
                                -NumClients $numClients `
                                -Description $description



                                