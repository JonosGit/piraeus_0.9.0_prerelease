#Cosmos DB Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.documents.azure.com:443
#Database = Cosmos DB database to use.
#Collection = Cosmos DB collection to use.
#Key = Cosmos DB security key
#NumClients = (Optional) Number of Cosmos DB clients to use to write messages.  Default is 1.
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$database = ""
$collection = ""
$key = ""
$numClients = 1
$description = ""


Add-PiraeusCosmosDbSubscription -ServiceUrl $url -SecurityToken $token `
                                -ResourceUriString $resource `
                                -Account $account `
                                -Database $database `
                                -Collection $collection `
                                -Key $key `
                                -NumClients $numClients `
                                -Description $description 
                               
                               
                                


                                
                                