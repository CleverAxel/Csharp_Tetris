
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DirtyLibrary.Primitives {
public static class TextureColor {
    public static readonly Texture2D Red = BuildTexture(Color.Red);
    public static readonly Texture2D Blue = BuildTexture(Color.Blue);
    public static readonly Texture2D Green = BuildTexture(Color.Green);
    public static readonly Texture2D Yellow = BuildTexture(Color.Yellow);

    private static Texture2D BuildTexture(Color color) {
        var texture = new Texture2D(Core.GraphicsDevice, 1, 1);
        texture.SetData([color]);
        return texture;
    }
}
}