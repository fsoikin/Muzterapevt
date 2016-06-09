#r "../tools/FAKE/tools/FakeLib.dll"
#load "Git.fsx"
open Fake

type Deployment = 
  { url: string; username: string; passwordKey: string; localDir: string }
  with 
    member d.password = getBuildParamOrDefault d.passwordKey ""
    member d.pushUrl = sprintf "https://%s:%s@%s" d.username d.password d.url
    member d.matchesUrl (u: string) = u.EndsWith d.url
  
let prodDeployment = { url = "muzterapevt.scm.azurewebsites.net:443/muzterapevt.git"; localDir = "./deploy/prod"; username = "name"; passwordKey = "ProdDeployPassword" }
let testDeployment = { url = "mut-test.scm.azurewebsites.net:443/mut-test.git"; localDir = "./deploy/test"; username = "name"; passwordKey = "TestDeployPassword" }

let gitUserEmail = getBuildParam "GitUserEmail"
let gitUserName = getBuildParam "GitUserName"

let fullName f = try FullName f with _ -> ""
let configuration = getBuildParamOrDefault "Configuration" "Release"
let failIfNone msg = function | Some v -> v | None -> failwith msg


let deploymentPackage deployment =
  let { localDir = dir; url = url } = deployment
  Git.createRepoIfNotExists dir deployment.pushUrl gitUserEmail gitUserName

  if not <| Git.hasRemote dir deployment.matchesUrl then
    failwith (sprintf "Directory %s exists, but is either not a git repo or does not have a remote of %s" dir url)

  let branch = Git.Information.getBranchName deployment.localDir
  Git.Branches.pull dir deployment.pushUrl branch

  let files = 
    !! "Web/bin/*.dll"
    -- "Web/node_modules"
    ++ "Web/Global.asax"
    ++ "Web/Views/**/*.cshtml"
    ++ "Web/Views/Web.config"
    ++ "Web/Web.config"
    ++ "Web/__Themes/**/*.js"
    ++ "Web/__Themes/**/*.html"
    ++ "Web/__Themes/**/*.css"
    -- "**/*.min.css"
    ++ "Web/__Themes/**/*.gif"
    ++ "Web/__Themes/**/*.png"
   
  CopyDir dir "Web" (fullName >> files.IsMatch)


let deploy deployment =
  let branch = Git.Information.getBranchName deployment.localDir
  let buildLabel = 
    match TeamCityHelper.TeamCityBuildNumber, TeamCityHelper.TeamCityBuildConfigurationName with
    | Some v, Some c -> sprintf "%s build v%s" c v 
    | _ -> sprintf "local %s build" configuration

  Git.Staging.StageAll deployment.localDir
  Git.Commit.Commit deployment.localDir (sprintf "Deployment from %s." buildLabel)
  Git.Branches.pushBranch deployment.localDir deployment.pushUrl branch


Target "Start" <| fun _ -> ()

Target "Build" <| fun _ ->
  !! "*.sln"
  |> MSBuild "" "Build" ["Configuration", configuration]
  |> Log "Build output: "

Target "Clean" <| fun _ ->
  !! "*.sln"
  |> MSBuild "" "Clean" ["Configuration", configuration]
  |> Log "Build output: "

Target "NugetRestore" <| fun _ ->
  "Muzterapevt.sln"
  |> RestoreMSSolutionPackages id

Target "PackageTest" <| fun _ -> deploymentPackage testDeployment
Target "PackageProd" <| fun _ -> deploymentPackage prodDeployment

Target "DeployTest" <| fun _ -> deploy testDeployment
Target "DeployProd" <| fun _ -> deploy prodDeployment


"Start"
  =?> ("Clean", hasBuildParam "Clean")
  ==> "NugetRestore" 
  ==> "Build"

"Build" ==> "PackageTest"
"Build" ==> "PackageProd"

"PackageTest" ==> "DeployTest"
"PackageProd" ==> "DeployProd"


RunTargetOrDefault "Build"