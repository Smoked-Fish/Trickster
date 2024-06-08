using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using StardewValley;

namespace Trickster.Framework.UI
{
    public class CustomImage : Element, ISingleTexture
    {
        public Texture2D? Texture { get; set; }
        public Rectangle TextureRect { get; init; }
        public bool Hidden { get; set; } = true;
        public bool IsMusic { get; init; }
        public string CueName { get; init; } = string.Empty;
        public override int Width => TextureRect.Width;
        public override int Height => TextureRect.Height;

        public CustomImage() { }
        public CustomImage(Texture2D tex)
        {
            Texture = tex;
            TextureRect = new Rectangle(0, 0, tex.Width / 2, tex.Height);
        }

        public override void Draw(SpriteBatch b)
        {
            if (Hidden) return;

            Vector2 origin = new(TextureRect.Width / 2f, TextureRect.Height / 2f);
            b.Draw(Texture, Position + origin, TextureRect, Color.White, 0.0f, origin, 1f, SpriteEffects.None, 0.0f);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}
