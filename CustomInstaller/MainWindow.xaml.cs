using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace CustomInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            InstallBtn.IsEnabled = false;
            InstallBtn.Content = "INSTALLING...";
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            try
            {
                await Task.Run(() => PerformInstall());
                StatusText.Text = "Installation Complete!";
                InstallBtn.Content = "LAUNCH";
                InstallBtn.IsEnabled = true;
                InstallBtn.Click -= InstallBtn_Click;
                InstallBtn.Click += (s, args) => 
                {
                    string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Antigravity", "Launcher");
                    string exePath = Path.Combine(targetDir, "NewLauncher.exe");
                    if(File.Exists(exePath))
                        System.Diagnostics.Process.Start(exePath);
                    Close();
                };
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
                InstallBtn.IsEnabled = true;
            }
        }

        private void PerformInstall()
        {
            string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Antigravity", "Launcher");
            
            // Clean target
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            Directory.CreateDirectory(targetDir);

            // Extract Resource
            string zipPath = Path.Combine(Path.GetTempPath(), "payload.zip");
            var assembly = Assembly.GetExecutingAssembly();
            
            // Find resource name
            string resourceName = "CustomInstaller.payload.zip"; 
            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new Exception("Payload not found in resources!");
                using (FileStream fileStream = new FileStream(zipPath, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }

            // Unzip
            ZipFile.ExtractToDirectory(zipPath, targetDir);
            File.Delete(zipPath);

            // Create Desktop Shortcut
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopPath, "New Launcher.lnk");
            string exePath = Path.Combine(targetDir, "NewLauncher.exe");
            
            CreateShortcut(shortcutPath, exePath);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateShortcut(string shortcutPath, string targetPath)
        {
            // Creating shortcuts in pure C# without COM is hard. 
            // Using a simple VBScript trick to avoid dependencies.
            string vbs = $@"
                Set oWS = WScript.CreateObject(""WScript.Shell"")
                sLinkFile = ""{shortcutPath}""
                Set oLink = oWS.CreateShortcut(sLinkFile)
                oLink.TargetPath = ""{targetPath}""
                oLink.WorkingDirectory = ""{Path.GetDirectoryName(targetPath)}""
                oLink.Save
            ";
            
            string vbsPath = Path.Combine(Path.GetTempPath(), "shortcut.vbs");
            File.WriteAllText(vbsPath, vbs);
            
            var proc = System.Diagnostics.Process.Start("cscript", $"/nologo \"{vbsPath}\"");
            proc.WaitForExit();
            File.Delete(vbsPath);
        }
    }
}