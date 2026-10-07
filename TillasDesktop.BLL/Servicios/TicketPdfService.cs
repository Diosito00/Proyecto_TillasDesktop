using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TillasDesktop.Entities.Facturacion;


namespace TillasDesktop.BLL.Servicios
{
    public class TicketPdfService
    {
        public void GenerarYGuardarTicket(string vendedor, string cliente, string metodoPago, decimal total, List<ItemCarritoDTO> carrito)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string carpetaTickets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
            if (!Directory.Exists(carpetaTickets)) Directory.CreateDirectory(carpetaTickets);

            string nombreArchivo = $"Ticket_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(carpetaTickets, nombreArchivo);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(4, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                    page.Content().Column(col =>
                    {
                        col.Item().Text("TILLAS DESKTOP").Bold().FontSize(14).AlignCenter();
                        col.Item().Text("Venta de Calzado").FontSize(9).AlignCenter();
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH.mm}");
                        col.Item().Text($"Cajero: {vendedor}");
                        col.Item().Text($"Cliente: {cliente}");
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(15);
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Text("C").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().AlignRight().Text("Subtotal").Bold();
                            });

                            foreach (var item in carrito)
                            {
                                decimal subtotal = item.Cantidad * item.Precio_Unitario;

                                tabla.Cell().Text(item.Cantidad.ToString());
                                tabla.Cell().Text($"{item.Nombre} (T:{item.Talle})");
                                tabla.Cell().AlignRight().Text(subtotal.ToString("C"));
                            }
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().AlignRight().Text($"TOTAL: {total:C}").Bold().FontSize(12);
                        col.Item().Text($"Pago: {metodoPago}");
                        col.Item().PaddingTop(10).Text("¡Gracias por su compra!").AlignCenter().Bold();                        
                    });
                });
            }).GeneratePdf(rutaCompleta);

            Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
        }
    }
}
