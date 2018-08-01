#IoT Hub Command Subscription
#Note: Send a command to a configured IoT Hub device

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.azure-devices.net
#DeviceId = Device ID represented by the subscription
#KeyName = Key name for command to device
#Key = Security key associated with key name
#PropertyName = (Optional) Property name to send with message.  Used with PropertyValue
#PropertyValue = (Optional) Propery value to send with message.  Used with ProperyName
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$deviceId = ""
$keyName = ""
$key = ""
$propertyName = ""
$propertyValue = ""
$description = ""


Add-PiraeusIotHubCommandSubscription -ServiceUrl $url -SecurityToken $token `
                                     -ResourceUriString $resource `
                                     -Account $account `
                                     -DeviceId $deviceId `
                                     -KeyName $keyName `
                                     -Key $key `
                                     -PropertyName $propertyName `
                                     -PropertyValue $propertyValue `
                                     -Description $description 

                                     
