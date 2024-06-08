using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace Trickster.Framework.UI
{
    public class CustomTextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor) : TextBox(textBoxTexture, caretTexture, font, textColor)
    {
        public delegate void TextChangedHandler(CustomTextBox sender);
        public event TextChangedHandler? TextChanged;

        public override void RecieveTextInput(char inputChar)
        {
            base.RecieveTextInput(inputChar);
            TextChanged?.Invoke(this);
        }

        public override void RecieveTextInput(string text)
        {
            base.RecieveTextInput(text);
            TextChanged?.Invoke(this);
        }

        public override void RecieveCommandInput(char command)
        {
            base.RecieveCommandInput(command);
            if (command == '\b' && Text.Length > 0) // Backspace
                TextChanged?.Invoke(this);
        }
    }
}
