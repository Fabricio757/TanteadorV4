using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TanteadorV4
{
    public static class BI
    {
        static public BiTorneos Torneos { set; get; } = new BiTorneos();
        static public BiZonas Zonas { set; get; } = new BiZonas();
        static public BiEquipos Equipos { set; get; } = new BiEquipos();
        static public BiJugadores Jugadores { set; get; } = new BiJugadores();
        static public BiPartidos Partidos { set; get; } = new BiPartidos();
        static public BiListaEquipos ListaEquipos { set; get; } = new BiListaEquipos();
    }

    public class BiBase
    {
        public ObjId Objeto { set {Persist.Objeto=value; } get {return Persist.Objeto; } }
        public SqlPersistObject Persist { set; get; }

        public async virtual Task<string> SePuedeBorrar()
        {
            return "";
        }

        public async Task<bool> Load()
        {
            Persist.Objeto = Objeto;
            return await Persist.Load();
        }

        public virtual async Task<ObjId> LoadxID(int id)
        {
            Persist.Objeto.ID = id;
            await Persist.Load();
            return Objeto;
        }

        public BiBase Set(ObjId objId)
        {
            Objeto = objId;
            return this;
        }

        public virtual async Task<List<ObjId>> RetornarLista()
        {
            //Persist = getPersist();
            Persist.Objeto = this.Objeto;

            return await Persist.GetItemsAsync();
        }

        public virtual void CargaCompleta()
        { }

        public virtual async Task<string> BeforeGuardar(EnumOperacion Operacion)
        {
            return "";
        }

        public virtual async Task<string> AfterGuardar(EnumOperacion Operacion)
        {
            return "";
        }

        public virtual async Task<string> GuardarItem(EnumOperacion Operacion)
        {
            string Mensaje = "";

            try
            {
                Mensaje = ItemValido();

                //Persist = getPersist();

                if (Mensaje == "")
                {
                    await BeforeGuardar(EnumOperacion.Nuevo);
                    if (Operacion == EnumOperacion.Nuevo)
                    {
                        await Persist.InsertItemAsync(Objeto);
                    }
                    else
                    {
                        await Persist.UpdateItemAsync(Objeto);
                    }
                    await AfterGuardar(EnumOperacion.Nuevo);
                }
            }
            catch (Exception ex)
            {
                Mensaje = ex.Message;
                throw new Exception(Mensaje);
            }

            return Mensaje;
        }

        public virtual string ItemValido()
        {
            return "";
        }

        public async Task<string> DeleteItem()
        {
            //Persist = getPersist();
            string mensaje = await SePuedeBorrar();

            if (mensaje == "")
            {
                await Persist.DeleteItemAsync(Objeto);
                mensaje = "Borrado";
            };
            return mensaje;
        }

    }

    public class BiTorneos : BiBase
    {
        public SqlPersistTorneos pTorneo { get { return (SqlPersistTorneos)Persist; } }
        public ObjTorneos oTorneo { set { Objeto = value; } get { return (ObjTorneos)Objeto; } }


        public BiTorneos() : base()
        {//Constructor
            Persist = SqlPersist.Torneos;
            //Objeto = new ObjTorneos();
        }

        public new BiTorneos Set(ObjId objId)
        {
            Objeto = objId;
            return this;
        }

        public async Task<List<ObjEquipos>> MisEquipos()
        {
            return await pTorneo.MisEquipos();
        }

        public async Task<List<ObjEquipos>> MisEquiposQueNoSonCabecera()
        {
            SqlPersist.Equipos.AddParametrosString("IdTorneo");
            SqlPersist.Equipos.ValoresParametros = new object[] { Objeto.ID };

            return await SqlPersist.Equipos.GetEquiposAsync(" and Id not in (select ifnull(IdEquipoCabezaDeSerie,0) from Zonas where idTorneo = " + Objeto.ID.ToString() + " )");
        }

        public async Task<List<ObjEquipos>> MisEquiposDisponibles()
        {
            SqlPersist.Equipos.AddParametrosString("IdTorneo");
            //SqlPersist.Equipos.AddConectoresParametrosString(", is Null");
            SqlPersist.Equipos.ValoresParametros = new object[] { Objeto.ID };

            return await SqlPersist.Equipos.GetEquiposAsync(" and ifnull(IdZona, 0) = 0 ");
        }

        public async Task<List<ObjZonas>> MisZonas()
        {
            SqlPersist.Zonas.NombresParametros = new[] { "IdTorneo" };
            ((ObjZonas)SqlPersist.Zonas.Objeto).IdTorneo = ((ObjTorneos)Objeto).ID;
            SqlPersist.Zonas.ObjetoToParametros();

            return await SqlPersist.Zonas.GetZonasAsync();
        }

        public async Task<List<ObjZonas>> MisZonas(int NivelLlave)
        {
            SqlPersist.Zonas.NombresParametros = new[] { "IdTorneo", "NivelLlave" };
            SqlPersist.Zonas.ValoresParametros = new object[] { this.pTorneo.oTorneo.ID, NivelLlave };

            return await SqlPersist.Zonas.GetZonasAsync();
        }

        public async Task<Boolean> TienePartidos()
        {
            List<ObjPartidos> L = await this.MisPartidos();
            return (L.Count > 0);
        }

        public async Task<List<ObjPartidos>> MisPartidos()
        {
            SqlPersist.JOIN.AddParametrosString("IdTorneo");
            SqlPersist.JOIN.ValoresParametros = new object[] { ((ObjTorneos)Objeto).ID };


            return await SqlPersist.JOIN.GetPartidos_Zona_Async();
        }

        public async Task<int> UltimaFecha()
        {
            List<ObjPartidos> LP = await MisPartidos();
            return LP.Max(x => x.FechaOrden);
        }

        public async override Task<string> SePuedeBorrar()
        {
            String mensaje = "";

            if (await TienePartidos())
                mensaje = "tiene partidos";

            List<ObjZonas> LZ = await MisZonas();
            if (LZ.Count > 0)
                mensaje = mensaje + (mensaje != "" ? ", " : "") + "tiene Zonas";

            this.pTorneo.oTorneo = this.oTorneo;
            List<ObjEquipos> LE = await pTorneo.MisEquipos();
            if (LE.Count > 0)
                mensaje = mensaje + (mensaje != "" ? ", " : "") + "tiene Equipos";

            mensaje = (mensaje != "" ? "El torneo " + mensaje + ". Por lo tanto no se puede borrar." : "");

            return mensaje;
        }

        public void BorrarTorneoGenerado()
        {
            SqlPersist.Torneos.oTorneo = this.pTorneo.oTorneo;
            SqlPersist.Torneos.BorrarTorneoGenerado();
        }

        public async Task<Boolean> PartidosFinalizados(int Nivel)
        {
            Boolean B = false;

            BiZonas vZonas = new BiZonas();

            List<ObjZonas> Lista = await this.MisZonas(Nivel);
            foreach (ObjZonas Z in Lista)
            {
                vZonas.pZona.oZona = Z;
                B = await vZonas.PartidosFinalizados();
            }

            return B;
        }

    }

    public class BiZonas : BiBase
    {
        public SqlPersistZonas pZona { get { return (SqlPersistZonas)Persist; } }
        public ObjZonas oZona { set { Objeto = value; } get { return (ObjZonas)Objeto; } }


        public BiZonas() : base()
        {//Constructor
            Persist = SqlPersist.Zonas;
            //Objeto = new ObjZonas();
        }

        public void Set(ObjZonas objZ)
        {
            oZona = objZ;
            //return this;
        }

        public new async Task<ObjZonas> LoadxID(int id)
        {
            Persist.Objeto.ID = id;
            await Persist.Load();
            return oZona;
        }


        public async Task<int> AddEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            Item.IdEquipo = Equipo.ID;
            Item.IdZona = Objeto.ID;
            await SqlPersist.ListaEquipos.InsertItemAsync(Item);

            Equipo.IdZona = Objeto.ID;
            await SqlPersist.Equipos.UpdateItemAsync(Equipo);

            return 0;
        }

        public async Task<int> GuardarPartidos(List<ObjPartidos> listaPartidos)
        {
            foreach (ObjPartidos Partido in listaPartidos)
                await SqlPersist.Partidos.InsertItemAsync(Partido);

            return 0;
        }

        public async Task<List<ObjEquipos>> MisEquipos()
        {
            string[] N = { "IdZona" };
            Object[] V = { ((ObjZonas)Objeto).ID };
            SqlPersist.Equipos.AddParametros(N, V);

            return await SqlPersist.Equipos.GetEquiposAsync();
        }

        public async Task<List<ObjEquipoListaEquipos>> MisEquiposListaEquipos()
        {
            string[] N = { "L.IdZona" };
            Object[] V = { ((ObjZonas)Objeto).ID };
            SqlPersist.JOIN.AddParametros(N, V);

            return await SqlPersist.JOIN.GetEquiposListaEquiposAsync();
        }

        public async Task<Boolean> PartidosFinalizados()
        {
            Boolean B = false;

            SqlPersist.Partidos.AddParametrosString("IdZona, Finalizado");
            SqlPersist.Partidos.ValoresParametros = new object[] { this.Objeto.ID, false };
            List<ObjId> L = await SqlPersist.Partidos.GetItemsAsync();

            B = L.Count == 0;

            return B;
        }

        public async Task<List<ObjEquipos>> MisEquiposClasificados()
        {
            List<ObjEquipos> ListaDeEquipos = new List<ObjEquipos>();

            BiTorneos Torneo = new BiTorneos();
            Torneo.oTorneo.ID = this.pZona.oZona.IdTorneo;
            await Torneo.Load();

            int CantidadClasificados = Torneo.oTorneo.CantidadClasificadosXZona;

            SqlPersist.JOIN.AddParametro_Only("L.IdZona", ((ObjZonas)Objeto).ID);
            List<ObjEquipoListaEquipos> Lista = await SqlPersist.JOIN.GetEquiposListaEquiposAsync(" order by Puntos ");

            for (int i = 0; i < CantidadClasificados; i++)
            {
                BI.Equipos.oEquipo.ID = Lista[i].ID;
                await BI.Equipos.Load();

                ListaDeEquipos.Add(BI.Equipos.oEquipo);
            }

            return (ListaDeEquipos);
        }

        public async Task<Boolean> CompletarZona_yPartidos(ObjZonas Zona)
        { /* Para cada Zona "llave", arma la lista de Equipos y los partidos*/
            /*
            VmListaEquipos vmListaEquipos = new VmListaEquipos();
            VmZonas vmZona = new VmZonas();
            VmEquipos vmEquipo = new VmEquipos();
            VmPartidos Partidos = new VmPartidos();

            //---------------------------
            //  Busco el primer Equipo

            vmZona.oZona.ID = Zona.IdZ1;
            await vmZona.Load();
            List<ObjEquipos> EquiposClasificados = await vmZona.MisEquiposClasificados();

            ObjListaEquipos itemEquipoLista = new ObjListaEquipos();
            itemEquipoLista.IdEquipo = ((ObjEquipos)EquiposClasificados[Zona.PosicionZ1 - 1]).ID;
            itemEquipoLista.IdZona = this.Objeto.ID;
            itemEquipoLista.Puntos = 0;

            await vmListaEquipos.AddEquipo(itemEquipoLista);

            //Modifico la zona del Equipo
            vmEquipo.oEquipo.ID = itemEquipoLista.IdEquipo;
            await vmEquipo.Load();
            vmEquipo.oEquipo.IdZona = Zona.ID;
            await vmEquipo.GuardarItem(EnumOperacion.Actualiza);

            int IdEquipo1 = itemEquipoLista.IdEquipo;

            //---------------------
            // El otro Equipo

            vmZona.oZona.ID = Zona.IdZ2;
            await vmZona.Load();
            EquiposClasificados = await vmZona.MisEquiposClasificados();

            itemEquipoLista = new ObjListaEquipos();
            itemEquipoLista.IdEquipo = (EquiposClasificados[Zona.PosicionZ2 - 1]).ID;
            itemEquipoLista.IdZona = Zona.ID;
            itemEquipoLista.Puntos = 0;

            await vmListaEquipos.AddEquipo(itemEquipoLista);

            //Modifico la zona del Equipo
            vmEquipo = new VmEquipos();
            vmEquipo.oEquipo.ID = itemEquipoLista.IdEquipo;
            await vmEquipo.Load();
            vmEquipo.oEquipo.IdZona = Zona.ID;
            await vmEquipo.GuardarItem(EnumOperacion.Actualiza);

            int IdEquipo2 = itemEquipoLista.IdEquipo;
            //---------------------

            // Crear partido
            Partidos.oPartido.IdEquipo1 = IdEquipo1;
            Partidos.oPartido.IdEquipo2 = IdEquipo2;
            Partidos.oPartido.IdZona = Zona.ID;

            string NombrePartido = "";
            vmEquipo.oEquipo.ID = IdEquipo1;
            await vmEquipo.Load();
            NombrePartido = vmEquipo.oEquipo.Nombre;

            vmEquipo.oEquipo.ID = IdEquipo2;
            await vmEquipo.Load();
            NombrePartido = NombrePartido + " vs " + vmEquipo.oEquipo.Nombre;

            Partidos.oPartido.Nombre = NombrePartido;
            await Partidos.pPartidos.InsertItemAsync(Partidos.pPartidos.oPartido);

            //-------------------------------------------
            */
            return true;
        }

    }

    public class BiEquipos : BiBase
    {
        public SqlPersistEquipos pEquipo { get { return (SqlPersistEquipos)Persist; } }
        public ObjEquipos oEquipo { set { Objeto = value; } get { return (ObjEquipos)Objeto; } }


        public BiEquipos() : base()
        {//Constructor
            Persist = SqlPersist.Equipos;
            Objeto = new ObjEquipos();
        }

        public new BiEquipos Set(ObjId objId)
        {
            Objeto = objId;
            return this;
        }

        public new async Task<ObjEquipos> LoadxID(int id)
        {
            Persist.Objeto.ID = id;
            await Persist.Load();
            return oEquipo;
        }

        public async Task<List<ObjPartidos>> MisPartidos(int IdZona = 0)
        {
            if (IdZona == 0)
            {
                SqlPersist.Partidos.AddParametrosString("IdEquipo1");
                SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
            }
            else
            {
                SqlPersist.Partidos.AddParametrosString("IdEquipo1, IdZona");
                SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID, IdZona };
            }
            List<ObjPartidos> r = await SqlPersist.Partidos.GetPartidosAsync();


            if (IdZona == 0)
            {
                SqlPersist.Partidos.AddParametrosString("IdEquipo2");
                SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
            }
            else
            {
                SqlPersist.Partidos.AddParametrosString("IdEquipo2, IdZona");
                SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID, IdZona };
            }
            List<ObjPartidos> r2 = await SqlPersist.Partidos.GetPartidosAsync();

            r.Concat<ObjId>(r2);

            return r;
        }

        public async Task<double> Puntos(int IdZona)
        {//Devuelve la cantidad de puntos del equipo en la zona
            double P = 0;

            SqlPersistZonas Zona = SqlPersist.Zonas;
            Zona.oZona.ID = IdZona;
            await Zona.Load();

            SqlPersistTorneos Torneo = SqlPersist.Torneos;
            Torneo.oTorneo.ID = Zona.oZona.IdTorneo;
            await Torneo.Load();

            int C_Ganados = await pEquipo.CantidadPartidosGanados(IdZona);
            int C_Empatados = await pEquipo.CantidadPartidosEmpatados(IdZona);
            int C_Perdidos = await pEquipo.CantidadPartidosPerdidos(IdZona);

            P = (C_Ganados * Torneo.oTorneo.Puntos_xGanados) + (C_Empatados * Torneo.oTorneo.Puntos_xEmpatados) + (C_Perdidos * Torneo.oTorneo.Puntos_xPerdidos);

            return P;
        }

    }

    public class BiJugadores : BiBase
    {
        public SqlPersistJugadores pJugador { get { return (SqlPersistJugadores)Persist; } }
        public ObjJugadores oJugador { set { Objeto = value; } get { return (ObjJugadores)Objeto; } }


        public BiJugadores() : base()
        {//Constructor
            Persist = SqlPersist.Jugadores;
            Objeto = new ObjJugadores();
        }
    }

    public class BiPartidos : BiBase
    {
        public SqlPersistPartidos pPartido { get { return (SqlPersistPartidos)Persist; } }
        public ObjPartidos oPartido { set { Objeto = value; } get { return (ObjPartidos)Objeto; } }


        public BiPartidos() : base()
        {//Constructor
            Persist = SqlPersist.Partidos;
            Objeto = new ObjPartidos();
        }

        public override string ItemValido()
        {
            string Mensaje = "";
/*
            if (_selectedEquipo1.ID == _selectedEquipo2.ID)
            //if (this.pPartidos.oPartido.IdEquipo2 == this.pPartidos.oPartido.IdEquipo2)
            { Mensaje = "No puede ser el mismo equipo"; }
*/
            return Mensaje;
        }


        public override async Task<string> AfterGuardar(EnumOperacion Operacion)
        {
            //Guardar los puntos del Equipo1

            ObjEquipos l_oEquipo = new ObjEquipos() { ID = this.oPartido.IdEquipo1 };
            BI.Equipos.oEquipo = l_oEquipo;
            await BI.Equipos.Load();

            ObjListaEquipos itemListaEquipo = new ObjListaEquipos() { IdEquipo = this.oPartido.IdEquipo1, IdZona = this.oPartido.IdZona };
            BI.ListaEquipos.oListaEquipo = itemListaEquipo;
            await BI.ListaEquipos.Load();

            BI.Equipos.oEquipo = l_oEquipo;
            itemListaEquipo.Puntos = await BI.Equipos.Puntos(this.oPartido.IdZona);
            string re = await BI.ListaEquipos.GuardarItem(EnumOperacion.Actualiza);

            //Guardar los puntos del Equipo2

            l_oEquipo = new ObjEquipos() { ID = this.oPartido.IdEquipo2 };
            await BI.Equipos.Set(l_oEquipo).Load();

            itemListaEquipo = new ObjListaEquipos() { IdEquipo = this.oPartido.IdEquipo2, IdZona = this.oPartido.IdZona };
            await BI.ListaEquipos.Set(itemListaEquipo).Load();
            itemListaEquipo.Puntos = await BI.Equipos.Puntos(this.oPartido.IdZona);
            re = await BI.ListaEquipos.GuardarItem(EnumOperacion.Actualiza);



            //  Armar las zonas de la llave 

            ObjZonas Z = new ObjZonas() { ID = this.oPartido.IdZona };
            
            BI.Zonas.oZona = Z;
            await BI.Zonas.Load();
            Z = BI.Zonas.oZona;
            

            await BI.Zonas.Load();

            ObjTorneos T = new ObjTorneos() { ID = BI.Zonas.oZona.IdTorneo };
            await BI.Torneos.Set(T).Load();

            Boolean partidosFinalizados = await BI.Torneos.Set(T).PartidosFinalizados(Z.NivelLLave);

            if (partidosFinalizados == true)
            {
                List<ObjZonas> ListaZonas = await BI.Torneos.MisZonas(Z.NivelLLave + 1);
                if (ListaZonas.Count > 0) //Si no es mayor que 0 es porque ya estamos en la final
                {
                    foreach (ObjZonas Zi in ListaZonas)
                    {
                        //Z.oZona = Zi;
                        await BI.Zonas.CompletarZona_yPartidos(Zi);
                    }
                }
            }

            return "";
        }

    }

    public class BiListaEquipos : BiBase
    {
        public SqlPersistListaEquipos pListaEquipo { get { return (SqlPersistListaEquipos)Persist; } }
        public ObjListaEquipos oListaEquipo { set ;  get; }


        public BiListaEquipos() : base()
        {//Constructor
            Persist = SqlPersist.ListaEquipos;
            oListaEquipo = new ObjListaEquipos();
        }

        public new BiListaEquipos Set(ObjListaEquipos objId)
        {
            this.oListaEquipo = objId;
            return this;
        }

        public virtual async Task<List<ObjId>> RetornarLista_Equipos(int IdTorneo)
        {
            //Persist = getPersist();
            //return await SqlPersist.ListaEquipos.GetItemsAsync_EquiposDisponibles(IdTorneo);
            return null;
        }

        public virtual async Task<List<ObjId>> RetornarLista_EquiposZona(int IdZona)
        {/* Retorna los equipos que estan en una zona, segun la lista zona */
            return await SqlPersist.ListaEquipos.GetItemsAsync_EquiposZona(IdZona);
        }

        public override async Task<string> GuardarItem(EnumOperacion Operacion)
        {
            if (Operacion == EnumOperacion.Nuevo)
                throw new Exception("No se puede usar este método, usar AddEquipo");
            else 
                await base.GuardarItem(Operacion);

            return "";
        }

        public async Task AddEquipo(ObjListaEquipos item)
        {
            await SqlPersist.ListaEquipos.InsertItemAsync(item);
        }

        public async Task AddEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            
            Item.IdZona = Equipo.IdZona;
            Item.IdEquipo = Equipo.ID;
            Item.Puntos = 0;
            await SqlPersist.ListaEquipos.InsertItemAsync(Item);

            await SqlPersist.Equipos.UpdateItemAsync(Equipo);
        }

        public async Task RemoveEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            Item.IdZona = Equipo.IdZona; 
            Item.IdEquipo = Equipo.ID;
            await SqlPersist.ListaEquipos.DeleteItemListaEquipo(Item);

            if (Equipo.IdZona == Equipo.IdZona)
            {
                Equipo.IdZona = 0;
                await SqlPersist.Equipos.UpdateItemAsync(Equipo);
            }
        }

    }

}
