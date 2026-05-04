using MedCitas.Core.DTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedCitas.Infrastructure.Services
{
    public class RagService
    {
        private readonly HttpClient _http;

        // Opciones de serialización: snake_case para coincidir con FastAPI
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public RagService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Consulta RAG completa (sin streaming). Devuelve la respuesta de phi3.
        /// </summary>
        public async Task<ConsultaResponse?> ConsultarAsync(ConsultaRequest request)
        {
            var response = await _http.PostAsJsonAsync("/consulta", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConsultaResponse>(_jsonOptions);
        }

        /// <summary>
        /// Consulta RAG con streaming SSE.
        /// Lee token a token desde /consulta/stream y los escribe en el outputStream del caller.
        /// El caller (controller) es responsable de configurar Content-Type: text/event-stream.
        /// </summary>
        public async Task StreamConsultaAsync(
            string pregunta,
            string pacienteId,
            Stream outputStream,
            CancellationToken cancellationToken = default)
        {
            var body = new
            {
                pregunta,
                paciente_id = pacienteId,
                multi_query = true
            };

            var json = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/consulta/stream")
            {
                Content = content
            };

            // ✅ ResponseHeadersRead: no esperar el body completo, leer mientras llega
            using var apiResponse = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            apiResponse.EnsureSuccessStatusCode();

            using var stream = await apiResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null) break;

                // Ignorar líneas vacías o sin prefijo SSE
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ", StringComparison.Ordinal))
                    continue;

                var data = line["data: ".Length..];

                // ✅ Señal de fin de stream
                if (data.TrimEnd() == "[DONE]")
                {
                    var doneBytes = Encoding.UTF8.GetBytes("data: [DONE]\n\n");
                    await outputStream.WriteAsync(doneBytes, cancellationToken);
                    await outputStream.FlushAsync(cancellationToken);
                    break;
                }

                // ✅ Parsear { "token": "..." } y hacer relay al cliente
                try
                {
                    using var doc = JsonDocument.Parse(data);
                    if (doc.RootElement.TryGetProperty("token", out var tokenElement))
                    {
                        var token = tokenElement.GetString() ?? string.Empty;
                        var sseEvent = $"data: {JsonSerializer.Serialize(token)}\n\n";
                        var bytes = Encoding.UTF8.GetBytes(sseEvent);
                        await outputStream.WriteAsync(bytes, cancellationToken);
                        await outputStream.FlushAsync(cancellationToken);
                    }
                }
                catch (JsonException)
                {
                    // Línea malformada del microservicio, ignorar y continuar
                }
            }
        }

        /// <summary>
        /// Sube la historia clínica de un paciente (PDF) y la vectoriza en Qdrant.
        /// </summary>
        public async Task<HistoriaResponse?> CargarHistoriaAsync(string pacienteId, Stream archivo, string nombreArchivo)
        {
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(archivo);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            form.Add(fileContent, "file", nombreArchivo);

            var response = await _http.PostAsync($"/paciente/{pacienteId}/historia", form);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<HistoriaResponse>(_jsonOptions);
        }

        /// <summary>
        /// Vectoriza historia clínica a partir de texto plano.
        /// </summary>
        public async Task<HistoriaResponse?> CargarHistoriaTextoAsync(
            string pacienteId,
            string textoHistoria,
            string fuente = "historia_clinica")
        {
            var body = new { texto = textoHistoria, fuente };
            var response = await _http.PostAsJsonAsync(
                $"/paciente/{pacienteId}/historia/texto", body, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<HistoriaResponse>(_jsonOptions);
        }

        /// <summary>
        /// Elimina el historial vectorizado de un paciente en Qdrant.
        /// </summary>
        public async Task EliminarHistoriaAsync(string pacienteId)
        {
            var response = await _http.DeleteAsync($"/paciente/{pacienteId}/historia");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Verifica si la API RAG está disponible.
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _http.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
