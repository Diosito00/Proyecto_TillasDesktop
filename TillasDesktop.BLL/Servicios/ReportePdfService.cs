// Importa clases básicas de .NET para el manejo de tipos y excepciones
using System;
// Importa colecciones genéricas para el manejo de listas (List<T>)
using System.Collections.Generic;
// Importa herramientas del sistema para ejecutar procesos externos (usado para abrir el visor de PDF)
using System.Diagnostics;
// Importa clases para interactuar con el sistema de archivos y directorios (Path, Directory)
using System.IO;
// Importa configuraciones regionales y de cultura (CultureInfo, fundamental para el formato de monedas y fechas)
using System.Globalization;
// Importa las interfaces fluidas de la librería QuestPDF para la creación estructurada de documentos
using QuestPDF.Fluent;
// Importa ayudas visuales y utilidades de diseño de QuestPDF (tamaños de página estándar, colores, etc.)
using QuestPDF.Helpers;
// Importa los componentes de infraestructura de QuestPDF (configuración de licencias, contenedores)
using QuestPDF.Infrastructure;
// Importa los modelos de transferencia de datos (DTOs) específicos de los reportes del sistema
using TillasDesktop.Entities.Reportes;
// Importa LINQ para realizar ordenamientos y consultas sobre colecciones en memoria
using System.Linq;


// Define el espacio de nombres de los servicios dentro de la capa de lógica de negocio (BLL)
namespace TillasDesktop.BLL.Services
{
    // Servicio encargado de la generación y exportación de reportes gerenciales en formato PDF
    // utilizando la librería QuestPDF. Incluye tablas detalladas, tarjetas de resumen (KPIs)
    // y la captura gráfica de los reportes visuales de la aplicación.
    public class ReportesPdfService
    {
        // Forzamos la cultura de Argentina ("es-AR") para asegurar que las monedas muestren siempre el símbolo "$"
        private readonly CultureInfo _formatoMoneda = new CultureInfo("es-AR");

        // Crea y obtiene la ruta de la carpeta de salida "ReportesPDF" ubicada junto al archivo ejecutable (.exe) de la aplicación.
        private string ObtenerRutaCarpeta()
        {
            // Combina la ruta base del directorio de ejecución con la subcarpeta "ReportesPDF"
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportesPDF");

            // Si la carpeta física no existe en el disco, la crea automáticamente
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            // Retorna la ruta absoluta de la carpeta lista para guardar archivos
            return carpeta;
        }

        // Abre automáticamente el archivo PDF recién creado utilizando el visor predeterminado del sistema operativo.
        private void AbrirPdf(string rutaCompleta)
        {
            // Inicia un nuevo proceso del sistema pasando la ruta del archivo y habilitando el uso del shell predeterminado
            Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
        }

        // REPORTE DE VENTAS
        public void GenerarReporteVentasPdf(DateTime inicio, DateTime fin, List<VentaResumenDTO> ventas, byte[] graficoBytes)
        {
            // Configuración obligatoria de la licencia comunitaria (Community) exigida por la librería QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Genera un nombre de archivo único utilizando la fecha y hora actual exacta para evitar sobreescrituras
            string nombreArchivo = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // Obtiene la ruta completa combinando la carpeta de salida y el nombre del archivo PDF
            string rutaCompleta = Path.Combine(ObtenerRutaCarpeta(), nombreArchivo);

            // Cálculos matemáticos previos para rellenar las tarjetas KPI superiores del reporte
            decimal totalRecaudado = ventas.Sum(v => v.Total);
            int cantidadVentas = ventas.Count;

            // Inicia la construcción estructurada del documento PDF utilizando la API fluida de QuestPDF
            Document.Create(container =>
            {
                // Define la estructura interna de las páginas del documento
                container.Page(page =>
                {
                    // Configuración de tamaño de página estándar A4 y definición de márgenes de 20 unidades con sufijo 'f' (float)
                    page.Size(PageSizes.A4);
                    page.Margin(20f);
                    // Establece el color de fondo general de la página como Blanco 
                    page.PageColor(Colors.White);
                    // Define la tipografía base predeterminada para todo el texto del documento (Tamaño 10, fuente "Lato")
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    // Sección de Encabezado institucional que aparecerá al inicio del reporte
                    page.Header().Column(col =>
                    {
                        // Título principal con estilo negrita, tamaño grande de 16 y color azul oscuro corporativo
                        col.Item().Text("TILLAS - REPORTE DE VENTAS").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);

                        // Subtítulo que muestra dinámicamente el rango de fechas evaluado en el reporte
                        col.Item().Text($"Período: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);

                        // Línea divisoria horizontal decorativa con un grosor de 1.5 y color azul medio
                        col.Item().PaddingVertical(5).LineHorizontal(1.5f).LineColor(Colors.Blue.Medium);
                    });

                    // Cuerpo principal del documento PDF
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // Incrustar el gráfico visual capturado desde la interfaz adaptándose al ancho
                        if (graficoBytes != null && graficoBytes.Length > 0)
                        {
                            col.Item().Image(graficoBytes).FitWidth();
                            col.Item().PaddingVertical(8); // Espaciado inferior de separación
                        }

                        // Tarjetas de resumen (KPIs) para rellenar visualmente el reporte
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            // Primera tarjeta KPI: Total Recaudado en dinero
                            row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                            {
                                c.Item().Text("TOTAL RECAUDADO").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                                // Muestra el monto total formateado en pesos argentinos usando la cultura local
                                c.Item().Text(totalRecaudado.ToString("C", _formatoMoneda)).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            });
                            // Espaciado constante de separación entre las dos tarjetas de KPIs
                            row.ConstantItem(10f);

                            // Segunda tarjeta KPI: Cantidad total de operaciones o ventas realizadas
                            row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                            {
                                c.Item().Text("VENTAS REALIZADAS").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                                c.Item().Text(cantidadVentas.ToString()).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            });
                        });

                        // Tabla de datos detallados
                        col.Item().Table(tabla =>
                        {
                            // Definición proporcional del ancho de las columnas que componen la tabla
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35f);  // ID
                                columns.RelativeColumn(2.5f); // Fecha y Hora
                                columns.RelativeColumn(3f);   // Vendedor
                                columns.RelativeColumn(3.5f); // Cliente
                                columns.RelativeColumn(2.5f); // Método de Pago
                                columns.RelativeColumn(2.5f); // Total
                            });

