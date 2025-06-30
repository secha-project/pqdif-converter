using Gemstone.PQDIF;
using Gemstone.PQDIF.Logical;

namespace PQDIFConverter
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            string filePath = args[0];
            var outputDirectory = args.Length > 1 ? args[1] : Constants.DefaultOutputDirectory;

            var basicData = await PqdifMetadata.GetBasicData(filePath);
            Console.WriteLine($"Found {basicData.Count} data source{(basicData.Count == 1 ? "" : "s")} from file '{filePath}'");
            foreach (var (sourceName, data) in basicData)
            {
                Console.WriteLine(
                    $"- '{sourceName}' with {data.RecordCount} record{(data.RecordCount == 1 ? "" : "s")} " +
                    $"and a start time of {data.StartTime:yyyy-MM-ddTHH:mm:ss.ffffffzzz}"
                );
            }
            Console.WriteLine("");

            var recordsWithIndex = await PqdifMetadata.GetObservationRecords(filePath);

            foreach (var (index, record) in recordsWithIndex)
            {
                Console.Write($"Handling record {index}/{recordsWithIndex.Count()} ");
                var (dataSourceName, data) = PqdifMetadata.GetObservationRecordData(record);
                await CsvWriter.WriteDataToCsv(outputDirectory, dataSourceName, index, data);
            }
        }
    }
}
