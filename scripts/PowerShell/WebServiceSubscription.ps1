#Web Service and Azure Function Subscription

#ResourceUriString = Piraeus resource to subscribe
#WebServiceUrl = URL of the Web service.
#TokenType = One of (JWT, SWT, X509, or None) Specifies the security to be sent to the Web service.
#Issuer = (Optional) Used only when required by (JWT | SWT) security token types.
#Audience = (Optional) Used only when required by (JWT | SWT) security token types.
#Key = (Optional) Used only with (JWT | SWT) security token types.
#Description = (Optional) Text description on the subscription

#Add REST Web Service Subscription with JWT security token

$resource = ""
$serviceUrl = ""
$issuer = ""
$audience = ""
$key = ""
$description = ""

Add-PiraeusWebServiceSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString $resource `
                                  -WebServiceUrl $serviceUrl `
                                  -Issuer $issuer `
                                  -Audience $audience `
                                  -TokenType Jwt `
                                  -Key $key `
                                  -Description $description

#Add Azure Function Subscription
#WebServiceURL encodes the Azure Function's "code" as query string parameter.
Add-PiraeusWebServiceSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString "" `
                                  -WebServiceUrl "" `                                 
                                  -TokenType None `
                                  -Description "Azure Function Subscription"