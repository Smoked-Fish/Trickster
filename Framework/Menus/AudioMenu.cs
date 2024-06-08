using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using Trickster.Framework.Utilities;
using Common.Utilities;
using Trickster.Framework.UI;

namespace Trickster.Framework.Menus
{
    public class AudioMenu : IClickableMenu
    {
        // Textures and UI elements
        private static readonly Texture2D ButtonTexture = ModEntry.ModHelper.ModContent.Load<Texture2D>("assets/buttons.png");
        private static CustomTable CueTable { get; } = new() { RowHeight = 18 };
        private static Label TitleLabel = null!;
        public static CustomTextBox SearchBar { get; private set; } = null!;
        private static CustomImage selectedFarmerCheck = null!;

        // Selected cue and farmer
        private static string? SelectedCue;
        private static Farmer? SelectedFarmer;

        // Base Elements
        private readonly RootElement rootUI;
        private readonly StaticContainer container;

        // Dimensions of the menu
        private const int Width = 800;
        private const int Height = 600;
        static AudioMenu()
        {
            foreach (string cue in CueUtility.GetAllCues(CueUtility.CueList))
            {
                var audioCue = new Label
                {
                    String = cue,
                    LocalPosition = new Vector2(16, 0),
                    NonBoldShadow = false,
                    Font = Game1.smallFont,
                    NonBoldScale = 1,
                    Callback = (_) => TableCallback(cue),
                };
                SelectedCue ??= cue;

                var musicIcon = new CustomImage()
                {
                    Texture = ButtonTexture,
                    TextureRect = new Rectangle(228, 46 - 10, 12, 14 + 10),
                    LocalPosition = new Vector2(15 - 12, 0),
                    CueName = cue,
                    Hidden = true,
                    IsMusic = true,
                };

                var favIcon = new CustomImage()
                {
                    Texture = ButtonTexture,
                    TextureRect = new Rectangle(214, 48 - 12, 14, 12 + 12),
                    LocalPosition = new Vector2(10 - 14 - 11, 10),
                    CueName = cue,
                    Hidden = true
                };

                CueTable.AddRow([audioCue, musicIcon, favIcon]);
            }
            CueTable.SortCueTable();
        }

        public AudioMenu() : base((Game1.uiViewport.Width - Width) / 2, (Game1.uiViewport.Height - Height) / 2, Width, Height)
        {
            rootUI = new RootElement();
            container = new StaticContainer
            {
                LocalPosition = new Vector2(xPositionOnScreen, yPositionOnScreen),
                Size = new Vector2(Width, (Height / 3) - 64)
            };
            rootUI.AddChild(container);

            TitleLabel = new Label
            {
                String = "",
                Font = Game1.smallFont,
                NonBoldShadow = false,
                LocalPosition = new Vector2(12, 8)
            };

            container.AddChild(TitleLabel);

            CueTable.LocalPosition = new Vector2(24, (Height / 3) - 16);
            CueTable.Size = new Vector2(Width - 48, Height - (Height / 3));

            AddButton("play", new Vector2(Width - 8 - 180, 8), container, PlayButtonCallback, out Button _);
            AddButton("stop", new Vector2(Width - 8 - 120, 8), container, StopButtonCallback, out Button _);
            AddButton("random", new Vector2(Width - 8 - 60, 8), container, RandomButtonCallback, out Button randButton);

            var sortButton = new Button
            {
                Texture = ButtonTexture,
                IdleTextureRect = new Rectangle(180, 0, 27, 60),
                HoverTextureRect = new Rectangle(180, 0, 27, 60),
                Callback = SortCallback
            };
            sortButton.LocalPosition = new Vector2(Width - 8 - sortButton.Width, (Height / 3) - (sortButton.Height/2));

            selectedFarmerCheck = new CustomImage
            {
                Texture = ButtonTexture,
                TextureRect = new Rectangle(207, 0, 24, 21),
                LocalPosition = new Vector2(0, 0),
                Hidden = true
            };
            rootUI.AddChild(selectedFarmerCheck);

            SearchBar = new CustomTextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), Game1.staminaRect, Game1.smallFont, Game1.textColor)
            {
                Width = 180 + 3, // 3 Buttons
                Height = 42
            };
            SearchBar.X = (int)randButton.Position.X - SearchBar.Width + randButton.Width;
            SearchBar.Y = (int)randButton.Position.Y + SearchBar.Height + 18 + 8;
            //SearchBar.X = (int)sortButton.Position.X - SearchBar.Width/2;
            //SearchBar.Y = (int)sortButton.Position.Y + SearchBar.Height + 18;
            //SearchBar.X = ((Game1.uiViewport.Width - Width) / 2) + (SearchBar.Width / 2);
            //SearchBar.Y = (Height / 3) - (SearchBar.Height / 2) + 6;
            SearchBar.TextChanged += SearchBarCallback;

