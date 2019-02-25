pushd %~dp0..\agent\src\Agent.Bootstrapper
dotnet publish -c Release -r ubuntu-x64 -o %~dp0..\target\agent\ubuntu-x64 --self-contained
popd