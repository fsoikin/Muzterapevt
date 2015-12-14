#r "tools/FAKE/tools/FakeLib.dll"
open Fake

type RemoteMode = Fetch | Push

let getRemotes repoDir =
  match Git.CommandHelper.runGitCommand repoDir "remote -v" with
  | false, _, _ -> Seq.empty
  | true, lines, _ -> lines |> Seq.choose (fun s -> 
      match s.Split([|' ';'\t'|], System.StringSplitOptions.RemoveEmptyEntries) with
      | [|name;url;"(fetch)"|] -> Some (name, url, Fetch)
      | [|name;url;"(push)"|] -> Some (name, url, Push)
      | _ -> None)

let getRemoteName repoDir matchUrl = 
  let remotes = getRemotes repoDir
  let matchRemote (_, url, _) = matchUrl url
  match Seq.tryFind matchRemote remotes with 
  | Some (name, _, _) -> Some name
  | _ -> None

let hasRemote repoDir matchUrl = Option.isSome <| getRemoteName repoDir matchUrl

let createRepoIfNotExists dir url =
  if not <| directoryExists dir then
    CreateDir dir
    Git.Repository.clone (DirectoryName dir) url (filename dir)