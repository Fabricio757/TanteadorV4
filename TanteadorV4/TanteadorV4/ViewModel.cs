using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;

namespace TanteadorV4
{
    //public delegate void vmEventHandler(object sender, EventArgs args);

    public interface INavigationVM
    {
        ViewModelBase ItemAtras { set; get; }
    }

    public class ViewModelBase : INotifyPropertyChanged, INavigationVM
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

        objId _Objeto;
        public objId Objeto { set { _Objeto = value; OnPropertyChanged(); } get { return _Objeto; } }
        public virtual void setItemPropertiesFromObject() { }
        protected SqlPersistObject Persist;

        protected ViewModelBase _ItemAtras;
        public virtual ViewModelBase ItemAtras { set { setAtras(value); } get { return _ItemAtras; } }

        protected void setAtras(ViewModelBase value)
        {
            _ItemAtras = value;
            Persist = getPersist();
            Persist.Filtro_GetItemsAsync = value.Objeto;
        }

        public virtual SqlPersistObject getPersist()
        {
            return null;
        }

        public virtual async Task<List<objId>> RetornarLista()
        {
            Persist = getPersist();
            return await Persist.GetItemsAsync();
        }

        public virtual void CargaCompleta()
        { }

        public virtual async Task<string> GuardarItem(EnumOperacion Operacion)
        {
            string Mensaje ="";

            try
            {
                Mensaje = ItemValido();

                Persist = getPersist();

                if (Mensaje == "")
                {
                    if (Operacion == EnumOperacion.Nuevo)
                    {
                        await Persist.InsertItemAsync(Objeto);
                    }
                    else
                    {
                        await Persist.UpdateItemAsync(Objeto);
                    }
                }
            }
            catch(Exception ex)
            {
                Mensaje = ex.Message;
            }

            return Mensaje;
        }

        public virtual string ItemValido()
        {
            return "";
        }

        public async void DeleteItem()
        {
            Persist = getPersist();
            await Persist.DeleteItemAsync(Objeto);
        }

        //
        public virtual void setObjeto(objId _Objeto)
        { }

        //
        public virtual void SetObjAtras(objId obj)
        { }

        public virtual void NewObject()
        { }

        public virtual void Mostrar(EventArgs e, bool recalculo = false)
        {
            EventHandler handler = OnMostrar;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }






    public class TorneosViewModel : ViewModelBase
    {
        public override SqlPersistObject getPersist()
        {
            return App.Database.Torneos;
        }

        public override void NewObject()
        {
            Objeto = new objTorneos();
        }

        public async Task<List<objEquipos>> MisEquipos()
        {
            return await App.Database.Torneos.MisEquipos((objTorneos)Objeto);
        }

        public async Task<List<objEquipos>> MisEquiposQueNoSonCabecera()
        {
            return await App.Database.Torneos.MisEquiposQueNoSonCabecera((objTorneos)Objeto);
        }

        public async Task<List<objEquipos>> MisEquiposDisponibles()
        {
            return await App.Database.Torneos.MisEquiposDisponibles((objTorneos)this.Objeto);
        }

        public async Task<List<objZonas>> MisZonas()
        {
            return await App.Database.Torneos.MisZonas((objTorneos)Objeto);
        }

        public Boolean TienePartidos()
        {
            return App.Database.Torneos.TienePartidos((objTorneos)Objeto);
        }

        public void BorrarPartidos()
        {
            App.Database.Torneos.BorrarPartidos((objTorneos)Objeto);
        }
    }

    
    public class ZonasViewModel : ViewModelBase
    {
        public override SqlPersistObject getPersist()
        {
            return App.Database.Zonas;
        }

        public override void NewObject()
        {
            Objeto = new objZonas();
            ((objZonas)Objeto).IdTorneo = _ItemAtras.Objeto.ID;

            Equipos = App.Database.Zonas.Equipos((objZonas)Objeto);

            _SelectedCabecera = null;
            OnPropertyChanged("SelectedCabecera");
        }

