using Discord;
using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IEmojiService
    {
        Emoji Get(int numer);
    }

    public class EmojiService : IEmojiService
    {
        public EmojiService()
        {
            string[] numbers = new[] { "0⃣", "1⃣", "2⃣", "3⃣", "4⃣", "5⃣", "6⃣", "7⃣", "8⃣", "9⃣" };
            var numberEmojis = new List<Emoji>();
            for (int i = 0; i < numbers.Length; i++)
            {
                numberEmojis.Add(new Emoji(numbers[i]));
            }
            NumberEmojis = numberEmojis.ToArray();
        }

        protected Emoji[] NumberEmojis { get; private set; }

        public Emoji Get(int number)
        {
            if (number < 0 || number > 9)
            {
                throw new ArgumentException("The number has to be between 0 and 9");
            }

            return NumberEmojis[number];
        }
    }
}
