using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using TouchTracking;

namespace TanteadorV4
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            TouchTracking.TouchEffect Efecto = new TouchEffect();
            Efecto.KeyAction += Efecto_KeyAction;
            Efecto.TouchAction += Efecto_TouchAction;

            //Principal.Effects.Add(Efecto);
        }

        private void Efecto_TouchAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Pressed)
                this.DisplayAlert("Display", "TouchAction", "Ok");
        }

 
        private void Efecto_KeyAction(object sender, KeyActionEventArgs args)
        {
           this.DisplayAlert("Display", "KeyAction", "Ok");
        }

        //await App.navigationP.PushAsync(new Tanteador());
        private async void Button_Clicked(object sender, EventArgs e)
        {
            await App.navigationP.PushAsync(new Tanteador());
        }

        private async void Button2_Clicked(object sender, EventArgs e)
        {
            await App.navigationP.PushAsync(new Page1());
        }
    }
}
