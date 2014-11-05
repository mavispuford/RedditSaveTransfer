using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace RedditSaveTransfer.Exporting
{
    public abstract class ExportMethod
    {
        protected abstract string Export();

        /// <summary>
        ///     Exports to bytes
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ExportToBytes()
        {
            return Encoding.UTF8.GetBytes(Export() ?? "");
        }

        /// <summary>
        ///     Exports to a file at the given path
        /// </summary>
        /// <param name="path"></param>
        public virtual void ExportToFile(string path)
        {
            File.WriteAllText(path, Export());
        }

        /// <summary>
        ///     Cleans up the given value for export
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string CleanUpValue(object value)
        {
            if (value == null) return "";
            if (value is Nullable && ((INullable)value).IsNull) return "";

            if (value is DateTime)
            {
                var date = value as DateTime?;

                return date.Value.ToString(date.Value.TimeOfDay.TotalSeconds == 0 ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss");
            }

            var output = value.ToString();

            if (output.Contains(",") || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            if (output.Contains("\n"))
                output = output.Replace("\n", "");

            return output;
        }

        /// <summary>
        /// Shortens the given string according to the limit
        /// </summary>
        /// <param name="input">String to shorten</param>
        /// <param name="limit">Length limit</param>
        /// <returns>Shortenes string with "..." appended at the end</returns>
        protected static string Shorten(string input, int limit)
        {
            var output = input;

            //Subtracting 3 from the limit just so the ellipsis make it add up to the limit
            if (output.Length > limit - 3)
                output = output.Substring(0, limit - 3) + "...";

            return output;
        }
    }
}
