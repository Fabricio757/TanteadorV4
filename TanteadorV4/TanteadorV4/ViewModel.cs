using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TanteadorV4
{
    //public delegate void vmEventHandler(object sender, EventArgs args);

    public interface INavigationVM
    {
        VmBase ItemAtras { set; get; }
    }

    public class Columna_Lista
    {
        public String Titulo { get; set; } = "";
        public String NombreAtributo { get; set; } = "";
        public Double Width { get; set; } = 0;

        public void Reset()
        {
            Titulo = ""; NombreAtributo = ""; Width = 0;
        }
    }


    public class VmLista : INotifyPropertyChanged//, INavigationVM
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
                
        public IEnumerable<ObjId> ItemsSource { get; set; }

        private Columna_Lista Columna_1 = new Columna_Lista(), Columna_2 = new Columna_Lista(), Columna_3 = new Columna_Lista();

        public Columna_Lista Col1 { get { return Columna_1; } }
        public Columna_Lista Col2 { get { return Columna_2; } }
        public Columna_Lista Col3 { get { return Columna_3; } }

        public void Columnas(Columna_Lista Col_1, Columna_Lista Col_2 = null, Columna_Lista Col_3 = null)
        {
            Columna_1.Reset();
            Columna_2.Reset();
            Columna_3.Reset();

            Columna_1 = Col_1;
            if (Col_2 != null)
                Columna_2 = Col_2;
            if (Col_3 != null)
                Columna_3 = Col_3;
        }

        public DataTemplate DateTemplate_configuracion()
        {
            var objDataTemplate = new DataTemplate(() =>
            {                
                var grid = new Grid() { Margin = 10};

                var col1 = new Label(); //,  FontAttributes = FontAttributes.Bold };
                var col2 = new Label();
                var col3 = new Label(); // { HorizontalTextAlignment = TextAlignment.End };

                col1.SetBinding(Label.TextProperty, Columna_1.NombreAtributo);

                if (Columna_2.NombreAtributo != "")
                    col2.SetBinding(Label.TextProperty, Columna_2.NombreAtributo);

                if (Columna_3.NombreAtributo != "")
                    col3.SetBinding(Label.TextProperty, Columna_2.NombreAtributo);


                grid.Children.Add(col1);
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_1.Width, GridUnitType.Star) });

                if (Columna_2.NombreAtributo != null)
                {
                    grid.Children.Add(col2, 1, 0);
                }
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_2.Width, GridUnitType.Star) });


                if (Columna_3.NombreAtributo != null)
                {
                    grid.Children.Add(col3, 2, 0);
                }
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_3.Width, GridUnitType.Star) });
                
                return new ViewCell { View = grid };
            });
            
            return objDataTemplate;
        }


        public DataTemplate HeaderTemplate_configuracion()
        {
                var grid = new Grid() { Margin = 10 };

                var col1 = new Label() {FontAttributes = FontAttributes.Bold, BackgroundColor = Color.DarkKhaki};
                var col2 = new Label() {FontAttributes = FontAttributes.Bold, BackgroundColor = Color.DarkKhaki };
                var col3 = new Label() {FontAttributes = FontAttributes.Bold, BackgroundColor = Color.DarkKhaki };
                
                col1.Text = Columna_1.Titulo;
                col2.Text = Columna_2.Titulo;
                col3.Text = Columna_3.Titulo;

                grid.Children.Add(col1);
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_1.Width, GridUnitType.Star) });

                if (Columna_2.NombreAtributo != null)
                {
                    grid.Children.Add(col2, 1, 0);
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_2.Width, GridUnitType.Star) });
                }

                if (Columna_3.NombreAtributo != null)
                {
                    grid.Children.Add(col3, 2, 0);
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Columna_3.Width, GridUnitType.Star) });
                }

                return new DataTemplate(() => { return grid; });
        }
    }

    public class VmBase : INotifyPropertyChanged, INavigationVM
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OnMostrar;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected SqlPersistObject _Persist;
        public SqlPersistObject Persist { set { _Persist = value; } get { _Persist = getPersist(); return _Persist; } }

        public SqlPersistListaEquipos pPersist { get { return (SqlPersistListaEquipos)Persist; } }

        public ObjId Objeto { set { Persist.Objeto = value; OnPropertyChanged(); } get { return Persist.Objeto; } }

        public VmBase() : base()
        {//Constructor
            _Persist = getPersist();
        }

        public virtual void setItemPropertiesFromObject() { }

        //ATRAS
        protected VmBase _ItemAtras;
        public virtual VmBase ItemAtras { set { setAtras(value); } get { return _ItemAtras; } }


        protected void setAtras(VmBase value)
        {
            _ItemAtras = value;
            //Persist = getPersist();
            //Persist.Objeto = value.Objeto;
        }

        public virtual SqlPersistObject getPersist()
        {
            return null;
        }

        public virtual async Task<List<ObjId>> RetornarLista()
        {
            Persist = getPersist();
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

                Persist = getPersist();

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
            }

            return Mensaje;
        }

        public virtual string ItemValido()
        {
            return "";
        }

        public async Task<string> DeleteItem()
        {
            Persist = getPersist();
            string mensaje = await SePuedeBorrar();

            if (mensaje == "")
            {
                await Persist.DeleteItemAsync(Objeto);
                mensaje = "Borrado";
            };
            return mensaje;
        }

        public async virtual Task<string> SePuedeBorrar()
        {
            return "";
        }

        //
        public virtual void setObjeto(ObjId _Objeto)
        { }

        //
        public virtual void SetObjAtras(ObjId obj)
        { }

        public virtual async void NewObject()
        { }

        public virtual void Mostrar(EventArgs e)
        {
            EventHandler handler = OnMostrar;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }



    public class VmTorneos : VmBase
    {
        public SqlPersistTorneos pTorneo { set { _Persist = value; } get { return (SqlPersistTorneos)_Persist; } }

        public override SqlPersistObject getPersist()
        {
            return App.Database.Torneos;
        }

        public override void NewObject()
        {
            Objeto = new ObjTorneos();
        }

        public async Task<List<ObjEquipos>> MisEquipos()
        {
            App.Database.Equipos.AddParametros(new[] { "IdTorneo" }, new object[] { this.pTorneo.oTorneo.ID });

            return await App.Database.Equipos.GetEquiposAsync();
        }

        public async Task<List<ObjEquipos>> MisEquiposQueNoSonCabecera()
        {
            App.Database.Equipos.AddParametrosString("IdTorneo");
            App.Database.Equipos.ValoresParametros = new object[] { Objeto.ID };

            return await App.Database.Equipos.GetEquiposAsync(" and Id not in (select ifnull(IdEquipoCabezaDeSerie,0) from Zonas where idTorneo = " + Objeto.ID.ToString() + " )");
        }

        public async Task<List<ObjEquipos>> MisEquiposDisponibles()
        {
            App.Database.Equipos.AddParametrosString("IdTorneo, IdZona");
            App.Database.Equipos.AddConectoresParametrosString(", is Null");
            App.Database.Equipos.ValoresParametros = new object[] { Objeto.ID, "" };

            return await App.Database.Equipos.GetEquiposAsync();
        }

        public async Task<List<ObjZonas>> MisZonas()
        {
            App.Database.Zonas.NombresParametros = new[] { "IdTorneo" };
            ((ObjZonas)App.Database.Zonas.Objeto).IdTorneo = ((ObjTorneos)Objeto).ID;
            App.Database.Zonas.ObjetoToParametros();

            return await App.Database.Zonas.GetZonasAsync();
        }

        public async Task<List<ObjZonas>> MisZonas(int NivelLlave)
        {
            App.Database.Zonas.NombresParametros = new[] { "IdTorneo", "NivelLlave" };
            App.Database.Zonas.ValoresParametros = new object[] { this.pTorneo.oTorneo.ID, NivelLlave};

            return await App.Database.Zonas.GetZonasAsync();
        }

        public async Task<Boolean> TienePartidos()
        {
            List<ObjPartidos> L = await this.MisPartidos();
            return (L.Count > 0);
        }

        public async Task<List<ObjPartidos>> MisPartidos()
        {
            App.Database.JOIN.AddParametrosString("IdTorneo");
            App.Database.JOIN.ValoresParametros = new object[] { ((ObjTorneos)Objeto).ID };

            
            return await App.Database.JOIN.GetPartidos_Zona_Async();
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

            List<ObjEquipos> LE = await MisEquipos();
            if (LE.Count > 0)
                mensaje = mensaje + (mensaje != "" ? ", " : "") + "tiene Equipos";

            mensaje = (mensaje != "" ? "El torneo " + mensaje + ". Por lo tanto no se puede borrar." : "");

            return mensaje;
        }

        public void BorrarTorneoGenerado()
        {
            App.Database.Torneos.oTorneo = this.pTorneo.oTorneo;
            App.Database.Torneos.BorrarTorneoGenerado();
        }
    }


    public class VmZonas : VmBase
    {
        public SqlPersistZonas pZona {set { _Persist = value; } get{ return (SqlPersistZonas)_Persist; } }

        IList<ObjEquipos> _Equipos;
        public IList<ObjEquipos> Equipos
        {
            get { return _Equipos; }
            set
            {
                if (_Equipos != value)
                {
                    _Equipos = value;
                    OnPropertyChanged();
                }
            }
        }

        ObjEquipos _SelectedCabecera;
        public ObjEquipos SelectedCabecera
        {
            get { return _SelectedCabecera; }
            set
            {
                if (value != null)
                    if (_SelectedCabecera != value)
                    {
                        _SelectedCabecera = value;
                        if (value != null)
                            ((ObjZonas)Objeto).IdEquipoCabezaDeSerie = _SelectedCabecera.ID;
                        else
                            ((ObjZonas)Objeto).IdEquipoCabezaDeSerie = 0;

                        OnPropertyChanged("SelectedCabecera");
                    }
            }
        }


        public override SqlPersistObject getPersist()
        {
            return App.Database.Zonas;
        }

        public override void NewObject()
        {
            Objeto = new ObjZonas();
        }

        public override void setItemPropertiesFromObject()
        {
            if (this.pZona.oZona == null)
            {
                ((ObjZonas)Objeto).IdTorneo = _ItemAtras.Objeto.ID;

                App.Database.Equipos.AddParametrosString("IdTorneo,IdZona");
                App.Database.Equipos.ValoresParametros = new object[] { ((ObjZonas)Objeto).IdTorneo, 0 };

                Equipos = App.Database.Equipos.GetEquiposAsync().Result;

                _SelectedCabecera = null;
                OnPropertyChanged("SelectedCabecera");
            }
            else
            {
                this.pZona.oZona.IdTorneo = this.ItemAtras.Objeto.ID; //Cargo el Id de Torneo.

                Object[] V = { ((ObjZonas)Objeto).ID };
                App.Database.JOIN.AddParametros(new[] { "L.IdZona" }, V);
                Equipos = App.Database.JOIN.GetEquipos_ListaEquiposAsync().Result;

                SelectedCabecera = Equipos.SingleOrDefault(a => a.ID == ((ObjZonas)Objeto).IdEquipoCabezaDeSerie);
            }
        }



        public async Task<int> AddEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            Item.IdEquipo = Equipo.ID;
            Item.IdZona = Objeto.ID;
            await App.Database.ListaEquipos.InsertItemAsync(Item);

            Equipo.IdZona = Objeto.ID;
            await App.Database.Equipos.UpdateItemAsync(Equipo);

            return 0;
        }

        public async Task<int> GuardarPartidos(List<ObjPartidos> listaPartidos)
        {
            foreach (ObjPartidos Partido in listaPartidos)
                await App.Database.Partidos.InsertItemAsync(Partido);

            return 0;
        }
        
        public async Task<List<ObjEquipos>> MisEquipos()
        {
            string[] N = { "IdZona"};
            Object[] V = { ((ObjZonas)Objeto).ID };
            App.Database.Equipos.AddParametros(N, V);

            return await App.Database.Equipos.GetEquiposAsync();
        }

        public async Task<List<ObjEquipoListaEquipos>> MisEquiposListaEquipos()
        {
            string[] N = { "L.IdZona" };
            Object[] V = { ((ObjZonas)Objeto).ID };
            App.Database.JOIN.AddParametros(N, V);

            return await App.Database.JOIN.GetEquiposListaEquiposAsync();
        }
    }


    public class VmEquipos : VmBase
    {
        public SqlPersistEquipos pEquipo { set { _Persist = value; } get { return (SqlPersistEquipos)_Persist; } }

        public override SqlPersistObject getPersist()
        {
            return App.Database.Equipos;
        }

        public override async void NewObject()
        {
            Objeto = new ObjEquipos();
            ((ObjEquipos)Objeto).IdTorneo = _ItemAtras.Objeto.ID;
        }

        public async Task<List<ObjPartidos>> MisPartidos()
        {
            App.Database.Partidos.AddParametrosString("IdEquipo1");
            App.Database.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
            List<ObjPartidos> r = await App.Database.Partidos.GetPartidosAsync();

            App.Database.Partidos.AddParametrosString("IdEquipo2");
            App.Database.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
            List<ObjPartidos> r2 = await App.Database.Partidos.GetPartidosAsync();

            r.Concat<ObjId>(r2);

            return r;
        }

        public async Task<double> Puntos(int IdZona)
        {/*Devuelve la cantidad de puntos del equipo en la zona*/
            double P = 0;

            SqlPersistZonas Zona = App.Database.Zonas;
            Zona.oZona.ID = IdZona;
            await Zona.Load();

            SqlPersistTorneos Torneo = App.Database.Torneos;
            Torneo.oTorneo.ID = Zona.oZona.IdTorneo;
            await Torneo.Load();

            int C_Ganados = await pEquipo.CantidadPartidosGanados(IdZona);
            int C_Empatados = await pEquipo.CantidadPartidosEmpatados(IdZona);
            int C_Perdidos = await pEquipo.CantidadPartidosPerdidos(IdZona);

            P = (C_Ganados * Torneo.oTorneo.Puntos_xGanados) + (C_Empatados * Torneo.oTorneo.Puntos_xEmpatados) + (C_Perdidos * Torneo.oTorneo.Puntos_xPerdidos);

            return P;
        }
        
    }


    public class VmListaEquipos : VmBase, INavigationVM
    {
        public override VmBase ItemAtras { set; get; }

        public override SqlPersistObject getPersist()
        {
            return App.Database.ListaEquipos;
        }

        public virtual async Task<List<ObjId>> RetornarLista_Equipos(int IdTorneo)
        {
            //Persist = getPersist();
            //return await App.Database.ListaEquipos.GetItemsAsync_EquiposDisponibles(IdTorneo);
            return null;
        }

        public virtual async Task<List<ObjId>> RetornarLista_EquiposZona(int IdZona)
        {/* Retorna los equipos que estan en una zona, segun la lista zona */
            return await App.Database.ListaEquipos.GetItemsAsync_EquiposZona(IdZona);
        }

        public async Task AddEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            Item.IdZona = ItemAtras.Objeto.ID; //es una zona
            Item.IdEquipo = Equipo.ID;
            await App.Database.ListaEquipos.InsertItemAsync(Item);

            Equipo.IdZona = ItemAtras.Objeto.ID;
            await App.Database.Equipos.UpdateItemAsync(Equipo);
        }

        public async Task RemoveEquipo(ObjEquipos Equipo)
        {
            ObjListaEquipos Item = new ObjListaEquipos();
            Item.IdZona = ItemAtras.Objeto.ID; //es una zona
            Item.IdEquipo = Equipo.ID;
            await App.Database.ListaEquipos.DeleteItemListaEquipo(Item);

            if (Equipo.IdZona == ItemAtras.Objeto.ID)
            {
                Equipo.IdZona = 0;
                await App.Database.Equipos.UpdateItemAsync(Equipo);
            }
        }


    }


    public class VmPartidos: VmBase
    {
        public SqlPersistPartidos pPartidos { set { _Persist = value; } get { return (SqlPersistPartidos)_Persist; } }

        public override SqlPersistObject getPersist()
        {
            return App.Database.Partidos;
        }

        public override async void NewObject()
        {
            Objeto = new ObjPartidos();
            ((ObjPartidos)Objeto).IdZona = _ItemAtras.Objeto.ID;

            App.Database.Equipos.AddParametrosString("IdZona");
            App.Database.Equipos.ValoresParametros = new object[] { ((ObjPartidos)Objeto).IdZona };

            MisEquipos = await App.Database.Equipos.GetEquiposAsync(); ;            

            _selectedEquipo1 = null;
            OnPropertyChanged("SelectedEquipo1");
            _selectedEquipo2 = null;
            OnPropertyChanged("SelectedEquipo2");
        }

        public override void setItemPropertiesFromObject()
        {
            App.Database.JOIN.AddParametro_Only("L.IdZona", this.ItemAtras.Objeto.ID);
            //App.Database.JOIN.Add_ParametrosQry("IdZona");

            MisEquipos = App.Database.JOIN.GetEquipos_ListaEquiposAsync().Result;
            SelectedEquipo1 = MisEquipos.SingleOrDefault(a => a.ID == ((ObjPartidos)Objeto).IdEquipo1);
            SelectedEquipo2 = MisEquipos.SingleOrDefault(a => a.ID == ((ObjPartidos)Objeto).IdEquipo2);
        }

        IList<ObjEquipos> _MisEquipos;
        public IList<ObjEquipos> MisEquipos
        {
            get { return _MisEquipos; }
            set
            {
                if (_MisEquipos != value)
                {
                    _MisEquipos = value;
                    OnPropertyChanged();
                }
            }
        }

        ObjEquipos _selectedEquipo1;
        public ObjEquipos SelectedEquipo1
        {
            get { return _selectedEquipo1; }
            set
            {
                if (value != null)
                if (_selectedEquipo1 != value)
                {
                    _selectedEquipo1 = value;
                    if (value != null)
                        ((ObjPartidos)Objeto).IdEquipo1 = _selectedEquipo1.ID;
                    else
                        ((ObjPartidos)Objeto).IdEquipo1 = 0;

                    OnPropertyChanged("SelectedEquipo1");
                }
            }
        }

        ObjEquipos _selectedEquipo2;
        public ObjEquipos SelectedEquipo2
        {
            get { return _selectedEquipo2; }
            set
            {
                if (value != null)
                if (_selectedEquipo2 != value)
                {
                    _selectedEquipo2 = value;
                    if (value != null)
                        ((ObjPartidos)Objeto).IdEquipo2 = _selectedEquipo2.ID;
                    else
                        ((ObjPartidos)Objeto).IdEquipo2 = 0;

                    OnPropertyChanged("SelectedEquipo2");
                }
            }
        }

        public override string ItemValido()
        {
            string Mensaje = "";

             if (_selectedEquipo1.ID == _selectedEquipo2.ID)
            //if (this.pPartidos.oPartido.IdEquipo2 == this.pPartidos.oPartido.IdEquipo2)
            { Mensaje = "No puede ser el mismo equipo"; }

            return Mensaje;
        }
       

        public override async Task<string> AfterGuardar(EnumOperacion Operacion)
        {
            //Guardar los puntos
            VmEquipos Equipo = new VmEquipos();
            Equipo.pEquipo.oEquipo.ID = pPartidos.oPartido.IdEquipo1;
            await Equipo.pEquipo.Load();

            SqlPersistListaEquipos Lista = App.Database.ListaEquipos;
            Lista.oListaEquipos.IdEquipo = pPartidos.oPartido.IdEquipo1;
            Lista.oListaEquipos.IdZona = pPartidos.oPartido.IdZona;
            await Lista.Load();


            Lista.oListaEquipos.Puntos = await Equipo.Puntos(pPartidos.oPartido.IdZona);
            int re = await Lista.UpdateItemAsync(Lista.oListaEquipos);


            Equipo.pEquipo.oEquipo.ID = pPartidos.oPartido.IdEquipo2;
            await Equipo.pEquipo.Load();

            //SqlPersistListaEquipos LE = new SqlPersistListaEquipos();
            Lista.oListaEquipos.IdEquipo = pPartidos.oPartido.IdEquipo2;
            Lista.oListaEquipos.IdZona = pPartidos.oPartido.IdZona;
            await Lista.Load();

            Lista.oListaEquipos.Puntos = await Equipo.Puntos(pPartidos.oPartido.IdZona);
            re = await Lista.UpdateItemAsync(Lista.oListaEquipos);

            return "";
        }

    }


    public class VmJugadores : VmBase
    {
        public SqlPersistJugadores pJugadores { set { _Persist = value; } get { return (SqlPersistJugadores)_Persist; } }

        public override SqlPersistObject getPersist()
        {
            return App.Database.Jugadores;
        }

        public override async void NewObject()
        {
            Objeto = new ObjJugadores();
            ((ObjJugadores)Objeto).IdEquipo = ((VmEquipos)_ItemAtras).pEquipo.oEquipo.ID;

            VmTorneos Torneo = new VmTorneos();
            Torneo.pTorneo.oTorneo.ID = ((VmEquipos)_ItemAtras).pEquipo.oEquipo.IdTorneo;            
            MisEquipos = await Torneo.MisEquipos();

            _selectedEquipo = null;
            OnPropertyChanged("SelectedEquipo");
        }

        public override void setItemPropertiesFromObject()
        {
            VmTorneos Torneo = new VmTorneos();
            Torneo.pTorneo.oTorneo.ID = ((VmEquipos)_ItemAtras).pEquipo.oEquipo.IdTorneo;
            MisEquipos = Torneo.MisEquipos().Result;
            SelectedEquipo = MisEquipos.SingleOrDefault(a => a.ID == ((ObjJugadores)Objeto).IdEquipo);
        }

        IList<ObjEquipos> _MisEquipos;
        public IList<ObjEquipos> MisEquipos
        {
            get { return _MisEquipos; }
            set
            {
                if (_MisEquipos != value)
                {
                    _MisEquipos = value;
                    OnPropertyChanged();
                }
            }
        }

        ObjEquipos _selectedEquipo;
        public ObjEquipos SelectedEquipo
        {
            get { return _selectedEquipo; }
            set
            {
                if (value != null)
                    if (_selectedEquipo != value)
                    {
                        _selectedEquipo = value;
                        if (value != null)
                            ((ObjJugadores)Objeto).IdEquipo = _selectedEquipo.ID;
                        else
                            ((ObjJugadores)Objeto).IdEquipo = 0;

                        OnPropertyChanged("SelectedEquipo");
                    }
            }
        }

    }


}



