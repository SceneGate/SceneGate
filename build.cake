#load "nuget:?package=PleOps.Cake&version=0.5.0"

Task("Define-Project")
    .Description("Fill specific project information")
    .IsDependeeOf("Build")
    .Does<BuildInfo>(info =>
{
    info.AddLibraryProjects("SceneGate.UI.Formats");
    info.AddApplicationProjects("SceneGate.UI.Gtk");
    info.AddApplicationProjects("SceneGate.UI.Mac");

    info.PreviewNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
