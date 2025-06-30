using Gemstone.PQDIF;
using Gemstone.PQDIF.Logical;
using LanguageExt;
using static LanguageExt.Prelude;

namespace PQDIFConverter
{
    public class ChannelMetadata
    {
        public Option<string> ChannelName { get; }
        public Option<string> ChannelPhase { get; }
        public Option<DateTime> StartTime { get; }
        public Option<string> QuantityMeasured { get; }
        public Option<string> QuantityStandardName { get; }
        public Option<Guid> QuantityTypeID { get; }

        public ChannelMetadata(SeriesInstance seriesInstance)
        {
            if (seriesInstance.Channel.Definition.ChannelName is not null)
                ChannelName = Some(seriesInstance.Channel.Definition.ChannelName);
            else
                ChannelName = None;

            try
            {
                QuantityMeasured = Some(seriesInstance.Channel.Definition.QuantityMeasured.ToString());
            }
            catch (InvalidDataException)
            {
                QuantityMeasured = None;
            }

            try
            {
                StartTime = Some(seriesInstance.Channel.ObservationRecord.StartTime);
            }
            catch (InvalidDataException)
            {
                StartTime = None;
            }

            try
            {
                ChannelPhase = Some(seriesInstance.Channel.Definition.Phase.ToString());
            }
            catch (InvalidDataException)
            {
                ChannelPhase = None;
            }

            try
            {
                Identifier? quantityType = QuantityType.GetInfo(seriesInstance.Channel.Definition.QuantityTypeID)
                    ?? throw new InvalidDataException("QuantityType is not valid.");
                QuantityStandardName = Some(quantityType.StandardName);
            }
            catch (InvalidDataException)
            {
                QuantityStandardName = None;
            }

            try
            {
                QuantityTypeID = Some(seriesInstance.Channel.Definition.QuantityTypeID);
            }
            catch (InvalidDataException)
            {
                QuantityTypeID = None;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ChannelMetadata other)
                return false;

            return ChannelName == other.ChannelName &&
                   ChannelPhase == other.ChannelPhase &&
                   StartTime == other.StartTime &&
                   QuantityMeasured == other.QuantityMeasured &&
                   QuantityStandardName == other.QuantityStandardName &&
                   QuantityTypeID == other.QuantityTypeID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ChannelName, ChannelPhase, StartTime, QuantityMeasured, QuantityStandardName, QuantityTypeID);
        }

        public string GetQuantityTypeString()
        {
            var guidToStringMap = new Dictionary<Guid, string>
            {
                { QuantityType.Phasor, "Phasor" },
                { QuantityType.WaveForm, "WaveForm" },
                { QuantityType.ValueLog, "ValueLog" },
                { QuantityType.Response, "Response" },
                { QuantityType.Flash, "Flash" },
                { QuantityType.Histogram, "Histogram" },
                { QuantityType.Histogram3D, "Histogram3D" },
                { QuantityType.CPF, "CPF" },
                { QuantityType.XY, "XY" },
                { QuantityType.MagDur, "MagDur" },
                { QuantityType.XYZ, "XYZ" },
                { QuantityType.MagDurTime, "MagDurTime" },
                { QuantityType.MagDurCount, "MagDurCount" },
            };
            return guidToStringMap.GetValueOrDefault(GetQuantityTypeID(), "Default");
        }

        public string GetChannelName() => ChannelName.IfNone(string.Empty);
        public string GetChannelPhase() => ChannelPhase.IfNone(string.Empty);
        public DateTime GetStartTime() => StartTime.IfNone(DateTime.MinValue);
        public string GetQuantityMeasured() => QuantityMeasured.IfNone(string.Empty);
        public string GetQuantityStandardName() => QuantityStandardName.IfNone(string.Empty);
        public Guid GetQuantityTypeID() => QuantityTypeID.IfNone(Guid.Empty);
    }
}
