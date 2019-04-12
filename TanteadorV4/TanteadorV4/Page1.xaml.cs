using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TanteadorV4
{
    public enum EnumABM { Torneo, Zona, Equipo, ListaEquipos, Partido, Jugadores };
    public enum EnumVista { vistaLista, vistaItem, DobleLista};
    public enum EnumOperacion { Nuevo, Actualiza };
    public enum EnumBindingType { Objeto, Item};


    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {


        EnumVista enumVista = EnumVista.vistaLista;
        Boolean controlesLimpios = false;
         
        
        private ViewModelBase ItemVM;
        private TorneosViewModel ItemTorneo;
        private ZonasViewModel ItemZona;
        private EquiposViewModel ItemEquipos;
        private PartidosViewModel ItemPartidos;
        private ListaEquiposViewModel ItemListaEquipos;
        private JugadoresViewModel ItemJugadores;


        public Page1()
        {
            InitializeComponent();

            ItemTorneo = new TorneosViewModel();
            ItemTorneo.OnMostrar += ItemTorneo_OnMostrar;
            ItemZona = new ZonasViewModel();
            ItemZona.OnMostrar += ItemZona_OnMostrar;
            ItemEquipos = new EquiposViewModel();
            ItemEquipos.OnMostrar += ItemEquipos_OnMostrar;
            ItemPartidos = new PartidosViewModel();
            ItemPartidos.OnMostrar += ItemPartidos_OnMostrar;

            ItemListaEquipos = new ListaEquiposViewModel();

            ItemJugadores = new JugadoresViewModel();
            ItemJugadores.OnMostrar += ItemJugadores_OnMostrar;
        }


        #region Mostrar

        private void ItemPartidos_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkPartidos.IsVisible = true;
            stkNombre.IsVisible = true;
            TituloABM.Text = "Partidos";

        }

        private void ItemEquipos_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkEquipos.IsVisible = true;
            stkNombre.IsVisible = true;

            btnJugadores.IsEnabled = ! controlesLimpios;

            TituloABM.Text = "Equipos";
        }

        private void ItemZona_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkZonas.IsVisible = true;
            stkNombre.IsVisible = true;

            btnListaEquipos.IsEnabled = ! controlesLimpios;
            btnPartidos.IsEnabled = ! controlesLimpios;

            TituloABM.Text = "Zonas";
        }

        private void ItemTorneo_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkTorneos.IsVisible = true;
            stkNombre.IsVisible = true;

            btnZonas.IsEnabled = ! controlesLimpios;
            btnEquipos.IsEnabled = ! controlesLimpios;

            if (ItemTorneo.TienePartidos())
                btnGenerarTorneo.Text = "Borrar Partidos";
            else
                btnGenerarTorneo.Text = "Generar Torneo";

            TituloABM.Text = "Torneos";
        }

        private void ItemJugadores_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkJugadores.IsVisible = true;
            stkNombre.IsVisible = true;

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
            LimpiarControles();
        }

        private async Task RefreshList()
        {
            Lista.ItemsSource = await ItemVM.RetornarLista();
        }

        private async Task VistaLista()
        {
            stkLista.IsVisible = true;
            stkControles.IsVisible = false;

            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            enumVista = EnumVista.vistaLista;
            await RefreshList();
        }

        private void VistaItem()
        {
            stkLista.IsVisible = false;
            stkControles.IsVisible = true;

            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            enumVista = EnumVista.vistaItem;
        }

        private void VistaDobleLista()
        {
            VistaItem();
            enumVista = EnumVista.vistaLista;
        }


        #endregion


        #region FuncionesABM

        private void Lista_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            bool recalculo = false;

            VistaItem();
            ItemVM.Objeto = ((objId)e.SelectedItem);

            BindingContext = ItemVM;
            ((ViewModelBase)BindingContext).setItemPropertiesFromObject();

            controlesLimpios = false;
            ItemVM.Mostrar(null, recalculo);
        }


        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            string Mensaje = "";

            ItemVM = (ViewModelBase)BindingContext;

            if (controlesLimpios)
            {
                Mensaje = await ItemVM.GuardarItem(EnumOperacion.Nuevo);
            }
            else
            {
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

        private void btnLimpiar_Clicked(object sender, EventArgs e)
        {
            LimpiarControles();
            VistaItem();
        }

        private async void btnDelete_Clicked(object sender, EventArgs e)
        {
            ItemVM.DeleteItem();
            await this.DisplayAlert("Mensaje", "Borrado", "Ok");
            await VistaLista();
        
        }

        private async void btnAtras_Clicked(object sender, EventArgs e)
        {
            controlesLimpios = false;
            if (enumVista == EnumVista.vistaItem)
            {
                await VistaLista();
                await RefreshList();
                
            }
            else
            {
                ItemVM = ItemVM.ItemAtras;
                BindingContext = ItemVM;
                ItemVM.Mostrar(null);
                VistaItem();
            };
        }

        private void OcultarStacks()
        {
            stkNombre.IsVisible = false;
            stkTorneos.IsVisible = false;
            stkZonas.IsVisible = false;
            stkEquipos.IsVisible = false;
            stkListaEquipos.IsVisible = false;
            stkPartidos.IsVisible = false;
            stkJugadores.IsVisible = false;
        }

        private void LimpiarControles()
        {
            ItemVM.Objeto = null;
            ItemVM.NewObject();
            BindingContext = ItemVM;
            controlesLimpios = true;
            ItemVM.Mostrar(null,true);
        }

        #endregion


        #region Controles

        private async void btnZonas_Clicked(object sender, EventArgs e)
        {
            ItemZona.ItemAtras = ItemTorneo;
            ItemVM = ItemZona;
            ItemVM.Mostrar(null);

            await VistaLista();
            
        }

        private async void btnEquipos_Clicked(object sender, EventArgs e)
        {
            ItemEquipos.ItemAtras = ItemTorneo;
            ItemVM = ItemEquipos;
            ItemVM.Mostrar(null);

            await VistaLista();
        }

        private async void btnGenerarTorneo_Clicked(object sender, EventArgs e)
        {
            Funciones f = new Funciones();
            if (btnGenerarTorneo.Text == "Generar Torneo")
            {
                f.GenerarTorneos(ItemTorneo);
                await this.DisplayAlert("Mensaje", "Torneo Generado", "Ok");
                btnGenerarTorneo.Text = "Borrar Torneo";
            }
            else
            {
                f.BorrarPartidos(ItemTorneo);
                await this.DisplayAlert("Mensaje", "Torneo Borrado", "Ok");
                btnGenerarTorneo.Text = "Generar Torneo";
            }
        }

        private async void btnListaEquipos_Clicked(object sender, EventArgs e)
        {
            OcultarStacks();
            stkListaEquipos.IsVisible = true;

            ItemListaEquipos.ItemAtras = ItemZona;
            ItemVM = ItemListaEquipos;
            ItemVM.ItemAtras = ItemZona;

            ListaEquipos_Torneo.ItemsSource = await ItemListaEquipos.RetornarLista_Equipos(ItemTorneo.Objeto.ID);
            ListaEquipos_Zona.ItemsSource = await ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);

            VistaDobleLista();

            TituloABM.Text = "Lista de Equipos";
        }

        private async void btnPartidos_Clicked(object sender, EventArgs e)
        {
            ItemPartidos.ItemAtras = ItemZona;
            ItemVM = ItemPartidos;

            await VistaLista();
        }

        private async void btnJugadores_Clicked(object sender, EventArgs e)
        {
            ItemJugadores.ItemAtras = ItemEquipos;
            ItemVM = ItemJugadores;

            await VistaLista();
        }

         private async void ListaEquipos_Torneo_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            objEquipos Item = (objEquipos)e.SelectedItem;
            await ItemListaEquipos.AddEquipo(Item);

            ListaEquipos_Torneo.ItemsSource = await ItemListaEquipos.RetornarLista_Equipos(ItemTorneo.Objeto.ID);
            ListaEquipos_Zona.ItemsSource = await ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);
        }

        private async void ListaEquipos_Zona_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            objEquipos Item = (objEquipos)e.SelectedItem;
            await ItemListaEquipos.RemoveEquipo(Item);
            ListaEquipos_Zona.ItemsSource = await ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);
            ListaEquipos_Torneo.ItemsSource = await ItemListaEquipos.RetornarLista_Equipos(ItemTorneo.Objeto.ID);
        }

        #endregion


    }




}