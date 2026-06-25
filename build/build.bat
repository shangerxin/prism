echo @off
echo usages:
echo build [PrismSourceRoot] [PrismPublishedBinRoot]

echo Build WebAPI at the root of Prism solution
if "%1"=="" (
	set PrismSourceRoot=%~dp0\..
) else (
	set PrismSourceRoot=%1
)
if "%2"=="" (
	set PrismBinRoot=bin
) else (
	set PrismBinRoot=%2
)

echo Prism solution root %PrismSourceRoot%
pushd %PrismSourceRoot%\prism.web.service
set PublishDir=PrismPublishedFiles
msbuild /p:DeployOnBuild=true /p:Configuration=Debug  /p:PublishProfile=Properties\PublishProfiles\FolderProfile.pubxml /p:PreBuildEvent= /p:PostBuildEvent=
if not exist %PublishDir%\Venv xcopy Venv %PublishDir%\Venv /S /C /I /H /R /Y
if not exist %PublishDir%\Script xcopy Script %PublishDir%\Script /S /C /I /H /R /Y
powershell -Command "(Get-Content '%PublishDir%\Web.config') -replace 'TestManagementDBTest', 'TestManagementDB' | Set-Content '%PublishDir%\Web.config'"
powershell -Command "(Get-Content '%PublishDir%\Web.config') -replace '%%userprofile%%\\source\\Prism\\prism.web.service\\bin', '%PrismBinRoot%' | Set-Content '%PublishDir%\Web.config'"
popd
echo Done.