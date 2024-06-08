using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Trickster.Framework.Menus;
using Trickster.Framework.Utilities;

namespace Trickster.Framework.UI
{
    public class CustomTable : Container
    {
        /*********
        ** Fields
        *********/
        public List<Element[]> Rows { get; } = [];

        private Vector2 SizeImpl;

        private const int RowPadding = 16;
        private int RowHeightImpl;
        private readonly bool FixedRowHeight;
        private int ContentHeight;

        /*********
        ** Accessors
        *********/
        public Vector2 Size
        {
            get => SizeImpl;
            set
            {
                SizeImpl = new Vector2(value.X, (int)value.Y / RowHeight * RowHeight);
                UpdateScrollbar();
            }
        }

        public int RowHeight
        {
            get => RowHeightImpl;
            set
            {
                RowHeightImpl = value + RowPadding;
                UpdateScrollbar();
            }
        }
        public Scrollbar Scrollbar { get; }
        public override int Width => (int)Size.X;
        public override int Height => (int)Size.Y;

        /*********
        ** Public methods
        *********/
        public CustomTable(bool fixedRowHeight = true)
        {
            FixedRowHeight = fixedRowHeight;
            UpdateChildren = false; // table will update children itself
            Scrollbar = new Scrollbar
            {
                LocalPosition = new Vector2(0, 0)
            };
            AddChild(Scrollbar);
        }

        public void SortCueTable()
        {
            Rows.Sort((row1, row2) =>
            {
                string? label1 = GetLabelString(row1);
                string? label2 = GetLabelString(row2);

                if (label1 == null || label2 == null)
                    return label1 != null ? -1 : label2 != null ? 1 : 0;

                bool isFavorite1 = CueUtility.IsFavoriteCue(label1);
                bool isFavorite2 = CueUtility.IsFavoriteCue(label2);

                if (isFavorite1 && !isFavorite2)
                    return -1;
                if (!isFavorite1 && isFavorite2)
                    return 1;

                int index1 = CueUtility.CueList.FindIndex(c => c.Name == label1);
                int index2 = CueUtility.CueList.FindIndex(c => c.Name == label2);

                return index1.CompareTo(index2);
            });
        }

        public void AddRow(Element[] elements)
        {
            Rows.Add(elements);
            int maxElementHeight = 0;
            foreach (var child in elements)
            {
                AddChild(child);
                maxElementHeight = Math.Max(maxElementHeight, child.Height);
            }
            ContentHeight += FixedRowHeight ? RowHeight : maxElementHeight + RowPadding;
            UpdateScrollbar();
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);
            if (IsHidden(isOffScreen))
                return;

            int topPx = 0;
            foreach (var row in Rows)
            {
                if (FilterRows(row, AudioMenu.SearchBar.Text) == null)
                    continue;

                if (ContainsMusicString(row, out Label? foundLabel))
                {
                    if (!ModEntry.Config.EnableMusicCues)
                        continue;

                    foreach (var rowElement in row)
                    {
                        if (rowElement is CustomImage { IsMusic: true } customImage)
                        {
                            customImage.Hidden = false;
                        }
                    }
                }

                if (ContainsFavorite(row))
                {
                    foreach (var rowElement in row)
                    {
                        if (rowElement is CustomImage { IsMusic: false } customImage)
                        {
                            customImage.Hidden = false;
                        }
                    }
                }
                else
                {
                    foreach (var rowElement in row)
                    {
                        if (rowElement is CustomImage { IsMusic: false } customImage)
                        {
                            customImage.Hidden = true;
                        }
                    }
                }

                int maxElementHeight = 0;
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, topPx - Scrollbar.TopRow * RowHeight);
                    bool isChildOffScreen = isOffScreen || IsElementOffScreen(element);

