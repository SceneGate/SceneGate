#load "nuget:?package=PleOps.Cake&version=0.4.2"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.WarningsAsErrors = false;

    // https://github.com/dotnet/sdk/issues/3803
    if (IsRunningOnWindows()) {
        info.SolutionFile = "src/SceneGate.Windows.sln";
        info.AddApplicationProjects("src/SceneGate.UI.Desktop/SceneGate.UI.Desktop.csproj");
    } else {
        info.SolutionFile = "src/SceneGate.sln";
    }

    // No libraries to publish
    info.PreviewNuGetFeed = "";
    info.StableNuGetFeed = "";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
