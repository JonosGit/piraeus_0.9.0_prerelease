#IoT Hub Device Subscription
#Note: Sends a message to the IoT Hub as a device

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.azure-devices.net
#DeviceId = Device ID represented by the subscription
#Key = Security key by the device/subscription to call the IoT Hub
#PropertyName = (Optional) Property name to send with message.  Used with PropertyValue
#PropertyValue = (Optional) Propery value to send with message.  Used with ProperyName
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$deviceId = ""
$key = ""
$propertyName = ""
$propertyValue = ""
$description = ""

Add-PiraeusIotHubDeviceSubscription -ServiceUrl $url -SecurityToken $token `
                                    -ResourceUriString $resource `
                                    -Account $account `
                                    -DeviceId $deviceId `
                                    -Key $key `
                                    -PropertyName $propertyName `
                                    -PropertyValue $propertyValue `
                                    -Description $description
