cd ../Gateways/WebGateway/WebGateway
msbuild WebGateway.csproj /t:restore,Clean,Rebuild  /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /tv:version:4.0
cd ../../../Docker/tcpudpgateway
powershell ./buildoutput.ps1
docker build -t tcpudpgateway .
cd ../orleans-silo
powershell ./buildoutput.ps1
docker build -t orleans-silo .
cd ../webgateway
docker build -t webgateway .
cd ..\
docker tag webgateway skunklab/webgateway
docker tag tcpudpgateway skunklab/tcpudpgateway
docker tag orleans-silo skunklab/orleans-silo
docker push skunklab/orleans-silo
docker push skunklab/tcpudpgateway
docker push skunklab/webgateway




