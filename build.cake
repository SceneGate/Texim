#load "nuget:?package=PleOps.Cake&version=0.7.0"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.WarningsAsErrors = false;

    info.AddLibraryProjects("Texim");
    info.AddLibraryProjects("Texim.Games");
    info.AddApplicationProjects("Texim.Tool");
    info.AddTestProjects("Texim.Tests");

    info.PreviewNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
    info.StableNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
