using Revisor.CapaDatos.Entidades;
using Revisor.CapaDatos.Llm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Revisor.CapaNegocios.Agentes
{
    // Flujo común de un subagente:
    //   1) herramienta determinista (regex)  2) LLM explica y completa  3) validar el JSON
    // Las fallas se lanzan como excepción; los DECORADORES se encargan de reintentar,
    // cortar por tiempo y convertir el error en una ejecución "Fallido".
    public abstract class AgenteLlm : AbstractSubAgente
    {
        protected readonly AbstractLlm llm;

        protected AgenteLlm(AbstractLlm llm) { this.llm = llm; }

        // Cada subagente hijo (Seguridad, Calidad, Pruebas) define qué archivo .txt usar y qué reglas Regex correr.
        protected abstract string ArchivoPrompt { get; }
        protected abstract List<Hallazgo> UsarHerramienta(string codigo);

        public override async Task<EjecucionAgente> Revisar(string codigo, CancellationToken ct)
        {
            var hallazgosHerramienta = UsarHerramienta(codigo);

            var instrucciones = CargadorPrompts.Cargar(ArchivoPrompt)
                .Replace("{{HALLAZGOS_HERRAMIENTA}}", JsonSerializer.Serialize(hallazgosHerramienta));

            // El código va delimitado como DATO, separado de las instrucciones (defensa ante prompt injection)
            var respuesta = await llm.Completar(instrucciones, $"<codigo>\n{codigo}\n</codigo>", ct);

            var hallazgosLlm = ParserRespuesta.Parsear(respuesta.Contenido, Nombre); // lanza si el JSON es inválido

            return new EjecucionAgente
            {
                Agente = Nombre,
                Estado = EstadoAgente.Ok,
                Tokens = respuesta.Tokens,
                RespuestaCruda = respuesta.Contenido,
                Hallazgos = hallazgosHerramienta.Concat(hallazgosLlm).ToList()
            };
        }
    }

    public static class CargadorPrompts
    {
        public static string Cargar(string archivo) =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Prompts", archivo));
    }

    public static class ParserRespuesta
    {
        // Valida que el LLM respondió con el contrato {"hallazgos":[...]}
        public static List<Hallazgo> Parsear(string json, string agente)
        {
            var limpio = json.Trim();
            if (limpio.StartsWith("```"))   // a veces el modelo envuelve en ```json ... ```
            {
                var inicio = limpio.IndexOf('{');
                var fin = limpio.LastIndexOf('}');
                if (inicio < 0 || fin < inicio) throw new FormatException("Respuesta sin JSON.");
                limpio = limpio[inicio..(fin + 1)];
            }

            using var doc = JsonDocument.Parse(limpio);   // JsonException si no es JSON
            if (!doc.RootElement.TryGetProperty("hallazgos", out var lista) || lista.ValueKind != JsonValueKind.Array)
                throw new FormatException("La respuesta no contiene la lista 'hallazgos'.");

            var resultado = new List<Hallazgo>();
            foreach (var h in lista.EnumerateArray())
            {
                if (!Enum.TryParse<Severidad>(Texto(h, "severidad"), true, out var severidad))
                    throw new FormatException($"Severidad inválida: '{Texto(h, "severidad")}'.");

                resultado.Add(new Hallazgo
                {
                    Agente = agente,
                    Severidad = severidad,
                    Linea = h.TryGetProperty("linea", out var l) && l.ValueKind == JsonValueKind.Number ? l.GetInt32() : null,
                    Regla = Texto(h, "regla"),
                    Mensaje = Texto(h, "mensaje"),
                    Sugerencia = Texto(h, "sugerencia")
                });
            }
            return resultado;
        }

        private static string Texto(JsonElement e, string propiedad) =>
            e.TryGetProperty(propiedad, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    }
}
