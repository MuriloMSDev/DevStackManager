using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DevStackManager.Components;

namespace DevStackManager
{
    /// <summary>
    /// Manages listing operations for installed and available component versions.
    /// Provides formatted table output for version information display.
    /// </summary>
    public static class ListManager
    {
        /// <summary>
        /// Width of the first column (component name) in the table display.
        /// </summary>
        private const int TABLE_COL1_WIDTH = 15;
        
        /// <summary>
        /// Width of the second column (versions list) in the table display.
        /// </summary>
        private const int TABLE_COL2_WIDTH = 40;
        
        /// <summary>
        /// Number of columns in horizontal table layout.
        /// </summary>
        private const int HORIZONTAL_TABLE_COLS = 6;
        
        /// <summary>
        /// Width of each cell in horizontal table layout.
        /// </summary>
        private const int HORIZONTAL_TABLE_CELL_WIDTH = 16;

        /// <summary>
        /// Lists all installed component versions in a formatted table.
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

        /// <summary>
        /// Prints the table header with column titles.
        /// </summary>
        private static void PrintTableHeader()
        {
            WriteColoredLine(CreateBorder('_'), ConsoleColor.Gray);
            WriteColoredLine(CreateTableRow("Tool", "Installed Versions"), ConsoleColor.Gray);
            WriteColoredLine(CreateSeparatorRow(), ConsoleColor.Gray);
        }

        /// <summary>
        /// Prints the table footer border.
        /// </summary>
        private static void PrintTableFooter()
        {
            WriteColoredLine(CreateBorder('¯'), ConsoleColor.Gray);
        }

        /// <summary>
        /// Creates a horizontal border character string.
        /// </summary>
        /// <param name="borderChar">Character to use for the border.</param>
        /// <returns>The border string.</returns>
        private static string CreateBorder(char borderChar)
        {
            return new string(borderChar, TABLE_COL1_WIDTH + TABLE_COL2_WIDTH + 3);
        }

        /// <summary>
        /// Creates a formatted table row with two columns.
        /// </summary>
        /// <param name="col1Text">First column text.</param>
        /// <param name="col2Text">Second column text.</param>
        /// <returns>The formatted row string.</returns>
        private static string CreateTableRow(string col1Text, string col2Text)
        {
            return $"|{CenterText(col1Text, TABLE_COL1_WIDTH)}|{CenterText(col2Text, TABLE_COL2_WIDTH)}|";
        }

        /// <summary>
        /// Creates a separator row between header and data rows.
        /// </summary>
        /// <returns>The separator row string.</returns>
        private static string CreateSeparatorRow()
        {
            return $"|{new string('-', TABLE_COL1_WIDTH)}+{new string('-', TABLE_COL2_WIDTH)}|";
        }

        /// <summary>
        /// Prints a component row with installation status.
        /// </summary>
        /// <param name="comp">The component to print.</param>
        private static void PrintComponentRow(ComponentInterface comp)
        {
            var installed = comp.ListInstalled();
            bool isInstalled = installed.Count > 0;
            
            var color = isInstalled ? ConsoleColor.Green : ConsoleColor.Red;
            var statusText = isInstalled ? string.Join(", ", installed) : "NOT INSTALLED";
            
            string row = CreateTableRow(comp.Name, statusText);
            PrintColoredTableRow(row, color);
        }

        /// <summary>
        /// Prints a table row with colored content between gray borders.
        /// </summary>
        /// <param name="row">The row string to print.</param>
        /// <param name="color">Color for the row content.</param>
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

