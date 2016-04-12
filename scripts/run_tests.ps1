
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

cd "$repoDir\test\use-dotnet-fssrgen-as-tool"

# restore

dotnet restore --source "$repoDir\src\dotnet-fssrgen\bin\Release"
if (-not $?) {
	exit 1
}

# run tool
$testProjDir = $pwd
$fw = "NETSTANDARD1_5"
$testProjName = "use-dotnet-fssrgen-as-tool"
dotnet fssrgen "$testProjDir\FSComp.txt" "$testProjDir\FSComp.fs" "$testProjDir\FSComp.resx" $fw "$testProjName"
if (-not $?) {
	exit 1
}

# build

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
