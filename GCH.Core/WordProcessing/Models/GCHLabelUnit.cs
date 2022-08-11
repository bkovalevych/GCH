namespace GCH.Core.WordProcessing.Models
{
    public class GCHLabelUnit : IUnit
    {
        public static Func<GCHLabelUnit, Task<Stream>> ComposeFunction { get; set; }

        public string ShortName { get; set; }

        public Task<Stream> Compose()
        {
            return ComposeFunction(this);
        }
    }
}
