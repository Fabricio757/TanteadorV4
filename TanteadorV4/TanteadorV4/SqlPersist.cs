using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Threading.Tasks;

namespace TanteadorV4
{


    public class SqlPersist
    {
        private SQLiteAsyncConnection database;
        public SqlPersistTorneos Torneos = new SqlPersistTorneos();
        public SqlPersistZonas Zonas = new SqlPersistZonas();
        public SqlPersistEquipos Equipos = new SqlPersistEquipos();
        public SqlPersistListaEquipos ListaEquipos = new SqlPersistListaEquipos();
        public SqlPersistPartidos Partidos = new SqlPersistPartidos();
        public SqlPersistJugadores Jugadores = new SqlPersistJugadores();


        public SqlPersist(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<objTorneos>().Wait();
            database.CreateTableAsync<objZonas>().Wait();
            database.CreateTableAsync<objEquipos>().Wait();
            database.CreateTableAsync<objListaEquipos>().Wait();
            database.CreateTableAsync<objPartidos>().Wait();
            database.CreateTableAsync<objJugadores>().Wait();

            //database.CreateTableAsync<SqlPersistZonas>().Wait();

            Torneos.database = database;
            Zonas.database = database;
            Equipos.database = database;
            ListaEquipos.database = database;
            Partidos.database = database;
            Jugadores.database = database;
        }
    }

    #region PERSISTS
    public class SqlPersistObject
    {
        public SQLiteAsyncConnection database { get; set; }

        public Object Filtro_GetItemsAsync { set; get; }

        public Task<int> UpdateItemAsync(objId item)
        {
            return database.UpdateAsync(item);
        }

        public Task<int> InsertItemAsync(objId item)
        {
            return database.InsertAsync(item);
        }

        public Task<int> DeleteItemAsync(objId Item)
        {
            return database.DeleteAsync(Item);
        }
        
        public virtual async Task<List<objId>> GetItemsAsync()
        {
            return null;
        }
    }


    public class SqlPersistTorneos : SqlPersistObject
    {
        public override async Task<List<objId>> GetItemsAsync()
        {
            List<objTorneos> Resultado = await database.QueryAsync<objTorneos>("SELECT T.* FROM [objTorneos] T order by nombre");
            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public async Task<List<objEquipos>> MisEquipos(objTorneos objTorneo)
        {
            string strSql = "select * from objEquipos where IdTorneo = " + objTorneo.ID.ToString();
            return await database.QueryAsync<objEquipos>(strSql);
        }

        public async Task<List<objEquipos>> MisEquiposQueNoSonCabecera(objTorneos objTorneo)
        {
            string strSql = "select * from objEquipos where IdTorneo = " + objTorneo.ID.ToString();            
            strSql = strSql + " and Id not in (select ifnull(IdEquipoCabezaDeSerie,0) from objZonas where idTorneo = " + objTorneo.ID.ToString() + ") ";
           

            return await database.QueryAsync<objEquipos>(strSql);
        }

        public async Task<List<objEquipos>> MisEquiposDisponibles(objTorneos objTorneo, bool Cabeceras = false)
        {
            string strSql = "select * from EquiposDisponibles where IdTorneo = " + objTorneo.ID.ToString();
            return await database.QueryAsync<objEquipos>(strSql);
        }

        public async Task<List<objZonas>> MisZonas(objTorneos objTorneo)
        {
            string strSql = "select * from objZonas where IdTorneo = " + objTorneo.ID.ToString();

            return await database.QueryAsync<objZonas>(strSql);
        }

        public Boolean TienePartidos(objTorneos objTorneo)
        {
            string strSql = "select count(1) from objPartidos P inner join objZonas Z on P.IdZona = Z.id where Z.IdTorneo = " + objTorneo.ID.ToString();

            int R = database.ExecuteScalarAsync<int>(strSql).Result;

            return (R > 0);
        }

        public void BorrarPartidos(objTorneos objTorneo)
        {
            string strSql = "update objEquipos set IdZona = null where IdTorneo = " + objTorneo.ID.ToString();
            database.ExecuteAsync(strSql);

            strSql = "update objZonas set IdEquipoCabezaDeSerie = null where IdTorneo = " + objTorneo.ID.ToString();
            database.ExecuteAsync(strSql);

            strSql = "delete from objListaEquipos where IdZona in (select Id from objZonas where IdTorneo =  " + objTorneo.ID.ToString() + ")";
            database.ExecuteAsync(strSql);

            strSql = "delete from objPartidos where IdZona in  (select Id from objZonas where IdTorneo = " + objTorneo.ID.ToString() + ")";
            database.ExecuteAsync(strSql);
        }
    }

