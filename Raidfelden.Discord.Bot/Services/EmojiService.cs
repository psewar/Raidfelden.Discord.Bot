using Discord;
using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IEmojiService
    {
        Emoji Get(int id);
    }

    public class EmojiService : IEmojiService
    {
        public EmojiService()
        {
            string[] keycaps = new[] {
                    "0⃣",
                    "1⃣",
                    "2⃣",
                    "3⃣",
                    "4⃣",
                    "5⃣",
                    "6⃣",
                    "7⃣",
                    "8⃣",
                    "9⃣",
                    "🇦",
                    "🇧",
                    "🇨",
                    "🇩",
                    "🇪",
                    "🇫",
                    "🇬",
                    "🇭",
                    "🇮",
                    "🇯",
                    "🇰",
                    "🇱",
                    "🇲",
                    "🇳",
                    "🇴",
                    "🇵",
                    "🇶",
                    "🇷",
                    "🇸",
                    "🇹",
                    "🇺",
                    "🇻",
                    "🇼",
                    "🇽",
                    "🇾",
                    "🇿"
            };
            var keycapEmojis = new List<Emoji>();
            for (int i = 0; i < keycaps.Length; i++)
            {
                keycapEmojis.Add(new Emoji(keycaps[i]));
            }
            KeycapEmojis = keycapEmojis.ToArray();
        }

        protected Emoji[] KeycapEmojis { get; private set; }

        public Emoji Get(int id)
        {
            if (id < 0 || id >= KeycapEmojis.Length)
            {
                throw new ArgumentException("Invalid keycap emoji id");
            }

            return KeycapEmojis[id];
        }
    }
}
