using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using System.Text;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Returns the namespace path to the provided object, including the object itself.
        /// </summary>
        public static string GetPath(this object obj) => obj.GetType().Namespace.Replace('.', '/') + "/" + obj.GetType().Name;

        /// <summary>
        ///     Returns the provided number with the correct ordinal suffix.<br/>
        ///     For example, 3 would return 3rd.
        /// </summary>
        public static string AddOrdinalSuffix(int positiveNumber)
        {
            if (positiveNumber <= 0)
                return positiveNumber.ToString();

            return (positiveNumber % 100) switch
            {
                11 or 12 or 13 => positiveNumber + "th",
                _ => (positiveNumber % 10) switch
                {
                    1 => positiveNumber + "st",
                    2 => positiveNumber + "nd",
                    3 => positiveNumber + "rd",
                    _ => positiveNumber + "th",
                },
            };
        }

        /// <summary>
        ///     Displays arbitrary text in the game chat with a desired color. This method expects to be called server-side in multiplayer, with the message display packet being sent to all clients from there.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="color">The color of the text.</param>
        public static void BroadcastText(string text, Color? color = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(text, color ?? Color.White);
            else if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color ?? Color.White);
        }

        /// <summary>
        ///     Displays the localized text gotten from the provided key in the chat, accounting for multiplayer.
        /// </summary>
        public static void BroadcastLocalizedText(string key, Color? textColor = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
            else if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.MultiplayerClient)
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), textColor ?? Color.White);
        }

        /// <summary>
        ///     Colors a message with the provided color using chat tags.
        /// </summary>
        public static string ColorMessage(string message, Color color)
        {
            StringBuilder builder;
            if (!message.Contains('\n'))
            {
                builder = new(message.Length + 12);
                builder.Append("[c/").Append(color.Hex3()).Append(':').Append(message).Append(']');
            }
            else
            {
                builder = new();
                foreach (string newlineSlice in message.Split('\n'))
                    builder.Append("[c/").Append(color.Hex3()).Append(':').Append(newlineSlice).Append(']').Append('\n');
            }
            return builder.ToString();
        }
    }
}
