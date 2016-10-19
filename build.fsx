System.IO.Directory.SetCurrentDirectory __SOURCE_DIRECTORY__
// FAKE build script
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
open System
open Fake.AppVeyor
open Fake
open Fake.Git
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open Fake.AssemblyInfoFile

// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------

let project = "FsSrGen"
let authors = ["Don Syme";"Enrico Sada";"Jared Hester"]

let gitOwner = "fsprojects"
let gitHome = "https://github.com/" + gitOwner

let gitName = "FsSrGen"
let gitRaw = environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/fsprojects"

// The rest of the code is standard F# build script
// --------------------------------------------------------------------------------------


Target "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "test/**/bin"
    ++ "test/**/obj"
    ++ "bin"
    |> CleanDirs
)


let assertExitCodeZero x = if x = 0 then () else failwithf "Command failed with exit code %i" x

let runCmdIn workDir exe = Printf.ksprintf (fun args -> Shell.Exec(exe, args, workDir) |> assertExitCodeZero)

/// Execute a dotnet cli command
let dotnet workDir = runCmdIn workDir "dotnet"


let root = __SOURCE_DIRECTORY__
let srcDir = root</>"src"
let testDir = root</>"test"
let genDir = srcDir</>"fssrgen"
let dotnetDir = srcDir</>"dotnet-fssrgen"
let buildTaskDir = srcDir</>"FSharp.SRGen.Build.Tasks"
let pkgOutputDir = root</>"bin"</>"packages"


Target "CreatePackages" (fun _ ->

    dotnet srcDir "restore"
    // Build FsSrGen nupkg
    dotnet genDir "restore"
    dotnet genDir "pack -c Release --output %s" pkgOutputDir
    // Build dotnet-fssrgen nupkg
    dotnet dotnetDir "restore"
    dotnet dotnetDir "pack -c Release --output %s" pkgOutputDir
    // Build FSharp.SRGen.Build.Tasks nupkg
    dotnet buildTaskDir "restore"
    dotnet buildTaskDir "pack -c Release --output %s" pkgOutputDir

)

// Run Tests for the dotnet cli tool
// --------------------------------------------------------------------------------------

let cliProjName = "use-dotnet-fssrgen-as-tool"
let testToolDir = root</>"test"</>cliProjName

Target "RunTestsTool" (fun _ ->
    dotnet testDir "restore"
    dotnet testToolDir "restore"
    dotnet testToolDir "fssrgen %s %s %s %s"
        (testToolDir</>"FSComp.txt") (testToolDir</>"FSComp.fs") (testToolDir</>"FSComp.resx") cliProjName
    dotnet testToolDir "build"
    dotnet testToolDir "test"

)

// Run Tests for the msbuild task
// --------------------------------------------------------------------------------------

let testTaskDir =  root</>"test"</>"use-fssrgen-as-msbuild-task"
let msbuild workDir = runCmdIn workDir "msbuild"
let nuget workDir = runCmdIn workDir ("packages"</>"Nuget.CommandLine"</>"tools"</>"nuget.exe")
let fssrgenTaskExe workDir = runCmdIn workDir (testTaskDir</>"bin"</>"Debug"</>"net46"</>"win7-x64"</>"use-fssrgen-as-msbuild-task.exe")

Target "RunTestsTask" (fun _ ->
    nuget testTaskDir "restore"
    dotnet testTaskDir "restore"
    msbuild testTaskDir "%s" (testTaskDir</>"FsSrGenAsMsbuildTask.msbuild /verbosity:detailed")
    dotnet testTaskDir "-v build"
    dotnet testTaskDir "run --framework netcoreapp1.0 -- --verbose"
    fssrgenTaskExe testToolDir "--verbose"

)


#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit


