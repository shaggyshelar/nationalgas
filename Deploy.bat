cd NG.Service/
dotnet publish --configuration Release
cd bin/Release/netcoreapp1.1/publish/
copy *.* \\ESPLS002\KP
cd ../../../../..
pause
