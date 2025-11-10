using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DevStackManager.Components;

namespace DevStackManager
{
    public static class ListManager
    {
        private const int TABLE_COL1_WIDTH = 15;
        private const int TABLE_COL2_WIDTH = 40;
        private const int HORIZONTAL_TABLE_COLS = 6;
        private const int HORIZONTAL_TABLE_CELL_WIDTH = 16;
        /// <summary>
        /// Lista todas as ferramentas instaladas em formato de tabela
        /// </summary>
        public static void ListInstalledVersions()
        {
            var allComponents = ComponentsFactory.GetAll();
            
            PrintTableHeader();

            foreach (var comp in allComponents)
            {
                PrintComponentRow(comp);
            }

            PrintTableFooter();
        }

        private static void PrintTableHeader()
        {
            WriteColoredLine(CreateBorder('_'), ConsoleColor.Gray);
            WriteColoredLine(CreateTableRow("Ferramenta", "Versões Instaladas"), ConsoleColor.Gray);
            WriteColoredLine(CreateSeparatorRow(), ConsoleColor.Gray);
        }

        private static void PrintTableFooter()
        {
            WriteColoredLine(CreateBorder('¯'), ConsoleColor.Gray);
        }

        private static string CreateBorder(char borderChar)
        {
            return new string(borderChar, TABLE_COL1_WIDTH + TABLE_COL2_WIDTH + 3);
        }

        private static string CreateTableRow(string col1Text, string col2Text)
        {
            return $"|{CenterText(col1Text, TABLE_COL1_WIDTH)}|{CenterText(col2Text, TABLE_COL2_WIDTH)}|";
        }

        private static string CreateSeparatorRow()
        {
            return $"|{new string('-', TABLE_COL1_WIDTH)}+{new string('-', TABLE_COL2_WIDTH)}|";
        }

        private static void PrintComponentRow(ComponentInterface comp)
        {
            var installed = comp.ListInstalled();
            bool isInstalled = installed.Count > 0;
            
            var color = isInstalled ? ConsoleColor.Green : ConsoleColor.Red;
            var statusText = isInstalled ? string.Join(", ", installed) : "NÃO INSTALADO";
            
            string row = CreateTableRow(comp.Name, statusText);
            PrintColoredTableRow(row, color);
        }

        private static void PrintColoredTableRow(string row, ConsoleColor color)
        {
            int firstPipe = row.IndexOf('|', 1);
            int secondPipe = row.IndexOf('|', firstPipe + 1);
            
            WriteColored("|", ConsoleColor.Gray);
            WriteColored(row.Substring(1, firstPipe - 1), color);
            WriteColored("|", ConsoleColor.Gray);
            WriteColored(row.Substring(firstPipe + 1, secondPipe - firstPipe - 1), color);
            WriteColoredLine("|", ConsoleColor.Gray);
        }

        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private static void WriteColoredLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Imprime uma tabela horizontal com versões disponíveis e instaladas
        /// </summary>
        public static void PrintHorizontalTable(VersionData data)
        {
            if (data.Status == "error")
            {
                WriteColoredLine(data.Message, ConsoleColor.Red);
                return;
            }

            if (data.Versions == null || data.Versions.Count == 0)
            {
                Console.WriteLine("Nenhuma versão encontrada.");
                return;
            }

            var sortedVersions = SortVersions(data.Versions, data.OrderDescending ?? true);
            var tableLayout = CalculateTableLayout(sortedVersions.Count);

            PrintHorizontalTableHeader(data.Header, tableLayout.tableWidth);
            PrintHorizontalTableRows(sortedVersions, data.Installed, tableLayout.rows);
            PrintHorizontalTableFooter(tableLayout.tableWidth);
        }

        private static List<string> SortVersions(List<string> versions, bool orderDescending)
        {
            return orderDescending 
                ? versions.OrderByDescending(v => v).ToList() 
                : versions.ToList();
        }

        private static (int rows, int tableWidth) CalculateTableLayout(int itemCount)
        {
            int rows = (int)Math.Ceiling((double)itemCount / HORIZONTAL_TABLE_COLS);
            int tableWidth = (HORIZONTAL_TABLE_CELL_WIDTH * HORIZONTAL_TABLE_COLS) + HORIZONTAL_TABLE_COLS + 1;
            return (rows, tableWidth);
        }

        private static void PrintHorizontalTableHeader(string header, int tableWidth)
        {
            Console.WriteLine(header);
            WriteColoredLine(new string('-', tableWidth), ConsoleColor.Gray);
        }

        private static void PrintHorizontalTableFooter(int tableWidth)
        {
            WriteColoredLine(new string('¯', tableWidth), ConsoleColor.Gray);
        }

        private static void PrintHorizontalTableRows(List<string> versions, List<string> installed, int rows)
        {
            for (int r = 0; r < rows; r++)
            {
                PrintHorizontalTableRow(versions, installed, r, rows);
            }
        }

        private static void PrintHorizontalTableRow(List<string> versions, List<string> installed, int rowIndex, int totalRows)
        {
            WriteColored("|", ConsoleColor.Gray);

            for (int c = 0; c < HORIZONTAL_TABLE_COLS; c++)
            {
                int idx = c * totalRows + rowIndex;
                PrintHorizontalTableCell(versions, installed, idx);
                WriteColored("|", ConsoleColor.Gray);
            }

            Console.WriteLine();
        }

        private static void PrintHorizontalTableCell(List<string> versions, List<string> installed, int index)
        {
            if (index >= versions.Count)
            {
                WriteColored(CenterText("", HORIZONTAL_TABLE_CELL_WIDTH), ConsoleColor.Gray);
                return;
            }

            string cleanVersion = Regex.Replace(versions[index], @"[^\d\.]", "");
            string cellText = CenterText(cleanVersion, HORIZONTAL_TABLE_CELL_WIDTH);
            var color = installed.Contains(cleanVersion) ? ConsoleColor.Green : ConsoleColor.Gray;

            WriteColored(cellText, color);
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