    public class SqlPersistZonas : SqlPersistObject
    {
        
        public override async Task<List<objId>> GetItemsAsync()
        {
            List<objZonas> Resultado = await database.QueryAsync<objZonas>("SELECT Z.*, Z.nombre Display FROM [objZonas] Z WHERE [IdTorneo] = " + ((objTorneos)Filtro_GetItemsAsync).ID.ToString());
            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public List<objEquipos> Equipos(objZonas Zona)
        {/*Retorna los equipos de la zona*/
            string sqlQ = "select * from[objEquipos] ";
            sqlQ = sqlQ + " WHERE IdZona = " + Zona.ID.ToString();

            var Resultado = database.QueryAsync<objEquipos>(sqlQ);
            return Resultado.Result;
        }


        public List<objEquipos> MisEquiposAsignados(objZonas Zona)
        {
                //objTorneos Torneo = (objTorneos)Filtro_GetItemsAsync;
                string sqlQ = "select * from[objEquipos] ";
                sqlQ = sqlQ + " WHERE IdZona = " + Zona.ID.ToString();

                var Resultado = database.QueryAsync<objEquipos>(sqlQ);
                return Resultado.Result;
            
        }
    }

    public class SqlPersistEquipos : SqlPersistObject
    {
        public override async Task<List<objId>> GetItemsAsync()
        {
            //return database.Table<objZonas>().ToListAsync();
            var Resultado = await database.QueryAsync<objEquipos>("SELECT E.*, E.nombre Display FROM [objEquipos] E WHERE [IdTorneo] = " + ((objTorneos)Filtro_GetItemsAsync).ID.ToString());

            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public Task<List<objEquipos>> GetEquiposSinZonaAsync(int p_IdTorneo)
        {
            //return database.Table<objZonas>().ToListAsync();            
            return database.QueryAsync<objEquipos>("SELECT * FROM [objEquipos] WHERE [IdZona] = 0 and [IdTorneo] = " + ((objTorneos)Filtro_GetItemsAsync).ID.ToString());

        }

        public Task<objEquipos> GetAsync(int ID)
        {
            return database.GetAsync<objEquipos>(ID);
        }

        public objEquipos GetEquipo(int ID)
        {
            var Result = database.GetAsync<objEquipos>(ID);
            return (objEquipos)Result.Result;
        }
    }

    public class SqlPersistListaEquipos
    {
        public SQLiteAsyncConnection database;

