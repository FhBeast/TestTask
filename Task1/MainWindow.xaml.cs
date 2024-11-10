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
using static System.Windows.Forms.LinkLabel;
using System.Numerics;

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

        private const string _latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string _russianChars = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЫЭЮЯабвгдежзиклмнопрстуфхцчшщыэюя";

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
                    string date = GenerateRandomDate(random).ToString("yyyy-MM-dd");
                    string latinChars = GenerateRandomString(random, _latinCharsNumber);
                    string russianChars = GenerateRandomRussianString(random, _russianCharsNumber);
                    int evenInt = GenerateRandomEvenInt(random, 1, _upperRangeOfEven);
                    double decimalNumber = GenerateRandomDecimal(random, 1, _upperRangeOfDecimal);
                    string line = $"{date}||{latinChars}||{russianChars}||{evenInt}||{decimalNumber:F8}";
                    writer.WriteLine(line);
                }
            }
        }

        /// <summary> 
        /// Generates a random date within the last 5 years. 
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <returns>A random DateTime within the last 5 years.</returns>
        private static DateTime GenerateRandomDate(Random random)
        {
            DateTime start = DateTime.Now.AddYears(-5);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }

        /// <summary>
        /// Generates a random string of specified length composed of Latin characters.
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <param name="length">The length of the random string.</param>
        /// <returns>A random string of Latin characters.</returns>
        private static string GenerateRandomString(Random random, int length)
        {

            return new string(Enumerable.Repeat(_latinChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random string of specified length composed of Russian characters.
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <param name="length">The length of the random string.</param>
        /// <returns>A random string of Russian characters.</returns>
        static string GenerateRandomRussianString(Random random, int length)
        {
            return new string(Enumerable.Repeat(_russianChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random even integer within a specified range.
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>A random even integer within the specified range.</returns>
        static int GenerateRandomEvenInt(Random random, int min, int max)
        {
            int number = random.Next(min, max);
            return number % 2 == 0 ? number : ++number;
        }

        /// <summary>
        /// Generates a random positive decimal number with 8 decimal places within a specified range.
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>A random positive decimal number with 8 decimal places within the specified range.</returns>
        static double GenerateRandomDecimal(Random random, double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Merges multiple text files into one, removing lines that contain the specified character combination.
        /// </summary>
        private void ButtonMerge_Click(object sender, RoutedEventArgs e)
        {
            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + _folderPath, "File_*.txt");

            int totalDeletedLines = 0;
            using StreamWriter writer = new(_outputFile);
            foreach (string inputFile in inputFiles)
            {
                using StreamReader reader = new(inputFile);
                string line;
                int deletedLines = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(TextBoxDelStr.Text))
                    {
                        deletedLines++;
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
                totalDeletedLines += deletedLines;
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
                bulkCopy.NotifyAfter = 1000;
                bulkCopy.SqlRowsCopied += (sender, e) =>
                {
                    importedRows += 1000; 
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBarRows.Value = importedRows;
                        TextProgressRows.Content = $"{importedRows} of {totalRows}";
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
    }
}