                    if (!isChildOffScreen || element is Label) // Labels must update anyway to get rid of hovertext on scrollwheel
                        element.Update(isOffScreen: isChildOffScreen);
                    maxElementHeight = Math.Max(maxElementHeight, element.Height);
                }
                topPx += FixedRowHeight ? RowHeight : maxElementHeight + RowPadding;
            }

            if (topPx != ContentHeight)
            {
                ContentHeight = topPx;
                Scrollbar.Rows = PxToRow(ContentHeight);
            }

            Scrollbar.Update();
        }
        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (IsHidden())
                return;

            // calculate draw area
            var backgroundArea = new Rectangle((int)Position.X - 32, (int)Position.Y - 32, (int)Size.X + 64, (int)Size.Y + 64);
            const int contentPadding = 12;
            var contentArea = new Rectangle(backgroundArea.X + contentPadding, backgroundArea.Y + contentPadding, backgroundArea.Width - contentPadding * 2, backgroundArea.Height - contentPadding * 2);

            // draw background
            IClickableMenu.drawTextureBox(b, backgroundArea.X, backgroundArea.Y, backgroundArea.Width, backgroundArea.Height, Color.White);
            b.Draw(Game1.menuTexture, contentArea, new Rectangle(64, 128, 64, 64), Color.White); // Smoother gradient for the content area.

            // draw table contents
            // This uses a scissor rectangle to clip content taller than one row that might be
            // drawn past the bottom of the UI, like images or complex options.
            Element? renderLast = null;
            InScissorRectangle(b, contentArea, contentBatch =>
            {
                foreach (var row in Rows)
                {
                    if (!ModEntry.Config.EnableMusicCues && ContainsMusicString(row, out Label? foundLabel))
                        continue;

                    if (FilterRows(row, AudioMenu.SearchBar.Text) == null)
                        continue;

                    foreach (var element in row)
                    {
                        if (IsElementOffScreen(element))
                            continue;
                        if (element == RenderLast)
                        {
                            renderLast = element;
                            continue;
                        }
                        element.Draw(contentBatch);
                    }
                }
            });
            renderLast?.Draw(b);

            Scrollbar.Draw(b);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a child element is outside the table's current display area.</summary>
        /// <param name="element">The child element to check.</param>
        private bool IsElementOffScreen(Element element) => element.Position.Y + element.Height < Position.Y || element.Position.Y > Position.Y + Size.Y;

        private void UpdateScrollbar()
        {
            Scrollbar.LocalPosition = new Vector2(Size.X + 48, Scrollbar.LocalPosition.Y);
            Scrollbar.RequestHeight = (int)Size.Y;
            Scrollbar.Rows = PxToRow(ContentHeight);
            Scrollbar.FrameSize = (int)(Size.Y / RowHeight);
        }

        private static void InScissorRectangle(SpriteBatch spriteBatch, Rectangle area, Action<SpriteBatch> draw)
        {
            // render the current sprite batch to the screen
            spriteBatch.End();

            // start temporary sprite batch
            using SpriteBatch contentBatch = new(Game1.graphics.GraphicsDevice);
            GraphicsDevice device = Game1.graphics.GraphicsDevice;
            Rectangle prevScissorRectangle = device.ScissorRectangle;

            // render in scissor rectangle
            try
            {
                device.ScissorRectangle = area;
                contentBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);

                draw(contentBatch);

                contentBatch.End();
            }
            finally
            {
                device.ScissorRectangle = prevScissorRectangle;
            }

            // resume previous sprite batch
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        private int PxToRow(int px) => (px + RowHeight - 1) / RowHeight;

        private static bool ContainsFavorite(Element[] elements)
        {
            foreach (var element in elements)
            {
                if (element is not Label label) continue;

                var selectedCue = CueUtility.CueList.Find(cue => cue.Name == label.String);
                if (selectedCue == null) continue;

                return selectedCue.Favorite;
            }

            return false;
        }

        private static bool ContainsMusicString(Element[] elements, out Label? foundLabel)
        {
            foreach (var element in elements)
            {
                if (element is Label label && CueUtility.MusicCueList.Contains(label.String))
                {
                    foundLabel = label;
                    return true;
                }
            }

            foundLabel = null;
            return false;
        }

        private static string? GetLabelString(Element[] row)
        {
            // Find the label within the elements of the row
            var labelElement = row.FirstOrDefault(e => e is Label);

            return ((Label?)labelElement)?.String;
        }

        // Filters Non-Matching
        private static Element[]? FilterRows(Element[] row, string search)
        {
            bool matchFound = false;

            foreach (var element in row)
            {
                if (element is not Label label || !(label.String?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                matchFound = true;
                break;
            }

            return !matchFound ? null : row;
        }
    }
}