#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK.Graphics;

namespace OpenTK.Platform.Egl
{
    // Holds information about an EGL window.
    class EglWindowInfo : IWindowInfo
    {
        #region Fields

        IntPtr handle;
        IntPtr display;
        IntPtr surface;
        bool disposed;

        #endregion

        #region Constructors

        public EglWindowInfo(IntPtr handle, IntPtr display)
            : this(handle, display, IntPtr.Zero)
        {
        }

        public EglWindowInfo(IntPtr handle, IntPtr display, IntPtr surface)
        {
            Handle = handle;
            Surface = surface;

            if (display == IntPtr.Zero)
            {
                display = Egl.GetDisplay(IntPtr.Zero);
            }

            Display = display;

            int dummy_major, dummy_minor;
            if (!Egl.Initialize(Display, out dummy_major, out dummy_minor))
            {
                throw new GraphicsContextException(String.Format("Failed to initialize EGL, error {0}.", Egl.GetError()));
            }
        }

        #endregion

        #region Public Members

        public IntPtr Handle { get { return handle; } set { handle = value; } }

        public IntPtr Display { get { return display; } private set { display = value; } }

        public IntPtr Surface { get { return surface; } private set { surface = value; } }

        public void CreateWindowSurface(IntPtr config)
        {
            Surface = Egl.CreateWindowSurface(Display, config, Handle, IntPtr.Zero);
			if (Surface==IntPtr.Zero)
			{
                throw new GraphicsContextException(String.Format(
                    "[EGL] Failed to create window surface, error {0}.", Egl.GetError()));
			}
        }

        //public void CreatePixmapSurface(EGLConfig config)
        //{
        //    Surface = Egl.CreatePixmapSurface(Display, config, Handle, null);
        //}

        public void CreatePbufferSurface(IntPtr config)
        {
            // Extract info from current config
            int r, g, b, a;
            int d, s;
            int sample_buffers, samples;
            int renderable_flags;
            Egl.GetConfigAttrib(display, config, Egl.RED_SIZE, out r);
            Egl.GetConfigAttrib(display, config, Egl.GREEN_SIZE, out g);
            Egl.GetConfigAttrib(display, config, Egl.BLUE_SIZE, out b);
            Egl.GetConfigAttrib(display, config, Egl.ALPHA_SIZE, out a);
            Egl.GetConfigAttrib(display, config, Egl.DEPTH_SIZE, out d);
            Egl.GetConfigAttrib(display, config, Egl.STENCIL_SIZE, out s);
            Egl.GetConfigAttrib(display, config, Egl.SAMPLE_BUFFERS, out sample_buffers);
            Egl.GetConfigAttrib(display, config, Egl.SAMPLES, out samples);
            Egl.GetConfigAttrib(display, config, Egl.RENDERABLE_TYPE, out renderable_flags);

            // Fill attrib list for new config
            IntPtr[] configs = new IntPtr[1];
            int[] attribList = new int[]
            {
                Egl.SURFACE_TYPE, Egl.PBUFFER_BIT,
                Egl.RENDERABLE_TYPE, renderable_flags,

                Egl.RED_SIZE, r,
                Egl.GREEN_SIZE, g,
                Egl.BLUE_SIZE, b,
                Egl.ALPHA_SIZE, a,

                Egl.DEPTH_SIZE, d,
                Egl.STENCIL_SIZE, s,

                Egl.SAMPLE_BUFFERS, sample_buffers,
                Egl.SAMPLES, samples,

                Egl.NONE,
            };

            // Choose config
            int num_configs;
            if (!Egl.ChooseConfig(display, attribList, configs, configs.Length, out num_configs) || num_configs == 0)
            {
                throw new GraphicsModeException(String.Format("Failed to retrieve GraphicsMode, error {0}", Egl.GetError()));
            }

            // Create PBuffer
            var pbufferAttribList = new[]
            {
                Egl.WIDTH, 1,
                Egl.HEIGHT, 1,
                Egl.TEXTURE_TARGET, Egl.NO_TEXTURE,
                Egl.TEXTURE_FORMAT, Egl.NO_TEXTURE,
                Egl.NONE,
            };

            Surface = Egl.CreatePbufferSurface(Display, configs[0], pbufferAttribList);
        }

        public void DestroySurface()
        {
            if (Surface != IntPtr.Zero)
            {
                if (Egl.GetCurrentSurface(Egl.DRAW) == Surface)
                    Egl.MakeCurrent(Display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                if (!Egl.DestroySurface(Display, Surface))
                    Debug.Print("[Warning] Failed to destroy {0}:{1}.", Surface.GetType().Name, Surface);
                Surface = IntPtr.Zero;
            }
        }

        public void TerminateDisplay()
        {
            if (Display != IntPtr.Zero)
            {
                if (!Egl.Terminate(Display))
                    Debug.Print("[Warning] Failed to terminate display {0}.", Display);
                Display = IntPtr.Zero;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    DestroySurface();
                    disposed = true;
                }
                else
                {
                    Debug.Print("[Warning] Failed to destroy {0}:{1}.", this.GetType().Name, Handle);
                }
            }
        }

        ~EglWindowInfo()
        {
            Dispose(false);
        }

        #endregion
    }
}
