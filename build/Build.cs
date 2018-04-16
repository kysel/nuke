// Copyright Matthias Koch, Sebastian Karasek 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.CodeGeneration.Model;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.InspectCode;
using Nuke.Common.Tools.OpenCover;
using Nuke.Common.Tools.Xunit;
using Nuke.Core;
using Nuke.Core.Utilities;
using Nuke.Core.Utilities.Collections;
using static Nuke.CodeGeneration.CodeGenerator;
using static Nuke.CodeGeneration.SchemaGenerator;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Gitter.GitterTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.InspectCode.InspectCodeTasks;
using static Nuke.Common.Tools.OpenCover.OpenCoverTasks;
using static Nuke.Common.Tools.Xunit.XunitTasks;
using static Nuke.Core.IO.FileSystemTasks;
using static Nuke.Core.IO.PathConstruction;
using static Nuke.Core.EnvironmentInfo;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Restore);

    [Parameter("Indicates to push to nuget.org feed.")] readonly bool NuGet;
    [Parameter("ApiKey for the specified source.")] readonly string ApiKey;
    [Parameter("Gitter authentication token.")] readonly string GitterAuthToken;
    [Parameter("Amount of changes to announce in Gitter.")] readonly int? AnnounceChanges;

    string Source => NuGet
        ? "https://api.nuget.org/v3/index.json"
        : "https://www.myget.org/F/nukebuild/api/v2/package";

    //[GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;
    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Executes(() =>
        {
            DeleteDirectories(GlobDirectories(SourceDirectory, "*/bin", "*/obj"));
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => DefaultDotNetRestore);
        });

  
    Target Publish => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var project = Solution.GetProject("Nuke.CodeGeneration");
            DotNetPublish(s => DefaultDotNetPublish
                .SetProject(project)
                .SetFramework("netstandard2.0"));
            DotNetPublish(s => DefaultDotNetPublish
                .SetProject(project)
                .SetFramework("net461"));
        });

    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

  
   

   
   

    string MetadataDirectory => RootDirectory / ".." / "nuke-tools" / "metadata";
    string GenerationDirectory => RootDirectory / "source" / "Nuke.Common" / "Tools";
    string ToolSchemaFile => SourceDirectory / "Nuke.CodeGeneration" / "schema.json";

    Target Generate => _ => _
        .Executes(() =>
        {
            GenerateSchema<Tool>(
                ToolSchemaFile,
                GitRepository.GetGitHubDownloadUrl(ToolSchemaFile),
                "Tool metadata schema file by NUKE");

            GenerateCode(
                MetadataDirectory,
                GenerationDirectory,
                baseNamespace: "Nuke.Common.Tools",
                useNestedNamespaces: true,
                gitRepository: GitRepository.FromLocalDirectory(MetadataDirectory).NotNull());
        });


}
