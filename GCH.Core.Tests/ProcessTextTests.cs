using GCH.Core.WordProcessing.Models;
using GCH.Core.WordProcessing.Requests.ProcessText;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GCH.Core.Tests
{
    public class ProcessTextTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [DatapointSource]
        public (string, List<IUnit>)[] values = new (string, List<IUnit>)[] 
        { 
            new ("", new List<IUnit>()),
            new("some text", new List<IUnit>() { new TextUnit() { Text = "some text"} }),
            new("\"Full\"", new List<IUnit>() { new GCHLabelUnit() { ShortName = "Full" } }),
            new("\"start\" and This", new List<IUnit>() { new GCHLabelUnit() { ShortName = "start" }, new TextUnit() { Text = " and This" } }),
            new("some \"End\"", new List<IUnit>() {new TextUnit() { Text = "some " }, new GCHLabelUnit() { ShortName = "End" } }),
            new("\"One\"\"Two\"", new List<IUnit>() { new GCHLabelUnit() { ShortName = "One" }, new GCHLabelUnit() { ShortName = "Two" } }),
            new("text before \"One\" text after", new List<IUnit>() { new TextUnit() { Text = "text before " }, new GCHLabelUnit() { ShortName = "One" }, new TextUnit() { Text = " text after" } }), 
            new("text before \"One\" text middle \"Two\" text after", new List<IUnit>() { new TextUnit() { Text = "text before " }, new GCHLabelUnit() { ShortName = "One" }, new TextUnit() { Text = " text middle " }, new GCHLabelUnit() { ShortName = "Two" }, new TextUnit() { Text = " text after" } })
        };

        [Theory]
        public async Task Handle_WithValidData((string, List<IUnit>) val)
        {
            // Arrange
            var (text, labels) = val;
            var request = new ProcessTextRequest()
            {
                Text = text
            };
            var handler = new ProcessTextHandler();

            // Act

            var result = await handler.Handle(request, default);
            
            // Assert
            result.Match(it =>
            {
                var expected = JsonConvert.SerializeObject(labels);
                var actual = JsonConvert.SerializeObject(it);
                Assert.AreEqual(expected, actual);
                return true;
            }, err =>
            {
                Assert.True(false);
                return false;
            });
        }
    }
}