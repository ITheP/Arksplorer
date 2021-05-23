using System;
using System.Windows.Media;

namespace Arksplorer
{
    public static class ColorHelper
    {
        /// <summary>
        /// Converts % HSL to Color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="luminance"></param>
        /// <returns></returns>
        public static Color HSLToColor(double hue, double saturation, double luminance)
        {
            double t1, t2;
            double r, g, b;
            byte byteR, byteG, byteB;

            // make sure hue is always 0->1
            hue -= Math.Floor(hue);

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
            r = hue + 0.33333333;
            g = hue;
            b = hue - 0.33333333;

            byteR = CalcComponent(t1, t2, r);
            byteG = CalcComponent(t1, t2, g);
            byteB = CalcComponent(t1, t2, b);

            return Color.FromRgb(byteR, byteG, byteB);
        }

        private static byte CalcComponent(double t1, double t2, double component)
        {

            // We also clamp/wrap colours to make sure they remain in a 0-1 range
            if (component < 0.0)
                component += 1.0;
            else if (component > 1.0)
                component -= 1.0;

            double c;

            if ((component * 6.0) < 1.0)
                c = t2 + (t1 - t2) * 6 * component;
            else
            {
                if ((component * 2.0) < 1.0)
                    c = t1;
                else
                {
                    if ((component * 3) < 2.0)
                        c = t2 + (t1 - t2) * (0.66666666 - component) * 6;
                    else
                        c = t2;
                }
            }

            return (byte)(c * 255);
        }

        /// <summary>
        /// Convery RGB to HSL
        /// </summary>
        /// <param name="r">0.0 to 1.0</param>
        /// <param name="g">0.0 to 1.0</param>
        /// <param name="b">0.0 to 1.0</param>
        /// <param name="h">Resulting Hue</param>
        /// <param name="s">Resulting Saturdation</param>
        /// <param name="l">Resulting Luminance</param>
        public static void RGBToHSL(int r, int g, int b, out double h, out double s, out double l)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double tR = r / 255.0;
            double tG = g / 255.0;
            double tB = b / 255.0;

            double min = tR;
            if (tG < min)
                min = tG;
            if (tB < min) min = tB;

            double max = tR;
            if (tG > max)
                max = tG;
            if (tB > max)
                max = tB;

            double diff = max - min;
            l = (max + min) / 2;

            if (Math.Abs(diff) < 0.00001)
            {
                s = 0.0d;
                h = 0.0d;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5d)
                    s = diff / (max + min);
                else
                    s = diff / (2.0 - max - min);

                double rDist = (max - tR) / diff;
                double gDist = (max - tG) / diff;
                double bDist = (max - tB) / diff;

                if (tR == max)
                    h = bDist - gDist;
                else if (tG == max)
                    h = 2 + rDist - bDist;
                else
                    h = 4 + gDist - rDist;

                h *= 60;

                if (h < 0)
                    h += 360;
            }
        }

        ///// <summary>
        /////  Attempt to auto-sort colors into something vaugly nice, without getting too complicated.
        /////  Superceeded by just manually sorting color table into something nice.
        /////  Left here for future reference/possible changes
        ///// </summary>
        ///// <param name="arkColor"></param>
        ///// <returns></returns>
        //public static int SortKeyFromColor(ArkColor arkColor)
        //{
        //    if (arkColor == null || arkColor.Color == null)
        //        return -1;

        //    Color color = ((SolidColorBrush)arkColor.Color).Color;

        //    RGBToHSL(color.R, color.G, color.B, out double h, out double s, out double l);

        //    int result = ((int)h) << 16;
        //    result += ((int)(l * 255.0)) << 8;
        //    result += ((int)(s * 255.0));

        //    return result;
        //}

    }

}
