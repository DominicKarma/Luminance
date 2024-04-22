using System;
using System.Collections.Generic;
using System.Reflection;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.GameContent.UI.States;
using System.Linq;
using Terraria.UI;
using Terraria.Localization;

namespace Luminance.Core.MenuInfoUI
{
    internal class InternalInfoUIManager : ModSystem, IExistingDetourProvider
    {
        #region Fields
        private static List<InfoUIManager> infoManagers;

        private static IEnumerable<PlayerInfoIcon> playerInfoIcons;

        private static IEnumerable<WorldInfoIcon> worldInfoIcons;

        private static FieldInfo playerInfoDataField;

        private static FieldInfo playerLeftTextField;

        private static FieldInfo playerRightTextField;

        private static FieldInfo worldLeftTextField;

        private static FieldInfo worldRightTextField;

        private static string currentMouseText;

        private static List<string> ExtraTextToAppend;

        private static bool textShouldShow;
        #endregion

        #region Loading
        internal static void RegisterManager(InfoUIManager manager)
        {
            infoManagers ??= [];
            infoManagers.Add(manager);
        }

        public override void PostSetupContent()
        {
            if (infoManagers == null)
                return;

            List<PlayerInfoIcon> playerIcons = [];
            List<WorldInfoIcon> worldIcons = [];

            foreach (var manager in infoManagers)
            {
                playerIcons.AddRange(manager.GetPlayerInfoIcons());
                worldIcons.AddRange(manager.GetWorldInfoIcons());
            }

            playerInfoIcons = playerIcons.OrderBy(item => item.Priority);
            worldInfoIcons = worldIcons.OrderBy(item => item.Priority);
        }

        void IExistingDetourProvider.Subscribe()
        {
            playerInfoIcons = [];
            worldInfoIcons = [];
            playerInfoDataField = typeof(UICharacterListItem).GetField("_data", UniversalBindingFlags);
            playerLeftTextField = typeof(UICharacterListItem).GetField("_buttonLabel", UniversalBindingFlags);
            playerRightTextField = typeof(UICharacterListItem).GetField("_deleteButtonLabel", UniversalBindingFlags);
            worldLeftTextField = typeof(UIWorldListItem).GetField("_buttonLabel", UniversalBindingFlags);
            worldRightTextField = typeof(UIWorldListItem).GetField("_deleteButtonLabel", UniversalBindingFlags);
            ExtraTextToAppend = [];
            On_UICharacterListItem.DrawSelf += DrawInfoIcons_Player;
            On_UICharacterSelect.Draw += DrawHoverText;
            On_UIWorldListItem.DrawSelf += DrawInfoIcons_World;
        }

        void IExistingDetourProvider.Unsubscribe()
        {
            playerInfoIcons = null;
            worldInfoIcons = null;
            playerInfoDataField = null;
            playerLeftTextField = null;
            playerRightTextField = null;
            worldLeftTextField = null;
            worldRightTextField = null;
            infoManagers = null;
            ExtraTextToAppend = null;
            On_UICharacterListItem.DrawSelf -= DrawInfoIcons_Player;
            On_UICharacterSelect.Draw -= DrawHoverText;
            On_UIWorldListItem.DrawSelf -= DrawInfoIcons_World;
        }
        #endregion

        #region Player Screen
        private void DrawInfoIcons_Player(On_UICharacterListItem.orig_DrawSelf orig, UICharacterListItem self, SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);

            var player = (playerInfoDataField.GetValue(self) as PlayerFileData).Player;
            var activeIcons = playerInfoIcons.Where(icon => icon.ShouldAppear(player));
            int count = activeIcons.Count();
            if (count <= 0)
                return;

            float idealIconSize = 20f;
            float iconPadding = 5f;
            CalcuateMaxIcons(self, count, idealIconSize + iconPadding, 117f, playerLeftTextField, playerRightTextField, out var currentX, out var maxToDraw);

