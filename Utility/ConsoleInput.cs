using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QRay.Utility
{
    internal class ConsoleInput
    {
        public static T GetInput<T>(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                string input = Console.ReadLine();
                if (input == null || input.Trim() == "") continue;

                try
                {
                    // Attempt to convert the input to the desired type
                    if (typeof(T) == typeof(bool))
                    {
                        if (input.Trim().ToLower() == "yes")
                        {
                            return (T)(object)true;
                        }
                        else if (input.Trim().ToLower() == "no")
                        {
                            return (T)(object)false;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    T value = (T)Convert.ChangeType(input, typeof(T));
                    return value;
                }
                catch
                {
                    Console.WriteLine($"Invalid input. Please enter a value of type {typeof(T).Name}.");
                }
            }
        }

        public static T GetInput<T>(string prompt, string regexp)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                string input = Console.ReadLine();
                if (input == null || input.Trim() == "") continue;

                try
                {
                    // Attempt to convert the input to the desired type
                    if (typeof(T) == typeof(bool))
                    {
                        if (input.Trim().ToLower() == "yes")
                        {
                            return (T)(object)true;
                        }
                        else if (input.Trim().ToLower() == "no")
                        {
                            return (T)(object)false;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!Regex.IsMatch(input, regexp))
                    {
                        Console.WriteLine("Please provide a valid input.");
                        continue;
                    }

                    T value = (T)Convert.ChangeType(input, typeof(T));
                    return value;
                }
                catch
                {
                    Console.WriteLine($"Invalid input. Please enter a value of type {typeof(T).Name}.");
                }
            }
        }
    }
}
