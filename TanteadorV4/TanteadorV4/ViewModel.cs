using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;


using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;

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

        protected ObjId _Objeto;
        public ObjId Objeto { set { _Objeto = value; OnPropertyChanged(); } get { return _Objeto; } }
        //public ObjId Objeto { set { Bi.Objeto = value; OnPropertyChanged(); } get { return Bi.Objeto; } }
        public void ConectarBI() { Bi.Objeto = _Objeto; }


        //Creo estos objetos para que los parametros sean persistentes a la VM y no se pierdan en las slqPersits, y BI
        public string[] NombresParametros = new string[0];
        public string[] ConectoresParametros = new string[0];
        public Object[] ValoresParametros = new Object[0];

        public void AddParametrosString(string value) { NombresParametros = value.Split(','); }
        public void AddConectoresParametrosString(string value) { ConectoresParametros = value.Split(','); }
        public void AddParametros(string[] pNombreParametros, Object[] pValoresParametros, string[] pConectoresParametros = null) { NombresParametros = pNombreParametros; ValoresParametros = pValoresParametros; ConectoresParametros = pConectoresParametros; }
        public void AddParametro_Only(string pNombre, object pValor) { NombresParametros = new string[] { pNombre }; ValoresParametros = new object[] { pValor }; }



        public BiBase Bi { get { return getBi(); } }

        public virtual BiBase getBi() { return null;  }

        public virtual void setItemPropertiesFromObject() { }

        //ATRAS
        protected VmBase _ItemAtras;
        public virtual VmBase ItemAtras { set { _ItemAtras = value; } get { return _ItemAtras; } }

        //
        public virtual void SetObjAtras(ObjId obj)
        { }

        public virtual async void NewObject()
        {
            Objeto = new ObjId();
        }

        public virtual void Mostrar(EventArgs e)
        {
            EventHandler handler = OnMostrar;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void SetAtrasFK()
        { }

        public virtual async Task<string> GuardarItem(EnumOperacion Operacion)
        {
            SetAtrasFK();
            //this.Bi.Objeto = this.Objeto;
            return await this.Bi.GuardarItem(Operacion);
        }

    }

    public class VmTorneos : VmBase
    {
        public BiTorneos bTorneo { get { return BI.Torneos; } }
        public ObjTorneos oTorneo { set { Objeto = value; } get { return (ObjTorneos)Objeto; } }


        public VmTorneos() : base()
        {//Constructor            
            oTorneo = new ObjTorneos();
        }

        public override BiBase getBi() { return bTorneo; }

        public override void NewObject()
        {
            oTorneo = new ObjTorneos();
        }
    }


    public class VmZonas : VmBase
    {
        public BiZonas bZona { get { return BI.Zonas; } }
        public ObjZonas oZona { set { Objeto = value; } get { return (ObjZonas)Objeto; } }


        public VmZonas() : base()
        {//Constructor
            oZona = new ObjZonas();
        }

        public override BiBase getBi() { return bZona; }

        public override void NewObject()
        {
            oZona = new ObjZonas();
        }


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



        public override void setItemPropertiesFromObject()
        {
            if (this.bZona.pZona.oZona == null)
            {
                ((ObjZonas)Objeto).IdTorneo = _ItemAtras.Objeto.ID;

                SqlPersist.Equipos.AddParametrosString("IdTorneo,IdZona");
                SqlPersist.Equipos.ValoresParametros = new object[] { ((ObjZonas)Objeto).IdTorneo, 0 };

                Equipos = SqlPersist.Equipos.GetEquiposAsync().Result;

                _SelectedCabecera = null;
                OnPropertyChanged("SelectedCabecera");
            }
            else
            {
                this.bZona.pZona.oZona.IdTorneo = this.ItemAtras.Objeto.ID; //Cargo el Id de Torneo.

                Object[] V = { ((ObjZonas)Objeto).ID };
                SqlPersist.JOIN.AddParametros(new[] { "L.IdZona" }, V);
                Equipos = SqlPersist.JOIN.GetEquipos_ListaEquiposAsync().Result;

                SelectedCabecera = Equipos.SingleOrDefault(a => a.ID == ((ObjZonas)Objeto).IdEquipoCabezaDeSerie);
            }
        }

        protected override void SetAtrasFK()
        {
            this.oZona.IdTorneo = this.ItemAtras.Objeto.ID;
        }
    }


    public class VmEquipos : VmBase
    {
        public BiEquipos bEquipo { get { return BI.Equipos; } }
        public ObjEquipos oEquipo { set { Objeto = value; } get { return (ObjEquipos)Objeto; } }


        public VmEquipos() : base()
        {//Constructor
            oEquipo = new ObjEquipos();
        }

        public override BiBase getBi() { return bEquipo; }

        public override async void NewObject()
        {
            Objeto = new ObjEquipos();
            ((ObjEquipos)Objeto).IdTorneo = _ItemAtras.Objeto.ID;
        }

        protected override void SetAtrasFK()
        {
            Type T = this.ItemAtras.GetType();
            if (T.Name == "VmTorneos")
            {
                ObjTorneos Torneo = (ObjTorneos)this.ItemAtras.Objeto;
                //this.oEquipo.IdZona = null;
                this.oEquipo.IdTorneo = Torneo.ID;
            }
            else
            {
                ObjZonas Z = (ObjZonas)this.ItemAtras.Objeto;
                this.oEquipo.IdZona = Z.ID;
                this.oEquipo.IdTorneo = Z.IdTorneo;
            }
        }
        /*
                public async Task<List<ObjPartidos>> MisPartidos()
                {
                    SqlPersist.Partidos.AddParametrosString("IdEquipo1");
                    SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
                    List<ObjPartidos> r = await SqlPersist.Partidos.GetPartidosAsync();

                    SqlPersist.Partidos.AddParametrosString("IdEquipo2");
                    SqlPersist.Partidos.ValoresParametros = new object[] { pEquipo.Objeto.ID };
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

                */
    }


    public class VmListaEquipos : VmBase, INavigationVM
    {
        public BiListaEquipos bListaEquipo { get { return BI.ListaEquipos; } }
        public ObjListaEquipos oListaEquipo { set; get;}


        public VmListaEquipos() : base()
        {//Constructor
            oListaEquipo = new ObjListaEquipos();
        }

        public override BiBase getBi()
        {
            return bListaEquipo;
        }

        public override VmBase ItemAtras { set; get; }


    }


    public class VmPartidos: VmBase
    {
        public BiPartidos bPartido { get { return BI.Partidos; } }
        public ObjPartidos oPartido { set { Objeto = value; } get { return (ObjPartidos)Objeto; } }


        public VmPartidos() : base()
        {//Constructor
            oPartido = new ObjPartidos();
        }

        public override BiBase getBi()
        {
            return bPartido;
        }

        public override async void NewObject()
        {
            Objeto = new ObjPartidos();
            ((ObjPartidos)Objeto).IdZona = _ItemAtras.Objeto.ID;

            SqlPersist.Equipos.AddParametrosString("IdZona");
            SqlPersist.Equipos.ValoresParametros = new object[] { ((ObjPartidos)Objeto).IdZona };

            MisEquipos = await SqlPersist.Equipos.GetEquiposAsync(); ;            

            _selectedEquipo1 = null;
            OnPropertyChanged("SelectedEquipo1");
            _selectedEquipo2 = null;
            OnPropertyChanged("SelectedEquipo2");
        }

        public override void setItemPropertiesFromObject()
        {
            SqlPersist.JOIN.AddParametro_Only("L.IdZona", this.ItemAtras.Objeto.ID);
            //SqlPersist.JOIN.Add_ParametrosQry("IdZona");

            MisEquipos = SqlPersist.JOIN.GetEquipos_ListaEquiposAsync().Result;
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
/*
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

            SqlPersistListaEquipos Lista = SqlPersist.ListaEquipos;
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

            //  Armar las zonas de la llave 

            VmZonas Z = new VmZonas();
            Z.pZona.oZona.ID = pPartidos.oPartido.IdZona;
            await Z.pZona.Load();

            VmTorneos Torneo = new VmTorneos();
            Torneo.Objeto.ID = Z.pZona.oZona.IdTorneo;
            await Torneo.pTorneo.Load();

            Boolean partidosFinalizados = await Torneo.PartidosFinalizados(Z.pZona.oZona.NivelLLave);
            
            if (partidosFinalizados == true)
            {
                List<ObjZonas> ListaZonas = await Torneo.MisZonas(Z.pZona.oZona.NivelLLave + 1);
                if (ListaZonas.Count > 0) //Si no es mayor que 0 es porque ya estamos en la final
                {
                    foreach (ObjZonas Zi in ListaZonas)
                    {
                        //Z.oZona = Zi;
                        await Z.CompletarZona_yPartidos(Zi);
                    }
                }
            }

            return "";
        }
        */
    }


    public class VmJugadores : VmBase
    {
        public BiJugadores bJugador { get { return BI.Jugadores; } }
        public ObjJugadores oJugador { set { Objeto = value; } get { return (ObjJugadores)Objeto; } }


        public VmJugadores() : base()
        {//Constructor
            oJugador = new ObjJugadores();
        }

        public override BiBase getBi()
        {
            return bJugador;
        }

        public override async void NewObject()
        {
            Objeto = new ObjJugadores();
            ((ObjJugadores)Objeto).IdEquipo = ((VmEquipos)_ItemAtras).bEquipo.pEquipo.oEquipo.ID;

            VmTorneos Torneo = new VmTorneos();
            Torneo.bTorneo.pTorneo.oTorneo.ID = ((VmEquipos)_ItemAtras).bEquipo.pEquipo.oEquipo.IdTorneo;            
            MisEquipos = await Torneo.bTorneo.MisEquipos();

            _selectedEquipo = null;
            OnPropertyChanged("SelectedEquipo");
        }

        public override void setItemPropertiesFromObject()
        {
            VmTorneos Torneo = new VmTorneos();
            Torneo.bTorneo.pTorneo.oTorneo.ID = ((VmEquipos)_ItemAtras).bEquipo.pEquipo.oEquipo.IdTorneo;
            MisEquipos = Torneo.bTorneo.MisEquipos().Result;
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



