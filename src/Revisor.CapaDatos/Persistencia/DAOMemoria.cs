using Revisor.CapaDatos.Entidades;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Persistencia
{
    public class DAOMemoria : AbstractDAO
    {
        private readonly ConcurrentDictionary<string, Revision> datos = new();

        public override Task Insertar(Revision revision) { datos[revision.Id] = revision; return Task.CompletedTask; }
        public override Task Actualizar(Revision revision) { datos[revision.Id] = revision; return Task.CompletedTask; }
        public override Task<Revision?> Obtener(string id) =>
            Task.FromResult(datos.TryGetValue(id, out var r) ? r : null);
        public override Task<List<Revision>> Listar(int limite = 50) =>
            Task.FromResult(datos.Values.OrderByDescending(r => r.FechaCreacion).Take(limite).ToList());
    }
}
