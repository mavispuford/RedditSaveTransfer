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
    /// Exports to an HTML file
    /// **Thanks to Chris and Coder Net from stackoverflow.com for example code
    /// http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
    /// </summary>
    public class HTMLExport
    {
        public List<SavedListing> Objects;

        public HTMLExport(List<SavedListing> objects)
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

            List<string> fields = new List<string>();

            //Grab the property names that will be exported
            foreach (string p in SelectPropertiesWindow.PropertiesToExport)
                fields.Add(p);

            sb.Append("<!DOCTYPE html>\n<html>\n");

            //CSS Style Stuff
            sb.Append("<head>\n");
            sb.Append("<style type=\"text/css\">\n");

            sb.Append("" +
            "body\n" +
            "{\n" +
            "    line-height: .8em;\n" +
            "}\n" +
            "#hor-zebra\n" +
            "{\n" +
            "    align: center;\n" +
            "    font-family: \"Lucida Sans Unicode\", \"Lucida Grande\", Sans-Serif;\n" +
            "    font-size: 12px;\n" +
            "    margin-top: 50px;\n" +
            "    margin-bottom: 50px;\n" +
            "    margin-left: 100px;\n" +
            "    margin-right: 100px;\n" +
            "    min-width: 800px;\n" +
            "    text-align: left;\n" +
            //Border
            "    border-collapse: collapse;\n" +
            "    border-style: solid;\n" +
	        "    border-width: 1px;\n" +
	        "    border-color:  #e8edff;\n" +
            //
            "}\n" +
            "#hor-zebra thead\n" +
            "{\n" +
            "    font-size: 14px;\n" +
            "    font-weight: bold;\n" +
            "    padding: 10px 8px;\n" +
            "    color: #039;\n" +
            "}\n" +
            "#hor-zebra th\n" +
            "{\n" +
            "    font-size: 14px;\n" +
            "    font-weight: normal;\n" +
            "    padding: 10px 8px;\n" +
            "    color: #039;\n" +
            "}\n" +
            "#hor-zebra td\n" +
            "{\n" +
            "    padding: 4px;\n" +
            "    color: #669;\n" +
            //Border
            "    border-style: solid;\n" +
            "    border-width: 1px;\n" +
            "    border-color:  #e8edff;\n" +
            //
            "}\n" +
            "#hor-zebra .odd\n" +
            "{\n" +
            "    background: #e8edff; \n" +
            "}\n" +
            ");\n");

            sb.Append("</style>\n");
            sb.Append("</head>\n\n");   

            sb.Append("<body>\n\n");

            if (Objects.Count > 0)
            {
                sb.Append("<table id=\"hor-zebra\" summary=\"Saved Reddit Links\" align=\"center\">\n");

                sb.Append("<thead>\n");
                sb.Append("<tr>\n");

                //Print out the table headers
                foreach (KeyValuePair<string, string> pair in Objects[0].Properties)
                {
                    if (fields.Contains(pair.Key))
                    {
                        if (pair.Key == "over_18")
                            sb.Append("<td>NSFW?</td>\n");
                        else if (pair.Key == "num_comments")
                            sb.Append("<td>comments</td>\n");
                        else if (pair.Key.Contains("created_utc"))
                            sb.Append("<td>created</td>\n");
                        else
                            sb.Append("<td>" + pair.Key).Append("</td>\n");
                    }
                }

                sb.Append("</tr>\n");
                sb.Append("</thead>\n");

                sb.Append("<tbody>\n");
                int rowNum = 0;

                //Add the property values to the table
                foreach (SavedListing listing in Objects)
                {
                    if (rowNum % 2 == 0)
                        sb.Append("<tr>\n");
                    else
                        sb.Append("<tr class=\"odd\">\n");

                    foreach (KeyValuePair<string, string> pair in listing.Properties)
                    {
                        if (fields.Contains(pair.Key))
                        {
                            if (pair.Key.Contains("url"))
                                sb.Append("<td title=\"" + pair.Value + "\"> <a href=\"" + pair.Value + "\">" + AddWordBreaks(Shorten(pair.Value, 40), 10) +  "</a></td>\n");
                            else if (pair.Key.Contains("id"))
                                sb.Append("<td> <a href=\"http://www.reddit.com/comments/" + pair.Value + "\">" + pair.Value + "</a></td>\n");
                            else if (pair.Key.Contains("over_18"))
                                sb.Append("<td>" + (bool.Parse(pair.Value) ? "<font color=\"red\">NSFW</font>" : "No") + "</td>\n");
                            else if (pair.Key.Contains("created_utc"))
                                sb.Append("<td>" + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(pair.Value)).ToShortDateString() + "</td>\n");
                            else
                                sb.Append("<td>" + MakeValueCsvFriendly(pair.Value)).Append("</td>\n");
                        }
                    }
                    sb.Append("</tr>\n\n");

                    sb.Remove(sb.Length - 1, 1).AppendLine();
                    rowNum++;
                }

                sb.Append("</tbody>\n");
                sb.Append("</table>\n");
            }
            else
            {
                sb.Append("\n\nNO OBJECTS TO EXPORT<br>");
            }

            sb.Append("\n\n</body>\n</html>");

            return sb.ToString();
        }

        /// <summary>
        /// This is needed for Chrome and possibly other browsers that don't put 
        /// any word breaks in words that don't have spaces (URLS in this case)
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="spacing">How often you want a word break</param>
        /// <returns></returns>
        public string AddWordBreaks(string input, int spacing)
        {
            string output = "";

            for (int i = 0; i < input.Length; i += spacing)
            {
                if (i == 0)
                    output += input.Substring(0, spacing) + "<wbr></wbr>";
                else if (i + spacing >= input.Length)
                    output += input.Substring(i);
                else
                    output += input.Substring(i, spacing) + "<wbr></wbr>";                
            }

            return output;
        }

        /// <summary>
        /// Shortens the given string according to the limit
        /// </summary>
        /// <param name="input">String to shorten</param>
        /// <param name="limit">Length limit</param>
        /// <returns>Shortenes string with "..." appended at the end</returns>
        public string Shorten(string input, int limit)
        {
            string output = input;

            //Subtracting 3 from the limit just so the ellipsis make it add up to the limit
            if (output.Length > limit - 3)
                output = output.Substring(0, limit - 3) + "...";

            return output;
        }

        //Export to a file.
        public void ExportToFile(string path)
        {
            File.WriteAllText(path, Export());
        }

        //Export as binary data.
        public byte[] ExportToBytes()
        {
            return Encoding.UTF8.GetBytes(Export());
        }

        //Make it look pretty
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
