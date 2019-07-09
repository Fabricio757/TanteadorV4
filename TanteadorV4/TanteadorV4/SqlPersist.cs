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

        protected string NombreTabla = ""; 

        public ObjId Objeto { set; get; }

        public string[] NombresParametros = new string[0];
        public string[] ConectoresParametros = new string[0];
        public Object[] ValoresParametros = new Object [0];
                
        public void AddParametrosString(string value) { NombresParametros = value.Split(','); }
        public void AddConectoresParametrosString(string value) { ConectoresParametros = value.Split(','); }
        public void AddParametros(string[] pNombreParametros, Object[] pValoresParametros, string[] pConectoresParametros = null) { NombresParametros = pNombreParametros; ValoresParametros = pValoresParametros; ConectoresParametros = pConectoresParametros; }
        public void AddParametro_Only(string pNombre, object pValor) { NombresParametros = new string[] { pNombre };  ValoresParametros = new object[] { pValor }; }


        public Task<int> UpdateItemAsync(ObjId item)
        {
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

        public virtual async Task<Boolean> Load()
        {
                return false;
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

        protected string Conector_i (int i)
        {
            string Conector = " = ? ";
            try
            {
                if (ConectoresParametros[i] != "")
                    Conector = ConectoresParametros[i];
            }
            catch
            {
                Conector = " = ? ";
            }

            return Conector;
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

                    sqlQ = sqlQ + NombresParametros[i] + Conector_i(i);
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

        public Task<List<ObjEquipoListaEquipos>> GetEquiposListaEquiposAsync(string addQryString = "")
        {
            try
            {
                string sqlQ = "select E.Nombre, E.ID, L.Puntos, L.IdZona from Equipos E inner join ListaEquipos L on L.IdEquipo = E.Id  " + ArmarParametros() + " " + addQryString;
                var Resultado = database.QueryAsync<ObjEquipoListaEquipos>(sqlQ, ValoresParametros);

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
            NombreTabla = "Torneos";
            Objeto = new ObjTorneos();
        }

        public override async Task<Boolean> Load()
        {
            try
            {
                Objeto = await database.GetAsync<ObjTorneos>(Objeto.ID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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

            //borrar las zonas de nivel 1+
            strSql = "delete from Zonas where NivelLlave > 0 and IdTorneo = " + this.oTorneo.ID.ToString();
            await database.ExecuteAsync(strSql);
        }

    }

    public class SqlPersistZonas : SqlPersistObject
    {
        public ObjZonas oZona { set { Objeto = value;  } get { return (ObjZonas)Objeto; } }

        public SqlPersistZonas() : base()
        {
            NombreTabla = "Zonas";
            Objeto = new ObjZonas();
        }

        public override async Task<Boolean> Load()
        {
            try
            {
                Objeto = await database.GetAsync<ObjZonas>(Objeto.ID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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


    }

    public class SqlPersistEquipos : SqlPersistObject
    {
        public ObjEquipos oEquipo { set { Objeto = value; } get { return (ObjEquipos)Objeto; } }
            
        public SqlPersistEquipos() : base()
        {
            NombreTabla = "Equipos";
            Objeto = new ObjEquipos();
        }


        public override async Task<List<ObjId>> GetItemsAsync()
        {
            List<ObjEquipos> R = await GetEquiposAsync();
            var Resultado = R.ConvertAll(X => (ObjId)X);

            return Resultado;
        }


        public override async Task<Boolean> Load()
        {
            try
            {
                Objeto = await database.GetAsync<ObjEquipos>(Objeto.ID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<ObjEquipos>> GetEquiposAsync(string addQryString = "")
        {
            string sqlQ = "select * from Equipos " + ArmarParametros() + " " + addQryString; 
            List<ObjEquipos> Resultado = await database.QueryAsync<ObjEquipos>(sqlQ, ValoresParametros);

            return Resultado;
        }


        public async Task<int> CantidadPartidosGanados(int IdZona)
        {
            string sqlQ = "select count(1) from Partidos P where Finalizado = 1 and IdZona = ? ";
            sqlQ = sqlQ + " and (IdEquipo1= ? and GolesEquipo1 > GolesEquipo2 or IdEquipo2 = ? and GolesEquipo1 < GolesEquipo2)";

            int Resultado = await database.ExecuteScalarAsync<int>(sqlQ, new object[] { IdZona, oEquipo.ID, oEquipo.ID });

            return Resultado;
        }

        public async Task<int> CantidadPartidosEmpatados(int IdZona)
        {

            string sqlQ = "select count(1) from Partidos P where Finalizado = 1 and IdZona = ? ";
            sqlQ = sqlQ + " and (IdEquipo1= ? and GolesEquipo1 = GolesEquipo2 or IdEquipo2 = ? and GolesEquipo1 = GolesEquipo2)";

            int Resultado = await database.ExecuteScalarAsync<int>(sqlQ, new object[] { IdZona, oEquipo.ID, oEquipo.ID });

            return Resultado;
        }

        public async Task<int> CantidadPartidosPerdidos(int IdZona)
        {
            string sqlQ = "select count(1) from Partidos P where Finalizado = 1 and IdZona = ? ";
            sqlQ = sqlQ + " and (IdEquipo1= ? and GolesEquipo1 < GolesEquipo2 or IdEquipo2 = ? and GolesEquipo1 > GolesEquipo2)";

            int Resultado = await database.ExecuteScalarAsync<int>(sqlQ, new object[] { IdZona, oEquipo.ID, oEquipo.ID });

            return Resultado;
        }
    }

    public class SqlPersistListaEquipos : SqlPersistObject
    {
        public ObjListaEquipos oListaEquipos { set; get; }

        public SqlPersistListaEquipos() : base()
        {
            NombreTabla = "ListaEquipos";
            oListaEquipos = new ObjListaEquipos();
        }
                
        public async Task<List<ObjId>> GetItemsAsync_EquiposZona(int IdZona)
        {
            var Resultado = await database.QueryAsync<ObjEquipos>("SELECT E.* FROM [Equipos] E inner join [ListaEquipos] L on E.Id = L.IdEquipo WHERE L.[Idzona] = " + IdZona.ToString());

            List<ObjId> R = Resultado.ConvertAll(X => (ObjId)X);
            return R;
        }

        public override async Task<Boolean> Load()
        {
            try
            {
                IList<ObjListaEquipos> Lista = await database.QueryAsync<ObjListaEquipos>("SELECT * FROM ListaEquipos WHERE IdEquipo = "+ oListaEquipos.IdEquipo.ToString() + " and IdZona = " + oListaEquipos.IdZona.ToString());
                oListaEquipos = Lista[0];
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<int> UpdateItemAsync(ObjListaEquipos item)
        {
            return database.ExecuteAsync("Update LISTAEQUIPOS set Puntos = ? where IdEquipo = ? and IdZona = ? ", new object[] { item.Puntos, item.IdEquipo, item.IdZona });
            //return database.UpdateAsync(item);
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

    }

    public class SqlPersistPartidos : SqlPersistObject
    {
        public ObjPartidos oPartido { set { Objeto = value; } get { return (ObjPartidos)Objeto; } }

        public SqlPersistPartidos() : base()
        {
            NombreTabla = "Partidos";
            Objeto = new ObjPartidos();
        }

        public override async Task<Boolean> Load()
        {
            try
            {
                Objeto = await database.GetAsync<ObjPartidos>(Objeto.ID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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
    }

    public class SqlPersistJugadores : SqlPersistObject
    {
        public ObjJugadores oJugadores { set { Objeto = value; } get { return (ObjJugadores)Objeto; } }

        public SqlPersistJugadores() : base()
        {
            NombreTabla = "Jugadores";
            Objeto = new ObjJugadores();
        }


        public Task<List<ObjJugadores>> GetEquipos_Async()
        {
            string sqlQ = "select * from Jugadores " + ArmarParametros();

            var Resultado = database.QueryAsync<ObjJugadores>(sqlQ, ValoresParametros);
            return Resultado;
        }

        public override async Task<Boolean> Load()
        {
            try
            {
                Objeto = await database.GetAsync<ObjJugadores>(Objeto.ID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }

    #endregion

    #region OBJETOS BASE DE DATOS

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

        public Double Puntos_xGanados { get; set; }
        public Double Puntos_xEmpatados { get; set; }
        public Double Puntos_xPerdidos { get; set; }

        public int ReadOnly { get; set; }
    }

    [Table("Zonas")]
    public class ObjZonas : ObjId
    {
        public int IdTorneo { get; set; }
        public Boolean esLLave { get; set; }
        public int NivelLLave { get; set; }

        public int IdZ1 { get; set; }
        public int PosicionZ1 { get; set; }
        public int IdZ2 { get; set; }
        public int PosicionZ2 { get; set; }

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
        public double Puntos { get; set; } = 0;
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
        public Boolean Finalizado { get; set; }
    }

    [Table("Jugadores")]
    public class ObjJugadores : ObjId
    {
        public string pathFoto { get; set; }
        public int IdEquipo { get; set; }
    }

    #endregion


    public class ObjEquipoListaEquipos
    {
        public string Nombre { get; set; }
        public int ID { get; set; }

        public int Puntos { get; set; }
        public int IdZona { get; set; }
    }

}