        public async Task<List<objId>> GetItemsAsync_EquiposDisponibles(int IdTorneo)
        {
            string strSql = "SELECT * FROM EquiposDisponibles WHERE [IdTorneo] = " + IdTorneo.ToString();

            var Resultado = await database.QueryAsync<objEquipos>(strSql);

            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public async Task<List<objId>> GetItemsAsync_EquiposZona(int IdZona)
        {
            var Resultado = await database.QueryAsync<objEquipos>("SELECT E.* FROM [objEquipos] E inner join [objListaEquipos] L on E.Id = L.IdEquipo WHERE L.[Idzona] = " + IdZona.ToString());

            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public Task<int> UpdateItemAsync(objListaEquipos item)
        {
            return database.UpdateAsync(item);
        }

        public Task<int> DeleteItemListaEquipo(objListaEquipos item)
        {
            return database.ExecuteAsync("DELETE FROM [objListaEquipos] WHERE [IdEquipo] = " + item.IdEquipo.ToString() + " and [IdZona] = " + item.IdZona.ToString());
        }

        public Task<int> InsertItemAsync(objListaEquipos item)
        {
            return database.InsertAsync(item);
        }

        public Task<List<objEquipos>> GetItemsAsync(int p_IdZona = 0)
        {
            string strSql = "SELECT E.*, E.Nombre Display FROM [objListaEquipos] LE inner join [objEquipos] E on LE.IdEquipo = E.ID";
            if (p_IdZona > 0)
            {
                strSql = strSql + " WHERE LE.[IdZona] = " + p_IdZona.ToString();
            }
            return database.QueryAsync<objEquipos>(strSql);
        }
    }

    public class SqlPersistPartidos : SqlPersistObject
    {
        public override async Task<List<objId>> GetItemsAsync()
        {
            string S = "select P.*, P.nombre || ' fecha: ' || P.fecha Display FROM [objPartidos] P ";
            S = S + " WHERE P.[IdZona] = " + ((objZonas)Filtro_GetItemsAsync).ID.ToString() + " order by FechaOrden";

            List<objPartidos> Resultado = await database.QueryAsync<objPartidos>(S);
            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }


        public List<objEquipos> MisEquipos
        {
            get {
                string sqlQ = "select E.* from[objEquipos] E inner join [objListaEquipos] L on L.IdEquipo = E.Id ";
                sqlQ = sqlQ + " WHERE L.[IdZona] = " + ((objZonas)Filtro_GetItemsAsync).ID.ToString();

                var Resultado = database.QueryAsync<objEquipos>(sqlQ);
                return Resultado.Result;
                }
        }

    }

    public class SqlPersistJugadores : SqlPersistObject
    {
        public override async Task<List<objId>> GetItemsAsync()
        {
            List<objJugadores> Resultado = await database.QueryAsync<objJugadores>("SELECT * FROM [objJugadores] WHERE [IdEquipo] = " + ((objEquipos)Filtro_GetItemsAsync).ID.ToString());
            List<objId> R = Resultado.ConvertAll(X => (objId)X);
            return R;
        }

        public List<objEquipos> MisEquipos
        {
            get
            {
                objEquipos Equipo = (objEquipos)Filtro_GetItemsAsync;
                string sqlQ = "select * from[objEquipos] ";
                sqlQ = sqlQ + " WHERE IdTorneo = " + Equipo.IdTorneo.ToString();

                var Resultado = database.QueryAsync<objEquipos>(sqlQ);
                return Resultado.Result;
            }
        }
    }

    #endregion

    #region Objetos
    public class objId
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Nombre { get; set; }
    }


    public class objTorneos : objId
    {
        public DateTime Fecha { get; set; }
        public string Titulo1 { get; set; }
        public string Titulo2 { get; set; }
        public string pathFoto { get; set; }
        public string Lugar { get; set; }
    }

    public class objZonas : objId
    {
        public int IdTorneo { get; set; }
        public Boolean esLLave { get; set; }
        public int IdEquipoCabezaDeSerie { get; set; }
    }

    public class objEquipos : objId
    {
        public int IdTorneo { get; set; }
        public int IdZona { get; set; }
        public string pathFoto { get; set; }
    }

    public class objListaEquipos 
    {
        public int IdZona { get; set; }
        public int IdEquipo { get; set; }
    }

    public class objPartidos : objId
    {
        public int IdEquipo1 { get; set; }
        public int IdEquipo2 { get; set; }
        public int FechaOrden { get; set; }
        public string Resultado { get; set; }
        public int GolesEquipo1 { get; set; }
        public int GolesEquipo2 { get; set; }
        public int IdZona { get; set; }
        public string PartidoRevancha { get; set; }
        public DateTime Reloj { get; set; }
    }

    public class objJugadores : objId
    {
        public string pathFoto { get; set; }
        public int IdEquipo { get; set; }
    }
    #endregion


}
