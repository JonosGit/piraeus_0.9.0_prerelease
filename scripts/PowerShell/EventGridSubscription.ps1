#Event Grid Subscription

#ResourceUriString = Piraeus resource to subscribe
#TopicKey = Event Grid topic key.
#Host = Full host name of the Event Grid, e.g., piraeussampletopic.eastus-1.eventgrid.azure.net 
#NumClients = (Optional) Number of event hubs to use to write message.  Default is 1.
#Description = (Optional) Text description on the subscription

$resource = ""
$topicKey = ""
$host = ""
$numClients = 1
$description

Add-PiraeusEventGridSubscription -ServiceUrl $url -SecurityToken $token `
                                 -ResourceUriString $resource `
                                 -TopicKey $topicKey `
                                 -Host $host `
                                 -NumClients $numClients `
                                 -Description $description

