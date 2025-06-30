// using Gemstone.PQDIF;
using Gemstone.PQDIF.Logical;

namespace PQDIFConverter
{
    public class BasicSourceData
    {
        public int RecordCount{ get; set; }
        public DateTime StartTime{ get; set; }
    }

    static class PqdifMetadata
    {
        public static async Task<Dictionary<string, BasicSourceData>> GetBasicData(string fileName)
        {
            Dictionary<string, BasicSourceData> basicData = [];
            await using LogicalParser parser = new(fileName);
            try
            {
                await parser.OpenAsync();
                while (await parser.HasNextObservationRecordAsync())
                {
                    var record = await parser.NextObservationRecordAsync();
                    var sourceName = record.DataSource.DataSourceName;
                    if (!basicData.TryGetValue(sourceName, out BasicSourceData? data))
                    {
                        basicData[sourceName] = new BasicSourceData
                        {
                            RecordCount = 1,
                            StartTime = record.StartTime
                        };
                    }
                    else
                    {
                        data.RecordCount++;
                        if (record.StartTime < data.StartTime)
                        {
                            data.StartTime = record.StartTime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error processing file {fileName}: {ex.Message}");
                throw;
            }
            finally
            {
                await parser.CloseAsync();
            }

            return basicData;
        }

        public static (string, List<SeriesValue>) GetObservationRecordData(ObservationRecord record)
        {
            var dataSourceName = record.DataSource.DataSourceName;

            var data = record
                .ChannelInstances
                .SelectMany(channelInstance => channelInstance.SeriesInstances)
                .GroupBy(seriesInstance => new ChannelMetadata(seriesInstance))
                .SelectMany(
                    channelGroup => GetSeriesValues(
                        channelGroup.Key,
                        [.. channelGroup.Select(seriesInstance => new MeasurementMetadata(seriesInstance))]
                    )
                )
                .ToList();

            return (dataSourceName, data);
        }

        public static async Task<IEnumerable<(int, ObservationRecord)>> GetObservationRecords(string fileName)
        {
            await using LogicalParser parser = new(fileName);
            try
            {
                await parser.OpenAsync();

                var observationRecords = new List<(int, ObservationRecord)>();
                int counter = 1;
                while (await parser.HasNextObservationRecordAsync())
                {
                    observationRecords.Add((counter, await parser.NextObservationRecordAsync()));
                    counter++;
                }
                return observationRecords;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error processing file {fileName}: {ex.Message}");
                throw;
            }
            finally
            {
                await parser.CloseAsync();
            }
        }

        public static List<SeriesValue> GetSeriesValues(ChannelMetadata channel, List<MeasurementMetadata> measurements)
        {
            var timestampSeriesList = measurements
                .Where(measurement => measurement.GetValueTypeID() == SeriesValueType.Time)
                .Select(measurement => measurement.GetDateTimeValues(channel.GetStartTime()))
                .ToList()?? throw new InvalidDataException("No time series found in measurements.");
            if (timestampSeriesList.Count == 0)
            {
                throw new InvalidDataException("Time series is empty.");
            }

            foreach (var measurement in measurements)
            {
                measurement.SetHeader(channel, null);
            }
            Dictionary<string, int> headerCounts = measurements
                .Where(measurement => measurement.GetValueTypeID() != SeriesValueType.Time)
                .Select(measurement => measurement.GetHeader())
                .GroupBy(header => header)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            DateTime startTime = timestampSeriesList
                .Select(ts => ts.FirstOrDefault())
                .Min();
            DateTime endTime = timestampSeriesList
                .Select(ts => ts.LastOrDefault())
                .Max();

            Dictionary<string, int> headerIndex = [];
            var seriesValues = new List<SeriesValue>();
            foreach (var measurement in measurements)
            {
                if (measurement.GetValueTypeID() == SeriesValueType.Time)
                {
                    continue;
                }

                var seriesHeader = measurement.GetHeader();
                if (headerCounts[seriesHeader] > 1)
                {
                    if (!headerIndex.TryGetValue(seriesHeader, out int index))
                    {
                        headerIndex[seriesHeader] = 0;
                    }
                    else
                    {
                        headerIndex[seriesHeader]++;
                    }
                    measurement.SetHeader(channel, headerIndex[seriesHeader]);
                }

                var values = measurement.GetDoubleValues();
                var currentTimestampSeries = timestampSeriesList
                    .FirstOrDefault(ts => ts.Count == values.Count);
                if (currentTimestampSeries == null)
                {
                    Console.Error.WriteLine($"Warning: Measurement '{measurement.GetHeader()}' has {values.Count} values, but no time series has that many timestamps.");
                    throw new InvalidDataException("Measurement values count does not match any timestamp series count.");
                }

                foreach (var (value, timestamp) in values.Zip(currentTimestampSeries, (v, t) => (v, t)))
                {
                    seriesValues.Add(
                        new SeriesValue(
                            channel.GetQuantityTypeString(),
                            measurement.GetHeader(),
                            timestamp,
                            value
                        )
                    );
                }
            }

            return seriesValues;
        }
    }
}
