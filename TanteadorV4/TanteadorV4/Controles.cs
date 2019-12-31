using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace TanteadorV4
{
    public class RandomizeFHA
    {
        static private int Seed = 0;
        static public String ssSeed = "";

        RandomizeFHA()
        { }

        public static int Next(int Desde, int Hasta)
        {
            /*Random rnd = new Random(Seed);
            Seed = rnd.Next(Seed) + 1;
            Seed = Seed % 1000;
            return rnd.Next(Desde, Hasta);*/
            Seed++;
            int R = (Math.Abs((int)DateTime.Now.Ticks) + 1) % 1000;
            return ((R % (Hasta - Desde)) + Desde);
        }

        public static void Reset()
        {
            Seed = 0;
            ssSeed = "";
        }

        public static int GetSeed()
        {
            return (Seed);
        }

        public static string Get_ssSeed()
        {
            return (ssSeed);
        }

    }

    class Funciones
    {
        public async Task<bool> GenerarTorneos(BiTorneos Torneo)
        {/*Genera los partidos del Torneo*/
            List<ObjEquipos> Equipos = await Torneo.MisEquiposQueNoSonCabecera();
            List<ObjZonas> Zonas = await Torneo.MisZonas();

            BiZonas zonaVM = new BiZonas();

            /*Verifia que existan las zonas*/
            if (Zonas.Count > 0)
            {
                /*Carga los equipos en la zona*/
                while (Equipos.Count > 0)
                {
                    foreach (ObjZonas z in Zonas)
                    {
                        if (Equipos.Count > 0)
                        {
                            zonaVM.Objeto = z;
                            
                            ObjEquipos E = UnEquipo(Equipos);
                            await zonaVM.AddEquipo(E);

                            /*Si la zona no tiene Cabecera, agregarla*/
                            if (z.IdEquipoCabezaDeSerie == 0)
                            {
                                z.IdEquipoCabezaDeSerie = E.ID;
                                await SqlPersist.Zonas.UpdateItemAsync(z);
                            }
                        }
                    }
                }

                
                /*Genera los Partidos*/
                foreach (ObjZonas z in Zonas)
                {
                    zonaVM.Objeto = z;
                    //zonaVM.ItemAtras = Torneo;
                    int R = await GenerarPartidos(zonaVM);
                }

                ObjTorneos T = ((ObjTorneos)Torneo.Objeto);
                if (T.IdaYVuelta == 1)
                {
                    var r = await GenerarPartidosVuelta(T);
                }

                GenerarLlave(Torneo.Objeto.ID);
            }

            return true;
        }

        private ObjEquipos UnEquipo(List<ObjEquipos> Equipos)
        {/*Devuelve un Equipo al azar de la lista de Equipos*/
            int Indice = RandomizeFHA.Next(0, Equipos.Count);
            ObjEquipos Equipo = Equipos[Indice];
            Equipos.Remove(Equipo);

            return Equipo;
        }

        private async Task<int> GenerarPartidos(BiZonas zonaVM)
        {
            int Mitad;
            int Cantidad;

            //zonaVM.setItemPropertiesFromObject();

            BiPartidos PartidosVM = new BiPartidos();

            List<ObjEquipos> losEquipos = new List<ObjEquipos>();

            List<ObjEquipos> misEquiposAsignados = await zonaVM.MisEquipos(); // zonaVM.MisEquipos_Sync();
            foreach (ObjEquipos Equipo in misEquiposAsignados)
            {
                losEquipos.Add(Equipo);
            }

            /*Si es impar lo hago par agregando un elemento 0*/
            int Resto;
            Cantidad = losEquipos.Count;
            Mitad = Math.DivRem(Cantidad, 2, out Resto);

            ObjEquipos Equipo0 = new ObjEquipos();
            Equipo0.ID = 0;
            Equipo0.Nombre = "- Fecha Libre -";
            if (Resto > 0)
            {
                losEquipos.Add(Equipo0);
                Mitad++;
            }

            List<ObjPartidos> losPartidos = new List<ObjPartidos>();

            Cantidad = losEquipos.Count;
            int CantidadFechas = 0;

            if (Cantidad == 2)
                CantidadFechas = 1;
            else
                CantidadFechas = Cantidad - 1;
               // CantidadFechas = ((Cantidad * Cantidad) - (((Cantidad + 1) * Cantidad) / 2)) / 2;

            /*Recorro las Fecha*/
            for (int Fecha = 1; Fecha <= CantidadFechas; Fecha++)
            {
                /*Arma los partidos de la fecha*/
                for (int PartidosFecha = 0; PartidosFecha < Mitad; PartidosFecha++)
                {
                    ObjPartidos Partido = new ObjPartidos();
                    Partido.IdEquipo1 = losEquipos[PartidosFecha].ID;
                    Partido.IdEquipo2 = losEquipos[PartidosFecha + Mitad].ID;

                    Partido.IdZona = zonaVM.Objeto.ID;
                    Partido.FechaOrden = Fecha;

                    Partido.Nombre = losEquipos[PartidosFecha].Nombre + " vs " + losEquipos[PartidosFecha + Mitad].Nombre;

                    losPartidos.Add(Partido);
                }

                /*Hago el desplazamiento del array para generar la proxima fecha*/
                ObjEquipos IdEquipoSegundo = losEquipos[1];
                losEquipos.RemoveAt(1);
                losEquipos.Add(IdEquipoSegundo);
            }

            await zonaVM.GuardarPartidos(losPartidos);

            return 0;
        }

        private async Task<int> GenerarPartidosVuelta(ObjTorneos T)
        {
            BI.Torneos.oTorneo = T;

            int UltimaFecha = await BI.Torneos.UltimaFecha();

            List<ObjPartidos> Partidos_Ida = await BI.Torneos.MisPartidos();
            foreach (ObjPartidos P in Partidos_Ida)
            {
                ObjPartidos newP = new ObjPartidos();

                newP.FechaOrden = P.FechaOrden + UltimaFecha;

                newP.IdEquipo1 = P.IdEquipo2;
                newP.IdEquipo2 = P.IdEquipo1;
                newP.IdZona = P.IdZona;
                //newP.PartidoRevancha = 1;

                string NombrePartido = "";
                ObjEquipos E = new ObjEquipos();
                //BiEquipos Equipo = new BiEquipos();

                if (P.IdEquipo2 > 0)
                {
                    E = await BI.Equipos.LoadxPK(P.IdEquipo2);
                    NombrePartido = E.Nombre;
                }
                else
                    NombrePartido = NombrePartido + " vs " + "- Fecha Libre -";

                if (P.IdEquipo1 > 0)
                {
                    E = await BI.Equipos.LoadxPK(P.IdEquipo1);
                    NombrePartido = E.Nombre;
                }
                else
                    NombrePartido = NombrePartido +" vs " + "- Fecha Libre -";
                               

                newP.Nombre = NombrePartido;
                await BI.Partidos.Set(newP).GuardarItem(EnumOperacion.Nuevo);

            }
            return 0;
        }

        public void BorrarTorneoGenerado(BiTorneos Torneo)
        {
            Torneo.BorrarTorneoGenerado();
        }

        public async void GenerarLlave(int IdTorneo)
        {
            int t = 1;

            ObjTorneos Torneo = await SqlPersist.Torneos.LoadxPk(IdTorneo);
            

            int Clasificados = Torneo.CantidadClasificadosXZona;

            BI.Torneos.oTorneo = Torneo;
            List<ObjZonas> Zonas = await BI.Torneos.MisZonas(0);
            int CantidadZonas = Zonas.Count;
            

            BiZonas vZonas = new BiZonas();

            ObjZonas Z1, Z2, ZonaNueva;

            for (int i = 0; i < CantidadZonas; i++)
            {
                Z1 = Zonas[i];

                int Cant_Clas = 0;

                if (i == Zonas.Count - 1)
                { 
                    Z2 = Zonas[0];
                    Cant_Clas = (int)((double)Clasificados / 2);
                }
                else
                {
                    Z2 = Zonas[i + 1];
                    Cant_Clas = (int)Math.Ceiling((double)Clasificados / 2);
                }
                    

                for (int j = 0; j < Cant_Clas; j++)
                {
                    ZonaNueva = new ObjZonas();
                    ZonaNueva.IdZ1 = Z1.ID;
                    ZonaNueva.PosicionZ1 = j + 1;

                    ZonaNueva.IdZ2 = Z2.ID;
                    ZonaNueva.PosicionZ2 = Clasificados - j;

                    ZonaNueva.IdTorneo = Torneo.ID;
                    ZonaNueva.esLLave = true;
                    ZonaNueva.NivelLLave = 1;

                    ZonaNueva.Nombre = "Nivel 1 Nro: " + t.ToString() + " " + Z1.Nombre + " (" + (j+1).ToString() + ") " + Z2.Nombre + " (" + (Clasificados - j).ToString() + ")";

                    vZonas.Objeto = ZonaNueva;
                    await vZonas.GuardarItem(EnumOperacion.Nuevo);
                    t++;
                }
            }


            // LOS DEMÁS NIVELES
            string descNivel = "Nivel";
            int Nivel = 1;
            BI.Torneos.oTorneo = Torneo;
            Zonas = await BI.Torneos.MisZonas(Nivel);
            CantidadZonas = Zonas.Count;
            int CantidadNiveles = (int)Math.Ceiling(Math.Sqrt(CantidadZonas));

            while (CantidadZonas > 1) 
            {

                for (int i = 0; i < CantidadZonas; i=i+2)
                {
                    Z1 = Zonas[i];

                    ZonaNueva = new ObjZonas();
                    ZonaNueva.IdZ1 = Z1.ID;
                    ZonaNueva.PosicionZ1 = 1;

                    if (i < CantidadZonas-1)
                        Z2 = Zonas[i + 1];
                    else
                        Z2 = Z1;

                    ZonaNueva.IdZ2 = Z2.ID;
                    ZonaNueva.PosicionZ2 = 1;

                    ZonaNueva.IdTorneo = Torneo.ID;
                    ZonaNueva.esLLave = true;
                    ZonaNueva.NivelLLave = Nivel + 1;

                    if (Nivel == CantidadNiveles)
                        descNivel = "Final";
                    if (Nivel == CantidadNiveles-1)
                        descNivel = "SemiF";
                    if ((Nivel == CantidadNiveles - 2)&&(CantidadNiveles - 2 > 0))
                        descNivel = "4tos ";
                    if ((Nivel == CantidadNiveles - 3) && (CantidadNiveles - 3 > 0))
                        descNivel = "8vos ";

                    ZonaNueva.Nombre = descNivel + " " + (Nivel + 1).ToString() + " Nro: " + t.ToString() + "  [" + Z1.Nombre.Substring(13,2) + " - " + Z2.Nombre.Substring(13, 2) + "]";

                    vZonas.Objeto = ZonaNueva;
                    await vZonas.GuardarItem(EnumOperacion.Nuevo);
                    t++;
                }


                Nivel++;
                BI.Torneos.oTorneo = Torneo;
                Zonas = await BI.Torneos.MisZonas(Nivel);
                CantidadZonas = Zonas.Count;
            }


        }

         

    }
}
