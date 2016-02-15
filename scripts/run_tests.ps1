
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

# restore

cd "$repoDir\test\use-dotnet-fssrgen-as-tool"

dotnet restore
if (-not $?) {
	exit 1
}

dotnet build
if (-not $?) {
	exit 1
}

dotnet test
if (-not $?) {
	exit 1
}

Pop-Location

exit 0
