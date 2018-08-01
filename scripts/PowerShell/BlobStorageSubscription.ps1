#Azure Blob Storage Subscription

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.blob.core.windows.net
#Container = Container in blob store to write messages.
#BlobType = (Block | Page | Append)
#            Block will write each message discretely as a Block Blob
#            Page will write each message discretely as a Page Blob
#            Append will append each message with newline separator to a single file.
#            When using Append 'Filename' parameter is required.
#Filename = Used only when BlobType is Append.
#Key = Blob storage security key.
#NumClients = (Optional) Number of blob clients to use to write messages.  Default is 1.
#Description = (Optional) Text description on the subscription

Import-Module Piraeus


$resource = ""
$account = ""
$container = ""
$filename = ""
$key = ""
$numClients = 1
$description = ""


Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token `
                                   -ResourceUriString $resource `
                                   -Account $account `
                                   -Container $container `
                                   -BlobType Append `
                                   -Filename $filename `
                                   -Key $key `
                                   -NumClients $numClients `
                                   -Description $description