namespace BCP.Reports.API.Application.Services;

using BCP.Reports.API.Application.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

public class ExcelReportService
{
    public byte[] GenerarReporteTransacciones(
        GenerarReporteRequest request,
        List<TransaccionReporte> transacciones)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();

        // Hoja 1: Resumen
        var wsResumen = package.Workbook.Worksheets.Add("Resumen");
        ConfigurarHojaResumen(wsResumen, request, transacciones);

        // Hoja 2: Detalle de transacciones
        var wsDetalle = package.Workbook.Worksheets.Add("Transacciones");
        ConfigurarHojaDetalle(wsDetalle, transacciones);

        return package.GetAsByteArray();
    }

    private static void ConfigurarHojaResumen(
        ExcelWorksheet ws,
        GenerarReporteRequest request,
        List<TransaccionReporte> transacciones)
    {
        var colorBCP = Color.FromArgb(0, 48, 135); // BCP Blue #003087
        var colorBCPRed = Color.FromArgb(227, 6, 19); // BCP Red #E30613

        // Encabezado
        ws.Cells["A1:F1"].Merge = true;
        ws.Cells["A1"].Value = "BANCO DE CRÉDITO DE BOLIVIA S.A.";
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Size = 16;
        ws.Cells["A1"].Style.Font.Color.SetColor(colorBCP);
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A2:F2"].Merge = true;
        ws.Cells["A2"].Value = $"Reporte de {request.Tipo}";
        ws.Cells["A2"].Style.Font.Size = 13;
        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A3"].Value = "Período:";
        ws.Cells["B3"].Value = $"{request.PeriodoInicio:dd/MM/yyyy} - {request.PeriodoFin:dd/MM/yyyy}";
        ws.Cells["A4"].Value = "Generado:";
        ws.Cells["B4"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Línea separadora
        ws.Cells["A5:F5"].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        ws.Cells["A5:F5"].Style.Border.Bottom.Color.SetColor(colorBCPRed);

        // Métricas resumen
        ws.Cells["A7"].Value = "RESUMEN ESTADÍSTICO";
        ws.Cells["A7"].Style.Font.Bold = true;
        ws.Cells["A7"].Style.Font.Size = 12;
        ws.Cells["A7"].Style.Font.Color.SetColor(colorBCP);

        var confirmadas = transacciones.Count(t => t.Estado == "Confirmada");
        var rechazadas = transacciones.Count(t => t.Estado == "Rechazada");
        var monto = transacciones.Where(t => t.Estado == "Confirmada").Sum(t => t.Monto);

        var metricas = new[]
        {
            ("Total Transacciones", transacciones.Count.ToString()),
            ("Confirmadas", confirmadas.ToString()),
            ("Rechazadas", rechazadas.ToString()),
            ("Monto Total Cobrado", $"Bs. {monto:N2}"),
            ("Promedio por Transacción", confirmadas > 0 ? $"Bs. {monto / confirmadas:N2}" : "Bs. 0.00"),
            ("Tasa de Éxito", transacciones.Count > 0 ? $"{(double)confirmadas / transacciones.Count:P1}" : "0%")
        };

        for (int i = 0; i < metricas.Length; i++)
        {
            ws.Cells[8 + i, 1].Value = metricas[i].Item1;
            ws.Cells[8 + i, 1].Style.Font.Bold = true;
            ws.Cells[8 + i, 2].Value = metricas[i].Item2;
        }

        ws.Column(1).Width = 30;
        ws.Column(2).Width = 25;
    }

    private static void ConfigurarHojaDetalle(
        ExcelWorksheet ws,
        List<TransaccionReporte> transacciones)
    {
        var colorBCP = Color.FromArgb(0, 48, 135);
        var headers = new[] { "ID Transacción", "Referencia Cliente", "Ref. Externa BCP",
            "Monto (Bs.)", "Estado", "Cod. Autorización", "Fecha y Hora" };

        // Encabezados de columna
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(colorBCP);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.White);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Datos
        var colorFila = new[] { Color.White, Color.FromArgb(240, 244, 252) };
        for (int i = 0; i < transacciones.Count; i++)
        {
            var t = transacciones[i];
            var row = i + 2;
            var bgColor = colorFila[i % 2];

            ws.Cells[row, 1].Value = t.IdTransaccion.ToString()[..8] + "...";
            ws.Cells[row, 2].Value = t.ReferenciaCliente;
            ws.Cells[row, 3].Value = t.ReferenciaExterna ?? "-";
            ws.Cells[row, 4].Value = t.Monto;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 5].Value = t.Estado;
            ws.Cells[row, 6].Value = t.CodigoAutorizacion ?? "-";
            ws.Cells[row, 7].Value = t.FechaHora.ToString("dd/MM/yyyy HH:mm:ss");

            // Color estado
            var estadoCell = ws.Cells[row, 5];
            estadoCell.Style.Font.Color.SetColor(t.Estado switch
            {
                "Confirmada" => Color.FromArgb(0, 128, 0),
                "Rechazada" => Color.FromArgb(180, 0, 0),
                _ => Color.FromArgb(180, 120, 0)
            });
            estadoCell.Style.Font.Bold = true;

            for (int col = 1; col <= 7; col++)
            {
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(bgColor);
            }
        }

        // Auto ajuste de columnas
        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        ws.Cells[ws.Dimension.Address].Style.Border.BorderAround(ExcelBorderStyle.Medium, colorBCP);
    }
}
