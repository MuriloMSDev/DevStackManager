using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace DevStackUninstaller
{
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            try
            {
                var mainWindow = CreateUninstallerWindow();
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting uninstaller: {ex.Message}", 
                    "Uninstaller Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private static Window CreateUninstallerWindow()
        {
            try
            {
                var window = new Window
                {
                    Title = $"DevStack Uninstaller v{GetVersion()}",
                    Width = 600,
                    Height = 450,
                    MinWidth = 500,
                    MinHeight = 350,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.CanResize,
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
                };

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

                // Header
                var headerPanel = new StackPanel
                {
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                    Orientation = Orientation.Horizontal
                };
                headerPanel.Children.Add(new TextBlock
                {
                    Text = "🗑️ DevStack Uninstaller",
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(20, 20, 0, 20),
                    VerticalAlignment = VerticalAlignment.Center
                });
                Grid.SetRow(headerPanel, 0);
                mainGrid.Children.Add(headerPanel);

                // Warning message
                var warningPanel = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 245, 200)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(20, 10, 20, 10),
                    Padding = new Thickness(15)
                };

                var warningStack = new StackPanel();
                warningStack.Children.Add(new TextBlock
                {
                    Text = "⚠️ Atenção",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5)
                });
                warningStack.Children.Add(new TextBlock
                {
                    Text = "Esta ação removerá completamente o DevStack do seu sistema, incluindo:",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                var itemsList = new StackPanel { Margin = new Thickness(20, 5, 0, 5) };
                itemsList.Children.Add(new TextBlock { Text = "• Todos os arquivos de programa" });
                itemsList.Children.Add(new TextBlock { Text = "• Configurações e dados do usuário" });
                itemsList.Children.Add(new TextBlock { Text = "• Atalhos da área de trabalho e menu iniciar" });
                itemsList.Children.Add(new TextBlock { Text = "• Entradas do registro do Windows" });
                itemsList.Children.Add(new TextBlock { Text = "• Serviços instalados" });
                warningStack.Children.Add(itemsList);

                warningPanel.Child = warningStack;
                Grid.SetRow(warningPanel, 1);
                mainGrid.Children.Add(warningPanel);

                // Installation details
                var detailsPanel = new StackPanel { Margin = new Thickness(20, 10, 20, 10) };
                
                var installPath = GetInstallationPath();
                if (!string.IsNullOrEmpty(installPath))
                {
                    detailsPanel.Children.Add(new TextBlock
                    {
                        Text = "Pasta de instalação encontrada:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    detailsPanel.Children.Add(new TextBlock
                    {
                        Text = installPath,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 0)),
                        FontFamily = new FontFamily("Consolas"),
                        Margin = new Thickness(20, 0, 0, 15)
                    });
                }

                // Progress area
                var progressPanel = new StackPanel { Margin = new Thickness(20, 10, 20, 10) };
                var progressBar = new ProgressBar
                {
                    Height = 20,
                    Margin = new Thickness(0, 5, 0, 5),
                    Visibility = Visibility.Collapsed
                };
                var statusText = new TextBlock
                {
                    Text = "Pronto para desinstalar",
                    Margin = new Thickness(0, 5, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                progressPanel.Children.Add(progressBar);
                progressPanel.Children.Add(statusText);
                detailsPanel.Children.Add(progressPanel);

                Grid.SetRow(detailsPanel, 2);
                mainGrid.Children.Add(detailsPanel);

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(20, 10, 20, 20)
                };

                var cancelButton = new Button
                {
                    Content = "Cancelar",
                    Width = 100,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontWeight = FontWeights.Bold
                };

                var uninstallButton = new Button
                {
                    Content = "🗑️ Desinstalar",
                    Width = 130,
                    Height = 35,
                    Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontWeight = FontWeights.Bold
                };

                cancelButton.Click += (s, e) => window.Close();
                uninstallButton.Click += async (s, e) =>
                {
                    var result = MessageBox.Show(
                        "Tem certeza que deseja remover completamente o DevStack?\n\nEsta ação não pode ser desfeita.",
                        "Confirmar Desinstalação",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await PerformUninstallation(progressBar, statusText, uninstallButton, cancelButton);
                    }
                };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(uninstallButton);
                Grid.SetRow(buttonPanel, 4);
                mainGrid.Children.Add(buttonPanel);

                window.Content = mainGrid;
                return window;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating uninstaller window: {ex.Message}", 
                    "Window Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private static string GetVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
                return version ?? "2.0.0";
            }
            catch
            {
                return "2.0.0";
            }
        }

        private static string GetInstallationPath()
        {
            try
            {
                // Try to get installation path from registry
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\DevStack");
                if (key != null)
                {
                    var path = key.GetValue("InstallPath")?.ToString();
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        return path;
                    }
                }

                // Try to get from current executable location
                var currentPath = AppContext.BaseDirectory;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    if (File.Exists(Path.Combine(currentPath, "DevStack.exe")) || 
                        File.Exists(Path.Combine(currentPath, "DevStackGUI.exe")))
                    {
                        return currentPath;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static async Task PerformUninstallation(ProgressBar progressBar, TextBlock statusText, Button uninstallButton, Button cancelButton)
        {
            try
            {
                progressBar.Visibility = Visibility.Visible;
                uninstallButton.IsEnabled = false;
                cancelButton.IsEnabled = false;

                var installPath = GetInstallationPath();

                // Step 1: Stop services
                progressBar.Value = 10;
                statusText.Text = "Parando serviços...";
                await Task.Delay(500);
                await StopDevStackServices();

                // Step 2: Remove shortcuts
                progressBar.Value = 30;
                statusText.Text = "Removendo atalhos...";
                await Task.Delay(500);
                await RemoveShortcuts();

                // Step 3: Clean registry
                progressBar.Value = 50;
                statusText.Text = "Limpando registro...";
                await Task.Delay(500);
                await CleanRegistry();

                // Step 4: Remove files (except uninstaller)
                progressBar.Value = 70;
                statusText.Text = "Removendo arquivos...";
                await Task.Delay(500);
                await RemoveFiles(installPath);

                // Step 5: Schedule uninstaller self-deletion
                progressBar.Value = 90;
                statusText.Text = "Finalizando...";
                await Task.Delay(500);

                progressBar.Value = 100;
                statusText.Text = "Desinstalação concluída!";

                MessageBox.Show(
                    "DevStack foi removido com sucesso do seu sistema.\n\nO uninstaller será fechado agora.",
                    "Desinstalação Concluída",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Schedule self-deletion
                await ScheduleSelfDeletion();

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                statusText.Text = "Erro durante a desinstalação";
                MessageBox.Show($"Erro durante a desinstalação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                uninstallButton.IsEnabled = true;
                cancelButton.IsEnabled = true;
            }
        }

        private static async Task StopDevStackServices()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Stop any DevStack related processes
                    var processes = Process.GetProcessesByName("DevStack");
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000);
                        }
                        catch { }
                    }

                    processes = Process.GetProcessesByName("DevStackGUI");
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000);
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private static async Task RemoveShortcuts()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Remove desktop shortcuts
                    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var shortcuts = new[] { "DevStack.lnk", "DevStack GUI.lnk", "DevStack CLI.lnk" };
                    
                    foreach (var shortcut in shortcuts)
                    {
                        var shortcutPath = Path.Combine(desktop, shortcut);
                        if (File.Exists(shortcutPath))
                        {
                            File.Delete(shortcutPath);
                        }
                    }

                    // Remove start menu shortcuts
                    var startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                    var devstackFolder = Path.Combine(startMenu, "Programs", "DevStack");
                    if (Directory.Exists(devstackFolder))
                    {
                        Directory.Delete(devstackFolder, true);
                    }
                }
                catch { }
            });
        }

        private static async Task CleanRegistry()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Remove DevStack registry entries
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\DevStack", false);
                    
                    // Remove from Programs and Features
                    var uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack";
                    Registry.LocalMachine.DeleteSubKeyTree(uninstallKey, false);
                }
                catch { }
            });
        }

        private static async Task RemoveFiles(string installPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                    {
                        var uninstallerPath = Path.Combine(AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                        
                        // Remove all files except the uninstaller
                        var files = Directory.GetFiles(installPath, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            if (!string.Equals(file, uninstallerPath, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch { }
                            }
                        }

                        // Remove empty directories
                        var directories = Directory.GetDirectories(installPath, "*", SearchOption.AllDirectories);
                        foreach (var dir in directories.OrderByDescending(d => d.Length))
                        {
                            try
                            {
                                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                                {
                                    Directory.Delete(dir);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            });
        }

        private static async Task ScheduleSelfDeletion()
        {
            await Task.Run(() =>
            {
                try
                {
                    var currentPath = Path.Combine(AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                    var installPath = AppContext.BaseDirectory;
                    
                    var batchContent = $@"
@echo off
timeout /t 2 /nobreak > nul
del ""{currentPath}""
if exist ""{installPath}"" rmdir ""{installPath}"" 2>nul
del ""%~f0""
";
                    var batchPath = Path.Combine(Path.GetTempPath(), "DevStackUninstaller_Cleanup.bat");
                    File.WriteAllText(batchPath, batchContent);
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = batchPath,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                catch { }
            });
        }
    }
}
