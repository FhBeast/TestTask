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

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Generates 100 text files with 100,000 lines each.
        /// </summary>
        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            Random random = new();
            Directory.CreateDirectory(_folderPath);
            for (int fileIndex = 1; fileIndex <= _filesNumber; fileIndex++)
            {
                string filePath = System.IO.Path.Combine(_folderPath, $"File_{fileIndex}.txt");
                using StreamWriter writer = new(filePath, false, Encoding.UTF8);
                for (int lineIndex = 0; lineIndex < _linesFileNumber; lineIndex++)
                {
                    string date = RandomUtil.GenerateRandomDate(random).ToString("yyyy-MM-dd");
                    string latinChars = RandomUtil.GenerateRandomString(random, _latinCharsNumber);
                    string russianChars = RandomUtil.GenerateRandomRussianString(random, _russianCharsNumber);
                    int evenInt = RandomUtil.GenerateRandomEvenInt(random, 1, _upperRangeOfEven);
                    double decimalNumber = RandomUtil.GenerateRandomDecimal(random, 1, _upperRangeOfDecimal);
                    string line = $"{date}||{latinChars}||{russianChars}||{evenInt}||{decimalNumber:F8}";
                    writer.WriteLine(line);
                }
            }
        }        

        /// <summary>
        /// Merges multiple text files into one, removing lines that contain the specified character combination.
        /// </summary>
        private void ButtonMerge_Click(object sender, RoutedEventArgs e)
        {
            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + _folderPath, "File_*.txt");

            string delStr = TextBoxDelStr.Text;

            int totalDeletedLines = 0;
            using StreamWriter writer = new(_outputFile);
            foreach (string inputFile in inputFiles)
            {
                using StreamReader reader = new(inputFile);
                string line;
                int deletedLines = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (delStr != String.Empty && line.Contains(delStr))
                    {
                        deletedLines++;
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
                totalDeletedLines += deletedLines;
                MergeLog.Content = $"{totalDeletedLines} lines were removed";
            }
        }

        /// <summary>
        /// Imports data files into the database.
        /// </summary>
        private async void ButtonPullToTheDataBase_Click(object sender, RoutedEventArgs e)
        {
            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + _folderPath, "File_*.txt");

            await Task.Run(() =>
            {
                int totalFiles = inputFiles.Length;
                int importedFiles = 0;

                Dispatcher.Invoke(() =>
                {
                    ProgressBarFiles.Maximum = totalFiles;
                });

                foreach (string inputFile in inputFiles)
                {
                    ImportFileToDatabase(inputFile);
                    importedFiles++;
                }

                Dispatcher.Invoke(() =>
                {
                    ProgressBarFiles.Value = 0;
                });
            });
        }

        /// <summary> 
        /// Imports data from the file into the database.
        /// </summary>
        /// <param name="filePath">The path to the file containing the data to import.</param>
        /// <returns>A task that represents the asynchronous operation of data import.</returns>
        private async void ImportFileToDatabase(string filePath)
        {
            await Task.Run(() =>
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
    }
}