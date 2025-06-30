using Gemstone.PQDIF;
using Gemstone.PQDIF.Logical;
using LanguageExt;
using static LanguageExt.Prelude;

namespace PQDIFConverter
{
    public class DataSourceMetadata
    {
        public Option<string> Name { get; }
        public Option<string> Owner { get; }
        public Option<string> SourceType { get; }
        public Option<string> SourceStandardName { get; }
        public Option<string> VendorName { get; }
        public Option<string> VendorStandardName { get; }

        public DataSourceMetadata(SeriesInstance seriesInstance)
        {
            try
            {
                Name = Some(seriesInstance.Channel.ObservationRecord.DataSource.DataSourceName);
            }
            catch (InvalidDataException)
            {
                Name = None;
            }

            try
            {
                Owner = Some(seriesInstance.Channel.ObservationRecord.DataSource.DataSourceOwner);
            }
            catch (InvalidDataException)
            {
                Owner = None;
            }

            try
            {
                Identifier? dataSourceTypeTag =
                    DataSourceType.GetInfo(seriesInstance.Channel.ObservationRecord.DataSource.DataSourceTypeID)
                        ?? throw new InvalidDataException("DataSourceTypeID is not valid.");
                SourceType = Some(dataSourceTypeTag.Name);
                SourceStandardName = Some(dataSourceTypeTag.StandardName);
            }
            catch (InvalidDataException)
            {
                SourceType = None;
                SourceStandardName = None;
            }

            try
            {
                Identifier? vendorTag =
                    Vendor.GetInfo(seriesInstance.Channel.ObservationRecord.DataSource.VendorID)
                        ?? throw new InvalidDataException("VendorID is not valid.");
                VendorName = Some(vendorTag.Name);
                VendorStandardName = Some(vendorTag.StandardName);
            }
            catch (InvalidDataException)
            {
                VendorName = None;
                VendorStandardName = None;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DataSourceMetadata other)
                return false;

            return Name == other.Name &&
                   Owner == other.Owner &&
                   SourceStandardName == other.SourceStandardName &&
                   VendorStandardName == other.VendorStandardName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Owner, SourceStandardName, VendorStandardName);
        }

        public string GetName() => Name.IfNone(string.Empty);
        public string GetOwner() => Owner.IfNone(string.Empty);
        public string GetSourceType() => SourceType.IfNone(string.Empty);
        public string GetSourceStandardName() => SourceStandardName.IfNone(string.Empty);
        public string GetVendorName() => VendorName.IfNone(string.Empty);
        public string GetVendorStandardName() => VendorStandardName.IfNone(string.Empty);
    }
}
