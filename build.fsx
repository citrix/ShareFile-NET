#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System
open MSBuildHelper
open Fake.AssemblyInfoFile

RestorePackages()

// Properties
let authors = ["Citrix ShareFile"]
let buildDir = "./build/"
let packagingRoot = "./packaging/"
let packagingDir = packagingRoot @@ "sharefile"
let nugetVersion = getBuildParamOrDefault "nugetVersion" "3.0.5"
// DO NOT INCREMENT THIS VALUE -- Will cause issues with PowerShell and StrongNamed versions of the assembly
let assemblyVersion = getBuildParamOrDefault "assemblyVersion" "3.0.0"
let assemblyFileVersion = getBuildParamOrDefault "assemblyFileVersion" "3.0.5"
let nugetAccessKey = getBuildParamOrDefault "nugetkey" ""
let nugetDestination = getBuildParamOrDefault "nugetserver" ""
let title = "ShareFile Client SDK"
let signedTitle = "ShareFile Client SDK - StrongName"

let projectName = "ShareFile.Api.Client"
let signedProjectName = "ShareFile.Api.Client.StrongName"
let projectDescription = "A ShareFile API client library for .NET"
let projectSummary = projectDescription

let buildMode = getBuildParamOrDefault "buildMode" "Release"
let signKeyPath = getBuildParamOrDefault "signKeyPath" Environment.CurrentDirectory @@ "ShareFile.Api.Client.snk"
let signRequested = getBuildParamOrDefault "sign" "false"

// Internal will just hide some APIs that likely won't be useful outside of ShareFile relating
// to administrative actions.
let buildType = getBuildParamOrDefault "for" "internal"

// *** Define Targets ***
Target "Clean" (fun () ->
    CleanDirs [buildDir; packagingRoot; packagingDir]
)

Target "AssemblyInfo" (fun () ->

    let assemblyInfo = 
        [   Attribute.Product projectName
            Attribute.Title title
            Attribute.Version assemblyVersion
            Attribute.FileVersion assemblyFileVersion
            Attribute.Copyright "Copyright © Citrix ShareFile 2014" ]
    let assemblyInfoInternal = [ Attribute.InternalsVisibleTo "ShareFile.Api.Client.Core.Internal" ]

    let applyAssemblyInfo = fun assemblyInfoFile -> CreateCSharpAssemblyInfo assemblyInfoFile (if buildType = "internal" then assemblyInfo @ assemblyInfoInternal else assemblyInfo)
    [ "./Core/Properties/AssemblyInfo.cs"; "./Net45/Properties/AssemblyInfo.cs" ] |> Seq.iter applyAssemblyInfo
)

Target "Build" (fun () ->
    let defaultConstants = "CODE_ANALYSIS"
    let signParameter =
        if signRequested = "true" then "True"
        else "False"

    let constants =
        if signRequested = "true" then defaultConstants + ";SIGNED"
        else defaultConstants

    let constants =
        if buildType = "internal" then constants + ";ShareFile"
        else constants
    
    let baseBuildParams = 
        [
            "Optimize", "True"
            "DebugSymbols", "False"
            "Configuration", buildMode
            "SignAssembly", signParameter
            "AssemblyOriginatorKeyFile", signKeyPath
            "GenerateDocumentation", "True"
        ]

    let buildParams = List.append baseBuildParams ["DefineConstants", constants + ";Portable;Async"]
    let net40PBuildParams = List.append baseBuildParams ["DefineConstants", constants + ";Net40"]
    let net45BuildParams = List.append baseBuildParams ["DefineConstants", constants + ";Async"]
    let net45CoreBuildParams = List.append baseBuildParams ["DefineConstants", constants + ";Async;NETFX_CORE"]
    
    let solutionProperties slnName =
        if buildType = "internal" then slnName + ".Internal"
        else slnName

    let coreSolutionName = solutionProperties "Core"
    let net45SolutionName = solutionProperties "Net45"
    let net40SolutionName = solutionProperties "Net40"
    let net45CoreSolutionName = solutionProperties "Net45Core"

    MSBuild (buildDir @@ "Portable") "Clean;Build" buildParams ["./ShareFile.Api.Client." + coreSolutionName + ".sln"]
    |> Log "AppBuild-Output: "
    CleanDirs ["./Core" @@ "obj"]
    MSBuild (buildDir @@ "Net45") "Clean;Build" net45BuildParams ["./ShareFile.Api.Client." + net45SolutionName + ".sln"]
    |> Log "AppBuild-Output: "
    CleanDirs ["./Core" @@ "obj"]
    MSBuild (buildDir @@ "Net40") "Clean;Build" net40PBuildParams ["./ShareFile.Api.Client." + net40SolutionName + ".sln"]
    |> Log "AppBuild-Output: "
    CleanDirs ["./Core" @@ "obj"]
    MSBuild (buildDir @@ "NetCore45") "Clean;Build" net45CoreBuildParams ["./ShareFile.Api.Client." + net45CoreSolutionName + ".sln"]
    |> Log "AppBuild-Output: "
    CleanDirs ["./Core" @@ "obj"]
)

