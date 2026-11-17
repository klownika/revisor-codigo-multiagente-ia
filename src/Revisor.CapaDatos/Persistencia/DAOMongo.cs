using MongoDB.Driver;
using Revisor.CapaDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Persistencia
{
    public class DAOMongo : AbstractDAO
    {
        private readonly IMongoCollection<Revision> revisiones;

        public DAOMongo(IMongoDatabase baseDatos)
        {
            revisiones = baseDatos.GetCollection<Revision>("revisiones");
        }

        public override Task Insertar(Revision revision) => revisiones.InsertOneAsync(revision);

        public override Task Actualizar(Revision revision) =>
            revisiones.ReplaceOneAsync(r => r.Id == revision.Id, revision);

        public override async Task<Revision?> Obtener(string id) =>
            await revisiones.Find(r => r.Id == id).FirstOrDefaultAsync();

        public override Task<List<Revision>> Listar(int limite = 50) =>
            revisiones.Find(_ => true).SortByDescending(r => r.FechaCreacion).Limit(limite).ToListAsync();
    }
}
