using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// サムネイルイメージを作成するクラス
    /// </summary>
    static class ThumbnailModule
    {
        /// <summary>
        /// 引数に与えられたファイルパスの画像を縦横比を固定して返します。
        /// </summary>
        /// <param name="path">サムネイル画像のファイルパス</param>
        /// <param name="width">サムネイルの横幅</param>
        /// <param name="height">サムネイルの縦幅</param>
        /// <returns>引数に指定した大きさのサムネイル画像</returns>
        public static Image CreateThumbnail(string path, Size imgSize)
        {
            Bitmap originalBitmap = new Bitmap(path);
            //縦横比の計算
            int x, y;
            double w = (double)imgSize.Width / originalBitmap.Width;
            double h = (double)imgSize.Height / originalBitmap.Height;
            if (w <= h)
            {
                x = imgSize.Width;
                y = (int)(imgSize.Width * (w / h));
            }
            else
            {
                x = (int)(imgSize.Height * (h / w));
                y = imgSize.Height;
            }

            //描画位置を計算
            int sx = (imgSize.Width - x) / 2;
            int sy = (imgSize.Height - y) / 2;

            //imagelistに合わせたサムネイルを描画
            Bitmap bitmap = new Bitmap(imgSize.Width, imgSize.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(originalBitmap, sx, sy, x, y);

            return bitmap;
        }

        /// <summary>
        /// 引数に与えられたファイルパスの画像を縦横比を固定して返します。
        /// </summary>
        /// <param name="path">サムネイル画像のファイルパス</param>
        /// <param name="width">サムネイルの横幅</param>
        /// <param name="height">サムネイルの縦幅</param>
        /// <returns>引数に指定した大きさのサムネイル画像</returns>
        public static ImageSource CreateThumbnailImageSource(string path, Size imgSize)
        {
            Bitmap bitmap = (Bitmap)CreateThumbnail(path, imgSize);
            using (Stream st = new MemoryStream())
            {
                bitmap.Save(st, System.Drawing.Imaging.ImageFormat.Png);
                st.Seek(0, SeekOrigin.Begin);
                return BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}