Target "CreateNuGetPackage" (fun () ->
    let netCore45Dir = packagingDir @@ "lib/netcore45/"
    let net45Dir = packagingDir @@ "lib/net45/"
    let net40Dir = packagingDir @@ "lib/net40/"
    let net40ClientDir = packagingDir @@ "lib/net40-client/"
    let portableDir = packagingDir @@ "lib/portable-net45+wp80+win8+wpa81/"
    
    let nugetTitle =
        if signRequested = "true" then signedTitle
        else title
    let nugetProjectName =
        if signRequested = "true" then signedProjectName
        else projectName

    CleanDirs [net45Dir; net40Dir; net40ClientDir; portableDir; netCore45Dir]
    
    CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Core.dll")
    CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Net45.dll")
    CopyFile net40Dir (buildDir @@ "Net40/ShareFile.Api.Client.Core.dll")
    CopyFile net40ClientDir (buildDir @@ "Net40/ShareFile.Api.Client.Core.dll")
    CopyFile portableDir (buildDir @@ "Portable/ShareFile.Api.Client.Core.dll")
    CopyFile netCore45Dir (buildDir @@ "NetCore45/ShareFile.Api.Client.Core.dll")
    CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Core.xml")
    CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Net45.xml")
    CopyFile net40Dir (buildDir @@ "Net40/ShareFile.Api.Client.Core.xml")
    CopyFile net40ClientDir (buildDir @@ "Net40/ShareFile.Api.Client.Core.xml")
    CopyFile portableDir (buildDir @@ "Portable/ShareFile.Api.Client.Core.xml")
    CopyFile netCore45Dir (buildDir @@ "NetCore45/ShareFile.Api.Client.Core.xml")
    
    if buildType = "internal" then
        CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Core.Internal.dll")
        CopyFile net40Dir (buildDir @@ "Net40/ShareFile.Api.Client.Core.Internal.dll")
        CopyFile net40ClientDir (buildDir @@ "Net40/ShareFile.Api.Client.Core.Internal.dll")
        CopyFile portableDir (buildDir @@ "Portable/ShareFile.Api.Client.Core.Internal.dll")
        CopyFile netCore45Dir (buildDir @@ "NetCore45/ShareFile.Api.Client.Core.Internal.dll")
        CopyFile net45Dir (buildDir @@ "Net45/ShareFile.Api.Client.Core.Internal.xml")
        CopyFile net40Dir (buildDir @@ "Net40/ShareFile.Api.Client.Core.Internal.xml")
        CopyFile net40ClientDir (buildDir @@ "Net40/ShareFile.Api.Client.Core.Internal.xml")
        CopyFile portableDir (buildDir @@ "Portable/ShareFile.Api.Client.Core.Internal.xml")
        CopyFile netCore45Dir (buildDir @@ "NetCore45/ShareFile.Api.Client.Core.Internal.xml")
    
    NuGet (fun p ->
        {p with
            Authors = authors
            Project = nugetProjectName
            Description = projectDescription
            OutputPath = packagingRoot
            Summary = projectSummary
            WorkingDir = packagingDir
            Version = nugetVersion
            PublishUrl = nugetDestination
            AccessKey = nugetAccessKey
            Publish = false
            Title = nugetTitle
            ReleaseNotes = "" }) "ShareFile.Api.Client.nuspec"
)

Target "Default" DoNothing

// *** Define Dependencies ***
"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CreateNuGetPackage"
  ==> "Default"

// *** Start Build ***
RunTargetOrDefault "Default"