using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers;
using GCH.Core.TelegramLogic.Interfaces;
using LanguageExt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.TelegramTriggerFunction
{
    public class CreateOrConcatCreatingVoiceFunction
    {
        private readonly IWrappedTelegramClient _client;
        private readonly IVoiceLabelSource _source;
        private BlobContainerClient _blobVoicesContainerClient;
        private BlobContainerClient _blobCreatedContainerClient;
        
        public CreateOrConcatCreatingVoiceFunction(IWrappedTelegramClient client, IVoiceLabelSource source)
        {
            _client = client;
            _source = source;
        }

        [FunctionName("CreateOrConcatCreatingVoiceFunction")]
        public async Task Run(
            [QueueTrigger(queueName: "voices", Connection = "BlobConnectionString")] string rawMsg,
            [Blob(
            blobPath: "voices",
            access: FileAccess.ReadWrite,
            Connection = "BlobConnectionString")] BlobContainerClient blobVoicesContainerClient,
            [Blob(
            blobPath: "uservoices",
            access: FileAccess.ReadWrite,
            Connection = "BlobConnectionString")] BlobContainerClient blobCreatedContainerClient
            )
        {
            _blobVoicesContainerClient = blobVoicesContainerClient;
            _blobCreatedContainerClient = blobCreatedContainerClient;

            await (from msg in Parse(rawMsg)
                from voice in GetVoiceToAdd(msg)
                from size in AddVoice(msg, voice)
                select (msg, size)).IfSucc(async (msgAndSize) =>
                {
                    var (msg, size) = msgAndSize;
                    var paged = await _source.LoadAsync(
                        int.Parse(msg.ChatState.TryGetValue("offset", out var offset) ? offset : "0"));
                    var buttons = ChatVoiceHelpers.AddFooterButtons(paged, msg.FileName);
                    var markup = new InlineKeyboardMarkup(buttons);
                    await _client.Client.SendTextMessageAsync(
                        msg.ChatId,
                        $"{size} voices added. You can add max 10 parts.",
                        replyMarkup: markup);
                });

        }

        private TryAsync<QueueMessageToAddVoice> Parse(string rawMsg)
        {
            return new TryAsync<QueueMessageToAddVoice>(
                async () => JsonConvert.DeserializeObject<QueueMessageToAddVoice>(rawMsg));
        }

        private TryAsync<Stream> GetVoiceToAdd(QueueMessageToAddVoice msg)
        {
            return new TryAsync<Stream>(async () =>
            {
                var stream = new MemoryStream();
                if (!string.IsNullOrEmpty(msg.VoiceLabelName))
                {
                    var blob = _blobVoicesContainerClient
                        .GetBlobClient(msg.VoiceLabelName);
                    await blob.DownloadToAsync(stream);
                }
                else if (!string.IsNullOrEmpty(msg.ChatVoiceTelegramId))
                {
                    var t = await _client.Client
                        .GetFileAsync(msg.ChatVoiceTelegramId);
                    if (t.FileSize > 5 * 1 << 20)
                    {
                        throw new Exception("too big");
                    }
                    await _client.Client
                        .DownloadFileAsync(t.FilePath, stream);
                }
                stream.Position = 0;
                return stream;
            });
        }

        private TryAsync<int> AddVoice(QueueMessageToAddVoice msg, Stream voiceToAdd)
        {
            return new TryAsync<int>(async () =>
            {
                var blobInstance = _blobCreatedContainerClient.GetAppendBlobClient(msg.FileName + ".ogg");
                var exists = await blobInstance.ExistsAsync();
                if (exists)
                {
                    await blobInstance.AppendBlockAsync(voiceToAdd);
                }
                else
                {
                    await blobInstance.CreateAsync();
                    await blobInstance.AppendBlockAsync(voiceToAdd);
                }
                voiceToAdd.Dispose();
                var props = await blobInstance.GetPropertiesAsync();
                return props.Value.BlobCommittedBlockCount;
            });
        }

    }
}
