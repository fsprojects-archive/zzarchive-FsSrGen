
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

cd "$repoDir\test\use-dotnet-fssrgen-as-tool"

# restore

dotnet restore
if (-not $?) {
	exit 1
}

# build

dotnet build
if (-not $?) {
	exit 1
}

# build another time, because .resx are built before precompile event

dotnet build
if (-not $?) {
	exit 1
}

# run tests

dotnet test
if (-not $?) {
	exit 1
}

Pop-Location

exit 0
