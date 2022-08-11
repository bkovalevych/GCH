namespace GCH.Core.WordProcessing.Models
{
    public class TextUnit : IUnit
    {
        public string Text { get; set; }

        public static Func<TextUnit, Task<Stream>> ComposeFunction { get; set; }

        public Task<Stream> Compose()
        {
            return ComposeFunction(this);
        }
    }
}
