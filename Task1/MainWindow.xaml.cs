using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using Task1.Util;
using Task1.Model;
using System.Windows.Threading;

namespace Task1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _filesNumber = 100;
        private readonly int _linesFileNumber = 100000;

        private readonly int _latinCharsNumber = 10;
        private readonly int _russianCharsNumber = 10;
        private readonly int _upperRangeOfEven = 100000000;
        private readonly int _upperRangeOfDecimal = 20;

        private readonly int _notifyAfter = 1000;

        private readonly string _outputFile = "Merged_file.txt";
        private readonly string _folderPath = "GeneratedFiles";

        private readonly string _connectionString = "Server=localhost;Database=TextFileDataDB;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly string _procedureCalcName = "CalculateSumAndMedian";
        private readonly string _procedureClearDBName = "TruncateImportedData";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Generates 100 text files with 100,000 lines each.
        /// </summary>
        private async void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            BlockButtons();

            await Task.Run(() =>
            {
                FileGenerator.GenerateFiles(_folderPath,
                                        _filesNumber,
                                        _linesFileNumber,
                                        _latinCharsNumber,
                                        _russianCharsNumber,
                                        _upperRangeOfEven,
                                        _upperRangeOfDecimal);
            });

            UnblockButtons();
        }

        /// <summary>
        /// Merges multiple text files into one, removing lines that contain the specified character combination.
        /// </summary>
        private async void ButtonMerge_Click(object sender, RoutedEventArgs e)
        {
            BlockButtons();
            int removedLinesNumber = 0;
            string removingText = TextBoxDelStr.Text;

            await Task.Run(() =>
            {
                removedLinesNumber = FileGenerator.MergeFiles(_folderPath, _outputFile, removingText);
            });

            MergeLog.Content = $"{removedLinesNumber} lines were removed";
            UnblockButtons();
        }

        /// <summary>
        /// Imports data files into the database.
        /// </summary>
        private async void ButtonPullToTheDataBase_Click(object sender, RoutedEventArgs e)
        {
            BlockButtons();

            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + _folderPath, "File_*.txt");
            ProgressBarFiles.Maximum = inputFiles.Length;

            await Task.Run(() =>
            {
                foreach (string inputFile in inputFiles)
                {
                    ImportFileToDatabase(inputFile);
                }
            });

            ProgressBarFiles.Value = 0;
            UnblockButtons();
        }

        /// <summary> 
        /// Imports data from the file into the database.
        /// </summary>
        /// <param name="filePath">The path to the file containing the data to import.</param>
        /// <returns>A task that represents the asynchronous operation of data import.</returns>
        private async void ImportFileToDatabase(string filePath)
        {
            DataTable dataTable = new();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Columns.Add("LatinString", typeof(string));
            dataTable.Columns.Add("CyrillicString", typeof(string));
            dataTable.Columns.Add("EvenNumber", typeof(int));
            dataTable.Columns.Add("FloatNumber", typeof(float));

            int totalRows = 0;
            int importedRows = 0;
            string line;

            using StreamReader reader = new(filePath);
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split("||", StringSplitOptions.None);
                DataRow row = dataTable.NewRow();
                row["ID"] = 1;
                row["Date"] = DateTime.Parse(parts[0]);
                row["LatinString"] = parts[1];
                row["CyrillicString"] = parts[2];
                row["EvenNumber"] = int.Parse(parts[3]);
                row["FloatNumber"] = float.Parse(parts[4]);
                dataTable.Rows.Add(row); totalRows++;
            }

            Dispatcher.Invoke(() =>
            {
                ProgressBarRows.Maximum = totalRows;
            });

            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlBulkCopy bulkCopy = new(connection);
            bulkCopy.DestinationTableName = "ImportedData";
            bulkCopy.NotifyAfter = _notifyAfter;
            bulkCopy.SqlRowsCopied += (sender, e) =>
            {
                importedRows += _notifyAfter;

                Dispatcher.Invoke(() =>
                {
                    ProgressBarRows.Value = importedRows;
                });
            };

            bulkCopy.WriteToServer(dataTable);

            Dispatcher.Invoke(() =>
            {
                ProgressBarRows.Value = 0;
                ProgressBarFiles.Value++;
            });
        }

        /// <summary>
        /// Updates the TextProgressRows label to show the current value and the maximum value of the ProgressBar.
        /// </summary>
        private void ProgressBarRows_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextProgressRows.Content = $"{ProgressBarRows.Value} of {ProgressBarRows.Maximum}";
        }

        /// <summary>
        /// Updates the TextProgressFiles label to show the current value and the maximum value of the ProgressBar.
        /// </summary>
        private void ProgressBarFiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextProgressFiles.Content = $"{ProgressBarFiles.Value} of {ProgressBarFiles.Maximum}";
        }

        /// <summary>
        /// Calls a stored procedure to calculate the sum of even numbers and the median of decimal numbers.
        /// Displays the results in the ProcedureOutput text block.
        /// </summary>
        private async void ButtonSumAndMedian_Click(object sender, RoutedEventArgs e)
        {
            BlockButtons();

            await Task.Run(() =>
            {
                DatabaseManager dbManager = new(_connectionString);
                DataTable resultTable = dbManager.CallProcedure(_procedureCalcName);

                if (resultTable.Rows.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProcedureOutput.Text = $"Sum of an even numbers: {resultTable.Rows[0]["SumEvenNumber"]}\r"
                                               + $"The decimal madian: {resultTable.Rows[0]["MedianFloatNumber"]}";
                    });
                }
            });

            UnblockButtons();
        }

        /// <summary>
        /// Calls a stored procedure to truncate the ImportedData table in the database.
        /// </summary>
        private async void ButtomDeleteFromDB_Click(object sender, RoutedEventArgs e)
        {
            BlockButtons();

            await Task.Run(() =>
            {
                DatabaseManager dbManager = new(_connectionString);
                dbManager.CallProcedure(_procedureClearDBName);
            });

            UnblockButtons();
        }

        private void BlockButtons()
        {
            ButtonGenerate.IsEnabled = false;
            ButtonMerge.IsEnabled = false;
            ButtonPullToTheDataBase.IsEnabled = false;
            ButtomDeleteFromDB.IsEnabled = false;
            ButtonSumAndMedian.IsEnabled = false;
        }

        private void UnblockButtons()
        {
            ButtonGenerate.IsEnabled = true;
            ButtonMerge.IsEnabled = true;
            ButtonPullToTheDataBase.IsEnabled = true;
            ButtomDeleteFromDB.IsEnabled = true;
            ButtonSumAndMedian.IsEnabled = true;
        }
    }
}