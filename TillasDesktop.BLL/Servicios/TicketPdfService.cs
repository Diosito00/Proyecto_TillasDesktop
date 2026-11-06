// Importa clases fundamentales del sistema (como la clase base Exception o tipos de datos)
using System;
// Importa colecciones genéricas para manejar listas de objetos (como List<T>)
using System.Collections.Generic;
// Importa herramientas para ejecutar procesos externos (utilizado para abrir el visor de PDF predeterminado de Windows)
using System.Diagnostics;
// Importa clases para interactuar con archivos y directorios del disco (Path, File, Directory)
using System.IO;
// Importa herramientas para codificación y manejo de textos (si fuera necesario procesar caracteres especiales)
using System.Text;
// Importa clases de enlace de datos de WPF (utilizado habitualmente en arquitecturas de interfaz)
using System.Windows.Data;
// Importa configuraciones regionales y de cultura (CultureInfo, fundamental para el formato de monedas y fechas locales)
using System.Globalization;
// Importa las interfaces fluidas de la librería QuestPDF para la creación declarativa de documentos
using QuestPDF.Fluent;
// Importa ayudas visuales, tamaños estándar y utilidades de diseño de QuestPDF
using QuestPDF.Helpers;
// Importa los componentes de infraestructura de QuestPDF (configuración de licencias, contenedores)
using QuestPDF.Infrastructure;
// Importa las entidades o DTOs (Data Transfer Objects) correspondientes al carrito de compras y facturación
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.BLL.Servicios
{
    // Servicio dedicado exclusivamente a la generación de comprobantes PDF.
    public class TicketPdfService
    {
        
        /// Genera un ticket en formato PDF, lo guarda en el disco y lo abre automáticamente.
        public void GenerarYGuardarTicket(string vendedor, string cliente, string metodoPago, decimal total, List<ItemCarritoDTO> carrito)
        {
            // Configuración obligatoria de QuestPDF para evitar la marca de agua y excepciones.
            // La licencia Community es gratuita para proyectos pequeños/medianos.
            QuestPDF.Settings.License = LicenseType.Community;

            // Se fuerza el formato regional a Argentina ("es-AR") para garantizar 
            // que los valores monetarios se formateen correctamente con el símbolo "$" y separadores adecuados.
            var formatoMoneda = new CultureInfo("es-AR");

            // --- MANEJO DEL SISTEMA DE ARCHIVOS ---
            // Se define la ruta de la carpeta "Tickets" relativa al directorio donde se ejecuta la aplicación.
            string carpetaTickets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");

            // Si la carpeta no existe, se crea automáticamente.
            if (!Directory.Exists(carpetaTickets)) Directory.CreateDirectory(carpetaTickets);

            // Se genera un nombre de archivo único basado en la fecha y hora exacta (ej: Ticket_20261104_041030.pdf)
            // para evitar sobrescribir tickets anteriores.
            string nombreArchivo = $"Ticket_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(carpetaTickets, nombreArchivo);

            // --- CONSTRUCCIÓN DEL DOCUMENTO PDF ---
            // Se utiliza la API Fluida de QuestPDF para maquetar el documento de forma declarativa.
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración especial para impresoras térmicas (ticketeras).
                    // 'ContinuousSize' fija el ancho en 80 milímetros, pero permite que el alto 
                    // crezca dinámicamente según la cantidad de productos en el ticket.
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(4, Unit.Millimetre); // Margen pequeño para aprovechar el papel
                    page.PageColor(Colors.White);

                    // Se define la fuente predeterminada "Lato" (nativa de QuestPDF) y un tamaño base de 9pt.
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                    // El contenido principal se apila verticalmente usando una 'Column'.
                    page.Content().Column(col =>
                    {
                        // === ENCABEZADO DEL COMERCIO CON LOGO ===
                        col.Item().Row(row =>
                        {
                            // Columna Izquierda: Logo de la empresa
                            string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");

                            // Solo dibuja la imagen si el archivo realmente existe en la carpeta.
                            if (File.Exists(rutaLogo))
                            {
                                // Se reserva un espacio constante de 22 puntos de ancho para el logo.
                                row.ConstantItem(22).Image(rutaLogo);
                            }

                            // Columna Derecha: Nombre de la empresa
                            // 'RelativeItem' hace que esta columna ocupe el espacio restante de la fila.
                            row.RelativeItem().PaddingLeft(2).Column(empresa =>
                            {
                                empresa.Item().Text("TILLAS").Bold().FontSize(14).AlignLeft();
                            });
                        });

                        // Línea divisoria gris clara
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // === DATOS FISCALES DE LA EMPRESA ===
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().PaddingLeft(2).Column(empresa =>
                            {
                                empresa.Item().Text("C.U.I.T. Nro.: 30-12345678-9").FontSize(9).AlignLeft();
                                empresa.Item().Text("Ing. Brutos: 901-123456-7").FontSize(9).AlignLeft();
                                empresa.Item().Text("Domicilio: CALLE FALSA 123 / C.P.(3400)").FontSize(9).AlignLeft();
                                empresa.Item().Text("CORRIENTES / PCIA. DE CORRIENTES").FontSize(9).AlignLeft();
                                empresa.Item().Text("Inicio de Actividades: 01/01/2010").FontSize(9).AlignLeft();
                                empresa.Item().Text("IVA RESPONSABLE INSCRIPTO").FontSize(9).AlignLeft();
                            });
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // === DATOS DE LA OPERACIÓN ===                        
                        col.Item().Text($"Vendedor: {vendedor}").FontSize(9);
                        col.Item().Text("DV: bf").FontSize(9); // Dato estático de ejemplo (Dígito Verificador)

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Detalles de la factura, fecha y hora del momento de la emisión.
                        col.Item().Text("TIQUE FACTURA \"B\" (Cód.082)   N° 00362-00147985").Bold().FontSize(9);
                        col.Item().Text($"Fecha {DateTime.Now:dd/MM/yyyy}   Hora {DateTime.Now:HH:mm:ss}").FontSize(9);

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // === DATOS DEL CLIENTE ===
                        col.Item().Text($"CLIENTE: {cliente.ToUpper()}").Bold().FontSize(9);

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // === TABLA DINÁMICA DE PRODUCTOS ===
                        col.Item().Table(tabla =>
                        {
                            // Definición del ancho de las 3 columnas
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(15); // Columna 1 (Cantidad): Ancho fijo pequeño
                                columns.RelativeColumn();   // Columna 2 (Producto): Se expande para llenar el medio
                                columns.ConstantColumn(50); // Columna 3 (Subtotal): Ancho fijo de 50pt para evitar saltos de línea numéricos
                            });

                            // Encabezados de la tabla
                            tabla.Header(header =>
                            {
                                header.Cell().Text("C").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().AlignRight().Text("Subt.").Bold();
                            });

                            // Iteración sobre la lista de productos pasados por parámetro
                            foreach (var item in carrito)
                            {
                                // Cálculo matemático del subtotal por renglón
                                decimal subtotal = item.Cantidad * item.Precio_Unitario;

                                // Llenado de las celdas
                                tabla.Cell().Text(item.Cantidad.ToString());
                                tabla.Cell().Text($"{item.Nombre} (T:{item.Talle})");

                                // Se aplica el 'formatoMoneda' para que pinte "$ 10.000,00" y se alinea a la derecha
                                tabla.Cell().AlignRight().Text(subtotal.ToString("C", formatoMoneda));
                            }
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // === PIE DE PÁGINA Y TOTALES ===
                        // Se muestra el gran total destacado en tamaño 12 y en negrita.
                        col.Item().AlignRight().Text($"TOTAL: {total.ToString("C", formatoMoneda)}").Bold().FontSize(12);
                        col.Item().Text($"Pago: {metodoPago}");
                        col.Item().PaddingTop(10).Text("¡Gracias por su compra!").AlignCenter().Bold();
                    });
                });
            })
            .GeneratePdf(rutaCompleta); // Acción final: renderiza y guarda físicamente el archivo PDF en el disco.

            // --- APERTURA DEL ARCHIVO ---
            // Le pide a Windows que abra el archivo recién creado usando el programa predeterminado 
            // del sistema operativo para leer PDFs (ej: Edge, Chrome, Acrobat Reader).
            Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
        }
    }
}