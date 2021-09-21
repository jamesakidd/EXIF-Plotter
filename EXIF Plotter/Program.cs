using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ExifLib;

namespace EXIF_Plotter
{
    class Program
    {
        public static double convertGeoFormat(double[] geoArr)
        {
            return Math.Round(Math.Sign(geoArr[0]) * Math.Abs(geoArr[0] + geoArr[1] / 60.0 + geoArr[2] / 3600.0), 6);
        }

        public static string parseLatLong(string file, ref List<string> errors)
        {
            string retVal = null;
            try
            {
                using (ExifReader reader = new ExifReader(file))
                {

                    if (reader.GetTagValue<double[]>(ExifTags.GPSLatitude, out var lat))
                    {
                        retVal = convertGeoFormat(lat).ToString(CultureInfo.CurrentCulture);
                    }

                    if (reader.GetTagValue<double[]>(ExifTags.GPSLongitude, out var lon))
                    {
                        retVal += $",-{convertGeoFormat(lon).ToString(CultureInfo.CurrentCulture)}";
                    }

                }
            }
            catch (ExifLibException)
            {
                errors.Add($"No EXIF data found in {file.Substring(file.IndexOf(@"\", StringComparison.Ordinal))}");
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            return retVal;
        }

        static void Main()
        {
            List<string> errors = new List<string>();
            var files = Directory.GetFiles(@"pics\");
            var coordsList = new Dictionary<string, string>();

            foreach (string file in files)
            {
                var tempCoord = parseLatLong(file, ref errors);
                if (tempCoord != null)
                {
                    coordsList.Add(tempCoord, file);
                }
            }

            string uri = $"https://www.google.ca/maps/dir/";
            if (coordsList.Any())
            {
                var last = coordsList.Last();

                foreach (var coord in coordsList)
                {
                    if (coord.Key.Equals("0,-0"))
                    {
                        errors.Add($"No GPS data found in {coord.Value.Substring(coord.Value.IndexOf(@"\", StringComparison.Ordinal))}");
                        continue;
                    }

                    if (coord.Equals(last))
                    {
                        uri += $"/@{coord.Key},17.5z";
                    }
                    else
                    {
                        uri += coord.Key + "/";
                    }
                }

                Console.WriteLine("\n\n---------=============== Picture Coordinates ===============---------\n");
                foreach (KeyValuePair<string, string> coord in coordsList)
                {
                    Console.WriteLine($"{coord.Value}: {coord.Key}");
                }

                Console.WriteLine("\n\n");

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { UseShellExecute = true, FileName = uri });
            }

            if (!errors.Any()) return;
            errors.ForEach(Console.WriteLine);
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
