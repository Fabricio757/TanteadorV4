using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Threading.Tasks;
using System.Reflection;


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

        public SqlPersistJOIN JOIN = new SqlPersistJOIN();


        public SqlPersist(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ObjDeportes>().Wait();
            database.CreateTableAsync<ObjTorneos>().Wait();
            database.CreateTableAsync<ObjZonas>().Wait();
            database.CreateTableAsync<ObjEquipos>().Wait();
            database.CreateTableAsync<ObjListaEquipos>().Wait();
            database.CreateTableAsync<ObjPartidos>().Wait();
            database.CreateTableAsync<ObjJugadores>().Wait();
            database.CreateTableAsync<ObjDeportes>().Wait();

            //database.CreateTableAsync<SqlPersistZonas>().Wait();

            Torneos.database = database;
            Zonas.database = database;
            Equipos.database = database;
            ListaEquipos.database = database;
            Partidos.database = database;
            Jugadores.database = database;

            JOIN.database = database;
        }
    }

    #region PERSISTS
    public class SqlPersistObject
    {
        public SQLiteAsyncConnection database { get; set; }
        //public Dictionary<int, string> ParametrosQry = new Dictionary<int, string>();

        public ObjId Objeto { set; get; }

        public string[] NombresParametros = new string[0]; 
        public Object[] ValoresParametros = new Object [0];
                
        public void AddParametrosString(string value) { NombresParametros = value.Split(','); } 
        public void AddParametros(string[] pNombreParametros, Object[] pValoresParametros) { NombresParametros = pNombreParametros; ValoresParametros = pValoresParametros; }
        public void AddParametro_Only(string pNombre, object pValor) { NombresParametros = new string[] { pNombre };  ValoresParametros = new object[] { pValor }; }


        public Task<int> UpdateItemAsync(ObjId item)
        {
            string v = "";
            v.Split(',');
            return database.UpdateAsync(item);
        }

        public Task<int> InsertItemAsync(ObjId item)
        {            
            return database.InsertAsync(item);
        }

        public Task<int> DeleteItemAsync(ObjId Item)
        {
            return database.DeleteAsync(Item);
        }

        public virtual async Task<List<ObjId>> GetItemsAsync()
        {
            return null;
        }

        public virtual async Task<ObjId> GetItemAsync()
        {
            //return await database.FindAsync<ObjId>(Objeto.ID);
            return null;
        }

        public void ObjetoToParametros(Type myType, ObjId p_objTipo)
        {//Carga la lista de propiedades del objeto en la lista de parametros para realizar el query
            int i = 0;
            ValoresParametros = new object[NombresParametros.Length];

            if (Objeto != null)
            {
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                foreach (string Parametro in NombresParametros)
                {
                    foreach (PropertyInfo prop in props)
                        if (Parametro == prop.Name)
                        {
                            ValoresParametros[i] = prop.GetValue(Objeto, null);
                            i++;
                        }
                }
            }
        }

        public void ObjetoToParametros()
        {//Carga la lista de propiedades del objeto en la lista de parametros para realizar el query            
            ObjetoToParametros(Objeto.GetType(), Objeto);
        }

        protected string ArmarParametros()
        {//Arma el where de acuerdo a los Parametros
            Boolean laPrimera = true;
            int i = 0;
            string sqlQ = "";

            if (NombresParametros != null)
                foreach (string Parametro in NombresParametros)
                {
                    if (laPrimera)
                    {
                        sqlQ = sqlQ + " WHERE ";
                        laPrimera = false;
                    }
                    else
                        sqlQ = sqlQ + " AND ";

                    sqlQ = sqlQ + NombresParametros[i] + " = ? ";
                    i++;
                }

            return sqlQ;
        }
    }

    public class SqlPersistJOIN : SqlPersistObject
    {
        public Task<List<ObjPartidos>> GetPartidos_Zona_Async(string addQryString = "")
        {
            string sqlQ = "select P.* from Partidos P inner join Zonas Z on P.IdZona = Z.id " + ArmarParametros() + " " + addQryString;
            var Resultado = database.QueryAsync<ObjPartidos>(sqlQ, ValoresParametros);

            return Resultado;
        }


        public Task<List<ObjEquipos>> GetEquipos_ListaEquiposAsync(string addQryString = "")
        {
            try
            {
                string sqlQ = "select E.* from Equipos E inner join ListaEquipos L on L.IdEquipo = E.Id  " + ArmarParametros() + " " + addQryString;
                var Resultado = database.QueryAsync<ObjEquipos>(sqlQ, ValoresParametros);

                return Resultado;
            }
            catch (Exception Exc)
            {
                throw Exc;
            }
        }

      
    }

    public class SqlPersistTorneos : SqlPersistObject
    {
        public ObjTorneos oTorneo { set { Objeto = value; } get { return (ObjTorneos)Objeto; } }
                
        public SqlPersistTorneos() : base()
        {//Constructor
            Objeto = new ObjTorneos();
        }

        public override async Task<List<ObjId>> GetItemsAsync()
        {
            try
            {
                List<ObjTorneos> R = await GetTorneosAsync();
                var Resultado = R.ConvertAll(X => (ObjId)X);

                return Resultado;
            }
            catch (Exception Exc)
            {
                throw Exc;
            }
        }

        public async Task<List<ObjTorneos>> GetTorneosAsync(string addQryString = "")
        {
            try
            {
                string sqlQ = "select * from Torneos " + ArmarParametros() + " " + addQryString;

                List<ObjTorneos> Resultado = await database.QueryAsync<ObjTorneos>(sqlQ, ValoresParametros);

                return Resultado;
            }
            catch (Exception Exc)
            {
                throw Exc;
            }
        }

        public async void BorrarTorneoGenerado()
        {
            string strSql = "update Equipos set IdZona = null where IdTorneo = " + this.oTorneo.ID.ToString();
            await database.ExecuteAsync(strSql);

            strSql = "update Zonas set IdEquipoCabezaDeSerie = null where IdTorneo = " + this.oTorneo.ID.ToString();
            await database.ExecuteAsync(strSql);

            strSql = "delete from ListaEquipos where IdZona in (select Id from Zonas where IdTorneo =  " + this.oTorneo.ID.ToString() + ")";
            await database.ExecuteAsync(strSql);

            strSql = "delete from Partidos where IdZona in  (select Id from Zonas where IdTorneo = " + this.oTorneo.ID.ToString() + ")";
            await database.ExecuteAsync(strSql);
        }
    }

    public class SqlPersistZonas : SqlPersistObject
    {
        public ObjZonas oZona { set { Objeto = value;  } get { return (ObjZonas)Objeto; } }

        public SqlPersistZonas() : base()
        {
            Objeto = new ObjZonas();
        }

        public override async Task<List<ObjId>> GetItemsAsync()
        {
            List<ObjZonas> R = await GetZonasAsync();
            var Resultado = R.ConvertAll(X => (ObjId)X);

            return Resultado;
        }
        
        public async Task<List<ObjZonas>> GetZonasAsync()
        {
            string sqlQ = "select * from Zonas " + ArmarParametros();
            List<ObjZonas> Resultado = await database.QueryAsync<ObjZonas>(sqlQ, ValoresParametros);

            return Resultado;
        }
        /*
        public List<ObjEquipos> Equipos(ObjZonas Zona)
        {//Retorna los equipos de la zona
            string sqlQ = "select * from[ObjEquipos] ";
            sqlQ = sqlQ + " WHERE IdZona = " + Zona.ID.ToString();

            var Resultado = database.QueryAsync<ObjEquipos>(sqlQ);
            return Resultado.Result;
        }

        public async Task<List<ObjEquipos>> MisEquipos(ObjZonas Zona)
        {
            string sqlQ = "select * from[ObjEquipos] ";
            sqlQ = sqlQ + " WHERE IdZona = " + Zona.ID.ToString();

            var Resultado = await database.QueryAsync<ObjEquipos>(sqlQ);
            return Resultado;
        }

        public List<ObjEquipos> MisEquipos_Sync(ObjZonas Zona)
        {
            //return this.MisEquipos(Zona).Result;
            string sqlQ = "select * from[ObjEquipos] ";
            sqlQ = sqlQ + " WHERE IdZona = " + Zona.ID.ToString();

            var Resultado = database.QueryAsync<ObjEquipos>(sqlQ);
            return Resultado.Result;            
        }*/
    }

    public class SqlPersistEquipos : SqlPersistObject
    {
        public ObjEquipos oEquipo { set { Objeto = value; } get { return (ObjEquipos)Objeto; } }
            
        public SqlPersistEquipos() : base()
        {
            Objeto = new ObjEquipos();
        }


        public override async Task<List<ObjId>> GetItemsAsync()
        {
            List<ObjEquipos> R = await GetEquiposAsync();
            var Resultado = R.ConvertAll(X => (ObjId)X);

            return Resultado;
        }

        public async Task<List<ObjEquipos>> GetEquiposAsync(string addQryString = "")
        {
            string sqlQ = "select * from Equipos " + ArmarParametros() + " " + addQryString; 
            List<ObjEquipos> Resultado = await database.QueryAsync<ObjEquipos>(sqlQ, ValoresParametros);

            return Resultado;
        }
        /*
        public ObjEquipos GetEquipo()
        {            
            var Result = database.GetAsync<ObjEquipos>(oEquipo.ID);
            return (ObjEquipos)Result.Result;
        }*/

        public virtual async Task<ObjEquipos> GetItemAsync()
        {
            return await database.FindAsync<ObjEquipos>(Objeto.ID);
        }

        public async Task<int> Load()
        {
            this.oEquipo =  await database.FindAsync<ObjEquipos>(Objeto.ID);
            return 0;
        }
    }

    public class SqlPersistListaEquipos : SqlPersistObject
    {
        //public SQLiteAsyncConnection database;

        public async Task<List<ObjId>> GetItemsAsync_EquiposZona(int IdZona)
        {
            var Resultado = await database.QueryAsync<ObjEquipos>("SELECT E.* FROM [Equipos] E inner join [ListaEquipos] L on E.Id = L.IdEquipo WHERE L.[Idzona] = " + IdZona.ToString());

            List<ObjId> R = Resultado.ConvertAll(X => (ObjId)X);
            return R;
        }

        public Task<int> UpdateItemAsync(ObjListaEquipos item)
        {
            return database.UpdateAsync(item);
        }

        public Task<int> DeleteItemListaEquipo(ObjListaEquipos item)
        {
            database.ExecuteAsync("UPDATE ObjEquipos SET IdZona = null WHERE Id = " + item.IdEquipo.ToString());
            return database.ExecuteAsync("DELETE FROM [ListaEquipos] WHERE [IdEquipo] = " + item.IdEquipo.ToString() + " and [IdZona] = " + item.IdZona.ToString());
        }

        public Task<int> InsertItemAsync(ObjListaEquipos item)
        { 
            return database.InsertAsync(item);
        }

        public Task<List<ObjEquipos>> GetItemsAsync(int p_IdZona = 0)
        {
            string strSql = "SELECT E.*, E.Nombre Display FROM [ListaEquipos] LE inner join [Equipos] E on LE.IdEquipo = E.ID";
            if (p_IdZona > 0)
            {
                strSql = strSql + " WHERE LE.[IdZona] = " + p_IdZona.ToString();
            }
            return database.QueryAsync<ObjEquipos>(strSql);
        }
    }

    public class SqlPersistPartidos : SqlPersistObject
    {
        public ObjPartidos oPartido { set { Objeto = value; } get { return (ObjPartidos)Objeto; } }

        public SqlPersistPartidos() : base()
        {
            Objeto = new ObjPartidos();
        }

        public override async Task<List<ObjId>> GetItemsAsync()
        {
            var R = await GetPartidosAsync();
            var Resultado = R.ConvertAll(X => (ObjId)X);

            return Resultado;
        }

        public async Task<List<ObjPartidos>> GetPartidosAsync()
        {
            string sqlQ = "select P.* from Partidos P " + ArmarParametros() + " order by P.FechaOrden ";
            List<ObjPartidos> Resultado = await database.QueryAsync<ObjPartidos>(sqlQ, ValoresParametros);

            return Resultado;
        }
        /*    
     public List<ObjEquipos> MisEquipos
     {
         get {
             string sqlQ = "select E.* from[ObjEquipos] E inner join [ObjListaEquipos] L on L.IdEquipo = E.Id ";
             sqlQ = sqlQ + " WHERE L.[IdZona] = " + ((ObjZonas)Filtro_GetItemsAsync).ID.ToString();

             var Resultado = database.QueryAsync<ObjEquipos>(sqlQ);
             return Resultado.Result;
             }
     }*/

    }

    public class SqlPersistJugadores : SqlPersistObject
    {
        public ObjJugadores oJugadores { set { Objeto = value; } get { return (ObjJugadores)Objeto; } }

        public SqlPersistJugadores() : base()
        {
            Objeto = new ObjJugadores();
        }

        public Task<List<ObjJugadores>> GetEquipos_Async()
        {
            string sqlQ = "select * from Jugadores " + ArmarParametros();

            var Resultado = database.QueryAsync<ObjJugadores>(sqlQ, ValoresParametros);
            return Resultado;
        }

        
      
    }

    #endregion

    #region OBJETOS

    public class ObjId
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Nombre { get; set; }
    }

    [Table("Deportes")]
    public class ObjDeportes : ObjId
    {
        public string Descripcion {get; set; }
        public string PathImagen { get; set; }
    }

    [Table("Torneos")]
    public class ObjTorneos : ObjId
    {
        public int IdDeporte { get; set; }

        public DateTime Fecha { get; set; }
        [NotNull]
        public string Titulo1 { get; set; }
        public string Titulo2 { get; set; }

        public string pathFoto { get; set; }
        public string Lugar { get; set; }

        public int CantidadClasificadosXZona { get; set; }
        public int IdaYVuelta { get; set; }
        public int IdaYVuelta_Llave { get; set; }

        public int ReadOnly { get; set; }
    }

    [Table("Zonas")]
    public class ObjZonas : ObjId
    {
        public int IdTorneo { get; set; }
        public Boolean esLLave { get; set; }
        public int IdEquipoCabezaDeSerie { get; set; }
    }

    [Table("Equipos")]
    public class ObjEquipos : ObjId
    {
        public int IdTorneo { get; set; }
        public int IdZona { get; set; }
        public string pathFoto { get; set; }
    }

    [Table("ListaEquipos")]
    public class ObjListaEquipos 
    {
        public int IdZona { get; set; }
        public int IdEquipo { get; set; }
    }

    [Table("Partidos")]
    public class ObjPartidos : ObjId
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

    [Table("Jugadores")]
    public class ObjJugadores : ObjId
    {
        public string pathFoto { get; set; }
        public int IdEquipo { get; set; }
    }

    #endregion


}
