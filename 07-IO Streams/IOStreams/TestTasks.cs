using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace IOStreams
{
	public static class TestTasks
	{
		/// <summary>
		/// Parses Resourses\Planets.xlsx file and returns the planet data: 
		///   Jupiter     69911.00
		///   Saturn      58232.00
		///   Uranus      25362.00
		///    ...
		/// See Resourses\Planets.xlsx for details
		/// </summary>
		/// <param name="xlsxFileName">source file name</param>
		/// <returns>sequence of PlanetInfo</returns>
		public static IEnumerable<PlanetInfo> ReadPlanetInfoFromXlsx(string xlsxFileName)
		{
            // TODO : Implement ReadPlanetInfoFromXlsx method using System.IO.Packaging + Linq-2-Xml

            // HINT : Please be as simple & clear as possible.
            //        No complex and common use cases, just this specified file.
            //        Required data are stored in Planets.xlsx archive in 2 files:
            //         /xl/sharedStrings.xml      - dictionary of all string values
            //         /xl/worksheets/sheet1.xml  - main worksheet

            var pathToSharedStrings = "/xl/sharedStrings.xml";
            var pathToSheet = "/xl/worksheets/sheet1.xml";

            XDocument xmlSharedStrings = null,
                      xmlSheet = null;

            using (var package = Package.Open(xlsxFileName, FileMode.Open, FileAccess.Read))
            {
                xmlSharedStrings = package.LoadToXml(pathToSharedStrings);
                xmlSheet = package.LoadToXml(pathToSheet);
            }

            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            // 2 last element - the name of the columns
            // xmlSharedStrings.Descendants(ns + "t").Select(x => (string)x).Reverse().Skip(2).Reverse()
            var names = xmlSharedStrings.Descendants(ns + "t").Select(x => (string)x);
            var radius = xmlSheet.Descendants(ns + "row")
                .Skip(1)
                .Select(x => (double)x.Elements(ns + "c").ElementAt(1));

            return radius.Zip(names, (r, n) => new PlanetInfo()
            {
                Name = n,
                MeanRadius = r
            });
        }

        private static XDocument LoadToXml(this Package package, string xmlPath)
        {
            var uri = new Uri(xmlPath, UriKind.Relative);

            using (var stream = package.GetPart(uri).GetStream(FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(stream);
            }
        }


        /// <summary>
        /// Calculates hash of stream using specifued algorithm
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="hashAlgorithmName">hash algorithm ("MD5","SHA1","SHA256" and other supported by .NET)</param>
        /// <returns></returns>
        public static string CalculateHash(this Stream stream, string hashAlgorithmName)
		{
            // TODO : Implement CalculateHash method
            using (var algorithm = HashAlgorithm.Create(hashAlgorithmName))
            {
                if(algorithm == null)
                {
                    throw new ArgumentException("Algorithm not found", nameof(hashAlgorithmName));
                }

                return string.Join(string.Empty, algorithm.ComputeHash(stream).Select(x => x.ToString("X2")));
            }
		}


		/// <summary>
		/// Returns decompressed strem from file. 
		/// </summary>
		/// <param name="fileName">source file</param>
		/// <param name="method">method used for compression (none, deflate, gzip)</param>
		/// <returns>output stream</returns>
		public static Stream DecompressStream(string fileName, DecompressionMethods method)
		{
            // TODO : Implement DecompressStream method
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            switch (method)
            {
                case DecompressionMethods.GZip:
                    return new GZipStream(stream, CompressionMode.Decompress);

                case DecompressionMethods.Deflate:
                    return new DeflateStream(stream, CompressionMode.Decompress);
            }

            return stream;
        }


		/// <summary>
		/// Reads file content econded with non Unicode encoding
		/// </summary>
		/// <param name="fileName">source file name</param>
		/// <param name="encoding">encoding name</param>
		/// <returns>Unicoded file content</returns>
		public static string ReadEncodedText(string fileName, string encoding)
		{
            // TODO : Implement ReadEncodedText method
            return File.ReadAllText(fileName, Encoding.GetEncoding(encoding));
		}
	}


	public class PlanetInfo : IEquatable<PlanetInfo>
	{
		public string Name { get; set; }
		public double MeanRadius { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", Name, MeanRadius);
		}

		public bool Equals(PlanetInfo other)
		{
			return Name.Equals(other.Name)
				&& MeanRadius.Equals(other.MeanRadius);
		}
	}
}