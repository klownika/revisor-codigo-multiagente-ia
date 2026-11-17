using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Persistencia
{
    public sealed class ConexionMongo
    {
        private static ConexionMongo? unicaInstancia = null;
        private static readonly object candado = new();

        public IMongoDatabase BaseDatos { get; }

        private ConexionMongo(string cadenaConexion, string nombreBaseDatos)
        {
            BaseDatos = new MongoClient(cadenaConexion).GetDatabase(nombreBaseDatos);
        }

        public static ConexionMongo dameInstancia(string cadenaConexion, string nombreBaseDatos)
        {
            lock (candado)   // a diferencia del lab, aquí hay varios hilos (agentes en paralelo)
            {
                if (unicaInstancia == null) unicaInstancia = new ConexionMongo(cadenaConexion, nombreBaseDatos);
                return unicaInstancia;
            }
        }
    }
}
