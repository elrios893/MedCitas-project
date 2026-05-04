using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.DTOs
{
    public class ContextoClinico
    {
        public string? Nombre { get; set; }
        public int? Edad { get; set; }
        public string? Sexo { get; set; }
        public List<string> Diagnosticos { get; set; } = new();
        public List<string> MedicamentosActuales { get; set; } = new();
        public List<string> Alergias { get; set; } = new();
        public string? NotasAdicionales { get; set; }
    }

    public class ConsultaRequest
    {
        public string Pregunta { get; set; } = string.Empty;
        public string? PacienteId { get; set; }
        public ContextoClinico? ContextoClinico { get; set; }
        public string? FiltroTipo { get; set; }
        public int TopK { get; set; } = 6;
        public bool MultiQuery { get; set; } = true;
    }

    public class FuenteChunk
    {
        public string Medicamento { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public float Score { get; set; }
        public int Pagina { get; set; }
        public bool VigilanciaIntensiva { get; set; }
    }

    public class ConsultaResponse
    {
        public string Respuesta { get; set; } = string.Empty;
        public List<FuenteChunk> Fuentes { get; set; } = new();
        public List<string> SubQueriesUsadas { get; set; } = new();
        public int TiempoMs { get; set; }
    }

    public class HistoriaResponse
    {
        public string PacienteId { get; set; } = string.Empty;
        public int ChunksCreados { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
