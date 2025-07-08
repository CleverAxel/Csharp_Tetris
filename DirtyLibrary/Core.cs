using System;
using DirtyLibrary.External.SDL2;
using DirtyLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DirtyLibrary {
    public class Core : Game {
        internal static Core s_instance;
        public static Core Instance => s_instance;



        protected RenderTarget2D renderTarget2D;
        protected Rectangle renderDestRect = Rectangle.Empty;
        public ref Rectangle RenderDestRect => ref renderDestRect;
        private short _virtualWidth;
        private short _virtualHeight;
        private bool _isResizing = false;



        public static GraphicsDeviceManager Graphics { get; private set; }

        public static new GraphicsDevice GraphicsDevice { get; private set; }

        public static SpriteBatch SpriteBatch { get; private set; }

        public static new ContentManager Content { get; private set; }

        public static InputManager InputManager { get; private set; } = new InputManager(new MouseManager(), new KeyboardManager());
        public static AudioController Audio { get; private set; } = new AudioController();

        public static int TimeElapsed { get; protected set; }
        public static float DeltaTime { get; protected set; }
        public static float OsScale { get; protected set; }



        public Core(string title, short virtualWidth, short virtualHeight, bool fullScreen) {
            if (s_instance != null) {
                throw new InvalidOperationException($"Only a single Core instance can be created");
            }

            s_instance = this;
            _virtualWidth = virtualWidth;
            _virtualHeight = virtualHeight;

            Graphics = new GraphicsDeviceManager(this);

            IntPtr sdlHandle = Window.Handle;
            OsScale = GetDisplayScale(sdlHandle);

            Graphics.PreferredBackBufferWidth = (int)(virtualWidth * OsScale);
            Graphics.PreferredBackBufferHeight = (int)(virtualHeight * OsScale);
            OsScale = 1f / OsScale;
            
            Graphics.IsFullScreen = fullScreen;
            Graphics.ApplyChanges();

            Window.Title = title;

            Content = base.Content;
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            // Window.IsBorderless = true;
            // Graphics.SynchronizeWithVerticalRetrace = true;
        }

        public ref Rectangle GetRenderDestRect() {
            return ref renderDestRect;
        }

        protected override void Initialize() {
            base.Initialize();

            GraphicsDevice = base.GraphicsDevice;

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            InitializeRenderTarget();
        }

        private void InitializeRenderTarget() {
            renderTarget2D = new RenderTarget2D(GraphicsDevice, _virtualWidth, _virtualHeight);
            Window.ClientSizeChanged += OnWindowResize;
            CalculateRenderRectangle();
        }

        private void CalculateRenderRectangle() {
            _isResizing = true;
            float windowWidth = Window.ClientBounds.Width;
            float windowHeight = Window.ClientBounds.Height;

            float scaleX = windowWidth / renderTarget2D.Width;
            float scaleY = windowHeight / renderTarget2D.Height;
            float scale = scaleX < scaleY ? scaleX : scaleY;

            renderDestRect.Width = (int)(renderTarget2D.Width * scale);
            renderDestRect.Height = (int)(renderTarget2D.Height * scale);

            renderDestRect.X = (int)(windowWidth - renderDestRect.Width) / 2;
            renderDestRect.Y = (int)(windowHeight - renderDestRect.Height) / 2;
            _isResizing = false;
        }

        private void OnWindowResize(object sender, EventArgs e) {
            if (!_isResizing)
                CalculateRenderRectangle();
        }

        public static bool WindowHasFocus() {

            return Instance.IsActive;
        }

        private static float GetDisplayScale(IntPtr sdlWindowHandle) {
            int displayIndex = SDL.SDL_GetWindowDisplayIndex(sdlWindowHandle);
            if (displayIndex < 0)
                return 1f;

            if (SDL.SDL_GetDisplayDPI(displayIndex, out float ddpi, out float hdpi, out float vdpi) == 0) {
                return ddpi / 96f;
            } else {
                return 1f;
            }
        }
    }
}