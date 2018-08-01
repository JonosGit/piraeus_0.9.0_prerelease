#Service Bus Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.servicebus.windows.net
#Topic = Service Bus topic to send the message
#KeyName = Service Bus key name.
#Key = Service Bus security key.
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$topic = ""
$keyName = ""
$key = ""
$description = ""

Add-PiraeusServiceBusSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString $resource `
                                  -Account $account `
                                  -Topic $topic `
                                  -KeyName $keyName `
                                  -Key $key `
                                  -Description $description
