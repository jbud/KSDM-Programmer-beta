using System;
using System.Diagnostics;
using System.Security.Claims;

namespace KSDMProgrammer2
{
    public static class Helper
    {
        /// <summary>
        /// Returns a string from between two strings inside of a string.
        /// Example: getStringBetween("Hello World!", " ", "!"); // Returns: "World"
        /// </summary>
        /// <param name="s">The string to search</param>
        /// <param name="start">The beginning of the search</param>
        /// <param name="end">The end of the search</param>
        /// <returns>The found string; null if failed.</returns>
        public static string getStringBetween(string s, string start, string end)
        {
            int pTo, pFrom;
            try
            {
                pFrom = s.IndexOf(start) + start.Length;
                pTo = s.IndexOf(end);
                return s.Substring(pFrom, pTo - pFrom);
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
            
        }
        /// <summary>
        /// Returns extension of a given filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Extension</returns>
        public static string getFileExtension(string filename)
        {
            string ext = filename.Substring(filename.LastIndexOf('.') + 1);
            return ext;
        }

        /// <summary>
        /// Swaps values in an array between X and Y positions
        /// Example: array = Swap<int>(array, 0, array.Length - 1);
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="arr">The Array</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        public static T[] Swap<T>(this T[] arr, int x, int y)
        {
            T t = arr[x];
            arr[x] = arr[y];
            arr[y] = t;
            return arr;
        }
        /// <summary>
        /// Constrain a value between a low and high value.
        /// </summary>
        /// <param name="n">Input</param>
        /// <param name="low">Low Value</param>
        /// <param name="high">High Value</param>
        /// <returns>Constrained number</returns>
        public static int Constrain(int n, int low, int high)
        {
            return Math.Max(Math.Min(n, high), low); ;
        }
        /// <summary>
        /// Map a range of values to another range.
        /// </summary>
        /// <param name="n">number to be mapped</param>
        /// <param name="startx">lower bound of the value's current range</param>
        /// <param name="endx">upper bound of the value's current range</param>
        /// <param name="starty">lower bound of the target's current range</param>
        /// <param name="endy">upper bound of the value's current range</param>
        /// <param name="bounds">Optional: Constrain within target bounds = true (default = false)</param>
        /// <returns>Mapped value</returns>
        public static int Map(int n, int startx, int endx, int starty, int endy, bool bounds = false)
        {
            int v = (n - startx) / (endx - startx) * (endy - starty) + starty;
            if (!bounds)
                return v;

            if (starty < endy)
                return Constrain(v, starty, endy);
            else
                return Constrain(v, endy, starty);

        }

        private static readonly string base36Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Base36Encode(int n)
        {
            int BaseN = base36Alphabet.Length;
            string v = "";
            bool isNeg = false;

            if (n <= 0)
            {
                isNeg = true;
                n = -n;
            }

            do
            {
                int i = n % BaseN;
                n /= BaseN;
                v = base36Alphabet[i] + v;
            } 
            while (n != 0);

            return isNeg ? "-" + v : v;
        }
        public static int Base36Decode(string b36)
        {
            bool isNeg = false;
            if (b36[0] == '-')
            {
                b36 = b36.Substring(1);
                isNeg = true;
            }
            int decrypted = 0;
            int BaseN = base36Alphabet.Length;
            for (int i = b36.Length - 1; i >= 0; i--)
            {
                char c = b36[b36.Length - 1 - i];
                int f = base36Alphabet.IndexOf(c);
                decrypted += f * (int)Math.Floor(Math.Pow(BaseN, i));
            }

            return isNeg ? -decrypted : decrypted;
        }
    }
}
