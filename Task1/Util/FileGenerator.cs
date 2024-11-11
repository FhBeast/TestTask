using System;
using System.IO;
using System.Text;

namespace Task1.Util
{
    /// <summary>
    /// Provides methods to generate text files containing random data.
    /// </summary>
    static class FileGenerator
    {
        /// <summary>
        /// Generates a specified number of text files, each containing a specified number of lines with random data.
        /// </summary>
        /// <param name="folderPath">The path to the folder where the files will be created.</param>
        /// <param name="filesNumber">The number of files to generate.</param>
        /// <param name="linesFileNumber">The number of lines each file should contain.</param>
        /// <param name="latinCharsNumber">The number of Latin characters in each generated string.</param>
        /// <param name="russianCharsNumber">The number of Russian characters in each generated string.</param>
        /// <param name="upperRangeOfEven">The upper range limit for generating random even integers.</param>
        /// <param name="upperRangeOfDecimal">The upper range limit for generating random decimal numbers.</param>
        public static void GenerateFiles(string folderPath,
                                         int filesNumber,
                                         int linesFileNumber,
                                         int latinCharsNumber,
                                         int russianCharsNumber,
                                         int upperRangeOfEven,
                                         int upperRangeOfDecimal)
        {
            Random random = new();
            Directory.CreateDirectory(folderPath);
            for (int fileIndex = 1; fileIndex <= filesNumber; fileIndex++)
            {
                string filePath = Path.Combine(folderPath, $"File_{fileIndex}.txt");
                using StreamWriter writer = new(filePath, false, Encoding.UTF8);
                for (int lineIndex = 0; lineIndex < linesFileNumber; lineIndex++)
                {
                    string date = RandomUtil.GenerateRandomDate(random).ToString("yyyy-MM-dd");
                    string latinChars = RandomUtil.GenerateRandomString(random, latinCharsNumber);
                    string russianChars = RandomUtil.GenerateRandomRussianString(random, russianCharsNumber);
                    int evenInt = RandomUtil.GenerateRandomEvenInt(random, 1, upperRangeOfEven);
                    double decimalNumber = RandomUtil.GenerateRandomDecimal(random, 1, upperRangeOfDecimal);
                    string line = $"{date}||{latinChars}||{russianChars}||{evenInt}||{decimalNumber:F8}";
                    writer.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Merges multiple text files into one, removing lines that contain the specified character combination.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing the input files.</param>
        /// <param name="outputFile">The name of file where merged content will be written.</param>
        /// <param name="delStr">The character combination to search for in lines to be deleted.</param>
        /// <returns>The total number of lines deleted across all input files.</returns>
        public static int MergeFiles(string folderPath, string outputFile, string delStr)
        {
            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + folderPath, "File_*.txt");

            int totalDeletedLines = 0;
            using StreamWriter writer = new(outputFile);
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
            }

            return totalDeletedLines;
        }
    }
}
