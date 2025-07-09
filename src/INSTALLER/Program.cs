using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace DevStackInstaller
{
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new Application();
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                
                var window = new InstallerWindow();
                app.MainWindow = window;
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting installer: {ex.Message}\n\nDetails: {ex}", 
                    "DevStack Installer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class InstallerWindow : Window
    {
        private TextBox pathTextBox = null!;
        private Button browseButton = null!;
        private Button installButton = null!;
        private ProgressBar progressBar = null!;
        private TextBlock statusText = null!;

        public InstallerWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing installer window: {ex.Message}", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private string GetVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version;
                return version ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string ExtractEmbeddedZip()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            // Try to find the resource with different possible names
            var resourceNames = assembly.GetManifestResourceNames();
            var zipResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("DevStack.zip"));
            
            if (zipResourceName == null)
            {
                throw new Exception("Embedded DevStack.zip resource not found in installer.");
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "DevStack_Install_" + Guid.NewGuid().ToString("N")[..8] + ".zip");
            
            using (var resourceStream = assembly.GetManifestResourceStream(zipResourceName))
            {
                if (resourceStream == null)
                {
                    throw new Exception("Could not access embedded DevStack.zip resource.");
                }
                
                using (var fileStream = File.Create(tempPath))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
            
            return tempPath;
        }

        private void InitializeComponent()
        {
            string version = GetVersion();
            Title = $"DevStack Installer v{version}";
            Width = 500;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            grid.Margin = new Thickness(20);

            // Title
            var titleText = new TextBlock
            {
                Text = "DevStack Manager Installer",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(titleText, 0);
            grid.Children.Add(titleText);

            // Installation path label
            var pathLabel = new TextBlock
            {
                Text = "Installation Path:",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(pathLabel, 1);
            grid.Children.Add(pathLabel);

            // Path selection
            var pathPanel = new StackPanel { Orientation = Orientation.Horizontal };
            pathTextBox = new TextBox
            {
                Width = 350,
                Height = 25,
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DevStack"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            browseButton = new Button
            {
                Content = "Browse...",
                Width = 80,
                Height = 25,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204))
            };
            browseButton.Click += BrowseButton_Click;

            pathPanel.Children.Add(pathTextBox);
            pathPanel.Children.Add(browseButton);
            Grid.SetRow(pathPanel, 2);
            grid.Children.Add(pathPanel);

            // Install button
            installButton = new Button
            {
                Content = "Install DevStack",
                Width = 150,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            installButton.Click += InstallButton_Click;
            Grid.SetRow(installButton, 3);
            grid.Children.Add(installButton);

            // Progress bar
            progressBar = new ProgressBar
            {
                Height = 20,
                Margin = new Thickness(0, 20, 0, 0),
                Visibility = Visibility.Hidden
            };
            Grid.SetRow(progressBar, 4);
            grid.Children.Add(progressBar);

            // Status text
            statusText = new TextBlock
            {
                Text = "Ready to install",
                Foreground = Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(statusText, 5);
            grid.Children.Add(statusText);

            Content = grid;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Use SaveFileDialog to select folder (workaround)
            var dialog = new SaveFileDialog
            {
                Title = "Select installation folder",
                FileName = "Select Folder",
                Filter = "Folder|*.folder",
                InitialDirectory = pathTextBox.Text
            };

            if (dialog.ShowDialog() == true)
            {
                pathTextBox.Text = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            string installPath = pathTextBox.Text;

            if (string.IsNullOrWhiteSpace(installPath))
            {
                MessageBox.Show("Please select an installation path.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? tempZipPath = null;
            try
            {
                installButton.IsEnabled = false;
                browseButton.IsEnabled = false;
                pathTextBox.IsEnabled = false;
                progressBar.Visibility = Visibility.Visible;
                progressBar.IsIndeterminate = true;

                statusText.Text = "Extracting embedded installation files...";
                tempZipPath = await Task.Run(() => ExtractEmbeddedZip());

                statusText.Text = "Creating installation directory...";
                Directory.CreateDirectory(installPath);

                statusText.Text = "Installing DevStack files...";
                await Task.Run(() => 
                {
                    ZipFile.ExtractToDirectory(tempZipPath, installPath, true);
                });

                statusText.Text = "Registering installation...";
                await Task.Run(() => RegisterInstallation(installPath));

                statusText.Text = "Installation completed successfully!";
                progressBar.IsIndeterminate = false;
                progressBar.Value = 100;

                var result = MessageBox.Show("DevStack has been installed successfully!\n\nWould you like to create desktop shortcuts?", 
                    "Installation Complete", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    CreateDesktopShortcuts(installPath);
                }

                MessageBox.Show("Installation process completed. You can now close the installer.", 
                    "Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                installButton.IsEnabled = true;
                browseButton.IsEnabled = true;
                pathTextBox.IsEnabled = true;
                progressBar.Visibility = Visibility.Hidden;
                statusText.Text = "Installation failed";
            }
            finally
            {
                // Clean up temporary zip file
                if (tempZipPath != null && File.Exists(tempZipPath))
                {
                    try
                    {
                        File.Delete(tempZipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not delete temporary file: {ex.Message}");
                    }
                }
            }
        }

        private void CreateDesktopShortcuts(string installPath)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                
                // Create shortcut for CLI
                string cliPath = Path.Combine(installPath, "DevStack.exe");
                if (File.Exists(cliPath))
                {
                    CreateShortcut(Path.Combine(desktopPath, "DevStack CLI.lnk"), cliPath, "DevStack Command Line Interface");
                }

                // Create shortcut for GUI
                string guiPath = Path.Combine(installPath, "DevStackGUI.exe");
                if (File.Exists(guiPath))
                {
                    CreateShortcut(Path.Combine(desktopPath, "DevStack GUI.lnk"), guiPath, "DevStack Graphical Interface");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create desktop shortcuts: {ex.Message}", "Warning", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                // Create a simple batch file shortcut as fallback
                string batchContent = $"@echo off\ncd /d \"{Path.GetDirectoryName(targetPath)}\"\nstart \"\" \"{Path.GetFileName(targetPath)}\"";
                string batchPath = shortcutPath.Replace(".lnk", ".bat");
                File.WriteAllText(batchPath, batchContent);
                
                // Try to create proper shortcut using PowerShell
                string psScript = $@"
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
$Shortcut.TargetPath = '{targetPath}'
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetPath)}'
$Shortcut.Description = '{description}'
$Shortcut.Save()
";
                
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{psScript}\"",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                process.WaitForExit();
                
                // If PowerShell succeeded, remove the batch file
                if (process.ExitCode == 0 && File.Exists(shortcutPath))
                {
                    try { File.Delete(batchPath); } catch { }
                }
            }
            catch
            {
                // Fallback already created above
            }
        }

        private void RegisterInstallation(string installPath)
        {
            try
            {
                var version = GetVersion();
                var uninstallerPath = Path.Combine(installPath, "DevStack-Uninstaller.exe");
                var displayName = "DevStack Manager";
                
                // Register installation path in HKCU for easy access by uninstaller
                using var userKey = Registry.CurrentUser.CreateSubKey(@"Software\DevStack");
                userKey.SetValue("InstallPath", installPath);
                userKey.SetValue("Version", version);
                userKey.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd"));
                
                // Register in Windows Programs and Features (if running as admin)
                try
                {
                    using var uninstallKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack");
                    uninstallKey.SetValue("DisplayName", displayName);
                    uninstallKey.SetValue("DisplayVersion", version);
                    uninstallKey.SetValue("Publisher", "DevStackManager");
                    uninstallKey.SetValue("InstallLocation", installPath);
                    uninstallKey.SetValue("UninstallString", $"\"{uninstallerPath}\"");
                    uninstallKey.SetValue("QuietUninstallString", $"\"{uninstallerPath}\" /S");
                    uninstallKey.SetValue("NoModify", 1);
                    uninstallKey.SetValue("NoRepair", 1);
                    uninstallKey.SetValue("EstimatedSize", GetDirectorySize(installPath));
                    uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                    uninstallKey.SetValue("HelpLink", "https://github.com/devstack/devstack");
                    uninstallKey.SetValue("URLInfoAbout", "https://github.com/devstack/devstack");
                }
                catch
                {
                    // Not running as admin, skip HKLM registration
                    // The program will still appear in user-specific locations
                }
            }
            catch
            {
                // Registry operations failed, continue anyway
            }
        }

        private int GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var size = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                return (int)(size / 1024); // Size in KB
            }
            catch
            {
                return 0;
            }
        }
    }
}