        public override void setItemPropertiesFromObject()
        {
            Equipos = App.Database.Zonas.Equipos((objZonas)Objeto);
            SelectedCabecera = Equipos.SingleOrDefault(a => a.ID == ((objZonas)Objeto).IdEquipoCabezaDeSerie);
        }

        IList<objEquipos> _Equipos;
        public IList<objEquipos> Equipos
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

        objEquipos _SelectedCabecera;
        public objEquipos SelectedCabecera
        {
            get { return _SelectedCabecera; }
            set
            {
                if (value != null)
                    if (_SelectedCabecera != value)
                    {
                        _SelectedCabecera = value;
                        if (value != null)
                            ((objZonas)Objeto).IdEquipoCabezaDeSerie = _SelectedCabecera.ID;
                        else
                            ((objZonas)Objeto).IdEquipoCabezaDeSerie = 0;

                        OnPropertyChanged("SelectedCabecera");
                    }
            }
        }

        public async Task<int> AddEquipo(objEquipos Equipo)
        {
            objListaEquipos Item = new objListaEquipos();
            Item.IdEquipo = Equipo.ID;
            Item.IdZona = Objeto.ID;
            await App.Database.ListaEquipos.InsertItemAsync(Item);

            Equipo.IdZona = Objeto.ID;
            await App.Database.Equipos.UpdateItemAsync(Equipo);

            return 0;
        }

        public void GuardarPartidos(List<objPartidos> listaPartidos)
        {
            foreach (objPartidos Partido in listaPartidos)
                App.Database.Partidos.InsertItemAsync(Partido);
        }

        public async Task<List<objEquipos>> MisEquipos()
        {
            return await App.Database.Zonas.MisEquipos((objZonas)Objeto);
        }

        public List<objEquipos> MisEquipos_Sync()
        {
            return App.Database.Zonas.MisEquipos_Sync((objZonas)Objeto);
        }
    }

    
    public class EquiposViewModel : ViewModelBase
    {
        public override SqlPersistObject getPersist()
        {
            return App.Database.Equipos;
        }

        public override void NewObject()
        {
            Objeto = new objEquipos();
            ((objEquipos)Objeto).IdTorneo = _ItemAtras.Objeto.ID;
        }

        public Task<List<objPartidos>> MisPartidos(objEquipos Equipo)
        {
            return App.Database.Equipos.MisPartidos(Equipo.ID);
        }

        public Task<Boolean> EsCabecera(objEquipos Equipo)
        {
            return App.Database.Equipos.EsCabecera(Equipo.ID);
        }
    }


    public class ListaEquiposViewModel : ViewModelBase, INavigationVM
    {
        public override ViewModelBase ItemAtras { set; get; }

        public virtual async Task<List<objId>> RetornarLista_Equipos(int IdTorneo)
        {
            //Persist = getPersist();
            //return await App.Database.ListaEquipos.GetItemsAsync_EquiposDisponibles(IdTorneo);
            return null;
        }

        public virtual async Task<List<objId>> RetornarLista_EquiposZona(int IdZona)
        {/* Retorna los equipos que estan en una zona, segun la lista zona */
            return await App.Database.ListaEquipos.GetItemsAsync_EquiposZona(IdZona);
        }

        public async Task AddEquipo(objEquipos Equipo)
        {
            objListaEquipos Item = new objListaEquipos();
            Item.IdZona = ItemAtras.Objeto.ID; //es una zona
            Item.IdEquipo = Equipo.ID;
            await App.Database.ListaEquipos.InsertItemAsync(Item);

            Equipo.IdZona = ItemAtras.Objeto.ID;
            await App.Database.Equipos.UpdateItemAsync(Equipo);
        }

