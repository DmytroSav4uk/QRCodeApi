using ClosedXML.Excel;
using QRCodeApi.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            for (int i = 0; i < statistics.Count; i++)
            {
                var record = statistics[i];
                worksheet.Cell(i + 2, 1).Value = record.BackgroundColor;
                worksheet.Cell(i + 2, 2).Value = record.Bubble;
                worksheet.Cell(i + 2, 3).Value = record.Timestamp;
            }

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
            worksheet.Cell(2, 1).Value = "Color";
            worksheet.Cell(2, 2).Value = "Count";

            for (int i = 0; i < topColors.Count; i++)
            {
                worksheet.Cell(i + 3, 1).Value = topColors[i].Color;
                worksheet.Cell(i + 3, 2).Value = topColors[i].Count;
            }

            worksheet.Cell(10, 1).Value = "Bubble Usage %";
            worksheet.Cell(10, 2).Value = bubblePercentage;

            workbook.SaveAs(filePath);
        }
    }
}
