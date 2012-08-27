using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Data.SqlTypes;

namespace RedditSaveTransfer
{

    /// <summary>
    /// Exports to a CSV file
    /// **Thanks to Chris and Coder Net from stackoverflow.com for example code
    /// http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
    /// </summary>
    public class CsvExport
    {
        public List<SavedListing> Objects;

        public CsvExport(List<SavedListing> objects)
        {
            Objects = objects;
        }

        public string Export()
        {
            return Export(true);
        }

        public string Export(bool includeHeaderLine)
        {
            StringBuilder sb = new StringBuilder();

            if (includeHeaderLine)
            {
                //add header line.

                if (Objects.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in Objects[0].Properties)
                        sb.Append(pair.Key).Append(",");
                }

                sb.Remove(sb.Length - 1, 1).AppendLine();
            }

            //add value for each property.
            foreach (SavedListing listing in Objects)
            {
                foreach (KeyValuePair<string, string> pair in listing.Properties)
                {
                    sb.Append(MakeValueCsvFriendly(pair.Value)).Append(",");
                }
                sb.Remove(sb.Length - 1, 1).AppendLine();
            }

            return sb.ToString();
        }

        //export to a file.
        public void ExportToFile(string path)
        {
            File.WriteAllText(path, Export());
        }

        //export as binary data.
        public byte[] ExportToBytes()
        {
            return Encoding.UTF8.GetBytes(Export());
        }

        //get the csv value for field.
        private string MakeValueCsvFriendly(object value)
        {
            if (value == null) return "";
            if (value is Nullable && ((INullable)value).IsNull) return "";

            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString();

            if (output.Contains(",") || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            if (output.Contains("\n"))
                output = output.Replace("\n", "");

            return output;

        }
    }
}
