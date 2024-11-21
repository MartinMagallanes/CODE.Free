sealed partial class Build
{
    const string Version = "1.0.3";
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";

    protected override void OnBuildInitialized()
    {
        Configurations =
        [
            "Release*",
            "Installer*"
        ];

        Bundles =
        [
            Solution.CODE_Free,
        ];

        InstallersMap = new()
        {
            {Solution.Installer, Solution.CODE_Free},
        };
    }
}