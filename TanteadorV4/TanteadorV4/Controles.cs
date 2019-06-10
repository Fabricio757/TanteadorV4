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
        public async void GenerarTorneos(VmTorneos Torneo)
        {/*Genera los partidos del Torneo*/
            List<ObjEquipos> Equipos = await Torneo.MisEquiposQueNoSonCabecera();
            List<ObjId> Zonas = await Torneo.MisZonas();

            VmZonas zonaVM = new VmZonas();

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
                                await App.Database.Zonas.UpdateItemAsync(z);
                            }
                        }
                    }
                }

                
                /*Genera los Partidos*/
                foreach (ObjZonas z in Zonas)
                {
                    zonaVM.Objeto = z;
                    zonaVM.ItemAtras = Torneo;
                    int R = await GenerarPartidos(zonaVM);
                }

                ObjTorneos T = ((ObjTorneos)Torneo.Objeto);
                if (T.IdaYVuelta == 1)
                {
                    var r = await GenerarPartidosVuelta(Torneo);
                }
            }
        }

        private ObjEquipos UnEquipo(List<ObjEquipos> Equipos)
        {/*Devuelve un Equipo al azar de la lista de Equipos*/
            int Indice = RandomizeFHA.Next(0, Equipos.Count);
            ObjEquipos Equipo = Equipos[Indice];
            Equipos.Remove(Equipo);

            return Equipo;
        }

        private async Task<int> GenerarPartidos(VmZonas zonaVM)
        {
            int Mitad;
            int Cantidad;

            //zonaVM.setItemPropertiesFromObject();

            VmPartidos PartidosVM = new VmPartidos();

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

        private async Task<int> GenerarPartidosVuelta(VmTorneos Torneo)
        {
            VmPartidos PartidosVM = new VmPartidos();

            int UltimaFecha = await Torneo.UltimaFecha();

            List<ObjPartidos> Partidos_Ida = await Torneo.MisPartidos();
            foreach (ObjPartidos P in Partidos_Ida)
            {
                ObjPartidos newP = new ObjPartidos();

                newP.FechaOrden = P.FechaOrden + UltimaFecha;

                newP.IdEquipo1 = P.IdEquipo2;
                newP.IdEquipo2 = P.IdEquipo1;
                newP.IdZona = P.IdZona;
                //newP.PartidoRevancha = 1;

                string NombrePartido = "";
                VmEquipos Equipo = new VmEquipos();

                if (P.IdEquipo2 > 0)
                {
                    Equipo.pEquipo.oEquipo.ID = P.IdEquipo2;
                    await Equipo.pEquipo.Load();
                    NombrePartido = Equipo.pEquipo.oEquipo.Nombre;
                }
                else
                    NombrePartido = NombrePartido + " vs " + "- Fecha Libre -";

                if (P.IdEquipo1 > 0)
                {
                    Equipo.pEquipo.oEquipo.ID = P.IdEquipo1;
                    await Equipo.pEquipo.Load();
                    NombrePartido = NombrePartido + " vs " + Equipo.pEquipo.oEquipo.Nombre;
                }
                else
                    NombrePartido = NombrePartido +" vs " + "- Fecha Libre -";


                newP.Nombre = NombrePartido;

                PartidosVM.pPartidos.oPartido = newP;


                PartidosVM.Objeto = newP;

                VmZonas Zona = new VmZonas();
                Zona.pZona.oZona.ID = P.IdZona;
                PartidosVM.ItemAtras = Zona;
                
                PartidosVM.setItemPropertiesFromObject();
                await PartidosVM.GuardarItem(EnumOperacion.Nuevo);
            }
            return 0;
        }

        public void BorrarTorneoGenerado(VmTorneos Torneo)
        {
            Torneo.BorrarTorneoGenerado();
        }

    }
}
