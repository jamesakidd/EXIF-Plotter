using System;
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

        public static string parseLatLong(string file)
        {
            string retVal = null;

            using (ExifReader reader = new ExifReader(file))
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return retVal;
        }

        static void Main(string[] args)
        {
            var files = Directory.GetFiles(@"pics\");
            var coordsList = files.Select(parseLatLong).ToList();

            string uri = $"https://www.google.ca/maps/dir/";

            var last = coordsList.Last();
            foreach (string coord in coordsList)
            {
                if (coord.Equals("0,-0"))
                {
                    continue;
                }

                if (coord.Equals(last))
                {
                    uri += $"/@{coord},17.5z";
                }
                else
                {
                    uri += coord + "/"; 
                }
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { UseShellExecute = true, FileName = uri });
        }
    }
}
