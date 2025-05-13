using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;
using QRCodeApi.Services;


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
            
            public bool TransparentBackground { get; set; } = false;
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

            var qrBitmap = QrCodeBuilder
                .Create(request.Url)
                .WithPixelSize(request.PixelSize)
                .WithColors(request.ForegroundColor, request.BackgroundColor)
                .WithBubbleStyle(request.Bubble)
                .WithLogo(request.LogoBase64)
                .WithTransparentBackground(request.TransparentBackground) 
                .Build();


            using var outputStream = new MemoryStream();
            qrBitmap.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
            string base64 = Convert.ToBase64String(outputStream.ToArray());

            return Ok(new QrResponse { Base64Png = base64 });
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