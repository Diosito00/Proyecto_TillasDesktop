using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.BLL.Servicios
{
    // Servicio dedicado exclusivamente a la generación de comprobantes PDF.
    public class TicketPdfService
    {
        public void GenerarYGuardarTicket(string vendedor, string cliente, string metodoPago, decimal total, List<ItemCarritoDTO> carrito)
        {
            // Configuración obligatoria de QuestPDF para evitar la marca de agua y excepciones.
            // Se utiliza la licencia comunitaria gratuita (MIT) apta para empresas pequeñas.
            QuestPDF.Settings.License = LicenseType.Community;

            // Forzamos la configuración regional a Argentina ("es-AR").
            // Esto garantiza que el formateador "C" (Currency) siempre dibuje el símbolo "$" 
            // independientemente de si la computadora está configurada en España (€) o Estados Unidos (USD).
            var formatoMoneda = new CultureInfo("es-AR");

            // Manejo del sistema de archivos: Creamos la carpeta "Tickets" junto al ejecutable (.exe)
            string carpetaTickets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
            if (!Directory.Exists(carpetaTickets)) Directory.CreateDirectory(carpetaTickets);

            // Armamos el nombre del archivo usando un sello de tiempo para evitar sobreescrituras
            string nombreArchivo = $"Ticket_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(carpetaTickets, nombreArchivo);

            // Construcción del Documento PDF usando la API Fluida de QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // CONFIGURACIÓN DE PÁGINA PARA TICKETERA
                    // ContinuousSize permite fijar el ancho exacto de una impresora térmica estándar (80 mm),
                    // mientras que el alto crecerá dinámicamente según la cantidad de productos comprados.
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(4, Unit.Millimetre);
                    page.PageColor(Colors.White);

                    // Se utiliza "Lato" por defecto, ya que es la fuente integrada nativa de QuestPDF
                    // Evita cierres inesperados por falta de fuentes del sistema operativo.
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                    // DIBUJO DEL CONTENIDO APILADO (Columna)
                    page.Content().Column(col =>
                    {
                        // Encabezado del Comercio
                        col.Item().Text("TILLAS").Bold().FontSize(14).AlignCenter();
                        col.Item().Text("Venta de Calzado").FontSize(9).AlignCenter();
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Datos de la Operación
                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH.mm}");
                        col.Item().Text($"Cajero: {vendedor}");
                        col.Item().Text($"Cliente: {cliente}");
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // TABLA DINÁMICA DE PRODUCTOS
                        col.Item().Table(tabla =>
                        {
                            // Definición de anchos de columna: Constante para Cantidad y Subtotal, Relativo para el Nombre
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(15);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                            });

                            // Cabecera de la tabla
                            tabla.Header(header =>
                            {
                                header.Cell().Text("C").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().AlignRight().Text("Subtotal").Bold();
                            });

                            // Renglones iterativos según los DTOs recibidos desde la base de datos
                            foreach (var item in carrito)
                            {
                                decimal subtotal = item.Cantidad * item.Precio_Unitario;

                                tabla.Cell().Text(item.Cantidad.ToString());
                                tabla.Cell().Text($"{item.Nombre} (T:{item.Talle})");

                                // Aplicamos el formatoMoneda ("es-AR") para asegurar el símbolo correcto
                                tabla.Cell().AlignRight().Text(subtotal.ToString("C", formatoMoneda));
                            }
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // PIE DE PÁGINA Y TOTALES
                        // Aplicamos formatoMoneda al Total Grande
                        col.Item().AlignRight().Text($"TOTAL: {total.ToString("C", formatoMoneda)}").Bold().FontSize(12);
                        col.Item().Text($"Pago: {metodoPago}");
                        col.Item().PaddingTop(10).Text("¡Gracias por su compra!").AlignCenter().Bold();
                    });
                });
            })
            .GeneratePdf(rutaCompleta); // Guarda el archivo físico en el disco duro.

            // Pide a Windows que abra el archivo recién creado 
            // utilizando el lector PDF que el usuario tenga predeterminado (Edge, Chrome, Acrobat, etc.)
            Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
        }
    }
}