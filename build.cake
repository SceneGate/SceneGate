#load "nuget:?package=PleOps.Cake&version=0.4.2"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.WarningsAsErrors = false;

    info.AddApplicationProjects("SceneGate.CLI");

    // Only Windows can build WPF apps: https://github.com/dotnet/sdk/issues/3803
    if (IsRunningOnWindows()) {
        info.SolutionFile = "src/SceneGate.Windows.sln";
        info.AddApplicationProjects("SceneGate.UI.Wpf");
    } else {
        info.SolutionFile = "src/SceneGate.sln";
    }

    info.AddApplicationProjects("SceneGate.UI.Gtk");

    // Cannot build Mac version due to some issues in the task publish
    // info.AddApplicationProjects("SceneGate.UI.Mac");

    // No libraries to publish
    info.PreviewNuGetFeed = "";
    info.StableNuGetFeed = "";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

Task("Push-ArtifactsWithoutNuGets")
    .IsDependentOn("Define-Project")
    .IsDependentOn("Show-Info")
    .IsDependentOn("Push-Apps")     // only stable builds
    .IsDependentOn("Push-Doc")      // only preview and stable builds
    .IsDependentOn("Close-GitHubMilestone");    // only stable builds

string target = Argument("target", "Default");
RunTarget(target);