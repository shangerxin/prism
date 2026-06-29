using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace prism.infra.Helper
{
    public static class Converter
    {
        //https://joshclose.github.io/CsvHelper/
        public static string JsonToCsv(string json)
        {
            if(string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }

            StringWriter csvString = new StringWriter();
            using (CsvWriter csv = new CsvWriter(csvString, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                using (var dt = JsonConvert.DeserializeObject<DataTable>(json))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
            return csvString.ToString();
        }

        public static string CsvToJson(string csv, bool isWriteIndent = false, bool isCamelCase = false)
        {
            if(string.IsNullOrEmpty(csv))
            {
                return string.Empty;
            }

            var dynamicRecords = new List<dynamic>();
            using (var reader = new StringReader(csv))
            using (var csvReader = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {

                csvReader.Read();
                csvReader.ReadHeader();
                var headers = csvReader.HeaderRecord;
                while (csvReader.Read())
                {
                    // ExpandoObject acts as our dynamic object container
                    var record = new ExpandoObject() as IDictionary<string, object>;

                    foreach (var header in headers)
                    {
                        string rawValue = csvReader.GetField(header);
                        record[header] = InferAndParseType(rawValue);
                    }

                    dynamicRecords.Add(record);
                }

                var option = new JsonSerializerOptions
                {
                    WriteIndented = isWriteIndent,
                    PropertyNamingPolicy = isCamelCase ? JsonNamingPolicy.CamelCase : null,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
                };

                return System.Text.Json.JsonSerializer.Serialize(dynamicRecords, option);
            }
        }

        private static object InferAndParseType(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // 1. Try Integer
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intResult))
            {
                return intResult;
            }

            // 2. Try Double/Float (Use decimal if financial precision is needed)
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
            {
                return doubleResult;
            }

            // 3. Try Boolean
            if (bool.TryParse(input, out bool boolResult))
            {
                return boolResult;
            }

            // 4. Fallback to String
            return input;
        }
    }
}
