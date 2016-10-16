# exists the script if the preceeding command failed
function check-last { if(-not $?){ exit 1 }}

$scriptDir = split-path $script:MyInvocation.MyCommand.Path
$repoDir = split-path -parent $scriptdir

# restore
dotnet restore "$repoDir\src"
check-last   

# create fssrgen package
dotnet --verbose pack "$repoDir\src\fssrgen" -c Release --output "$repoDir\bin\packages"
check-last   


# create FSharp.SRGen.Build.Tasks package
dotnet --verbose pack "$repoDir\src\FSharp.SRGen.Build.Tasks" -c Release --output "$repoDir\bin\packages"
check-last   


# crete dotnet-fssrgen package
dotnet --verbose pack "$repoDir\src\dotnet-fssrgen" -c Release --output "$repoDir\bin\packages"
check-last  

exit 0
