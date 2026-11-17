using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Entidades
{
    public enum Severidad { Info, Baja, Media, Alta, Critica }

    public enum EstadoAgente { Pendiente, EnEjecucion, Ok, Fallido, TiempoAgotado }

    // Un problema encontrado por un subagente (o por su herramienta regex)
    public class Hallazgo
    {
        public string Agente { get; set; } = "";
        [BsonRepresentation(BsonType.String)]
        public Severidad Severidad { get; set; }
        public int? Linea { get; set; }
        public string Regla { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Sugerencia { get; set; } = "";
    }

    // Lo que hizo UN subagente dentro de una revisión (trazabilidad R8)
    public class EjecucionAgente
    {
        public string Agente { get; set; } = "";
        [BsonRepresentation(BsonType.String)]
        public EstadoAgente Estado { get; set; } = EstadoAgente.Pendiente;
        public long DuracionMs { get; set; }
        public int Tokens { get; set; }
        public int Intentos { get; set; }
        public string? Error { get; set; }
        public string? RespuestaCruda { get; set; }
        public List<Hallazgo> Hallazgos { get; set; } = new();
    }

    // Decisión humana (R7)
    public class Decision
    {
        public bool Aprobado { get; set; }
        public string Revisor { get; set; } = "";
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }

    // Documento que se guarda en MongoDB (colección "revisiones")
    public class Revision
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Codigo { get; set; } = "";
        public string Lenguaje { get; set; } = "csharp";
        public string Autor { get; set; } = "";
        public string Estado { get; set; } = "Pendiente";   // lo maneja el patrón State (CapaNegocios/Estados)
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public List<EjecucionAgente> Ejecuciones { get; set; } = new();
        public List<Hallazgo> HallazgosFinales { get; set; } = new();
        public string? Veredicto { get; set; }              // "pass" | "fail"
        public Decision? Decision { get; set; }
    }
}