            container.AddChild(CueTable);
            container.AddChild(sortButton);
        }

        private static void AddButton(string name, Vector2 position, StaticContainer container, Action<Element> callback, out Button buttonRef)
        {
            var button = new Button
            {
                Texture = ButtonTexture,
                IdleTextureRect = new Rectangle(name == "play" ? 0 : name == "stop" ? 60 : 120, 0, 60, 60),
                HoverTextureRect = new Rectangle(name == "play" ? 0 : name == "stop" ? 60 : 120, 60, 60, 60),
                LocalPosition = position,
                Callback = callback
            };
            container.AddChild(button);
            buttonRef = button;
        }

        private static void TableCallback(string cue)
        {
            Game1.playSound("shiny4");

            SelectedCue = cue;
        }

        private static void SearchBarCallback(CustomTextBox sender)
        {
        }
        private static void PlayButtonCallback(Element element)
        {
            if (SelectedCue == null) return;

            Game1.playSound("shiny4");

            var targetFarmer = SelectedFarmer ?? Game1.player;
            var targetLocation = targetFarmer.currentLocation;

            CueUtility.PrepareToPlayCue(SelectedCue, targetLocation);
        }

        private static void StopButtonCallback(Element element)
        {
            Game1.playSound("shiny4");
            CueUtility.StopAllPlayingCues();
        }

        private static void RandomButtonCallback(Element element)
        {
            Game1.playSound("shiny4");
            Random random = new();
            int index = random.Next(CueTable.Children.Length);
            var selectedLabel = CueTable.Children[index] as Label;
            SelectedCue = selectedLabel?.String;
        }

        private static void SortCallback(Element element)
        {
            Game1.playSound("shiny4");
            if (element is not Button button) return;

            switch (CueUtility.NextSortMode())
            {
                case 1:
                    button.IdleTextureRect = new Rectangle(180, 60, 27, 60);
                    button.HoverTextureRect = new Rectangle(180, 60, 27, 60);
                    break;
                case 2:
                    button.IdleTextureRect = new Rectangle(210, 60, 27, 60);
                    button.HoverTextureRect = new Rectangle(210, 60, 27, 60);
                    break;
                default:
                    button.IdleTextureRect = new Rectangle(180, 0, 27, 60);
                    button.HoverTextureRect = new Rectangle(180, 0, 27, 60);
                    break;
            }

            CueTable.SortCueTable();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            CueTable.Scrollbar.ScrollBy(Math.Clamp(-direction, -4, 4));
            base.receiveScrollWheelAction(direction);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            const int portraitWidth = 64;
            const int portraitHeight = 64;

            float startX = xPositionOnScreen + 12;
            float startY = yPositionOnScreen + container.Height - portraitHeight - 32;

            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (farmer.IsLocalPlayer) continue;

                var portraitBounds = new Rectangle((int)startX, (int)startY, portraitWidth, portraitHeight);

                if (portraitBounds.Contains(x, y))
                {
                    SelectedFarmer = SelectedFarmer == farmer ? null : farmer;
                    break;
                }
                startX += portraitWidth;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);

            foreach (var row in CueTable.Rows)
            {
                var label = row.FirstOrDefault(element => element is Label);
                if (label?.Bounds.Contains(x, y) == false) continue;

                foreach (var rowElement in row)
                {
                    if (rowElement is not CustomImage { IsMusic: false } customImage) continue;

                    var matchingCue = CueUtility.GetCueDetailsByName(customImage.CueName);
                    if (matchingCue != null)
                        matchingCue.Favorite = !matchingCue.Favorite;
                }
            }

            CueTable.SortCueTable();
        }

        public override void update(GameTime time)
        {
            rootUI.Update();
            SearchBar.Update();

            foreach (var child in CueTable.Children)
            {
                if (child is not Label label) continue;
                bool isSelected = label.String == SelectedCue;
                label.IdleTextColor = isSelected ? Color.Red : Game1.textColor;
                label.HoverTextColor = isSelected ? Color.Red : Game1.unselectedOptionColor;
            }


            if (SelectedFarmer == null)
            {
                selectedFarmerCheck.Hidden = true;
            }
            else if (!SelectedFarmer.isActive())
            {
                SelectedFarmer = null;
                selectedFarmerCheck.Hidden = true;
            }


            if (TitleLabel.String != SelectedCue) TitleLabel.String = $"{I18n.GetByKey("Text.Trickster.SelectedCue")}: {SelectedCue}";
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.None) return;
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose() && !SearchBar.Selected)
            {
                this.exitThisMenu();
            }
            else
            {
                if (!Game1.options.snappyMenus || !Game1.options.gamepadControls || this.overrideSnappyMenuCursorMovementBan()) return;
                this.applyMovementKey(key);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.options.showMenuBackground)
                base.drawBackground(b);
            else
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen - 12, yPositionOnScreen - 12, width + 24, height - 42 - (2 * (Height / 3)), Color.White, 1f, false);
            rootUI.Draw(b);
            SearchBar.Draw(b, false);

            float startX = xPositionOnScreen + 12;
            float startY = yPositionOnScreen + container.Height - 64 - 32;

            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (farmer.IsLocalPlayer) continue;

                bool isSelectedFarmer = farmer == SelectedFarmer;
                var portraitPosition = new Vector2(startX, startY);
                Rectangle portraitBounds = new((int)portraitPosition.X, (int)portraitPosition.Y, 64, 64);

                farmer.FarmerRenderer.drawMiniPortrat(b, isSelectedFarmer ? portraitPosition - new Vector2(2, 2) : portraitPosition, 0.5f, isSelectedFarmer ? 4.3f : 4f, 1, farmer);

                string farmerName = string.IsNullOrEmpty(farmer.Name) ? I18n.GetByKey("Text.Trickster.DefaultName") : farmer.Name;

                if (portraitBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    drawHoverText(b, farmerName, Game1.smallFont, textColor: Game1.textColor);

                if (isSelectedFarmer)
                {
                    selectedFarmerCheck.LocalPosition = new Vector2(portraitPosition.X + (portraitBounds.Width / 2) - (selectedFarmerCheck.Width/2), portraitPosition.Y + 64 + 6);
                    selectedFarmerCheck.Hidden = false;
                }

                startX += 64;
            }

            drawMouse(b);
        }
    }
}