                            // Cabecera institucional de la tabla con fondo azul oscuro y texto en blanco en negrita
                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("ID").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Vendedor").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Método Pago").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                            });

                            // Variable booleana de control para lograr un efecto visual de cebra (colores de fondo intercalados en las filas)
                            bool alternar = false;

                            // Itera sobre cada registro de venta recibido en la lista para agregarlo como fila en la tabla PDF
                            foreach (var v in ventas)
                            {
                                // Define el color de fondo de la celda actual alternando entre gris muy claro y blanco 
                                var colorFondo = alternar ? Colors.Grey.Lighten5 : Colors.White;
                                // Invierte el estado booleano para la siguiente fila
                                alternar = !alternar;

                                // Inserta celda por celda los datos correspondientes aplicando bordes inferiores suaves y espaciados internos
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

            // Genera y compila el archivo PDF físico guardándolo en la ruta completa especificada
            .GeneratePdf(rutaCompleta);

            // Abre de manera automática el PDF generado para que el usuario pueda visualizarlo al instante
            AbrirPdf(rutaCompleta);
        }

        // RENDIMIENTO DE VENDEDORES
        public void GenerarReporteVendedoresPdf(DateTime inicio, DateTime fin, List<RendimientoVendedorDTO> vendedores, byte[] graficoBytes)
        {
            // Configuración de la licencia comunitaria de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
            // Define el nombre único del archivo PDF de rendimiento de vendedores con marca de tiempo
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

                    // Encabezado específico para el reporte de rendimiento de persona
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

                        // Tabla detallada para listar el rendimiento individual por vendedor
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3f);   // Nombre del Vendedor
                                columns.RelativeColumn(2f);   // Cantidad de ventas
                                columns.RelativeColumn(2.5f); // Total recaudado
                                columns.RelativeColumn(2.5f); // Promedio por venta
                            });

                            // Cabecera de la tabla con los títulos de columnas correspondientes
                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Vendedor").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Ventas Realizadas").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total Recaudado").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Promedio / Venta").FontColor(Colors.White).Bold();
                            });

                            // Recorre la lista de vendedores y dibuja cada una de las filas con sus respectivos valores monetarios formateados
                            foreach (var v in vendedores)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(v.Vendedor);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(v.CantidadVentas.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(v.TotalRecaudado.ToString("C", _formatoMoneda));
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(v.PromedioPorVenta.ToString("C", _formatoMoneda));
                            }
                        });
                    });

                    // Pie de página dinámico con conteo de páginas
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })

            // Genera y compila el archivo PDF físico guardándolo en la ruta completa especificada
            .GeneratePdf(rutaCompleta);

            // Abre de manera automática el PDF generado para que el usuario pueda visualizarlo al instante
            AbrirPdf(rutaCompleta);
        }

        // PRODUCTOS MÁS VENDIDOS
        public void GenerarReporteProductosPdf(DateTime inicio, DateTime fin, string marca, string categoria, List<ProductoVendidoDTO> productos, byte[] graficoBytes)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            string nombreArchivo = $"ProductosMasVendidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string rutaCompleta = Path.Combine(ObtenerRutaCarpeta(), nombreArchivo);

            // Ordenamiento lógico multinivel mediante LINQ: primero agrupa/ordena por Marca, luego por Categoría y finalmente de mayor a menor por Cantidad Vendida
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

                    // Encabezado indicando los filtros aplicados de marca y categoría en el reporte de inventario
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

                        // Tabla detallada de artículos vendidos
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

                            // Cabecera de la tabla de productos
                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Código").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Modelo").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Marca").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Categoría").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Vendidos").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total Generado").FontColor(Colors.White).Bold();
                            });

                            // Itera sobre la lista de productos ya ordenada para construir cada fila de la tabla
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

                    // Pie de página estándar con numeración de páginas
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