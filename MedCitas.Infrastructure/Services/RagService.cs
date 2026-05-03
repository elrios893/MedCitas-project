using MedCitas.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        /// Consulta RAG con contexto del paciente. Devuelve la respuesta de phi3.
        /// </summary>
        public async Task<ConsultaResponse?> ConsultarAsync(ConsultaRequest request)
        {
            var response = await _http.PostAsJsonAsync("/consulta", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConsultaResponse>(_jsonOptions);
        }

        /// <summary>
        /// Sube la historia clínica de un paciente (PDF o TXT) y la vectoriza.
        /// </summary>
        public async Task<HistoriaResponse?> CargarHistoriaAsync(string pacienteId, Stream archivo, string nombreArchivo)
        {
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(archivo);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                nombreArchivo.EndsWith(".pdf") ? "application/pdf" : "text/plain"
            );
            form.Add(fileContent, "file", nombreArchivo);

            var response = await _http.PostAsync($"/paciente/{pacienteId}/historia", form);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<HistoriaResponse>(_jsonOptions);
        }

        /// <summary>
        /// Elimina el historial vectorizado de un paciente.
        /// </summary>
        public async Task EliminarHistoriaAsync(string pacienteId)
        {
            var response = await _http.DeleteAsync($"/paciente/{pacienteId}/historia");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Verifica si la API está disponible.
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
