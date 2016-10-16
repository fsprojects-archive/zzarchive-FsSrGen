# exists the script if the preceeding command failed
function check-last { if(-not $?){ exit 1 }}

$scriptDir = split-path $script:MyInvocation.MyCommand.Path
$repoDir = split-path -parent $scriptdir
$testDir = "$repoDir\test\use-fssrgen-as-msbuild-task"

$stored = $pwd
cd $testDir

# restore testproject and tools from package 
dotnet restore
& "$repoDir\.nuget\nuget.exe" restore
check-last  

# run tool
msbuild "$testdir\FsSrGenAsMsbuildTask.msbuild" /verbosity:detailed 
check-last  

# build
dotnet -v build 
check-last  

# run tests netcoreapp 1.0
dotnet run  --framework netcoreapp1.0 -- --verbose
check-last  

# run tests net45
.\bin\Debug\net46\win7-x64\use-fssrgen-as-msbuild-task.exe --verbose
check-last  

cd $stored

exit 0
