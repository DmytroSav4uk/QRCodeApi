using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using ZXing.Windows.Compatibility;
using QRCodeApi.Services;
using ZXing;
using ZXing.Common;
using Size = System.Drawing.Size;
using OpenCvSharp.Extensions;

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
        
        // adapter
        
        [HttpPost("readCode")]
        public IActionResult ReadQrCode([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image is required.");

            using var stream = image.OpenReadStream();
            using var bitmap = new Bitmap(stream);

          
            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryInverted = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(bitmap);

           
            if (result == null)
            {
                var processedBitmap = ApplyMorphologicalAdapter(bitmap);
                result = reader.Decode(processedBitmap);
            }

            if (result == null)
                return NotFound("QR code could not be read.");

            return Ok(new QrReadResponse { Text = result.Text });
        }

        private Bitmap ApplyMorphologicalAdapter(Bitmap original)
        {
            Mat mat = BitmapConverter.ToMat(original);
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(mat, mat, 128, 355, ThresholdTypes.Binary);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.Dilate(mat, mat, kernel);
            return BitmapConverter.ToBitmap(mat);
        }

    }
}