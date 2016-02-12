
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

# restore

cd $repoDir

dotnet restore
if (-not $?) {
	exit 1
}

# create src/FsSrGen package

cd "$repoDir\src\FsSrGen"

dotnet pack -c Release
if (-not $?) {
	exit 1
}

# create src/FSharp.SRGen.Build.Tasks package

cd "$repoDir\src\FSharp.SRGen.Build.Tasks"

dotnet pack -c Release
if (-not $?) {
	exit 1
}

# crete src/dotnet-fssrgen package

cd "$repoDir\src\dotnet-fssrgen"

dotnet pack -c Release
if (-not $?) {
	exit 1
}

# Done

Pop-Location

exit 0