            var elementInnerDimensions = self.GetInnerDimensions();

            DrawIcons(spriteBatch, elementInnerDimensions, currentX, maxToDraw, count, idealIconSize, iconPadding, activeIcons);
        }
        #endregion

        #region World Screen
        private void DrawInfoIcons_World(On_UIWorldListItem.orig_DrawSelf orig, UIWorldListItem self, SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);

            var data = self.Data;
            var activeIcons = worldInfoIcons.Where(icon => icon.ShouldAppear(data));
            int count = activeIcons.Count();
            if (count <= 0)
                return;

            float idealIconSize = 20f;
            float iconPadding = 5f;
            CalcuateMaxIcons(self, count, idealIconSize + iconPadding, 140, worldLeftTextField, worldRightTextField, out var currentX, out var maxToDraw);
            var elementInnerDimensions = self.GetInnerDimensions();
            DrawIcons(spriteBatch, elementInnerDimensions, currentX, maxToDraw, count, idealIconSize, iconPadding, activeIcons);
        }
        #endregion

        #region Icon Drawing
        private static void CalcuateMaxIcons(object self, int activeIconCount, float iconSpaceNeeded, float initialXOffset, FieldInfo leftText, FieldInfo rightText, out float currentX, out int maxToDraw)
        {
            UIText leftTextValue = leftText.GetValue(self) as UIText;
            UIText rightTextValue = rightText.GetValue(self) as UIText;

            float maxSpace = 435f;
            currentX = initialXOffset;
            bool changedSize = false;
            if (leftTextValue.Text != string.Empty)
            {
                float width = FontAssets.MouseText.Value.MeasureString(leftTextValue.Text).X + 10f;
                currentX += width;
                maxSpace -= width;
                changedSize = true;
            }
            else if (rightTextValue.Text != string.Empty)
            {
                float width = FontAssets.MouseText.Value.MeasureString(rightTextValue.Text).X + 10f;
                maxSpace -= width;
                changedSize = true;
            }

            maxToDraw = Math.Min(activeIconCount, (int)(maxSpace / iconSpaceNeeded));

            // If the size has changed, but only removed 1 icon, it won't actually change the size due to the +x text appearing, so account for that.
            if (changedSize && activeIconCount == maxToDraw + 1)
                maxToDraw--;
        }

        private static void DrawIcons(SpriteBatch spriteBatch, CalculatedStyle elementInnerDimensions, float currentX, int maxToDraw, int count, float idealIconSize, float iconPadding, IEnumerable<IInfoIcon> activeIcons)
        {
            var panelDrawPosition = new Vector2(elementInnerDimensions.X + currentX - 13f, elementInnerDimensions.Y + elementInnerDimensions.Height - 25);

            DrawPanel(spriteBatch, panelDrawPosition, (maxToDraw + (count > maxToDraw ? 1 : 0)) * (idealIconSize + iconPadding) + iconPadding);

            int totalDrawn = 0;
            int totalNotDrawn = 0;
            List<string> textsToAppend = [];

            foreach (var icon in activeIcons)
            {
                if (totalDrawn < maxToDraw)
                {
                    var iconTexture = ModContent.Request<Texture2D>(icon.TexturePath, AssetRequestMode.ImmediateLoad).Value;
                    Vector2 iconDrawPosition = new(elementInnerDimensions.X + currentX, elementInnerDimensions.Y + elementInnerDimensions.Height - 11f);

                    float iconScale = idealIconSize / (MathF.Max(float.Epsilon, MathF.Max(iconTexture.Width, iconTexture.Height)));
                    var hitbox = new Rectangle((int)(iconDrawPosition.X - iconTexture.Width * iconScale * 0.5f), (int)(iconDrawPosition.Y - iconTexture.Height * iconScale * 0.5f),
                        (int)(iconTexture.Width * iconScale), (int)(iconTexture.Height * iconScale));

                    if (hitbox.Contains(Main.MouseScreen.ToPoint()))
                    {
                        currentMouseText = icon.HoverTextKey;
                        textShouldShow = true;
                        int backglowAmount = 8;
                        for (int i = 0; i < backglowAmount; i++)
                        {
                            Vector2 drawOffset = (Tau * i / backglowAmount).ToRotationVector2() * 2f;
                            spriteBatch.Draw(iconTexture, iconDrawPosition + drawOffset + new Vector2(2, 2), null, Color.Black * 0.15f, 0f, iconTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);
                        }

                        for (int i = 0; i < backglowAmount; i++)
                        {
                            Vector2 drawOffset = (Tau * i / backglowAmount).ToRotationVector2() * 1.5f;
                            spriteBatch.Draw(iconTexture, iconDrawPosition + drawOffset, null, Color.White with { A = 0 } * 0.9f, 0f, iconTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);
                        }
                    }

                    // Drop shadow for style points.
                    spriteBatch.Draw(iconTexture, iconDrawPosition + new Vector2(2, 2), null, Color.Black * 0.3f, 0f, iconTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(iconTexture, iconDrawPosition, null, Color.White, 0f, iconTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);

                    currentX += iconTexture.Width * iconScale + iconPadding;
                    totalDrawn++;
                }
                else
                {
                    totalNotDrawn++;
                    textsToAppend.Add(icon.HoverTextKey);
                }
            }

            if (totalNotDrawn <= 0)
                return;
            
            string text = $"+{totalNotDrawn}";
            Vector2 textDrawPosition = new(elementInnerDimensions.X + currentX, elementInnerDimensions.Y + elementInnerDimensions.Height - 7f);
            Vector2 origin = FontAssets.MouseText.Value.MeasureString(text) * 0.5f;
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, textDrawPosition, Color.White, 0f, origin, Vector2.One * 0.8f);

            Rectangle area = new((int)(textDrawPosition.X - origin.X * 0.8f), (int)(textDrawPosition.Y - origin.Y * 0.8f), (int)(origin.X * 2.1f * 0.8f), (int)(origin.Y * 1.5f * 0.8f));
            if (area.Contains(Main.MouseScreen.ToPoint()))
                ExtraTextToAppend = textsToAppend;
        }

        private void DrawHoverText(On_UICharacterSelect.orig_Draw orig, UICharacterSelect self, SpriteBatch spriteBatch)
        {
            textShouldShow = false;
            ExtraTextToAppend.Clear();
            orig(self, spriteBatch);

            if (textShouldShow)
            {
                textShouldShow = false;
                int X = Main.mouseX + 14;
                int Y = Main.mouseY + 14;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, Language.GetTextValue(currentMouseText), new Vector2(X, Y), Color.White, 0f, Vector2.Zero, Vector2.One);
                currentMouseText = string.Empty;
            }
            else if (ExtraTextToAppend.Any())
            {
                int X = Main.mouseX + 14;
                int Y = Main.mouseY + 14;
                foreach (var key in ExtraTextToAppend)
                {
                    // This looks ugly, but you have bigger problems than that if you have enough to show this (and aren't Russian).
                    var locacalizedText = Language.GetTextValue(key);
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, locacalizedText, new Vector2(X, Y), Color.White, 0f, Vector2.Zero, Vector2.One);
                    Y += (int)FontAssets.MouseText.Value.MeasureString(locacalizedText).Y;
                }
            }
        }

        private static void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
        {
            var panelTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/InnerPanelBackground");

            spriteBatch.Draw(panelTexture.Value, position, new Rectangle(0, 0, 8, panelTexture.Height()), Color.White);
            spriteBatch.Draw(panelTexture.Value, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, panelTexture.Height()), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
            spriteBatch.Draw(panelTexture.Value, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, panelTexture.Height()), Color.White);
        }
        #endregion
    }
}
