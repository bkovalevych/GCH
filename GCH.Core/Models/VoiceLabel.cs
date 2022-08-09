namespace GCH.Core.Models
{
    public class VoiceLabel
    {
        public int Id { get; set; }
        public string RowKey { get; set; }

        public string PartitionKey { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Blob { get; set; }
    }
}
