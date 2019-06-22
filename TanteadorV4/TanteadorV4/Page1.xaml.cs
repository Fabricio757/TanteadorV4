﻿using System;
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


    public class IntToGridLengthConverter : IValueConverter
    {
        public IntToGridLengthConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = System.Convert.ToInt32(value);
            return new GridLength(intValue, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
            TituloABM.Text = "Partidos";
        }

        private void ItemEquipos_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkEquipos.IsVisible = true;
            stkNombre.IsVisible = true;

            btnJugadores.IsEnabled = ! controlesLimpios;

            TituloABM.Text = "Equipos";

            Lista.HeaderTemplate = new DataTemplate();
            
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

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();

            if (enumVista == EnumVista.vistaLista)
                BindingContext = vmLista;
        }

        private async void ItemTorneo_OnMostrar(object sender, EventArgs e)
        {
            OcultarStacks();
            stkTorneos.IsVisible = true;
            stkNombre.IsVisible = true;
            
            btnZonas.IsEnabled = ! controlesLimpios;
            btnEquipos.IsEnabled = ! controlesLimpios;

            if(ItemTorneo != null)
                if (ItemTorneo.Objeto.ID > 0)
                {
                    Boolean b = await ItemTorneo.TienePartidos();

                    if (b)
                        btnGenerarTorneo.Text = "Borrar Partidos";
                    else
                        btnGenerarTorneo.Text = "Generar Torneo";
                }
            
            TituloABM.Text = "Torneos";
            
            vmLista.Columnas(new Columna_Lista() { NombreAtributo = "Nombre", Titulo = "Torneo", Width= 1 }, new Columna_Lista() { NombreAtributo = "Titulo1", Titulo = "Titulo", Width = 1 });

            Lista.ItemTemplate = vmLista.DateTemplate_configuracion();

            if (enumVista == EnumVista.vistaLista)
                BindingContext = vmLista;        

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
            vmLista.ItemsSource = await ItemVM.RetornarLista();
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
                string Mensaje = await ItemVM.DeleteItem();

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
                    ItemVM = ItemVM.ItemAtras;
                    ItemVM.setItemPropertiesFromObject();
                    BindingContext = ItemVM;
                    ItemVM.Mostrar(null);
                    VistaItem();
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


        #region Controles

        private async void btnZonas_Clicked(object sender, EventArgs e)
        {
            try
            {
                ItemZona.ItemAtras = ItemTorneo;

                //Cargo los parametros para filtrar las zonas de este torneo
                ItemZona.Persist.NombresParametros = new[] { "IdTorneo" };
                ItemZona.Persist.ValoresParametros = new object[] { ItemTorneo.Objeto.ID };

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

                Object[] valoresParametros = new Object[] { ItemTorneo.pTorneo.oTorneo.ID };
                ItemEquipos.pEquipo.AddParametros(new[] { "IdTorneo" }, valoresParametros);

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
                    f.GenerarTorneos(ItemTorneo);
                    await this.DisplayAlert("Mensaje", "Torneo Generado", "Ok");
                    btnGenerarTorneo.Text = "Borrar Torneo";
                }
                else
                {
                    f.BorrarTorneoGenerado(ItemTorneo);
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


                ListaEquipos_Torneo.ItemsSource = await ItemTorneo.MisEquiposDisponibles();
                ListaEquipos_Zona.ItemsSource = await ItemZona.MisEquipos();
                //ListaEquipos_Zona.ItemsSource = await ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);

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

            ItemPartidos.Persist.AddParametro_Only("IdZona", ItemZona.Objeto.ID);

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
            ObjEquipos Item = (ObjEquipos)e.SelectedItem;
            await ItemListaEquipos.AddEquipo(Item);

            ListaEquipos_Torneo.ItemsSource = await ItemTorneo.MisEquiposDisponibles();
            ListaEquipos_Zona.ItemsSource = await ItemZona.MisEquipos();//ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);
        }

        private async void ListaEquipos_Zona_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {//Saco un equipo de la zona 
            try
            { 
                ObjEquipos Item = (ObjEquipos)e.SelectedItem;

                VmEquipos EquiposVM = new VmEquipos();
                EquiposVM.Objeto = Item;
                List<ObjPartidos> misPartidos = await EquiposVM.MisPartidos();

                

                if ((misPartidos.Count == 0) && (((ObjZonas)ItemZona.Objeto).IdEquipoCabezaDeSerie != Item.ID))
                {                
                    await ItemListaEquipos.RemoveEquipo(Item);

                    //ListaEquipos_Zona.ItemsSource = await ItemListaEquipos.RetornarLista_EquiposZona(ItemZona.Objeto.ID);
                    ListaEquipos_Zona.ItemsSource = await ItemZona.MisEquipos();
                    ListaEquipos_Torneo.ItemsSource = await ItemTorneo.MisEquiposDisponibles();
                }
                else
                    await DisplayAlert("Mensaje", "No se puede eliminar, es cabeza de serie o ya tiene partidos asignados", "Ok");
            }
            catch (Exception Ex)
            {
                await this.DisplayAlert("Mensaje", Ex.Message, "Ok");
            }
        }

        #endregion


    }




}