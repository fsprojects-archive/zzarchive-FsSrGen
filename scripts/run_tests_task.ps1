
#make path absolute
$repoDir = Split-Path -parent (Split-Path -parent $PSCommandPath)

Push-Location

cd "$repoDir\test\use-fssrgen-as-msbuild-task"


# restore package

& "$repoDir\.nuget\NuGet.exe" restore .\packages.config -PackagesDirectory packages -Source "$repoDir\bin\packages"
if (-not $?) {
	exit 1
}

# run tool
$testProjDir = $pwd
msbuild FsSrGenAsMsbuildTask.msbuild /verbosity:detailed 
if (-not $?) {
	exit 1
}


# restore test project

dotnet restore
if (-not $?) {
	exit 1
}


# build

dotnet --verbose build
if (-not $?) {
	exit 1
}

# run tests netstandard1.5

dotnet run --framework netstandard1.5 -- --verbose
if (-not $?) {
	exit 1
}

# run tests net46

.\bin\Debug\net46\win7-x64\use-fssrgen-as-msbuild-task.exe --verbose
if (-not $?) {
	exit 1
}

Pop-Location

exit 0
