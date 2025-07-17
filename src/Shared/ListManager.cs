using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DevStackManager.Components;

namespace DevStackManager
{
    public static class ListManager
    {
        /// <summary>
        /// Lista todas as ferramentas instaladas em formato de tabela
        /// </summary>
        public static void ListInstalledVersions()
        {
            var allComponents = ComponentsFactory.GetAll();
            int col1 = 15, col2 = 40;
            string header = new string('_', col1 + col2 + 3);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(header);
            string headerRow = $"|{CenterText("Ferramenta", col1)}|{CenterText("Versões Instaladas", col2)}|";
            Console.WriteLine(headerRow);
            string separator = $"|{new string('-', col1)}+{new string('-', col2)}|";
            Console.WriteLine(separator);
            Console.ResetColor();
            foreach (var comp in allComponents)
            {
                string ferramenta = CenterText(comp.Name, col1);
                var installed = comp.ListInstalled();
                if (installed.Count > 0)
                {
                    string status = CenterText(string.Join(", ", installed), col2);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(ferramenta);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("|");
                    Console.ResetColor();
                }
                else
                {
                    string status = CenterText("NÃO INSTALADO", col2);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(ferramenta);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(status);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("|");
                    Console.ResetColor();
                }
            }
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            string footer = new string('¯', col1 + col2 + 3);
            Console.WriteLine(footer);
            Console.ResetColor();
        }

        /// <summary>
        /// Imprime uma tabela horizontal com versões disponíveis e instaladas
        /// </summary>
        public static void PrintHorizontalTable(VersionData data)
        {
            if (data.Status == "error")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(data.Message);
                Console.ResetColor();
                return;
            }
            
            var items = data.Versions;
            string header = data.Header;
            var installed = data.Installed;
            bool orderDescending = data.OrderDescending ?? true;
            int cols = 6;
            
            if (items == null || items.Count == 0)
            {
                Console.WriteLine("Nenhuma versão encontrada.");
                return;
            }
            
            // Ordenar da maior para menor (descendente)
            if (orderDescending)
            {
                items = items.OrderByDescending(v => v).ToList();
            }
            
            int total = items.Count;
            int rows = (int)Math.Ceiling((double)total / cols);
            int width = 16;
            int tableWidth = (width * cols) + cols + 1;
            
            Console.WriteLine(header);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('-', tableWidth));
            Console.ResetColor();
            
            // Preencher linhas de cima para baixo (coluna 1: maior, coluna 2: próxima maior, etc)
            for (int r = 0; r < rows; r++)
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("|");
                for (int c = 0; c < cols; c++)
                {
                    int idx = c * rows + r;
                    if (idx < total)
                    {
                        string val = Regex.Replace(items[idx], @"[^\d\.]", "");
                        string cellText = CenterText(val, width);
                        
                        if (installed.Contains(val))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(cellText);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(cellText);
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.Write(CenterText("", width));
                    }
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
            
            // Garantir que a cor está resetada antes do rodapé
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('¯', tableWidth));
            Console.ResetColor();
        }

        /// <summary>
        /// Centraliza um texto em uma largura específica
        /// </summary>
        private static string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', width);
                
            if (text.Length >= width)
                return text.Substring(0, width);
                
            int padding = width - text.Length;
            int leftPadding = padding / 2;
            int rightPadding = padding - leftPadding;
            
            return new string(' ', leftPadding) + text + new string(' ', rightPadding);
        }

        /// <summary>
        /// Lista versões baseado no componente especificado
        /// </summary>
        public static void ListVersions(string component)
        {
            var comp = ComponentsFactory.GetComponent(component);
            if (comp != null)
            {
                var versions = comp.ListAvailable();
                var installed = comp.ListInstalled();
                var data = new VersionData
                {
                    Status = "ok",
                    Message = $"{versions.Count} versões encontradas para {component}",
                    Header = $"Versões disponíveis para {component}",
                    Versions = versions,
                    Installed = installed
                };
                PrintHorizontalTable(data);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"Componente não reconhecido: {component}", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Use 'DevStackManager help' para ver os componentes disponíveis.", ConsoleColor.Yellow);
            }
        }
    }
}