        public async Task RemoveEquipo(objEquipos Equipo)
        {
            objListaEquipos Item = new objListaEquipos();
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


    public class PartidosViewModel : ViewModelBase
    {
        public override SqlPersistObject getPersist()
        {
            return App.Database.Partidos;
        }

        public override void NewObject()
        {
            Objeto = new objPartidos();
            ((objPartidos)Objeto).IdZona = _ItemAtras.Objeto.ID;

            MisEquipos = App.Database.Partidos.MisEquipos;            

            _selectedEquipo1 = null;
            OnPropertyChanged("SelectedEquipo1");
            _selectedEquipo2 = null;
            OnPropertyChanged("SelectedEquipo2");
        }

        public override void setItemPropertiesFromObject()
        {
            MisEquipos = App.Database.Partidos.MisEquipos;
            SelectedEquipo1 = MisEquipos.SingleOrDefault(a => a.ID == ((objPartidos)Objeto).IdEquipo1);
            SelectedEquipo2 = MisEquipos.SingleOrDefault(a => a.ID == ((objPartidos)Objeto).IdEquipo2);
        }

        IList<objEquipos> _MisEquipos;
        public IList<objEquipos> MisEquipos
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

        objEquipos _selectedEquipo1;
        public objEquipos SelectedEquipo1
        {
            get { return _selectedEquipo1; }
            set
            {
                if (value != null)
                if (_selectedEquipo1 != value)
                {
                    _selectedEquipo1 = value;
                    if (value != null)
                        ((objPartidos)Objeto).IdEquipo1 = _selectedEquipo1.ID;
                    else
                        ((objPartidos)Objeto).IdEquipo1 = 0;

                    OnPropertyChanged("SelectedEquipo1");
                }
            }
        }

        objEquipos _selectedEquipo2;
        public objEquipos SelectedEquipo2
        {
            get { return _selectedEquipo2; }
            set
            {
                if (value != null)
                if (_selectedEquipo2 != value)
                {
                    _selectedEquipo2 = value;
                    if (value != null)
                        ((objPartidos)Objeto).IdEquipo2 = _selectedEquipo2.ID;
                    else
                        ((objPartidos)Objeto).IdEquipo2 = 0;

                    OnPropertyChanged("SelectedEquipo2");
                }
            }
        }

        public override string ItemValido()
        {
            string Mensaje = "";

            if (_selectedEquipo1.ID == _selectedEquipo2.ID)
            { Mensaje = "No puede ser el mismo equipo"; }

            return Mensaje;
        }

               
        public override async Task<string> GuardarItem(EnumOperacion Operacion)
        {
            string Mensaje = "";

            try
            {
                Mensaje = ItemValido();

                if (((objPartidos)Objeto).Nombre == "")
                {
                    ((objPartidos)Objeto).Nombre = _selectedEquipo1.Nombre + " vs " + _selectedEquipo2.Nombre;
                }

                Persist = getPersist();

                if (Mensaje == "")
                {
                    if (Operacion == EnumOperacion.Nuevo)
                    {
                        await Persist.InsertItemAsync(Objeto);
                    }
                    else
                    {
                        await Persist.UpdateItemAsync(Objeto);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensaje = ex.Message;
            }

            return Mensaje;
        }
    }


    public class JugadoresViewModel : ViewModelBase
    {
        public override SqlPersistObject getPersist()
        {
            return App.Database.Jugadores;
        }

        public override void NewObject()
        {
            Objeto = new objJugadores();
            ((objJugadores)Objeto).IdEquipo = _ItemAtras.Objeto.ID;

            MisEquipos = App.Database.Jugadores.MisEquipos;

            _selectedEquipo = null;
            OnPropertyChanged("SelectedEquipo");
        }

        public override void setItemPropertiesFromObject()
        {
            MisEquipos = App.Database.Jugadores.MisEquipos;
            SelectedEquipo = MisEquipos.SingleOrDefault(a => a.ID == ((objJugadores)Objeto).IdEquipo);
        }

        IList<objEquipos> _MisEquipos;
        public IList<objEquipos> MisEquipos
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

        objEquipos _selectedEquipo;
        public objEquipos SelectedEquipo
        {
            get { return _selectedEquipo; }
            set
            {
                if (value != null)
                    if (_selectedEquipo != value)
                    {
                        _selectedEquipo = value;
                        if (value != null)
                            ((objJugadores)Objeto).IdEquipo = _selectedEquipo.ID;
                        else
                            ((objJugadores)Objeto).IdEquipo = 0;

                        OnPropertyChanged("SelectedEquipo");
                    }
            }
        }

    }


}



