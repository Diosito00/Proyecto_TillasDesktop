using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TillasDesktop.Entities.Reportes;
using System.Linq;

namespace TillasDesktop.BLL.Services
{
    // Servicio encargado de la generación y exportación de reportes gerenciales en formato PDF
    // utilizando la librería QuestPDF. Incluye tablas detalladas, tarjetas de resumen (KPIs)
    // y la captura gráfica de los reportes visuales de la aplicación.
    public class ReportesPdfService
    {
        // Forzamos la cultura de Argentina ("es-AR") para asegurar que las monedas muestren siempre el símbolo "$"
        private readonly CultureInfo _formatoMoneda = new CultureInfo("es-AR");

        // Crea y obtiene la ruta de la carpeta de salida "ReportesPDF" junto al ejecutable de la aplicación.
        private string ObtenerRutaCarpeta()
        {
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportesPDF");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        // Abre automáticamente el archivo PDF recién creado utilizando el visor predeterminado del sistema operativo.
        private void AbrirPdf(string rutaCompleta)
        {
            Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
        }

        // REPORTE DE VENTAS
        public void GenerarReporteVentasPdf(DateTime inicio, DateTime fin, List<VentaResumenDTO> ventas, byte[] graficoBytes)
        {
            // Configuración obligatoria de la licencia comunitaria de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            string nombreArchivo = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(ObtenerRutaCarpeta(), nombreArchivo);

            // Cálculos para tarjetas KPI superiores
            decimal totalRecaudado = ventas.Sum(v => v.Total);
            int cantidadVentas = ventas.Count;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de tamaño de página A4 y márgenes con sufijo 'f' (float)
                    page.Size(PageSizes.A4);
                    page.Margin(20f);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    // Encabezado institucional del reporte
                    page.Header().Column(col =>
                    {
                        col.Item().Text("TILLAS - REPORTE DE VENTAS").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Período: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(5).LineHorizontal(1.5f).LineColor(Colors.Blue.Medium);
                    });

                    // Cuerpo del documento
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // Incrustar el gráfico visual capturado desde la interfaz adaptándose al ancho
                        if (graficoBytes != null && graficoBytes.Length > 0)
                        {
                            col.Item().Image(graficoBytes).FitWidth();
                            col.Item().PaddingVertical(8);
                        }

                        // Tarjetas de resumen (KPIs) para rellenar visualmente el reporte
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                            {
                                c.Item().Text("TOTAL RECAUDADO").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                                c.Item().Text(totalRecaudado.ToString("C", _formatoMoneda)).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            });
                            row.ConstantItem(10f);
                            row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                            {
                                c.Item().Text("VENTAS REALIZADAS").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                                c.Item().Text(cantidadVentas.ToString()).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            });
                        });

                        // Tabla de datos detallados
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35f);  // ID
                                columns.RelativeColumn(2.5f); // Fecha y Hora
                                columns.RelativeColumn(3f);   // Vendedor
                                columns.RelativeColumn(3.5f); // Cliente
                                columns.RelativeColumn(2.5f); // Método de Pago
                                columns.RelativeColumn(2.5f); // Total
                            });

                            // Cabecera de la tabla
                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("ID").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Vendedor").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Método Pago").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                            });

                            // Filas de datos iterativas con colores intercalados
                            bool alternar = false;
                            foreach (var v in ventas)
                            {
                                var colorFondo = alternar ? Colors.Grey.Lighten5 : Colors.White;
                                alternar = !alternar;

                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(v.ID.ToString());
                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(v.Fecha_Hora.ToString("dd/MM/yyyy HH:mm"));
                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(v.NombreVendedor);
                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(v.NombreCliente);
                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(v.MetodoPago);
                                tabla.Cell().Background(colorFondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(v.Total.ToString("C", _formatoMoneda));
                            }
                        });
                    });

                    // Pie de página con número de página dinámico
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(rutaCompleta);

            AbrirPdf(rutaCompleta);
        }

        // RENDIMIENTO DE VENDEDORES
        public void GenerarReporteVendedoresPdf(DateTime inicio, DateTime fin, List<RendimientoVendedorDTO> vendedores, byte[] graficoBytes)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            string nombreArchivo = $"RendimientoVendedores_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(ObtenerRutaCarpeta(), nombreArchivo);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20f);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("TILLAS - RENDIMIENTO DE VENDEDORES").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Período evaluado: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(5).LineHorizontal(1.5f).LineColor(Colors.Blue.Medium);
                    });

                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // Incrustar gráfico de barras de rendimiento adaptándose al ancho
                        if (graficoBytes != null && graficoBytes.Length > 0)
                        {
                            col.Item().Image(graficoBytes).FitWidth();
                            col.Item().PaddingVertical(8);
                        }

                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3f);   // Nombre del Vendedor
                                columns.RelativeColumn(2f);   // Cantidad de ventas
                                columns.RelativeColumn(2.5f); // Total recaudado
                                columns.RelativeColumn(2.5f); // Promedio por venta
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Vendedor").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Ventas Realizadas").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total Recaudado").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Promedio / Venta").FontColor(Colors.White).Bold();
                            });

                            foreach (var v in vendedores)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(v.Vendedor);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(v.CantidadVentas.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(v.TotalRecaudado.ToString("C", _formatoMoneda));
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(v.PromedioPorVenta.ToString("C", _formatoMoneda));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(rutaCompleta);

            AbrirPdf(rutaCompleta);
        }

        // PRODUCTOS MÁS VENDIDOS
        public void GenerarReporteProductosPdf(DateTime inicio, DateTime fin, string marca, string categoria, List<ProductoVendidoDTO> productos, byte[] graficoBytes)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            string nombreArchivo = $"ProductosMasVendidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(ObtenerRutaCarpeta(), nombreArchivo);

            // Ordenamiento lógico por Marca, Categoría y Cantidad Vendida para mantener estructura limpia
            var productosOrdenados = productos
                .OrderBy(p => p.Marca)
                .ThenBy(p => p.Categoria)
                .ThenByDescending(p => p.CantidadVendida)
                .ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20f);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("TILLAS - PRODUCTOS MÁS VENDIDOS").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Período: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}  |  Marca: {marca}  |  Categoría: {categoria}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(5).LineHorizontal(1.5f).LineColor(Colors.Blue.Medium);
                    });

                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // Incrustar gráfico de torta de productos adaptándose al ancho
                        if (graficoBytes != null && graficoBytes.Length > 0)
                        {
                            col.Item().Image(graficoBytes).FitWidth();
                            col.Item().PaddingVertical(8);
                        }

                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2f);   // Código
                                columns.RelativeColumn(3.5f); // Nombre del modelo
                                columns.RelativeColumn(2.5f); // Marca
                                columns.RelativeColumn(2.5f); // Categoría
                                columns.RelativeColumn(1.5f); // Cantidad vendida
                                columns.RelativeColumn(2.5f); // Total generado
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Código").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Modelo").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Marca").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Categoría").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Vendidos").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total Generado").FontColor(Colors.White).Bold();
                            });

                            foreach (var p in productosOrdenados)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(p.Codigo);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(p.Nombre);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(p.Marca).Bold();
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(p.Categoria);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(p.CantidadVendida.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(p.TotalGenerado.ToString("C", _formatoMoneda));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(rutaCompleta);

            AbrirPdf(rutaCompleta);
        }
    }
}