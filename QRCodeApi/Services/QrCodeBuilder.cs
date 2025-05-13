using QRCoder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace QRCodeApi.Services
{
    public class QrCodeBuilder
    {
        private string _url;
        private int _pixelSize = 20;
        private string _foregroundColor = "#000000";
        private string _backgroundColor = "#FFFFFF";
        private string _logoBase64 = null;
        private bool _useBubble = false;

        private QrCodeBuilder(string url)
        {
            _url = url;
        }

        public static QrCodeBuilder Create(string url) => new QrCodeBuilder(url);

        public QrCodeBuilder WithPixelSize(int size)
        {
            _pixelSize = size;
            return this;
        }

        private bool _transparentBackground = false;

        public QrCodeBuilder WithTransparentBackground(bool transparent)
        {
            _transparentBackground = transparent;
            return this;
        }

        public QrCodeBuilder WithColors(string fg, string bg)
        {
            _foregroundColor = fg;
            _backgroundColor = bg;
            return this;
        }

        public QrCodeBuilder WithLogo(string logoBase64)
        {
            _logoBase64 = logoBase64;
            return this;
        }

        public QrCodeBuilder WithBubbleStyle(bool useBubble)
        {
            _useBubble = useBubble;
            return this;
        }

        public Bitmap Build()
        {
            Bitmap qrBitmap = _useBubble ? GenerateBubbleQrCode() : GenerateStandardQrCode();

            if (!string.IsNullOrWhiteSpace(_logoBase64))
            {
                byte[] logoBytes = Convert.FromBase64String(_logoBase64);
                using var logoStream = new MemoryStream(logoBytes);
                using var logo = new Bitmap(logoStream);
                int logoSize = qrBitmap.Width / 5;
                using var resizedLogo = new Bitmap(logo, new Size(logoSize, logoSize));

                using Graphics g = Graphics.FromImage(qrBitmap);
                int x = (qrBitmap.Width - resizedLogo.Width) / 2;
                int y = (qrBitmap.Height - resizedLogo.Height) / 2;
                g.DrawImage(resizedLogo, x, y, resizedLogo.Width, resizedLogo.Height);
            }

            return qrBitmap;
        }

        private Bitmap GenerateStandardQrCode()
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(_url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] pngBytes = qrCode.GetGraphic(_pixelSize);

            using var ms = new MemoryStream(pngBytes);
            using var bitmap = new Bitmap(ms);

            return RecolorBitmap(bitmap, _foregroundColor, _backgroundColor, _transparentBackground);
        }

        private Bitmap GenerateBubbleQrCode()
        {
            using var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(_url, QRCodeGenerator.ECCLevel.Q);
            var modules = qrData.ModuleMatrix;
            int moduleSize = _pixelSize;
            int size = modules.Count * moduleSize;
            var bitmap = new Bitmap(size, size);
            var fgColor = ColorTranslator.FromHtml(_foregroundColor);
            var bgColor = ColorTranslator.FromHtml(_backgroundColor);

            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_transparentBackground ? Color.Transparent : bgColor);
            using var brush = new SolidBrush(fgColor);

            for (int y = 0; y < modules.Count; y++)
            {
                for (int x = 0; x < modules.Count; x++)
                {
                    if (modules[y][x])
                    {
                        int px = x * moduleSize;
                        int py = y * moduleSize;
                        g.FillEllipse(brush, px, py, moduleSize, moduleSize);
                    }
                }
            }

            return bitmap;
        }

        private Bitmap RecolorBitmap(Bitmap original, string fgHex, string bgHex, bool transparent)
        {
            var fg = ColorTranslator.FromHtml(fgHex);
            var bg = ColorTranslator.FromHtml(bgHex);

            var recolored = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(recolored);
    
            if (!transparent)
                g.Clear(bg);
            else
                g.Clear(Color.Transparent);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    var pixel = original.GetPixel(x, y);
                    if (pixel.R < 128)
                        recolored.SetPixel(x, y, fg);
                }
            }

            return recolored;
        }

    }
}
