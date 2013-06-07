using System;
using System.Globalization;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TimeRecorder.Main
{
    public static class TimeRecordExtensions
    {
        public static Row ToRow(this TimeRecord record, DateTime startTime)
        {
            var idCell = new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(record.Id.ToString(CultureInfo.InvariantCulture)),
            };

            var timeCell = new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(record.FullTime.ToString(Configuration.TimeFormat)),
            };

            var str = record.FullTime - startTime;
            var deltaCell = new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(str.ToString()),
            };

            var numberCell = new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(record.Number.ToString(CultureInfo.InvariantCulture)),
            };

            var row = new Row();
            row.Append(idCell, timeCell, numberCell, deltaCell);

            if (record.IsPossiblyWrong)
            {
                var isPossblyWrongCell = new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue("The number may be wrong!"),
                };

                row.Append(isPossblyWrongCell);
            }

            return row;
        }
    }
}