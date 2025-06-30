using Gemstone.PQDIF;
using Gemstone.PQDIF.Logical;
using LanguageExt;
using static LanguageExt.Prelude;

namespace PQDIFConverter
{
    public class MeasurementMetadata
    {
        public Option<string> ValueTypeName { get; }
        public Option<string> ValueTypeStandardName { get; }
        public Option<Guid> ValueTypeID { get; }
        public Option<QuantityUnits> Unit { get; }
        public int Count { get; }
        public int OriginalCount { get; }
        public List<object> Values { get; }
        private Option<string> Header { get; set; }
        public Option<Guid> CharacteristicID { get; }
        public double NominalQuantity { get; }
        public string SourceName { get; }

        public MeasurementMetadata(SeriesInstance seriesInstance)
        {
            try
            {
                Identifier? valueType = SeriesValueType.GetInfo(seriesInstance.Definition.ValueTypeID)
                    ?? throw new InvalidDataException("SeriesValueType is not valid.");
                ValueTypeName = Some(valueType.Name);
                ValueTypeStandardName = Some(valueType.StandardName);
                ValueTypeID = Some(seriesInstance.Definition.ValueTypeID);
            }
            catch (InvalidDataException)
            {
                ValueTypeName = None;
                ValueTypeStandardName = None;
                ValueTypeID = None;
            }

            try
            {
                Unit = Some(seriesInstance.Definition.QuantityUnits);
            }
            catch (InvalidDataException)
            {
                Unit = None;
            }
            Count = seriesInstance.SeriesValues.Size;
            OriginalCount = seriesInstance.OriginalValues.Count;
            Values = [.. seriesInstance.OriginalValues];
            Header = None;
            try
            {
                CharacteristicID = Some(seriesInstance.Definition.QuantityCharacteristicID);
            }
            catch (InvalidDataException)
            {
                CharacteristicID = None;
            }
            NominalQuantity = seriesInstance.Definition.SeriesNominalQuantity;
            SourceName = seriesInstance.Channel.Definition.DataSource.DataSourceName;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not MeasurementMetadata other)
                return false;

            return ValueTypeStandardName == other.ValueTypeStandardName &&
                   Count == other.Count &&
                   OriginalCount == other.OriginalCount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ValueTypeStandardName, Count);
        }

        public void SetHeader(ChannelMetadata channelMetadata, int? index)
        {
            if (GetValueTypeID() == SeriesValueType.Time)
            {
                Header = Some(Constants.Timestamp);
                return;
            }

            var guidToStringMap = new Dictionary<Guid, string>
            {
                { SeriesValueType.Avg, Constants.Avg },
                { SeriesValueType.Min, Constants.Min },
                { SeriesValueType.Max, Constants.Max },
                { SeriesValueType.Val, string.Empty },
            };

            var unitToSIMap = new Dictionary<QuantityUnits, string>
            {
                { QuantityUnits.Seconds, Constants.SI_Seconds },
                { QuantityUnits.Volts, Constants.SI_Volts },
                { QuantityUnits.Amps, Constants.SI_Amps },
                { QuantityUnits.VoltAmps, Constants.SI_VoltAmps },
                { QuantityUnits.Watts, Constants.SI_Watts },
                { QuantityUnits.Vars, Constants.SI_Vars },
                { QuantityUnits.Hertz, Constants.SI_Hertz },
                { QuantityUnits.Percent, Constants.SI_Percent },
                { QuantityUnits.VarHours, Constants.SI_VarHours },
                { QuantityUnits.WattHours, Constants.SI_WattHours },
                { QuantityUnits.VoltAmpHours, Constants.SI_VoltAmpHours },
                { QuantityUnits.Degrees, Constants.SI_Degrees },
            };

            string channelName = channelMetadata.GetChannelName().Replace(" ", "_");
            string quantity = channelMetadata.GetQuantityMeasured();
            string phase = channelMetadata.GetChannelPhase();
            string type = guidToStringMap.GetValueOrDefault(GetValueTypeID(), GetValueTypeName());
            string unit = unitToSIMap.GetValueOrDefault(GetUnit(), GetUnit().ToString());

            string headerString = channelName;
            if (quantity != string.Empty && quantity != "None")
            {
                headerString += $"_{quantity}";
            }
            if (phase != string.Empty && phase != "None")
            {
                headerString += $"_{phase}";
            }
            if (index is not null)
            {
                headerString += $"_{index:D2}";
            }
            if (type != string.Empty)
            {
                headerString += $"_{type}";
            }
            if (unit != string.Empty && unit != "None")
            {
                headerString += $"_({unit})";
            }
            Header = Some(headerString);
        }

        public string GetValueTypeName() => ValueTypeName.IfNone(string.Empty);
        public string GetValueTypeStandardName() => ValueTypeStandardName.IfNone(string.Empty);
        public QuantityUnits GetUnit() => Unit.IfNone(QuantityUnits.None);
        public string GetHeader() => Header.IfNone(string.Empty);
        public Guid GetValueTypeID() => ValueTypeID.IfNone(Guid.Empty);
        public Guid GetCharacteristicID() => CharacteristicID.IfNone(Guid.Empty);

        public List<double> GetDoubleValues()
        {
            // Assumes that Values are of type double or can be cast to double
            return [.. Values.Select(Convert.ToDouble)];
        }

        public List<DateTime> GetDateTimeValues(DateTime startTime)
        {
            if (Unit == QuantityUnits.Seconds)
            {
                return [.. Values.Select(value => startTime.AddSeconds(Convert.ToDouble(value)))];
            }
            if (Unit == QuantityUnits.Timestamp)
            {
                return [.. Values.Select(value => (DateTime)value)];
            }

            // The values are not timestamps => return empty list
            return [];
        }
    }
}
