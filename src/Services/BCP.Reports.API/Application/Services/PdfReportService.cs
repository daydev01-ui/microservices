namespace BCP.Reports.API.Application.Services;

using BCP.Reports.API.Application.DTOs;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

public class PdfReportService
{
    private static readonly Color ColorBCP = new DeviceRgb(0, 48, 135);
    private static readonly Color ColorBCPRed = new DeviceRgb(227, 6, 19);
    private static readonly Color ColorGray = new DeviceRgb(240, 240, 240);

    public byte[] GenerarReporteTransacciones(
        GenerarReporteRequest request,
        List<TransaccionReporte> transacciones)
    {
        using var ms = new MemoryStream();
        var writer = new PdfWriter(ms);
        var pdf = new PdfDocument(writer);
        var doc = new Document(pdf);

        // Encabezado
        doc.Add(new Paragraph("BANCO DE CRÉDITO DE BOLIVIA S.A.")
            .SetFontColor(ColorBCP)
            .SetFontSize(18)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER));

        doc.Add(new Paragraph($"Reporte de {request.Tipo}")
            .SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER));

        doc.Add(new Paragraph($"Período: {request.PeriodoInicio:dd/MM/yyyy} — {request.PeriodoFin:dd/MM/yyyy}")
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(new DeviceRgb(100, 100, 100)));

        // Línea separadora roja BCP
        doc.Add(new LineSeparator(new iText.Kernel.Geom.PageSize(595, 842))
            .SetStrokeColor(ColorBCPRed)
            .SetStrokeWidth(2));

        // Resumen estadístico
        doc.Add(new Paragraph("RESUMEN ESTADÍSTICO")
            .SetFontSize(12).SetBold().SetFontColor(ColorBCP)
            .SetMarginTop(15));

        var confirmadas = transacciones.Count(t => t.Estado == "Confirmada");
        var monto = transacciones.Where(t => t.Estado == "Confirmada").Sum(t => t.Monto);

        var tablaResumen = new Table(2).UseAllAvailableWidth();
        AgregarFilaResumen(tablaResumen, "Total Transacciones", transacciones.Count.ToString());
        AgregarFilaResumen(tablaResumen, "Confirmadas", confirmadas.ToString());
        AgregarFilaResumen(tablaResumen, "Rechazadas", transacciones.Count(t => t.Estado == "Rechazada").ToString());
        AgregarFilaResumen(tablaResumen, "Monto Total Cobrado", $"Bs. {monto:N2}");
        doc.Add(tablaResumen);

        // Tabla de transacciones
        doc.Add(new Paragraph("DETALLE DE TRANSACCIONES")
            .SetFontSize(12).SetBold().SetFontColor(ColorBCP)
            .SetMarginTop(20));

        var tabla = new Table(new float[] { 2, 3, 2, 2, 2 }).UseAllAvailableWidth();
        var headers = new[] { "Referencia", "Ref. BCP", "Monto (Bs.)", "Estado", "Fecha" };

        foreach (var h in headers)
        {
            tabla.AddHeaderCell(new Cell().Add(new Paragraph(h).SetBold().SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(ColorBCP).SetPadding(5));
        }

        for (int i = 0; i < transacciones.Count; i++)
        {
            var t = transacciones[i];
            var bg = i % 2 == 0 ? ColorGray : ColorConstants.WHITE;

            tabla.AddCell(CeldaEstilo(t.ReferenciaCliente, bg));
            tabla.AddCell(CeldaEstilo(t.ReferenciaExterna ?? "-", bg));
            tabla.AddCell(CeldaEstilo($"{t.Monto:N2}", bg));
            tabla.AddCell(CeldaEstilo(t.Estado, bg,
                t.Estado == "Confirmada" ? new DeviceRgb(0, 128, 0) :
                t.Estado == "Rechazada" ? new DeviceRgb(180, 0, 0) : null));
            tabla.AddCell(CeldaEstilo(t.FechaHora.ToString("dd/MM HH:mm"), bg));
        }

        doc.Add(tabla);

        // Pie de página
        doc.Add(new Paragraph($"\nGenerado el {DateTime.Now:dd/MM/yyyy HH:mm} | Sistema BCP QR Cobros")
            .SetFontSize(8).SetFontColor(new DeviceRgb(150, 150, 150))
            .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(20));

        doc.Close();
        return ms.ToArray();
    }

    private static void AgregarFilaResumen(Table tabla, string etiqueta, string valor)
    {
        tabla.AddCell(new Cell().Add(new Paragraph(etiqueta).SetBold()).SetPadding(4));
        tabla.AddCell(new Cell().Add(new Paragraph(valor)).SetPadding(4));
    }

    private static Cell CeldaEstilo(string texto, Color bg, Color? fontColor = null)
    {
        var p = new Paragraph(texto).SetFontSize(9);
        if (fontColor != null) p.SetFontColor(fontColor);
        return new Cell().Add(p).SetBackgroundColor(bg).SetPadding(4);
    }
}
