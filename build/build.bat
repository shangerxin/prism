@echo off
echo usages: 
echo Please set http/htps proxy if needed
echo build [PrismSourceRoot] [PrismPublishedBinRoot]

echo Build WebAPI at the root of Prism solution
if "%1"=="" (
	set PrismSourceRoot=%~dp0\..
) else (
	set PrismSourceRoot=%1
)
if "%2"=="" (
	set PrismBinRoot=C:\Prism\bin
) else (
	set PrismBinRoot=%2
)

echo Prism solution root %PrismSourceRoot%
pushd %PrismSourceRoot%
msbuild -t:restore /p:SolutionDir .\Prism.slnx  -p:RestorePackagesConfig=true -p:RestoreConfigFile=build\nuget.config
pushd %PrismSourceRoot%\prism.web.service
msbuild -t:restore /p:SolutionDir .\prism.web.service.csproj  -p:RestorePackagesConfig=true -p:RestoreConfigFile=..\build\nuget.config
if exist Properties\PublishProfiles\FolderProfile.pubxml copy Properties\PublishProfiles\FolderProfile.pubxml* %PrismSourceRoot%\build
set PublishDir=PrismPublishedFiles
msbuild /p:DeployOnBuild=true /p:Configuration=Debug  /p:PublishProfile=..\build\FolderProfile.pubxml /p:PreBuildEvent= /p:PostBuildEvent=
if not exist %PublishDir%\Venv xcopy Venv %PublishDir%\Venv /S /C /I /H /R /Y
if not exist %PublishDir%\Script xcopy Script %PublishDir%\Script /S /C /I /H /R /Y
powershell -Command "(Get-Content '%PublishDir%\Web.config') -replace 'TestManagementDBTest', 'TestManagementDB' | Set-Content '%PublishDir%\Web.config'"
powershell -Command "(Get-Content '%PublishDir%\Web.config') -replace '%%userprofile%%\\source\\Prism\\prism.web.service\\bin', '%PrismBinRoot%' | Set-Content '%PublishDir%\Web.config'"
popd
popd
echo Done.