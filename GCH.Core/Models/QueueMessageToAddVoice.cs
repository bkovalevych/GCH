namespace GCH.Core.Models
{
    public class QueueMessageToAddVoice
    {
        public long ChatId { get; set; }

        public string FileName { get; set; }

        public string ChatVoiceTelegramId { get; set; }

        public string VoiceLabelName { get; set; }

        public TimeSpan Duration { get; set; }

        public Dictionary<string, string> ChatState { get; set; }
    }
}
