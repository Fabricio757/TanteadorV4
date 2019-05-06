using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

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
        public async void GenerarTorneos(TorneosViewModel Torneo)
        {/*Genera los partidos del Torneo*/
            List<objEquipos> Equipos = await Torneo.MisEquiposQueNoSonCabecera();
            List<objZonas> Zonas = await Torneo.MisZonas();

            ZonasViewModel zonaVM = new ZonasViewModel();

            /*Verifia que existan las zonas*/
            if (Zonas.Count > 0)
            {
                /*Carga los equipos en la zona*/
                while (Equipos.Count > 0)
                {
                    foreach (objZonas z in Zonas)
                    {
                        if (Equipos.Count > 0)
                        {
                            zonaVM.Objeto = z;
                            
                            objEquipos E = UnEquipo(Equipos);
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
                foreach (objZonas z in Zonas)
                {
                    zonaVM.Objeto = z;
                    zonaVM.ItemAtras = Torneo;
                    GenerarPartidos(zonaVM);
                }
            }
        }

        private objEquipos UnEquipo(List<objEquipos> Equipos)
        {/*Devuelve un Equipo al azar de la lista de Equipos*/
            int Indice = RandomizeFHA.Next(0, Equipos.Count);
            objEquipos Equipo = Equipos[Indice];
            Equipos.Remove(Equipo);

            return Equipo;
        }

        private void GenerarPartidos(ZonasViewModel zonaVM)
        {
            int Mitad;
            int Cantidad;

            zonaVM.setItemPropertiesFromObject();

            PartidosViewModel PartidosVM = new PartidosViewModel();

            List<objEquipos> losEquipos = new List<objEquipos>();

            List<objEquipos> misEquiposAsignados = zonaVM.MisEquipos_Sync();
            foreach (objEquipos Equipo in misEquiposAsignados)
            {
                losEquipos.Add(Equipo);
            }

            /*Si es impar lo hago par agregando un elemento 0*/
            int Resto;
            Cantidad = losEquipos.Count;
            Mitad = Math.DivRem(Cantidad, 2, out Resto);

            objEquipos Equipo0 = new objEquipos();
            Equipo0.ID = 0;
            Equipo0.Nombre = "- Fecha Libre -";
            if (Resto > 0)
            {
                losEquipos.Add(Equipo0);
                Mitad++;
            }

            List<objPartidos> losPartidos = new List<objPartidos>();

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
                    objPartidos Partido = new objPartidos();
                    Partido.IdEquipo1 = losEquipos[PartidosFecha].ID;
                    Partido.IdEquipo2 = losEquipos[PartidosFecha + Mitad].ID;

                    Partido.IdZona = zonaVM.Objeto.ID;
                    Partido.FechaOrden = Fecha;

                    Partido.Nombre = losEquipos[PartidosFecha].Nombre + " vs " + losEquipos[PartidosFecha + Mitad].Nombre;

                    losPartidos.Add(Partido);
                }

                /*Hago el desplazamiento del array para generar la proxima fecha*/
                objEquipos IdEquipoSegundo = losEquipos[1];
                losEquipos.RemoveAt(1);
                losEquipos.Add(IdEquipoSegundo);
            }

            zonaVM.GuardarPartidos(losPartidos);
        }

        public void BorrarPartidos(TorneosViewModel Torneo)
        {
            Torneo.BorrarPartidos();
        }

    }
}
