using System.Collections.Generic;
using System.Text;

namespace RedditSaveTransfer.Exporting
{

    /// <summary>
    /// Exports to a CSV file
    /// **Thanks to Chris and Coder Net from stackoverflow.com for example code
    /// http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
    /// </summary>
    public class CsvExport : ExportMethod
    {
        private readonly List<SavedListing> _objects;

        public CsvExport(List<SavedListing> objects)
        {
            _objects = objects;
        }

        protected override string Export()
        {
            var sb = new StringBuilder();

            // Adds a header line to the csv file
            if (_objects.Count > 0)
            {
                foreach (var pair in _objects[0].Properties)
                    sb.Append(pair.Key).Append(",");
            }

            sb.Remove(sb.Length - 1, 1).AppendLine();

            // Add value of each property.
            foreach (var listing in _objects)
            {
                foreach (var pair in listing.Properties)
                {
                    sb.Append(CleanUpValue(pair.Value)).Append(",");
                }
                sb.Remove(sb.Length - 1, 1).AppendLine();
            }

            return sb.ToString();
        }
    }
}
