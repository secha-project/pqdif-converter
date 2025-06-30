namespace PQDIFConverter
{
    public class SeriesValue(string quantityType, string name, DateTime timestamp, double value)
    {
        // public int ObservationIndex { get; } = observationIndex;
        public string QuantityType { get; } = quantityType;
        public string Name { get; } = name;
        public DateTime Timestamp { get; } = timestamp;
        public double Value { get; } = value;

        public override string ToString()
        {
            return $"(QuantityType: {QuantityType}, Name: {Name}, Timestamp: {Timestamp}, Value: {Value})";
        }
    }
}
