using Revisor.CapaDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Revisor.CapaNegocios.Herramientas
{
    public record ReglaRegex(string Regla, Regex Patron, Severidad Severidad, string Mensaje, string Sugerencia);


    // Motor de análisis estático: busca errores conocidos con Regex ANTES de consultar a la IA,
    // dando hallazgos objetivos y con número de línea exacto.
    public static class DetectorRegex
    {
        private const RegexOptions Op = RegexOptions.IgnoreCase | RegexOptions.Compiled
            ;

        // 1. Reglas para el subagente de Seguridad (contraseñas quemadas, SQL injection, cadenas expuestas)
        public static readonly List<ReglaRegex> ReglasSeguridad = new()
        {
        new("SECRETO_EN_CODIGO",
            new(@"\b(password|pwd|clave|apikey|api_key|secret|token)\w*\s*=\s*""[^""]+""", Op),
            Severidad.Alta, "Credencial escrita directamente en el código.", "Moverla a variables de entorno o user-secrets."),

        new("SQL_CONCATENADO",
            new(@"""[^""]*\b(SELECT|INSERT|UPDATE|DELETE)\b[^""]*""\s*\+", Op),
            Severidad.Critica, "Consulta SQL armada concatenando texto (riesgo de SQL injection).", "Usar parámetros (SqlParameter)."),

        new("CADENA_CONEXION_EXPUESTA",
            new(@"(Data Source|Server)\s*=.*(Password|Pwd)\s*=", Op),
            Severidad.Alta, "Cadena de conexión con contraseña en el código.", "Leerla desde la configuración segura."),
         };


        // 2. Reglas para el subagente de Calidad (catch vacíos, código comentado, números mágicos)
        public static readonly List<ReglaRegex> ReglasCalidad = new()
        {
             new("CATCH_VACIO",
                new(@"catch\s*(\([^)]*\))?\s*\{\s*\}", Op),
                Severidad.Media, "Bloque catch vacío: el error se pierde.", "Registrar o manejar la excepción."),

            new("CODIGO_COMENTADO",
                new(@"^\s*//.*[;{}]\s*$", Op | RegexOptions.Multiline),
                Severidad.Baja, "Código comentado que quedó en el archivo.", "Eliminarlo; Git ya guarda el historial."),

            new("NUMERO_MAGICO",
                new(@"[<>]=?\s*\d{2,}\b", Op),
                Severidad.Baja, "Número literal en una condición (número mágico).", "Declararlo como constante con nombre."),
        };


        // 3. Reglas para el subagente de Pruebas (uso de Random, DateTime.Now o 'new' de dependencias)
        public static readonly List<ReglaRegex> ReglasPruebas = new()
        {
            new("ALEATORIO_NO_DETERMINISTA",
                new(@"new\s+Random\s*\(", Op),
                Severidad.Alta, "Uso de Random dentro de la lógica: el resultado no se puede probar.", "Inyectar el generador o recibir el valor como parámetro."),

            new("FECHA_DEL_SISTEMA",
                new(@"DateTime\.(Now|Today|UtcNow)", Op),
                Severidad.Media, "Depende de la fecha del sistema: difícil de probar.", "Recibir la fecha como parámetro."),

            new("DEPENDENCIA_CONCRETA",
                new(@"new\s+[\w\.]*(DAO|WS\w*|Service|Servicio)\s*\(", Op),
                Severidad.Media, "Crea su dependencia con 'new': no se puede reemplazar por un simulado en pruebas.", "Recibirla por constructor (clase abstracta)."),
    };


        // Recorre el código línea por línea, aplica las reglas del subagente y devuelve la lista de errores encontrados.
        public static List<Hallazgo> Analizar(string codigo, string agente, List<ReglaRegex> reglas)
        {
            var hallazgos = new List<Hallazgo>();
            var lineas = codigo.Replace("\r\n", "\n").Split('\n'); // Normaliza saltos de línea para contar bien

            for (int i = 0; i < lineas.Length; i++)
            {
                foreach (var regla in reglas)
                {
                    if (regla.Patron.IsMatch(lineas[i]))
                    {
                        hallazgos.Add(new Hallazgo
                        {
                            Agente = agente,
                            Severidad = regla.Severidad,
                            Linea = i + 1, // +1 porque las líneas de código empiezan en 1, no en 0
                            Regla = regla.Regla,
                            Mensaje = regla.Mensaje,
                            Sugerencia = regla.Sugerencia
                        });
                    }
                }
            }

            return hallazgos;
        }
    }

}
