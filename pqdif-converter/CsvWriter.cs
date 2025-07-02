namespace PQDIFConverter
{
    public static class CsvWriter
    {
        public static string GetCsvFileName(
            string dataSourceName,
            DateTime startTime,
            int observationIndex,
            string quantityType,
            int quantityTypeCount
        )
        {

            string safeDataSourceName = new([
                .. dataSourceName.Replace(" ", "_").Where(c => !Path.GetInvalidFileNameChars().Contains(c))
            ]);
            string fileNameBase = $"{safeDataSourceName}_{startTime:yyyy-MM-ddTHH-mm-ss}_{observationIndex:D4}";
            if (quantityTypeCount > 1)
            {
                fileNameBase += $"_{quantityType}";
            }
            return $"{fileNameBase}.csv";
        }

        private static readonly string[] firstColumn = ["Timestamp"];

        public static async Task WriteDataToCsv(
            string outputDirectory,
            string dataSourceName,
            int observationIndex,
            List<SeriesValue> data
        )
        {
            Dictionary<string, List<string>> names = data
                .GroupBy(x => x.QuantityType)
                .ToDictionary(x => x.Key, x => x.Select(x => x.Name).Distinct().ToList());
            DateTime startTime = data.Min(x => x.Timestamp);

            Directory.CreateDirectory(outputDirectory);

            int quantityTypeCounter = 1;
            foreach (var (quantityType, measurementNames) in names)
            {
                measurementNames.Sort();

                string fileName = GetCsvFileName(dataSourceName, startTime, observationIndex, quantityType, names.Count);
                string fullPath = Path.Combine(outputDirectory, fileName);
                using var writer = new StreamWriter(fullPath);

                string header = string.Join(",", firstColumn.Concat(measurementNames));
                await writer.WriteLineAsync(header);

                List<string> rowData = [
                    .. data
                        .Where(x => x.QuantityType == quantityType)
                        .GroupBy(x => x.Timestamp)
                        .Select(x => (x.Key, x.ToDictionary(sv => sv.Name, sv => sv.Value)))
                        .OrderBy(x => x.Key)
                        .Select(
                            x =>
                                new [] { $"{x.Key:yyyy-MM-ddTHH:mm:ss.ffffffzzz}" }.ToList()
                                .Concat(
                                    [.. measurementNames.Select(
                                        name => x.Item2.TryGetValue(name, out double value)
                                            ? value.ToString()
                                            : string.Empty
                                    )]
                                ).ToList()
                        )
                        .Select(x => string.Join(",", x))
                ];
                await writer.WriteLineAsync(string.Join("\n", rowData));

                if (quantityTypeCounter > 1)
                {
                    Console.Write("                      ");
                }
                Console.WriteLine($"=> Wrote {rowData.Count + 1} rows and {measurementNames.Count + 1} columns to '{fullPath}'");

                quantityTypeCounter++;
            }
        }
    }
}
