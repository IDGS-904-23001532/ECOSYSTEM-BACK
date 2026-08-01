using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecosystem_backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngresosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngresosController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. RESUMEN GENERAL (KPIs)
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/ingresos/resumen
         * ¿Qué enviar?: Nada.
         * ¿Qué devuelve?: Los totales históricos de toda la vida del sistema.
         * 
         * Ejemplo de respuesta:
         * {
         *   "totalVentas": 150000.50,
         *   "totalGastos": 80000.00,
         *   "utilidadNeta": 70000.50
         * }
         */
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumenGeneral()
        {
            // Solo sumamos el Total de Ventas en estado "Completo"[cite: 2]
            var totalVentas = await _context.Ventas
                .Where(v => v.Estado == "Completo")
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            // Sumamos el Total de todos los registros de Gastos[cite: 2]
            var totalGastos = await _context.Gastos
                .SumAsync(g => (decimal?)g.Total) ?? 0;

            var utilidadNeta = totalVentas - totalGastos;

            return Ok(new
            {
                TotalVentas = totalVentas,
                TotalGastos = totalGastos,
                UtilidadNeta = utilidadNeta
            });
        }

        // ==========================================
        // 2. BALANCE POR RANGO DE FECHAS
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/ingresos/periodo?inicio=2026-07-01&fin=2026-07-31
         * ¿Qué enviar?: Dos parámetros en la URL (inicio y fin) en formato YYYY-MM-DD.
         * ¿Qué devuelve?: Los totales calculados exclusivamente dentro de ese rango de fechas.
         * 
         * Ejemplo de respuesta (Mismo formato que resumen):
         * {
         *   "totalVentas": 45000.00,
         *   "totalGastos": 15000.00,
         *   "utilidadNeta": 30000.00
         * }
         */
        [HttpGet("periodo")]
        public async Task<IActionResult> GetBalancePorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        {
            // Ajustamos la fecha de fin para que cubra todo el día (hasta las 23:59:59)
            var finAjustado = fin.Date.AddDays(1).AddTicks(-1);

            var totalVentas = await _context.Ventas
                .Where(v => v.Estado == "Completo" && v.Fecha >= inicio.Date && v.Fecha <= finAjustado)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var totalGastos = await _context.Gastos
                .Where(g => g.Fecha >= inicio.Date && g.Fecha <= finAjustado)
                .SumAsync(g => (decimal?)g.Total) ?? 0;

            var utilidadNeta = totalVentas - totalGastos;

            return Ok(new
            {
                TotalVentas = totalVentas,
                TotalGastos = totalGastos,
                UtilidadNeta = utilidadNeta
            });
        }

        // ==========================================
        // 3. DATOS PARA GRÁFICA ANUAL (POR MES)
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/ingresos/grafica-anual/2026
         * ¿Qué enviar?: El año que se quiere consultar directamente en la URL.
         * ¿Qué devuelve?: Un arreglo de 12 posiciones (una por cada mes) con los totales. Ideal para gráficas de barras o líneas.
         * 
         * Ejemplo de respuesta:
         * [
         *   { "mes": 1, "nombreMes": "Enero", "ingresos": 50000, "gastos": 20000, "utilidad": 30000 },
         *   { "mes": 2, "nombreMes": "Febrero", "ingresos": 45000, "gastos": 30000, "utilidad": 15000 },
         *   ... (hasta diciembre)
         * ]
         */
        [HttpGet("grafica-anual/{anio}")]
        public async Task<IActionResult> GetGraficaAnual(int anio)
        {
            // Traemos los datos del año solicitado a memoria para agruparlos de forma segura independientemente del motor de BD (MySQL, SQL Server, etc.)
            var ventasDelAnio = await _context.Ventas
                .Where(v => v.Estado == "Completo" && v.Fecha.Year == anio)
                .Select(v => new { v.Fecha.Month, v.Total })
                .ToListAsync();

            var gastosDelAnio = await _context.Gastos
                .Where(g => g.Fecha.Year == anio)
                .Select(g => new { g.Fecha.Month, g.Total })
                .ToListAsync();

            var nombresMeses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            var reporteAnual = new List<object>();

            // Construimos la lista mes a mes (del 1 al 12)
            for (int i = 1; i <= 12; i++)
            {
                var ingresosMes = ventasDelAnio.Where(v => v.Month == i).Sum(v => (decimal?)v.Total) ?? 0;
                var gastosMes = gastosDelAnio.Where(g => g.Month == i).Sum(g => (decimal?)g.Total) ?? 0;

                reporteAnual.Add(new
                {
                    Mes = i,
                    NombreMes = nombresMeses[i - 1],
                    Ingresos = ingresosMes,
                    Gastos = gastosMes,
                    Utilidad = ingresosMes - gastosMes
                });
            }

            return Ok(reporteAnual);
        }
    }
}