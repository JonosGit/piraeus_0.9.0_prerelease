#Azure Storage Queue Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.queue.core.windows.net
#Queue = Name of Azure storage queue
#TTL = TimeSpan expiration of queued items
#Key = Storage security key
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$queue = ""
$ttl = New-TimeSpan.Minutes(5)
$key = ""
$description = ""


Add-PiraeusQueueStorageSubscription -ServiceUrl $url -SecurityToken $token `
                                    -ResourceUriString "" `
                                    -Account "" `
                                    -Queue "" `
                                    -TTL "" `
                                    -Key "" `
                                    -Description ""