/// helper function to release a single package to github
let releasePackage pkgName pkgSuffix =
    let user =
        match getBuildParam "github-user" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserInput "GitHub Username: "
    let pw =
        match getBuildParam "github-pw" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "GitHub Password: "
    let remote =
        Git.CommandHelper.getGitResult "" "remote -v"
        |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
        |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
        |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

    let releaseFile pkgSuffix =
        __SOURCE_DIRECTORY__ </> (sprintf "RELEASE_NOTES_%s.md" pkgSuffix)

    // Read release notes & version info from RELEASE_NOTES.md
    let makeNotes pkgSuffix : ReleaseNotes =
        LoadReleaseNotes (releaseFile  pkgSuffix)
    // configure branch for the release upload
    let releasePkg pkgName pkgSuffix release =
        // stage,tag, and commit the release notes for this specifc package
        let releaseFileName = releaseFile pkgSuffix
        StageFile "" releaseFileName |> ignore
        Git.Commit.Commit "" (sprintf "Bump %s version to %s" pkgName release.NugetVersion)
        Branches.pushBranch "" remote (Information.getBranchName "")
        // tag the branch release with the package version number
        let version = sprintf """%s.v%s""" pkgName release.NugetVersion
        Branches.tag "" version
        Branches.pushTag "" remote version
       

        createDraft gitOwner gitName version (release.SemVer.PreRelease <> None) release.Notes
    // release on github
    let uploadRelease client pkgName pkgSuffix =
        let notes = makeNotes pkgSuffix
        client
        |> releasePkg pkgName pkgSuffix notes
        |> uploadFile (pkgOutputDir</>(pkgName + "." + notes.NugetVersion + ".nupkg"))
        |> releaseDraft
        |> Async.RunSynchronously

    let client = createClient user pw
    uploadRelease client pkgName pkgSuffix

let ``fssrgen`` = "fssrgen"
let ``dotnet-fssrgen`` = "dotnet-fssrgen"
let ``FSharp.SRGen.Build.Tasks`` = "FSharp.SRGen.Build.Tasks"

Target "ReleaseFssrgen" (fun _ ->
    releasePackage ``fssrgen`` "FSSRGEN"
)

Target "ReleaseDotnetCli" (fun _ ->
    releasePackage ``dotnet-fssrgen`` "DOTNET_FSSRGEN"
)

Target "ReleaseBuildTask" (fun _ ->
    releasePackage ``FSharp.SRGen.Build.Tasks`` "FSHARP_SRGEN_BUILDTASKS"
)

/// publish a single nupkg determined by the package name
let singlePublish projName =
    let singlePackageDir = pkgOutputDir </> projName
    let nupkg =
        (!!(pkgOutputDir</>(projName + ".*.*.*.nupkg"))).Includes
        |> List.head
    CreateDir (pkgOutputDir </> projName)
    MoveFile singlePackageDir nupkg
    Paket.Push (fun p ->
        let apikey =
            match getBuildParam "nuget-apikey" with
            | s when not (String.IsNullOrWhiteSpace s) -> s
            | _ -> getUserInput "Nuget API Key: "
        { p with
            ApiKey = apikey
            WorkingDir = singlePackageDir })


Target "PublishFssrgen" (fun _ ->
    singlePublish ``fssrgen``
)

Target "PublishDotnetCli" (fun _ ->
    singlePublish ``dotnet-fssrgen``
)

Target "PublishBuildTask" (fun _ ->
    singlePublish ``FSharp.SRGen.Build.Tasks``
)

"Clean"
    ==> "CreatePackages"

Target "RunTests" DoNothing
"CreatePackages"
    =?> ("RunTestsTool",isWindows)
    =?> ("RunTestsTask",isWindows)
    ==> "RunTests"

// Build and run tests
Target "Build" DoNothing
"CreatePackages"
    ==> "RunTests"
    ==> "Build"


Target "GitHubReleaseAll" DoNothing

"Build"
  ==> "ReleaseFssrgen"
  ==> "GitHubReleaseAll"

"Build"
  ==> "ReleaseDotnetCli"
  ==> "GitHubReleaseAll"

"Build"
  ==> "ReleaseBuildTask"
  ==> "GitHubReleaseAll"


Target "PublishNugetAll" DoNothing

"ReleaseFssrgen"
  ==> "PublishFssrgen"
  ==> "PublishNugetAll"

"ReleaseDotnetCli"
  ==> "PublishDotnetCli"
  ==> "PublishNugetAll"

"ReleaseBuildTask"
  ==> "PublishBuildTask"
  ==> "PublishNugetAll"


RunTargetOrDefault "Build"
