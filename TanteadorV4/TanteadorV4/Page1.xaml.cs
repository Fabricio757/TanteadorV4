using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;


namespace TanteadorV4
{
    public enum EnumABM { Torneo, Zona, Equipo, ListaEquipos, Partido, Jugadores };
    public enum EnumVista { vistaLista, vistaItem, DobleLista };
    public enum EnumOperacion { Nuevo, Actualiza };
    public enum EnumBindingType { Objeto, Item };

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        EnumVista enumVista = EnumVista.vistaLista;
        Boolean controlesLimpios = false;
        
        private VmBase ItemVM;
        private VmTorneos ItemTorneo;
        private VmZonas ItemZona;
        private VmEquipos ItemEquipos;
        private VmPartidos ItemPartidos;
        private VmListaEquipos ItemListaEquipos;
        private VmJugadores ItemJugadores;

        private VmLista vmLista = new VmLista();


        public Page1()
        {
            InitializeComponent();

            ItemTorneo = new VmTorneos();
            ItemTorneo.OnMostrar += ItemTorneo_OnMostrar;
            ItemZona = new VmZonas();
            ItemZona.OnMostrar += ItemZona_OnMostrar;
            ItemEquipos = new VmEquipos();
            ItemEquipos.OnMostrar += ItemEquipos_OnMostrar;
            ItemPartidos = new VmPartidos();
            ItemPartidos.OnMostrar += ItemPartidos_OnMostrar;

            ItemListaEquipos = new VmListaEquipos();

            ItemJugadores = new VmJugadores();
            ItemJugadores.OnMostrar += ItemJugadores_OnMostrar;
        }


        #region VISTAS

        private void ItemPartidos_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkPartidos.IsVisible = true;
            stkNombre.IsVisible = true;

            vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo = "Partido", Width = 4 },
                             new Columna_Lista() { NombreAtributo = "FechaOrden", Titulo = "Fecha", Width = 1 });

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();
            Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

            TituloABM.Text = "Partidos";
        }

        private void ItemEquipos_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkEquipos.IsVisible = true;
            stkNombre.IsVisible = true;

            btnJugadores.IsEnabled = ! controlesLimpios;

            TituloABM.Text = "Equipos";

            vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo = "Equipo", Width = 1 });

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();
            Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

        }

        private void ItemZona_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkZonas.IsVisible = true;
            stkNombre.IsVisible = true;

            btnListaEquipos.IsEnabled = ! controlesLimpios;
            btnPartidos.IsEnabled = ! controlesLimpios;
            lstEquipo.IsEnabled = ! controlesLimpios;

            TituloABM.Text = "Zonas";

            vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo="Zona", Width=1 } );
            Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();
            Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

            if (enumVista == EnumVista.vistaLista)
                BindingContext = vmLista;
        }

        private async void ItemTorneo_OnMostrar(object sender, EventArgs e)
        {
            try
            {
                OcultarStacks();
                stkTorneos.IsVisible = true;
                stkNombre.IsVisible = true;

                btnZonas.IsEnabled = !controlesLimpios;
                btnEquipos.IsEnabled = !controlesLimpios;

                if (ItemTorneo != null)
                    if (ItemTorneo.Objeto.ID > 0)
                    {
                        ItemTorneo.ConectarBI();
                        Boolean b = await ItemTorneo.bTorneo.TienePartidos();

                        if (b)
                            btnGenerarTorneo.Text = "Borrar Partidos";
                        else
                            btnGenerarTorneo.Text = "Generar Torneo";
                    }

                TituloABM.Text = "Torneos";

                vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo = "Torneo", Width = 4 },
                    new Columna_Lista() { NombreAtributo = "Titulo1", Titulo = "Titulo", Width = 1 });

                Lista.ItemTemplate = vmLista.DateTemplate_configuracion();
                Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

                if (enumVista == EnumVista.vistaLista)
                    BindingContext = vmLista;
                
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private void ItemJugadores_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkJugadores.IsVisible = true;
            stkNombre.IsVisible = true;

            vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo = "Jugador", Width = 1 });

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();
            Lista.HeaderTemplate = vmLista.HeaderTemplate_configuracion();

            TituloABM.Text = "Jugadores";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            TituloABM.Text = "Torneos";

            ItemVM = ItemTorneo;

            await RefreshList();

            OcultarStacks();
            stkTorneos.IsVisible = true;
            stkNombre.IsVisible = true;
            enumVista = EnumVista.vistaLista;
            LimpiarControles();
            await VistaLista();
        }

        private async Task RefreshList()
        {
            /* Paso los parámetros Persistentes en VM a Persist(slqPersist) */
            ItemVM.Bi.Persist.NombresParametros = ItemVM.NombresParametros;
            ItemVM.Bi.Persist.ValoresParametros = ItemVM.ValoresParametros;
            ItemVM.Bi.Persist.ConectoresParametros = ItemVM.ConectoresParametros;

            vmLista.ItemsSource = await ItemVM.Bi.RetornarLista();
        }

        private async Task VistaLista()
        {
            stkLista.IsVisible = true;
            stkControles.IsVisible = false;

            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnLimpiar.IsEnabled = true;

            enumVista = EnumVista.vistaLista;
            await RefreshList();

            ItemVM.Mostrar(null); //Muestra las controles particulares de cada Objeto. 

            BindingContext = vmLista;
        }

        private void VistaItem()
        {
            stkLista.IsVisible = false;
            stkControles.IsVisible = true;

            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnLimpiar.IsEnabled = true;
            enumVista = EnumVista.vistaItem;
        }

        private void VistaDobleLista()
        {
            VistaItem();
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnLimpiar.IsEnabled = false;
            enumVista = EnumVista.vistaLista;
        }


        #endregion


        #region FuncionesABM

        private void Lista_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                VistaItem();
                ItemVM.Objeto = ((ObjId)e.SelectedItem);

                BindingContext = ItemVM;
                ((VmBase)BindingContext).setItemPropertiesFromObject();

                controlesLimpios = false;
                ItemVM.Mostrar(null);
            }
            catch (Exception Ex)
            {
                this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            try
            {
                string Mensaje = "";

                ItemVM = (VmBase)BindingContext;

                if (controlesLimpios)
                {
                    ItemVM.ConectarBI();
                    Mensaje = await ItemVM.GuardarItem(EnumOperacion.Nuevo);
                }
                else
                {
                    ItemVM.ConectarBI();
                    Mensaje = await ItemVM.GuardarItem(EnumOperacion.Actualiza);
                }

                if (Mensaje == "")
                {
                    Mensaje = "Guardado";
                    await this.DisplayAlert("Mensaje", Mensaje, "Ok");
                    await VistaLista();
                }
                else
                    await this.DisplayAlert("Mensaje", Mensaje, "Ok");
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private void btnLimpiar_Clicked(object sender, EventArgs e)
        {//Es el botón de Nuevo
            try
            {
                LimpiarControles();
                VistaItem();
            }
            catch (Exception Ex)
            {
                this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void btnDelete_Clicked(object sender, EventArgs e)
        {
            try
            {
                ItemVM.ConectarBI();
                string Mensaje = await ItemVM.Bi.DeleteItem();

                await this.DisplayAlert(Mensaje, "Resultado", "Ok");
                await VistaLista();
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }

        }

        private async void btnAtras_Clicked(object sender, EventArgs e)
        {
            try
            {

                controlesLimpios = false;
                if (enumVista == EnumVista.vistaItem)
                {
                    await VistaLista();
                    await RefreshList();
                }
                else
                {
                    if (TituloABM.Text == "Torneos")
                    {
                        await App.navigationP.PopAsync();
                    }
                    else
                    {
                        ItemVM = ItemVM.ItemAtras;
                        ItemVM.setItemPropertiesFromObject();
                        BindingContext = ItemVM;
                        VistaItem();
                        ItemVM.Mostrar(null);
                    };
                };
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private void OcultarStacks()
        {
            try
            {
                stkNombre.IsVisible = false;
                stkTorneos.IsVisible = false;
                stkZonas.IsVisible = false;
                stkEquipos.IsVisible = false;
                stkListaEquipos.IsVisible = false;
                stkPartidos.IsVisible = false;
                stkJugadores.IsVisible = false;
            }
            catch (Exception Ex)
            {
                this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private void LimpiarControles()
        {
            try
            {
                //Recreo el Item y le preservo el ItemAtras.
                VmBase itemAtras = ItemVM.ItemAtras;
                ItemVM.NewObject();
                if (itemAtras != null)
                    ItemVM.ItemAtras = itemAtras;

                enumVista = EnumVista.vistaItem;
                BindingContext = ItemVM;
                ((VmBase)BindingContext).setItemPropertiesFromObject();
                controlesLimpios = true;
                ItemVM.Mostrar(null);
            }
            catch (Exception Ex)
            {
                this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        #endregion


        #region Controles_Cambio_de_Entidad

        private async void btnZonas_Clicked(object sender, EventArgs e)
        {
            try
            {
                ItemZona.ItemAtras = ItemTorneo;

                //Cargo los parametros para filtrar las zonas de este torneo
                ItemZona.NombresParametros = new[] { "IdTorneo" };
                ItemZona.ValoresParametros = new object[] { ItemTorneo.Objeto.ID };

                ItemVM = ItemZona;
                await VistaLista();
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void btnEquipos_Clicked(object sender, EventArgs e)
        {
            try
            {
                ItemEquipos.ItemAtras = ItemTorneo;
                ItemVM = ItemEquipos;
                ItemVM.Mostrar(null);

                Object[] valoresParametros = new Object[] { ItemTorneo.bTorneo.pTorneo.oTorneo.ID };
                ItemEquipos.AddParametros(new[] { "IdTorneo" }, valoresParametros);

                await VistaLista();
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void btnGenerarTorneo_Clicked(object sender, EventArgs e)
        {
            try
            {
                Funciones f = new Funciones();
                if (btnGenerarTorneo.Text == "Generar Torneo")
                {
                    await f.GenerarTorneos(ItemTorneo.bTorneo);
                    await this.DisplayAlert("Mensaje", "Torneo Generado", "Ok");
                    btnGenerarTorneo.Text = "Borrar Torneo";
                }
                else
                {
                    f.BorrarTorneoGenerado(ItemTorneo.bTorneo);
                    await this.DisplayAlert("Mensaje", "Torneo Borrado", "Ok");
                    btnGenerarTorneo.Text = "Generar Torneo";
                }
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void btnListaEquipos_Clicked(object sender, EventArgs e)
        {
            try
            {
                OcultarStacks();
                stkListaEquipos.IsVisible = true;

                ItemListaEquipos.ItemAtras = ItemZona;
                ItemVM = ItemListaEquipos;
                ItemVM.ItemAtras = ItemZona;

                ItemTorneo.ConectarBI();
                ListaEquipos_Torneo.ItemsSource = await ItemTorneo.bTorneo.MisEquiposDisponibles();
                ItemZona.ConectarBI();
                ListaEquipos_Zona.ItemsSource = await ItemZona.bZona.MisEquiposListaEquipos();

                VistaDobleLista();

                TituloABM.Text = "Lista de Equipos";
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");                
            }
        }

        private async void btnPartidos_Clicked(object sender, EventArgs e)
        {
            ItemPartidos.ItemAtras = ItemZona;
            ItemVM = ItemPartidos;

            ItemPartidos.AddParametro_Only("IdZona", ItemZona.Objeto.ID);

            await VistaLista();
        }

        private async void btnJugadores_Clicked(object sender, EventArgs e)
        {
            ItemJugadores.ItemAtras = ItemEquipos;
            ItemVM = ItemJugadores;

            await VistaLista();
        }

        private async void ListaEquipos_Torneo_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {//Agrego un equipo a la zona
            try
            {
                ObjEquipos Item = (ObjEquipos)e.SelectedItem;
                Item.IdZona = ItemVM.ItemAtras.Objeto.ID;
                ItemListaEquipos.ConectarBI();
                await ItemListaEquipos.bListaEquipo.AddEquipo(Item);

                ItemTorneo.ConectarBI();
                ListaEquipos_Torneo.ItemsSource = await ItemTorneo.bTorneo.MisEquiposDisponibles();
                ItemZona.ConectarBI();
                ListaEquipos_Zona.ItemsSource = await ItemZona.bZona.MisEquiposListaEquipos();
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        private async void ListaEquipos_Zona_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {//Saco un equipo de la zona 
            try
            {
                ObjEquipoListaEquipos Item = (ObjEquipoListaEquipos)e.SelectedItem;

                await ItemListaEquipos.bListaEquipo.RemoveEquipo(Item);

                ItemTorneo.ConectarBI();
                ListaEquipos_Torneo.ItemsSource = await ItemTorneo.bTorneo.MisEquiposDisponibles();
                ItemZona.ConectarBI();
                ListaEquipos_Zona.ItemsSource = await ItemZona.bZona.MisEquiposListaEquipos();

            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        #endregion


    }




}