using System;
using System.Runtime.InteropServices;
using SkiaSharp;

//using SkiaSharp;

namespace wskia
{
    unsafe class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        struct EmscriptenWebGLContextAttributes
        {
            int alpha;
            internal int depth;
            internal int stencil;
            int antialias;
            int premultipliedAlpha;
            int preserveDrawingBuffer;
            int powerPreference;
            int failIfMajorPerformanceCaveat;

            internal int majorVersion;
            int minorVersion;

            int enableExtensionsByDefault;
            int explicitSwapControl;
            int proxyContextToMainThread;
            int renderViaOffscreenBackBuffer;
        }

        [DllImport("*")]
        internal static extern unsafe void emscripten_set_main_loop(delegate* unmanaged <void> f, int fps, int simulate_infinite_loop);
        [DllImport("*")]
        static extern IntPtr emscripten_webgl_create_context(byte* s, EmscriptenWebGLContextAttributes* attr);
        [DllImport("*")]
        static extern void emscripten_webgl_init_context_attributes(EmscriptenWebGLContextAttributes* attr);
        [DllImport("*")]
        static extern void emscripten_webgl_make_context_current(IntPtr context);
        [DllImport("*")]
        static extern void emscripten_get_canvas_element_size(byte* s, int* width, int* height);

        static IntPtr glContext;

        // "#canvas"  in the DOM
        static ReadOnlySpan<byte> s => new byte[]
        {
            0x23,
            0x63,0x61,0x6E,0x76, 0x61, 0x73,
            0x0
        };

        [UnmanagedCallersOnly(EntryPoint = "MainLoop")]
        static void MainLoop()
        {
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            // configure our brush
            var redBrush = new SKPaint
            {
                Color = new SKColor(0xff, 0, 0),
                IsStroke = true
            };
            var blueBrush = new SKPaint
            {
                Color = new SKColor(0, 0, 0xff),
                IsStroke = true
            };

            for (int i = 0; i < 64; i += 8)
            {
                var rect = new SKRect(i, i, 256 - i - 1, 256 - i - 1);
                canvas.DrawRect(rect, (i % 16 == 0) ? redBrush : blueBrush);
            }
        }
        static SKSurface surface;

        static void Main()
        {

            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");

            EmscriptenWebGLContextAttributes attrs;
            emscripten_webgl_init_context_attributes(&attrs);

            Console.WriteLine("emscripten_webgl_init_context_attributes");

            attrs.stencil = 8;
            // attrs.majorVersion = 2;
            fixed (byte* n = &(s[0]))
            {
                glContext = emscripten_webgl_create_context(n, &attrs);
            }
            Console.WriteLine("emscripten_webgl_create_context:");
            Console.WriteLine(glContext.ToInt32().ToString());

            emscripten_webgl_make_context_current(glContext);
            Console.WriteLine("emscripten_webgl_make_context_current");

            GRGlInterface grGlInterface = GRGlInterface.Create();
            Console.WriteLine("GRGlInterface.CreateWebGl");
            Console.WriteLine(grGlInterface.Handle.ToString());
            GRContext grContext = GRContext.CreateGl(grGlInterface);
            Console.WriteLine("GRContext.CreateGl");

            emscripten_set_main_loop(&MainLoop, 0, 0);


            const SKColorType colorType = SKColorType.Rgba8888;

            int width, height;
            fixed (byte* n = &(s[0]))
            {
                emscripten_get_canvas_element_size(n, &width, &height);
                Console.WriteLine("emscripten_get_canvas_element_size");
                Console.WriteLine(width + "," + height);
            }

            var info = new GRGlFramebufferInfo(0, colorType.ToGlSizedFormat());
            Console.WriteLine("GRGlFramebufferInfo");

            surface = SKSurface.Create(grContext, new GRBackendRenderTarget(width, height, 0, 8, info),
                colorType);
            if (surface != null)
            {
                Console.WriteLine("got a surface");
            }
            else
            {
                Console.WriteLine("Failed to create surface");
            }

            surface.Canvas.GetLocalClipBounds(out SKRect bounds);
            Console.WriteLine("clip");
            Console.WriteLine(bounds.Top + ", " + bounds.Left + " " + bounds.Bottom);
            surface.Canvas.DrawColor(SKColors.White);
        }
    }
}