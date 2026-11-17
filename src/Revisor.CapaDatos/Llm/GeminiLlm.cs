using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Revisor.CapaDatos.Llm
{
    public class GeminiLlm : AbstractLlm
    {
        private static readonly HttpClient http = new();
        private readonly string apiKey;
        private readonly string modelo;

        public GeminiLlm(string apiKey, string modelo)
        {
            this.apiKey = apiKey;
            this.modelo = modelo;
        }

        public override async Task<RespuestaLlm> Completar(string instruccionSistema, string contenidoUsuario, CancellationToken ct)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent";
            var cuerpo = new
            {
                system_instruction = new { parts = new[] { new { text = instruccionSistema } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = contenidoUsuario } } } },
                generationConfig = new { temperature = 0, responseMimeType = "application/json" }
            };

            using var peticion = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(cuerpo) };
            peticion.Headers.Add("x-goog-api-key", apiKey);

            using var respuesta = await http.SendAsync(peticion, ct);
            var json = await respuesta.Content.ReadAsStringAsync(ct);
            if (!respuesta.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini respondió {(int)respuesta.StatusCode}: {json}");

            using var doc = JsonDocument.Parse(json);
            var texto = doc.RootElement.GetProperty("candidates")[0]
                .GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
            var tokens = doc.RootElement.TryGetProperty("usageMetadata", out var uso) &&
                         uso.TryGetProperty("totalTokenCount", out var total) ? total.GetInt32() : 0;

            return new RespuestaLlm(texto, tokens);
        }
    }
}
