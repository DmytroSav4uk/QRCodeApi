using Microsoft.AspNetCore.Mvc;
using QRCoder;


namespace QRCodeApi.Controllers
{
    [ApiController]
    [Route("api/generateCode")]
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

        [HttpPost]
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
    }
}