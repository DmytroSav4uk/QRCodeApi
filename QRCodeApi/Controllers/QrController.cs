using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;


namespace QRCodeApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class QrController : ControllerBase
    {
        public class QrRequest
        {
            public string Url { get; set; }
            public int PixelSize { get; set; } = 20;
            public string ForegroundColor { get; set; } = "#000000";
            public string BackgroundColor { get; set; } = "#FFFFFF";
            public string? LogoBase64 { get; set; }
            public bool Bubble { get; set; } = false; 
        }



        public class QrResponse
        {
            public string Base64Png { get; set; }
        }

        public class QrReadResponse
        {
            public string Text { get; set; }
        }
        
        
      
        [HttpPost("generateCode")]
        public IActionResult GenerateQrCode([FromBody] QrRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("URL is required.");

            Bitmap qrBitmap = request.Bubble
                ? GenerateBubbleQrCode(request)
                : GenerateStandardQrCode(request);

            if (!string.IsNullOrWhiteSpace(request.LogoBase64))
            {
                byte[] logoBytes = Convert.FromBase64String(request.LogoBase64);
                using var logoStream = new MemoryStream(logoBytes);
                using var logo = new Bitmap(logoStream);

                int logoSize = qrBitmap.Width / 5;
                using var resizedLogo = new Bitmap(logo, new Size(logoSize, logoSize));

                using Graphics g = Graphics.FromImage(qrBitmap);
                int x = (qrBitmap.Width - resizedLogo.Width) / 2;
                int y = (qrBitmap.Height - resizedLogo.Height) / 2;
                g.DrawImage(resizedLogo, x, y, resizedLogo.Width, resizedLogo.Height);
            }

            using var outputStream = new MemoryStream();
            qrBitmap.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
            string base64 = Convert.ToBase64String(outputStream.ToArray());

            return Ok(new QrResponse { Base64Png = base64 });
        }

        
        
        private Bitmap GenerateStandardQrCode(QrRequest request)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(request.Url, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrData);
            byte[] pngBytes = qrCode.GetGraphic(request.PixelSize);

            using var ms = new MemoryStream(pngBytes);
            using var qrBitmap = new Bitmap(ms);

            return RecolorBitmap(qrBitmap, request.ForegroundColor, request.BackgroundColor);
        }
        private Bitmap GenerateBubbleQrCode(QrRequest request)
        {
            using var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(request.Url, QRCodeGenerator.ECCLevel.Q);
            var modules = qrData.ModuleMatrix;

            int moduleSize = request.PixelSize;
            int qrSize = modules.Count;
            int imgSize = qrSize * moduleSize;

            var fgColor = ColorTranslator.FromHtml(request.ForegroundColor);
            var bgColor = ColorTranslator.FromHtml(request.BackgroundColor);

            var bitmap = new Bitmap(imgSize, imgSize);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(bgColor);

            using var brush = new SolidBrush(fgColor);

            for (int y = 0; y < qrSize; y++)
            {
                for (int x = 0; x < qrSize; x++)
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
        
        private static Bitmap RecolorBitmap(Bitmap original, string fgHex, string bgHex)
        {
            Color fgColor = ColorTranslator.FromHtml(fgHex);
            Color bgColor = ColorTranslator.FromHtml(bgHex);

            Bitmap recolored = new Bitmap(original.Width, original.Height);
            using Graphics g = Graphics.FromImage(recolored);
            g.Clear(bgColor);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    {
                        recolored.SetPixel(x, y, fgColor);
                    }
                }
            }

            return recolored;
        }


        
        
        [HttpPost("readCode")]
        public IActionResult ReadQrCode([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image is required.");

            using var stream = image.OpenReadStream();
            using var bitmap = new Bitmap(stream);

            var reader = new BarcodeReader(); 
            var result = reader.Decode(bitmap);

            if (result == null)
                return NotFound("QR code could not be read.");

            return Ok(new QrReadResponse { Text = result.Text });
        }

        
    }
}