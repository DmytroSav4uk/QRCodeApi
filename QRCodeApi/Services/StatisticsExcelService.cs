using ClosedXML.Excel;
using QRCodeApi.Controllers;


namespace QRCodeApi.Services
{
    public static class StatisticsExcelService
    {
        private static readonly string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "statistics");

        public static void SaveRecordsExcel(List<StatisticsController.StatisticRecord> statistics)
        {
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, "StatisticsRecords.xlsx");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Statistics");

          
            worksheet.Cell(1, 1).Value = "BackgroundColor";
            worksheet.Cell(1, 2).Value = "Bubble";
            worksheet.Cell(1, 3).Value = "Timestamp";

            var headerRange = worksheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

         
            for (int i = 0; i < statistics.Count; i++)
            {
                var record = statistics[i];
                worksheet.Cell(i + 2, 1).Value = record.BackgroundColor;
                worksheet.Cell(i + 2, 2).Value = record.Bubble ? "Yes" : "No";
                worksheet.Cell(i + 2, 3).Value = record.Timestamp;

             
                worksheet.Cell(i + 2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(i + 2, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(i + 2, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

              
                worksheet.Cell(i + 2, 3).Style.DateFormat.Format = "yyyy-mm-dd HH:mm:ss";
            }

       
            var dataRange = worksheet.Range(1, 1, statistics.Count + 1, 3);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

           
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        public static void SaveSummaryExcel(List<StatisticsController.StatisticRecord> statistics)
        {
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, "StatisticsSummary.xlsx");

            if (statistics.Count == 0) return;

            var topColors = statistics
                .GroupBy(s => s.BackgroundColor)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            double bubblePercentage = (statistics.Count(s => s.Bubble) / (double)statistics.Count) * 100;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Summary");

      
            worksheet.Cell(1, 1).Value = "Top Colors";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;

           
            worksheet.Cell(2, 1).Value = "Color";
            worksheet.Cell(2, 2).Value = "Count";

            var headerRange = worksheet.Range(2, 1, 2, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

         
            for (int i = 0; i < topColors.Count; i++)
            {
                worksheet.Cell(i + 3, 1).Value = topColors[i].Color;
                worksheet.Cell(i + 3, 2).Value = topColors[i].Count;

                worksheet.Cell(i + 3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(i + 3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

          
            var topColorsRange = worksheet.Range(2, 1, topColors.Count + 2, 2);
            topColorsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            topColorsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

          
            worksheet.Cell(10, 1).Value = "Bubble Usage %";
            worksheet.Cell(10, 1).Style.Font.Bold = true;
            worksheet.Cell(10, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            worksheet.Cell(10, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(10, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(10, 2).Value = Math.Round(bubblePercentage, 2);
            worksheet.Cell(10, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(10, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Column(2).Style.NumberFormat.Format = "0.00";

           
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
    }
}
