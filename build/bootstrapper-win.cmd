pushd %~dp0..\agent\src\Agent.Bootstrapper
dotnet publish -c Release -r win-x64 -o %~dp0..\target\agent\win-x64 --self-contained
editbin /subsystem:windows %~dp0..\target\agent\win-x64\TimeRecorder.Agent.Bootstrapper.exe
popd