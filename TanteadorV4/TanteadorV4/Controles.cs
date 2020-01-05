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

            /*Si es impar lo hago par agregando un Equipo: "Fecha Libre" elemento 0*/
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
            int CantidadFechas = Cantidad - 1;


            /*Recorro las Fecha*/
            for (int Fecha = 1; Fecha <= CantidadFechas; Fecha++)
            {
                /*Arma los partidos de la fecha*/
                for (int PartidosFecha = 0; PartidosFecha < Mitad; PartidosFecha++)
                {
                    ObjPartidos Partido = new ObjPartidos();
                    Partido.IdEquipo1 = losEquipos[PartidosFecha].ID;
                    Partido.IdEquipo2 = losEquipos[Cantidad - PartidosFecha -1].ID;

                    Partido.IdZona = zonaVM.Objeto.ID;
                    Partido.FechaOrden = Fecha;


                    Partido.Nombre = losEquipos[PartidosFecha].Nombre + " vs " + losEquipos[Cantidad - PartidosFecha -1].Nombre;
                    if (Partido.IdEquipo1 == 0)
                    {
                        Partido.Nombre = losEquipos[Cantidad - PartidosFecha -1].Nombre + " - Fecha Libre -";
                        Partido.Finalizado = true;
                    }

                    if (Partido.IdEquipo2 == 0)
                    {
                        Partido.Nombre = losEquipos[PartidosFecha].Nombre + " - Fecha Libre -";
                        Partido.Finalizado = true;
                    }
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
                newP.PartidoRevancha = true;
                

                string NombreEquipo1 = "";
                string NombreEquipo2 = "";
                ObjEquipos E = new ObjEquipos();
                //BiEquipos Equipo = new BiEquipos();

                if (newP.IdEquipo1 > 0)
                {
                    E = await BI.Equipos.LoadxPK(P.IdEquipo2);
                    NombreEquipo1 = E.Nombre;
                };

                if (newP.IdEquipo2 > 0)
                {
                    E = await BI.Equipos.LoadxPK(P.IdEquipo1);
                    NombreEquipo2 = E.Nombre;
                };

                if ((newP.IdEquipo1 * newP.IdEquipo2) == 0) //Si alguno de los 2 es 0
                {
                    newP.Nombre = NombreEquipo1 + NombreEquipo2 + " - Fecha Libre -";
                    newP.Finalizado = true;
                }
                else
                    newP.Nombre = NombreEquipo1 + " vs " + NombreEquipo2;


                await SqlPersist.Partidos.InsertItemAsync(newP);
                
            }
            return 0;
        }

        public void BorrarTorneoGenerado(BiTorneos Torneo)
        {
            Torneo.BorrarTorneoGenerado();
        }

        public Boolean Impar(int N)
        {
            double R = Math.IEEERemainder(N, 2);
            return (R != 0);
        }

        public async void GenerarZonaLlave(ObjZonas Z1, ObjZonas Z2, int j, int CantidadClasificados, int IdTorneo, int Nivel, int T)
        {
            ObjZonas ZonaNueva = new ObjZonas();
            ZonaNueva.IdZ1 = Z1.ID;
            ZonaNueva.PosicionZ1 = j + 1;

            ZonaNueva.IdZ2 = Z2.ID;
            ZonaNueva.PosicionZ2 = CantidadClasificados - j;

            ZonaNueva.IdTorneo = IdTorneo;
            ZonaNueva.esLLave = true;
            ZonaNueva.NivelLLave = Nivel;

            ZonaNueva.Nombre = "Nivel 1 Nro: " + T.ToString() + " " + Z1.Nombre + " (" + (j + 1).ToString() + ") " + Z2.Nombre + " (" + (CantidadClasificados - j).ToString() + ")";

            await SqlPersist.Zonas.InsertItemAsync(ZonaNueva);

        }

        public async void GenerarZonaLlaveElimDir(ObjZonas Z1, ObjZonas Z2, int Nivel, int CantidadNiveles, int t, int IdTorneo)
        {
            string descNivel = "Nivel";
            ObjZonas ZonaNueva = new ObjZonas();
            ZonaNueva.IdZ1 = Z1.ID;
            ZonaNueva.PosicionZ1 = 1;

            ZonaNueva.IdZ2 = Z2.ID;
            ZonaNueva.PosicionZ2 = 1;

            ZonaNueva.IdTorneo = IdTorneo;
            ZonaNueva.esLLave = true;
            ZonaNueva.NivelLLave = Nivel + 1;

            if (Nivel == CantidadNiveles)
                descNivel = "Final";
            if (Nivel == CantidadNiveles - 1)
                descNivel = "SemiF";
            if ((Nivel == CantidadNiveles - 2) && (CantidadNiveles - 2 > 0))
                descNivel = "4tos ";
            if ((Nivel == CantidadNiveles - 3) && (CantidadNiveles - 3 > 0))
                descNivel = "8vos ";

            ZonaNueva.Nombre = descNivel + " " + (Nivel + 1).ToString() + " Nro: " + t.ToString() + "  [" + Z1.Nombre.Substring(13, 2) + " - " + Z2.Nombre.Substring(13, 2) + "]";

            await SqlPersist.Zonas.InsertItemAsync(ZonaNueva);
        }

        public async void GenerarLlave(int IdTorneo)
        {
            int t = 1;
            int IdZonaLiberada_0 = 0; 
            ObjZonas ZonaLibre = null;
            ObjZonas Z1, Z2, ZonaLiberada_0 = null, ZonaNueva = null;
            int Indice_0 = -1;

            ObjTorneos Torneo = await SqlPersist.Torneos.LoadxPk(IdTorneo);
            int CantidadClasificados = Torneo.CantidadClasificadosXZona;

            BI.Torneos.oTorneo = Torneo;
            List<ObjZonas> Zonas_0 = await BI.Torneos.MisZonas(0);

            int CantidadZonas = Zonas_0.Count;
            int Cant_Clas = (int)((double)CantidadClasificados / 2);

            if (Impar(CantidadClasificados * CantidadZonas))
            {
                Indice_0 = RandomizeFHA.Next(0, CantidadZonas);

                Indice_0 = 2; //fha

                ZonaLiberada_0 = Zonas_0[Indice_0];
                IdZonaLiberada_0 = ZonaLiberada_0.ID;

                ZonaLibre = new ObjZonas();
                ZonaLibre.ID = 0;
                ZonaLibre.Nombre = "- Libre -";
            }

            //GENERO LAS ZONAS DE NIVEL 1, LAS QUE TIENEN 1 O MAS DE UN EQUIPO CLASIFICADO DE CADA ZONA
            //Torneo.CantidadClasificadosXZona

            //Genero 1RO contra ultimo; 2DO contra anteultimo, .... Acá no importa la cantidad de zonas
            for (int i = 0; i < CantidadZonas; i++)
            {
                Z1 = Zonas_0[i];
                if (i == Zonas_0.Count - 1)//Cuando es la última Zona
                { 
                    Z2 = Zonas_0[0];                    
                }
                else
                {
                    Z2 = Zonas_0[i + 1]; 
                }

                for (int j = 0; j < Cant_Clas; j++)
                {
                    GenerarZonaLlave(Z1, Z2, j, CantidadClasificados, Torneo.ID, 1, t);                    
                }     
                t++;           
            }

            //Si la cantidad de Clasificados es impar, completo la fila del medio (El for anterior no lo hace)
            Z1 = null; Z2 = null;
            if (Impar(CantidadClasificados))
            {
                //Generando las zonas de la fila del medio, se enfrentan entre ellas
                for (int i = 0; i < CantidadZonas; i = i + 1)
                {
                    if (i != Indice_0)
                    {
                        if (Z1 is null)
                            Z1 = Zonas_0[i];
                        else
                        {
                            Z2 = Zonas_0[i];
                            GenerarZonaLlave(Z1, Z2, Cant_Clas, CantidadClasificados, Torneo.ID, 1, t);
                            t++;
                            Z1 = null; Z2 = null;
                        }
                    }
                }
                GenerarZonaLlave(ZonaLiberada_0, ZonaLibre, Cant_Clas, 0, Torneo.ID, 1, t);


                //Busco las Zonas Nivel 1 donde IdZ1 = IdZonaLiberada
                int IdZ_Z2 = 0;
                int Pos_Z2 = 0;

                List<ObjZonas> Zonas_Z1 = await BI.Torneos.MisZonas_Z1(IdZonaLiberada_0);
                foreach (ObjZonas Z in Zonas_Z1)
                {
                    if (Z.PosicionZ2 == 1)
                    {
                        IdZ_Z2 = Z.IdZ2;
                        Pos_Z2 = Z.PosicionZ2;

                        Z.IdZ2 = 0;
                        Z.PosicionZ2 = 0;
                        
                    }
                    else
                    {
                        int tmp_IdZ_Z2 = Z.IdZ2;
                        int tmp_Pos_Z2 = Z.PosicionZ2;

                        Z.IdZ2 = IdZ_Z2;
                        Z.PosicionZ2 = Pos_Z2;

                        IdZ_Z2 = tmp_IdZ_Z2;
                        Pos_Z2 = tmp_Pos_Z2;

                    }

                    Z1 = await SqlPersist.Zonas.LoadxPk(Z.IdZ1);
                    if (Z.IdZ2 == 0)
                        Z2 = ZonaLibre;
                    else
                        Z2 = await SqlPersist.Zonas.LoadxPk(Z.IdZ2);


                    Z.Nombre = "Nivel 1 Nro: " + Z.Nombre.Substring(13, 2) + " " + Z1.Nombre + " (" + (Z.PosicionZ1).ToString() + ") " + Z2.Nombre + " (" + (Z.PosicionZ2).ToString() + ")";
                    await SqlPersist.Zonas.UpdateItemAsync(Z);

                    }
                }
            




            // LOS DEMÁS NIVELES, tienen 1 solo clasificado por ZONA

            //string descNivel = "Nivel";
            int Nivel = 1;
            int t_N = -1;
            BI.Torneos.oTorneo = Torneo;
            List<ObjZonas> Zonas = await BI.Torneos.MisZonas(Nivel);
            CantidadZonas = Zonas.Count;
            int CantidadNiveles = (int)Math.Ceiling(Math.Sqrt(CantidadZonas));

            while (CantidadZonas > 1) 
            {
                int Indice = -1;
                if (Impar(CantidadZonas))
                {
                    Indice = RandomizeFHA.Next(0, CantidadZonas);
                }

                Z1 = null; Z2 = null;
                for (int i = 0; i < CantidadZonas; i++)
                {
                    if (i != Indice)
                    {
                        if (Z1 is null)
                        {
                            Z1 = Zonas[i];
                            t_N = t;
                            t++;
                        }
                        else
                        {
                            Z2 = Zonas[i];
                            GenerarZonaLlaveElimDir(Z1, Z2, Nivel, CantidadNiveles, t, Torneo.ID);
                            t++;
                            Z1 = null; Z2 = null;
                        }
                    }
                }

                if (Impar(CantidadZonas))
                {
                    Z1 = Zonas[Indice];
                    GenerarZonaLlaveElimDir(Z1, Z1, Nivel, CantidadNiveles, t_N, Torneo.ID);
                }

                Nivel++;
                BI.Torneos.oTorneo = Torneo;
                Zonas = await BI.Torneos.MisZonas(Nivel);
                CantidadZonas = Zonas.Count;
            }
            

        }

         

    }
}