        /// <summary>
        /// Writes colored text to the console without a newline.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="color">Text color.</param>
        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes colored text to the console with a newline.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="color">Text color.</param>
        private static void WriteColoredLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Prints a horizontal table with available and installed versions.
        /// Displays versions in a multi-column grid layout.
        /// </summary>
        /// <param name="data">Version data to display.</param>
        public static void PrintHorizontalTable(VersionData data)
        {
            if (data.Status == "error")
            {
                WriteColoredLine(data.Message, ConsoleColor.Red);
                return;
            }

            if (data.Versions == null || data.Versions.Count == 0)
            {
                Console.WriteLine("No versions found.");
                return;
            }

            var sortedVersions = SortVersions(data.Versions, data.OrderDescending ?? true);
            var tableLayout = CalculateTableLayout(sortedVersions.Count);

            PrintHorizontalTableHeader(data.Header, tableLayout.tableWidth);
            PrintHorizontalTableRows(sortedVersions, data.Installed, tableLayout.rows);
            PrintHorizontalTableFooter(tableLayout.tableWidth);
        }

        /// <summary>
        /// Sorts version strings in ascending or descending order.
        /// </summary>
        /// <param name="versions">List of version strings.</param>
        /// <param name="orderDescending">True for descending order, false for ascending.</param>
        /// <returns>Sorted version list.</returns>
        private static List<string> SortVersions(List<string> versions, bool orderDescending)
        {
            return orderDescending 
                ? versions.OrderByDescending(v => v).ToList() 
                : versions.ToList();
        }

        /// <summary>
        /// Calculates the row count and table width for horizontal layout.
        /// </summary>
        /// <param name="itemCount">Number of items to display.</param>
        /// <returns>Tuple containing row count and table width.</returns>
        private static (int rows, int tableWidth) CalculateTableLayout(int itemCount)
        {
            int rows = (int)Math.Ceiling((double)itemCount / HORIZONTAL_TABLE_COLS);
            int tableWidth = (HORIZONTAL_TABLE_CELL_WIDTH * HORIZONTAL_TABLE_COLS) + HORIZONTAL_TABLE_COLS + 1;
            return (rows, tableWidth);
        }

        /// <summary>
        /// Prints the horizontal table header with title and border.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="tableWidth">Total table width.</param>
        private static void PrintHorizontalTableHeader(string header, int tableWidth)
        {
            Console.WriteLine(header);
            WriteColoredLine(new string('-', tableWidth), ConsoleColor.Gray);
        }

        /// <summary>
        /// Prints the horizontal table footer border.
        /// </summary>
        /// <param name="tableWidth">Total table width.</param>
        private static void PrintHorizontalTableFooter(int tableWidth)
        {
            WriteColoredLine(new string('¯', tableWidth), ConsoleColor.Gray);
        }

        /// <summary>
        /// Prints all rows of the horizontal table.
        /// </summary>
        /// <param name="versions">All available versions.</param>
        /// <param name="installed">Installed versions list.</param>
        /// <param name="rows">Number of rows to print.</param>
        private static void PrintHorizontalTableRows(List<string> versions, List<string> installed, int rows)
        {
            for (int r = 0; r < rows; r++)
            {
                PrintHorizontalTableRow(versions, installed, r, rows);
            }
        }

        /// <summary>
        /// Prints a single row of the horizontal table with multiple columns.
        /// </summary>
        /// <param name="versions">All available versions.</param>
        /// <param name="installed">Installed versions list.</param>
        /// <param name="rowIndex">Current row index.</param>
        /// <param name="totalRows">Total number of rows.</param>
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

        /// <summary>
        /// Prints a single cell in the horizontal table, highlighting installed versions.
        /// </summary>
        /// <param name="versions">All available versions.</param>
        /// <param name="installed">Installed versions list.</param>
        /// <param name="index">Index of the version to display.</param>
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
        /// Centers text within a specified width using padding.
        /// </summary>
        /// <param name="text">Text to center.</param>
        /// <param name="width">Target width.</param>
        /// <returns>The centered text string.</returns>
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
        /// Lists available and installed versions for a specified component.
        /// </summary>
        /// <param name="component">Component name to list versions for.</param>
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
                    Message = $"{versions.Count} versions found for {component}",
                    Header = $"Available versions for {component}",
                    Versions = versions,
                    Installed = installed
                };
                PrintHorizontalTable(data);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"Unrecognized component: {component}", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Use 'DevStackManager help' to see available components.", ConsoleColor.Yellow);
            }
        }
    }
}
