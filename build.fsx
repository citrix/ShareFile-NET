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
let nugetVersion = getBuildParamOrDefault "nugetVersion" "3.0.8-b01"
// DO NOT INCREMENT THIS VALUE -- Will cause issues with PowerShell and StrongNamed versions of the assembly
let assemblyVersion = getBuildParamOrDefault "assemblyVersion" "3.0.0"
let assemblyFileVersion = getBuildParamOrDefault "assemblyFileVersion" "3.0.8-b01"
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

    let applyAssemblyInfo assemblyInfoFile = CreateCSharpAssemblyInfo assemblyInfoFile assemblyInfo
    [ "./Core/Properties/AssemblyInfo.cs"; "./Net45/Properties/AssemblyInfo.cs" ] |> Seq.iter applyAssemblyInfo
)

Target "Build" (fun () ->    
    let composeConstants solutionConstants = 
        [   [ "CODE_ANALYSIS" ] //default constants
            (if signRequested = "true" then [ "SIGNED" ] else [])
            (if buildType = "internal" then [ "ShareFile" ] else [])
            solutionConstants
        ] |> Seq.concat |> String.concat ";"

    let composeBuildParams constants = 
        [   "Optimize", "True"
            "DebugSymbols", "False"
            "Configuration", buildMode
            "SignAssembly", if signRequested = "true" then "True" else "False"
            "AssemblyOriginatorKeyFile", signKeyPath
            "GenerateDocumentation", "True"
            "DefineConstants", constants ]

    let composeSolutionName solutionName = "./ShareFile.Api.Client." + (if buildType = "internal" then solutionName + ".Internal" else solutionName) + ".sln"

    let solutions = //solution name, build subdirectory, compile constants
        [   "Core", "Portable", [ "Portable"; "Async" ]
            "Net40", "Net40", [ "Net40" ]
            "Net45", "Net45", [ "Async" ]        
            "Net45Core", "NetCore45", [ "Async"; "NETFX_CORE" ] ]

    let build (solutionName, solutionBuildDir, solutionConstants) = 
        MSBuild (buildDir @@ solutionBuildDir) "Clean;Build" (composeBuildParams <| composeConstants solutionConstants) [ composeSolutionName solutionName ]
            |> Log "AppBuild-Output: "
        CleanDirs ["./Core" @@ "obj"]

    solutions |> Seq.iter build
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
            Publish = true
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