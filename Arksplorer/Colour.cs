using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Arksplorer
{
    public static class Colour
    {
        /// <summary>
        /// Converts % HSL to RGB
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="luminance"></param>
        /// <returns></returns>
        public static Color RGBFromHSL(double hue, double saturation, double luminance)
        {
            double r, g, b;
            double t1, t2;
            double tR, tG, tB;
            byte byteR, byteG, byteB;

            hue *= 360.0;

            if (saturation == 0.0)
            {
                byteR = (byte)(luminance * 255.0);
                return Color.FromRgb(byteR, byteR, byteR);
            }

            if (luminance < 0.5)
                t1 = luminance * (saturation + 1.0);
            else
                t1 = luminance + saturation - luminance * saturation;

            t2 = 2.0 * luminance - t1;

            hue /= 360.0;
            // We also clamp/wrap colours to make sure they remain in a 0-1 range
            tR = hue + 0.33333333;
            tG = hue;
            tB = hue - 0.33333333;

            if (tR < 0.0)
                tR += 1.0;
            else if (tR > 1.0)
                tR -= 1.0;

            if (tG < 0.0)
                tG += 1.0;
            else if (tG > 1.0)
                tG -= 1.0;
            
            if (tB < 0.0)
                tB += 1.0;
            else if (tB > 1.0)
                tB -= 1.0;

            if ((tR * 6.0) < 1.0)
                r = t2 + (t1 - t2) * 6.0 * tR;
            else
            {
                if ((tR * 2.0) < 1.0)
                    r = t1;
                else
                {
                    if ((tR * 3) < 2.0)
                        r = t2 + (t1 - t2) * (0.66666666 - tR) * 6;
                    else
                        r = t2;
                }
            }

            if ((tG * 6.0) < 1.0)
                g = t2 + (t1 - t2) * 6.0 * tG;
            else
            {
                if ((tG * 2.0) < 1.0)
                    g = t1;
                else
                {
                    if ((tG * 3) < 2.0)
                        g = t2 + (t1 - t2) * (0.66666666 - tG) * 6;
                    else
                        g = t2;
                }
            }

            if ((tB * 6.0) < 1.0)
                b = t2 + (t1 - t2) * 6 * tB;
            else
            {
                if ((tB * 2.0) < 1.0)
                    b = t1;
                else
                {
                    if ((tB * 3) < 2.0)
                        b = t2 + (t1 - t2) * (0.66666666 - tB) * 6;
                    else
                        b = t2;
                }
            }

            byteR = (byte)(r * 255);
            byteG = (byte)(g * 255);
            byteB = (byte)(b * 255);

            return Color.FromRgb(byteR, byteG, byteB);
        }
    }
}
