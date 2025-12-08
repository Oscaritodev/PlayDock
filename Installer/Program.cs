using System;
using WixSharp;

class Program
{
    static void Main()
    {
        string buildDir = System.IO.Path.Combine(Environment.CurrentDirectory, "..", "Publish");
        
        var project = new Project("New Launcher",
            new Dir(@"%ProgramFiles%\Antigravity\Launcher",
                new DirFiles(System.IO.Path.Combine(buildDir, "*.*"))
            )
        );

        project.GUID = new Guid("6f330b47-26bc-4548-b7d6-778844895690");
        project.OutFileName = "NewLauncher";

        Compiler.BuildMsi(project);
    }
}
