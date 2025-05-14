using Microsoft.AspNetCore.Mvc;
using QRCodeApi.Services;

namespace QRCodeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        public class StatisticsDto
        {
            public string BackgroundColor { get; set; } = "#FFFFFF";
            public bool Bubble { get; set; } = false;
            public DateTime Timestamp { get; set; }
        }

        public class StatisticRecord
        {
            public string BackgroundColor { get; set; }
            public bool Bubble { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private static readonly List<StatisticRecord> _statistics = new();

        [HttpPost("SendStatistics")]
        public IActionResult SendStatistics([FromBody] StatisticsDto statisticsDto)
        {
            var record = new StatisticRecord
            {
                BackgroundColor = statisticsDto.BackgroundColor,
                Bubble = statisticsDto.Bubble,
                Timestamp = statisticsDto.Timestamp
            };

            _statistics.Add(record);

            StatisticsExcelService.SaveRecordsExcel(_statistics);
            StatisticsExcelService.SaveSummaryExcel(_statistics);

            return Ok(new { message = "Statistics saved and Excel files generated." });
        }
    }
}