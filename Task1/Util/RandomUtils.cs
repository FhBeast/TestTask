using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1.Util
{
    /// <summary>
    /// Provides methods to generate random values.
    /// </summary>
    public static class RandomUtils
    {
        private const string _latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string _russianChars = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЫЭЮЯабвгдежзиклмнопрстуфхцчшщыэюя";

        /// <summary> 
        /// Generates a random date within the last 5 years. 
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <returns>A random DateTime within the last 5 years.</returns>
        public static DateTime GenerateRandomDate(Random random)
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
        public static string GenerateRandomString(Random random, int length)
        {

            return new string(Enumerable.Repeat(_latinChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random string of specified length composed of Russian characters.
        /// </summary>
        /// <param name="random">An instance of Random.</param>
        /// <param name="length">The length of the random string.</param>
        /// <returns>A random string of Russian characters.</returns>
        public static string GenerateRandomRussianString(Random random, int length)
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
        public static int GenerateRandomEvenInt(Random random, int min, int max)
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
        public static double GenerateRandomDecimal(Random random, double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }
    }
}
