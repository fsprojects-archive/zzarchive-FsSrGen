
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

# restore

cd "$repoDir\src"

dotnet restore
if (-not $?) {
	exit 1
}

# restore workaround 1

cd "$repoDir\src\workaround1"

dotnet restore --packages packages
if (-not $?) {
	exit 1
}

# create src/fssrgen package

cd "$repoDir\src\fssrgen"

dotnet pack -c Release --output "$repoDir\bin\packages"
if (-not $?) {
	exit 1
}

# create src/FSharp.SRGen.Build.Tasks package

cd "$repoDir\src\FSharp.SRGen.Build.Tasks"

dotnet pack -c Release --output "$repoDir\bin\packages"
if (-not $?) {
	exit 1
}

# crete src/dotnet-fssrgen package

cd "$repoDir\src\dotnet-fssrgen"

dotnet pack -c Release --output "$repoDir\bin\packages"
if (-not $?) {
	exit 1
}

# Done

Pop-Location

exit 0
