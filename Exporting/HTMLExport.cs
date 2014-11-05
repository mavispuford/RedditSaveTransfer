using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSaveTransfer.Exporting
{
    /// <summary>
    /// Exports to an HTML file
    /// **Thanks to Chris and Coder Net from stackoverflow.com for example code
    /// http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
    /// </summary>
    public class HtmlExport : ExportMethod
    {
        private readonly List<SavedListing> _objects;

        public HtmlExport(List<SavedListing> objects)
        {
            _objects = objects;
        }

        protected override string Export()
        {
            var sb = new StringBuilder();

            var fields = SelectPropertiesWindow.PropertiesToExport;

            sb.Append("<!DOCTYPE html>\n<html>\n");

            // CSS Styling Stuff
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
                      "    min-width: 300px;\n" +
                      "    text-align: left;\n" +
                      // Border
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
                      // Border
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

            if (_objects.Count > 0)
            {
                sb.Append("<table id=\"hor-zebra\" summary=\"Saved Reddit Links\" align=\"center\">\n");

                sb.Append("<thead>\n");
                sb.Append("<tr>\n");

                // Print out the table headers
                foreach (var pair in _objects[0].Properties)
                {
                    if (!fields.Contains(pair.Key)) continue;

                    switch (pair.Key)
                    {
                        case "over_18":
                            sb.Append("<td>NSFW?</td>\n");
                            break;
                        case "num_comments":
                            sb.Append("<td>comments</td>\n");
                            break;
                        case "created_utc":
                            sb.Append("<td>created</td>\n");
                            break;
                        default:
                            sb.Append("<td>" + pair.Key).Append("</td>\n");
                            break;
                    }
                }

                sb.Append("</tr>\n");
                sb.Append("</thead>\n");

                sb.Append("<tbody>\n");
                var rowNum = 0;

                // Add the property values to the table
                foreach (var item in _objects)
                {
                    sb.Append(rowNum%2 == 0 ? "<tr>\n" : "<tr class=\"odd\">\n");

                    var commentsUrl = Common.BaseUrl + "/comments/";
                    
                    foreach (var pair in item.Properties.Where(pair => pair.Key.Contains("id")))
                    {
                        commentsUrl += pair.Value;
                        break;
                    }

                    foreach (var pair in item.Properties)
                    {
                        if (!fields.Contains(pair.Key)) continue;

                        if (pair.Key.Contains("url"))
                            sb.Append("<td title=\"" + pair.Value + "\"> <a href=\"" + pair.Value + "\">" +
                                      AddWordBreaks(Shorten(pair.Value, 40), 10) + "</a></td>\n");
                        else if (pair.Key.Contains("id"))
                            sb.Append("<td> <a href=\"" + commentsUrl + "\">" + pair.Value + "</a></td>\n");
                        else if (pair.Key.Contains("over_18"))
                            sb.Append("<td>" + (bool.Parse(pair.Value) ? "<font color=\"red\">NSFW</font>" : "No") +
                                      "</td>\n");
                        else if (pair.Key.Contains("created_utc"))
                            sb.Append("<td>" +
                                      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(
                                          double.Parse(pair.Value)).ToShortDateString() + "</td>\n");
                        else if (pair.Key.Contains("title"))
                            sb.Append("<td> <a href=\"" + commentsUrl + "\">" + CleanUpValue(pair.Value))
                                .Append("</a></td>\n");
                        else
                            sb.Append("<td>" + CleanUpValue(pair.Value)).Append("</td>\n");
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
        private static string AddWordBreaks(string input, int spacing)
        {
            var output = "";

            for (var i = 0; i < input.Length; i += spacing)
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
    }
}