# exists the script if the preceeding command failed
function check-last { if(-not $?){ exit 1 }}

$scriptDir = split-path $script:MyInvocation.MyCommand.Path
$repoDir = split-path -parent $scriptdir
$projName = "use-dotnet-fssrgen-as-tool"
$testDir = "$repoDir\test\$projName"

# restore
dotnet restore $testDir  # -f "$repoDir\bin\packages"
check-last  

# the working directory needs to be changed to a dir where the 
# project.json has 'fssrgen' listed as a tool so that it can
# be called with 'dotnet fssrgen'
$stored = $PWD
cd $testDir

# run tool
dotnet fssrgen "$testDir\FSComp.txt" "$testDir\FSComp.fs" "$testDir\FSComp.resx" $projName

cd $stored

# build
dotnet build "$testDir"
check-last  

# run tests
dotnet test "$testDir"
check-last  

exit 0
