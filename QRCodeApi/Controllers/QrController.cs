using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

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

            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(request.Url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrCodeAsPng = qrCode.GetGraphic(20);

            string base64 = Convert.ToBase64String(qrCodeAsPng);

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