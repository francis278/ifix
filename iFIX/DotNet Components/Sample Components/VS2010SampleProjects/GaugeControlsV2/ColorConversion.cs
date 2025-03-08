using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GaugeControlsV2
{
    public class ColorConversion
    {
        // Convert the Hue-saturation-Lightness format to an RGB color
        public static Color HSLToColor(double hue, double sat, double lum)
        {
            hue = Math.Min(hue, 1);
            sat = Math.Min(sat, 1);
            lum = Math.Min(lum, 1);

            double r = 0, g = 0, b = 0;
            double temp1, temp2;

            if (lum == 0)
                r = g = b = 0;
            else
            {
                if (sat == 0)
                    r = g = b = lum;
                else
                {
                    temp2 = lum <= 0.5 ? lum * (1.0 + sat) : lum + sat - lum * sat;
                    temp1 = 2.0 * lum - temp2;
                    double[] t3 = new double[] { hue + 1.0 / 3.0, hue, hue - 1.0 / 3.0 };
                    double[] clr = new double[] { 0, 0, 0 };

                    for (int i = 0; i < 3; i++)
                    {
                        if (t3[i] < 0)
                            t3[i] += 1.0;

                        if (t3[i] > 1)
                            t3[i] -= 1.0;

                        if (6.0 * t3[i] < 1.0)
                            clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
                        else if (2.0 * t3[i] < 1.0)
                            clr[i] = temp2;
                        else if (3.0 * t3[i] < 2.0)
                            clr[i] = temp1 + (temp2 - temp1) * (2.0 / 3.0 - t3[i]) * 6.0;
                        else
                            clr[i] = temp1;
                    }

                    r = clr[0];
                    g = clr[1];
                    b = clr[2];
                }
            }

            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        // Convert an RGB color to the Hue-saturation-Lightness format
        public static double[] ColorToHSL(Color c)
        {
            return new double[] { c.GetHue() / 360.0, c.GetSaturation(), c.GetBrightness() };
        } 
    }
}
