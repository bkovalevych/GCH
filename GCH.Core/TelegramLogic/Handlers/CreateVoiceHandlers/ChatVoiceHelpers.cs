using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public static class ChatVoiceHelpers
    {
        public static IEnumerable<IEnumerable<InlineKeyboardButton>> AddFooterButtons(PaginatedList<VoiceLabel> paginatedResult,
            string fileName)
        {
            var buttons = FillContent(paginatedResult);
            var pagingButtons = new List<InlineKeyboardButton>()
                .AddPreviousButton(paginatedResult)
                .AddStateButton(paginatedResult, fileName)
                .AddNextButton(paginatedResult);
            var footerButtons = new List<InlineKeyboardButton>()
                .AddCancelButton()
                .AddLoadVoiceButton(fileName);
            return buttons.Append(pagingButtons)
                .Append(footerButtons);
        }

        private static IEnumerable<InlineKeyboardButton[]> FillContent(PaginatedList<VoiceLabel> paginatedResult)
        {
            return paginatedResult.Items.Select(it => new InlineKeyboardButton[]
            {
                new InlineKeyboardButton(it.Description)
                {
                    CallbackData = Constants.CreateVoiceButtons.ContnetPrefix + it.Blob
                }
            });
        }

        private static IEnumerable<InlineKeyboardButton> AddPreviousButton(
            this IEnumerable<InlineKeyboardButton> buttons,
            PaginatedList<VoiceLabel> paginatedResult)
        {
            var offset = paginatedResult.Offset;
            if (paginatedResult.CanLoadPrevious)
            {
                return buttons.Append(new InlineKeyboardButton(Resources.Resources.LoadPrev)
                {
                    CallbackData = Constants.CreateVoiceButtons.Previous + (offset - Constants.DefaultPageSize)
                });
            }
            return buttons;
        }

        private static IEnumerable<InlineKeyboardButton> AddNextButton(
            this IEnumerable<InlineKeyboardButton> buttons,
            PaginatedList<VoiceLabel> paginatedResult)
        {
            var offset = paginatedResult.Offset;
            var count = paginatedResult.Count;
            if (paginatedResult.CanLoadNext)
            {
                return buttons.Append(new InlineKeyboardButton(Resources.Resources.LoadMore)
                {
                    CallbackData = Constants.CreateVoiceButtons.Next + (offset + count)
                });
            }
            return buttons;
        }

        private static IEnumerable<InlineKeyboardButton> AddCancelButton(
            this IEnumerable<InlineKeyboardButton> buttons)
        {
            return buttons.Append(new InlineKeyboardButton(Resources.Resources.Cancel)
            {
                CallbackData = Constants.CreateVoiceButtons.Cancel
            });
        }

        private static IEnumerable<InlineKeyboardButton> AddStateButton(
            this IEnumerable<InlineKeyboardButton> buttons,
            PaginatedList<VoiceLabel> paginatedResult,
            string fileName)
        {
            var offset = paginatedResult.Offset;
            var count = paginatedResult.Count;

            return buttons.Append(new InlineKeyboardButton($"{offset + 1} -  {offset + count}")
            {
                CallbackData = Constants.CreateVoiceButtons.State + offset + "|" + fileName
            });
        }

        private static IEnumerable<InlineKeyboardButton> AddLoadVoiceButton(
            this IEnumerable<InlineKeyboardButton> buttons,
            string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                return buttons.Append(new InlineKeyboardButton(Resources.Resources.GetCurrentVoice)
                {
                    CallbackData = Constants.CreateVoiceButtons.GetVoice + fileName
                });
            }
            return buttons;
        }

        private static string GetButtonCallBackInfoByPrefix(IEnumerable<IEnumerable<InlineKeyboardButton>> buttons, string prefix)
        {
            return buttons.SelectMany(it => it).FirstOrDefault(it => it.CallbackData.StartsWith(prefix))
                ?.CallbackData[prefix.Length..];
        }

        public static string GetFileName(IEnumerable<IEnumerable<InlineKeyboardButton>> buttons)
        {
            var afterPrefix = GetButtonCallBackInfoByPrefix(buttons, Constants.CreateVoiceButtons.State);
            return afterPrefix.Split('|')[1];
        }
        public static string GetOffset(IEnumerable<IEnumerable<InlineKeyboardButton>> buttons)
        {
            var afterPrefix = GetButtonCallBackInfoByPrefix(buttons, Constants.CreateVoiceButtons.State);
            return afterPrefix.Split('|')[0];
        }

        public static Dictionary<string, string> GetState(IEnumerable<IEnumerable<InlineKeyboardButton>> buttons)
        {
            var afterPrefix = GetButtonCallBackInfoByPrefix(buttons, Constants.CreateVoiceButtons.State);
            var parts = afterPrefix.Split('|');
            return new Dictionary<string, string>()
            {
                ["Offset"] = parts[0],
                ["FileName"] = parts[1]
            };
        }
    }
}
