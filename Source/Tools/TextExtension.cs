using System.Globalization;
using System.Windows;
using System.Windows.Media;


/* 
 * ################################################################################################
 * 
 * Source  : https://gist.github.com/badamczewski/06d9c86e6d78fc79905f943cb3545f51
 * 
 *              Some parts of the code have been removed or rewritten
 *           
 * ################################################################################################
 */


namespace LanguageTrainer.Source.Tools
{
    public static class TextExtension
    {
        public static PathGeometry ToPathGeometry(this string text, string fontName = "Nexa Bold", double fontSize = 42)
        {
            var formattedText = new FormattedText(
                textToFormat    : text,
                culture         : CultureInfo.InvariantCulture,
                flowDirection   : FlowDirection.LeftToRight,
                typeface        : new Typeface(
                    fontFamily  : new FontFamily(fontName),
                    style       : FontStyles.Normal,
                    weight      : FontWeights.Normal,
                    stretch     : FontStretches.Normal
                ),
                emSize          : fontSize,
                foreground      : Brushes.White,
                pixelsPerDip    : 100
            );

            var geometry        = formattedText.BuildGeometry(new Point(0, 0));
            var pathGeometry    = geometry.GetFlattenedPathGeometry();

            return pathGeometry.Clone();
        }
    }

}
