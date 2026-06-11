using CodeSceneExplorer.Infrastructure.CodeScene;

var options = CodeSceneApiOptions.FromEnvironment();
Console.WriteLine($"CodeScene Explorer is ready. Base URL: {options.BaseAddress}");
