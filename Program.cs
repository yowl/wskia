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
            internal int alpha;
            internal int depth;
            internal int stencil;
            internal int antialias;
            internal int premultipliedAlpha;
            internal int preserveDrawingBuffer;
            internal int powerPreference;
            internal int failIfMajorPerformanceCaveat;

            internal int majorVersion;
            internal int minorVersion;

            internal int enableExtensionsByDefault;
            internal int explicitSwapControl;
            internal int proxyContextToMainThread;
            internal int renderViaOffscreenBackBuffer;
        }

        [DllImport("*")]
        internal static extern unsafe void emscripten_set_main_loop(delegate* unmanaged <void> f, int fps, int simulate_infinite_loop);
#if WEBGL
        [DllImport("*")]
        static extern IntPtr emscripten_webgl_create_context(byte* s, EmscriptenWebGLContextAttributes* attr);
        [DllImport("*")]
        static extern void emscripten_webgl_init_context_attributes(EmscriptenWebGLContextAttributes* attr);
        [DllImport("*")]
        static extern void emscripten_webgl_make_context_current(IntPtr context);
#else
        [DllImport("*")]
        static extern IntPtr copyToCanvas(byte* buffer, int width, int height);
#endif
        [DllImport("*")]
        static extern void emscripten_get_canvas_element_size(byte* s, int* width, int* height);

        [DllImport("*")]
        static extern void glGetIntegerv(int code, int* intv);
        [DllImport("*")]
        static extern void glBindFramebuffer(int target, int buffer);
        [DllImport("*")]
        static extern void glClearColor(int c1, int c2, int c3, int c4);
        [DllImport("*")]
        static extern void glClearStencil(int stencil);
        [DllImport("*")]
        static extern void glClear(int flag);

        static IntPtr glContext;

        // "#canvas"  in the DOM
        static ReadOnlySpan<byte> s => new byte[]
        {
            0x23,
            0x63,0x61,0x6E,0x76, 0x61, 0x73,
            0x0
        };

        static int _sWidth, _sHeight;
        static int _frameIx;
        static int _skip;

        [UnmanagedCallersOnly(EntryPoint = "MainLoop")]
        static void MainLoop()
        {
            if (_skip == 0)
            {
                SKCanvas canvas = _surface.Canvas;

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
                    var iOffset = i + _frameIx;
                    var rect = new SKRect(iOffset, iOffset, 256 - iOffset - 1, 256 - iOffset - 1);
                    canvas.DrawRect(rect, (i % 16 == 0) ? redBrush : blueBrush);
                }
                _frameIx++;
                if (_frameIx == 16) _frameIx = 0;

                copyToCanvas((byte*) _surface.PeekPixels().GetPixels(), _sWidth, _sHeight);

                _skip = 2;
            }
            else
            {
                _skip--;
            }
        }
        
        static SKSurface? _surface;

        static void Main()
        {

            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");

#if WEBGL
            EmscriptenWebGLContextAttributes attrs;
            emscripten_webgl_init_context_attributes(&attrs);

            Console.WriteLine("emscripten_webgl_init_context_attributes");

            attrs.stencil = 8;
            attrs.majorVersion = 2;
            attrs.depth = 1;
            attrs.alpha = 1;
            attrs.enableExtensionsByDefault = 1;
            attrs.premultipliedAlpha = 1;

            fixed (byte* n = &(s[0]))
            {
                glContext = emscripten_webgl_create_context(n, &attrs);
            }
            Console.WriteLine("emscripten_webgl_create_context:");
            Console.WriteLine(glContext.ToInt32().ToString());
            emscripten_webgl_make_context_current(glContext);
            Console.WriteLine("emscripten_webgl_make_context_current");
//            var grContext = GRContext.CreateGl();
            GRGlInterface grGlInterface = GRGlInterface.Create();
            Console.WriteLine("GRGlInterface.CreateWebGl");
            Console.WriteLine(grGlInterface.Handle.ToString());
            GRContext grContext = GRContext.CreateGl(grGlInterface);
            if (grContext.Handle == IntPtr.Zero)
            {
                Console.WriteLine("GRContext.CreateGl failed");
            }
            else
            {
                Console.WriteLine("GRContext.CreateGl");
            }
            var grContext = GRContext.CreateGl();
            int samples;
            glGetIntegerv(0x80A9, &samples); // GL_SAMPLES
            Console.WriteLine("Samples");
            Console.WriteLine(samples.ToString());

            int stencils;
            glGetIntegerv(0x0D57, &stencils); // GL_STENCIL_BITS
            Console.WriteLine("Stencils");
            Console.WriteLine(stencils.ToString());
#endif

            const SKColorType colorType = SKColorType.Rgba8888;
            int width, height;
            fixed (byte* n = &(s[0]))
            {
                emscripten_get_canvas_element_size(n, &width, &height);
                Console.WriteLine("emscripten_get_canvas_element_size");
                Console.WriteLine(width + "," + height);
            }

            _sWidth = width;
            _sHeight = height;

#if WEBGL
            var info = new GRGlFramebufferInfo(0, colorType.ToGlSizedFormat());
            Console.WriteLine("GRGlFramebufferInfo");
            var backendRenderTarget = new GRBackendRenderTarget(width, height, 1 /*samples */, stencils, info);
            if (backendRenderTarget.Handle == IntPtr.Zero)
            {
                Console.WriteLine("failed to create GRBackendRenderTarget ");
            }
            if (!backendRenderTarget.IsValid)
            {
                Console.WriteLine("backendRenderTarget is not valid ");
            }
            else
            {
                Console.WriteLine("backendRenderTarget is valid ");
            }
            // WebGL should already be clearing the color and stencil buffers, but do it again here to
            // ensure Skia receives them in the expected state.
            // glBindFramebuffer(0x8D40  /* GL_FRAMEBUFFER */, 0);
            //glClearColor(0, 0, 0, 0);
            //          glClearStencil(0);
            //        glClear(0x00004000 /* GL_COLOR_BUFFER_BIT */ | 0x00000400 /* GL_STENCIL_BUFFER_BIT*/ );
            //  grContext.ResetContext(GRGlBackendState.RenderTarget | GRGlBackendState.Misc);
            // Console.WriteLine("context reset");

#endif

#if WEBGL
            surface = SKSurface.Create(grContext, backendRenderTarget, colorType);
#else
            _surface = SKSurface.Create(new SKImageInfo(width, height, colorType));
#endif
            if (_surface != null)
            {
                Console.WriteLine("got a surface");
            }
            else
            {
                Console.WriteLine("Failed to create surface");
                return;
            }

            copyToCanvas((byte*)_surface.PeekPixels().GetPixels(), width, height);
            emscripten_set_main_loop(&MainLoop, 0, 0);
        }
    }
}
