using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    internal class ResourceLoader
    {
        /// <summary>
        /// Checks if an embedded resource exists in the executing assembly.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to check.</param>
        /// <returns>
        /// A boolean value indicating whether the specified resource exists.
        /// Returns <c>true</c> if the resource exists; otherwise, <c>false</c>.
        /// </returns>
        public static bool Exists(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();

            return resources.Contains(resourceName);
        }

        /// <summary>
        /// Loads an embedded resource as an array of strings.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to load.</param>
        /// <returns>An array of strings representing the lines of the resource.</returns>
        /// <exception cref="Exception">Thrown if the specified resource is not found.</exception>
        public static string[] LoadEmbedded(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new Exception("Resource not found: " + resourceName);

            using var reader = new StreamReader(stream);

            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Loads an embedded resource as an array of strings, with an optional parameter to modify behavior.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to load.</param>
        /// <param name="option">
        /// A byte value representing an option for loading the resource.  
        /// Use <see cref="ResourceLoaderOptions.SkipHeaders"/> to skip the first line of the resource.
        /// </param>
        /// <returns>An array of strings representing the lines of the resource.</returns>
        /// <exception cref="Exception">Thrown if the specified resource is not found.</exception>
        public static string[] LoadEmbedded(string resourceName, ResourceLoaderOptions option)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new Exception("Resource not found: " + resourceName);

            using var reader = new StreamReader(stream);

            if (option == ResourceLoaderOptions.SkipHeaders) reader.ReadLine();

            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            return lines.ToArray();
        }
    }
}
