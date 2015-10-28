/* UTF-8 string wrapper
 *
 * Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 */

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;

namespace OpenTK.Platform
{
    /// <summary>
    /// .NET representation of a UTF8 string. Mostly used for marshalling between .NET and UTF-8.
    /// </summary>
    public unsafe static class UTF8String
    {
        /// <summary>
        /// String instance corresponding to the null terminated bytes pointed by <paramref name="o"/>.
        /// </summary>
        /// <param name="o">Unmanaged pointer from which data will be read.</param>
        public static string String(byte * o)
        {
            if (o == null)
            {
                return null;
            }
            else
            {
                byte* ptr = (byte*)o;
                // Count number of available bytes.
                while (*ptr != 0) { ptr++; }
                    // `nb' contains the number of bytes not including the null terminating one.
                long nb = ptr - (byte*)o;
                if (nb > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("UTF-8 string too large");
                }
                return Encoding.UTF8.GetString((byte*) o, (int) nb);
            }
        }

        /// <summary>
        /// Overload of <see cref="String(byte *)"/>
        /// </summary>
        public static string String(IntPtr o)
        {
            return String((byte *)o);
        }

        /// <summary>
        /// String instance corresponding to the sequence of the first <paramref name="n"/> bytes pointed by <paramref name="o"/>.
        /// </summary>
        /// <param name="o">Unmanaged pointer from which data will be read.</param>
        /// <param name="n">Index from where to copy the data</param>
        public static string String(byte* o, int n)
        {
            return (o == null ? null : Encoding.UTF8.GetString(o, n));
        }

        /// <summary>
        /// Overload of <see cref="String(byte *, int)"/>
        /// </summary>
        public static string String(IntPtr o, int n)
        {
            return (o == IntPtr.Zero ? null : Encoding.UTF8.GetString((byte*)o, n));
        }
    }
}
