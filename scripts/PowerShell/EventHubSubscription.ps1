#Event Hub Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.servicebus.windows.net
#Hub = Event Hub name
#KeyName = Key name for event hub
#Key = Security key associated with key name
#NumClients = (Optional) Number of event hub clients to use to write messages.  Default is 1.
#Description = (Optional) Text description on the subscription

$url = "http://piraeus.eastus2.cloudapp.azure.com"  #Replace with Host name or IP address of the Piraeus Web Gateway

$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"

$resource = "http://www.skunklab.io/resource-a"
$account = "piraeuseventhub"
$hub = "hub1"
$keyName = "RootManageSharedAccessKey"
$key = "taprF0ZVBNt7lR+oUpzlocz6D73L5qUjFCvoGNe8Q68="
$numClients = 1
$description = "Event Hub test"


Add-PiraeusEventHubSubscription -ServiceUrl $url -SecurityToken $token `
                                -ResourceUriString $resource `
                                -Account $account `
                                -Hub $hub `
                                -KeyName $keyName `
                                -Key $key `
                                -NumClients $numClients `
                                -Description $description
