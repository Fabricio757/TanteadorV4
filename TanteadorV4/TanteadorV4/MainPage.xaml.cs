using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading;

namespace TanteadorV4
{
    public class Timer
    {
        private readonly TimeSpan _timeSpan;
        private readonly Action _callback;
        public Boolean Active;
        private DateTime TiempoInicial;

        private TimeSpan TiempoAcumulado;
        
        private static CancellationTokenSource _cancellationTokenSource;

        public Timer(TimeSpan timeSpan, Action callback)
        {
            _timeSpan = timeSpan;
            _callback = callback;
            _cancellationTokenSource = new CancellationTokenSource();
            TiempoAcumulado = new TimeSpan();
            Active = false;
        }

        public void Start()
        {
            Active = true;
            TiempoInicial = System.DateTime.Now;
            CancellationTokenSource cts = _cancellationTokenSource; // safe copy
            Device.StartTimer(_timeSpan, () =>
            {
                if (cts.IsCancellationRequested)
                {
                    return false;
                }

                _callback.Invoke();
                return true; //true to continuous, false to single use
            });
        }

        public void Stop()
        {
            TiempoAcumulado = TiempoAcumulado.Add(System.DateTime.Now - TiempoInicial);
            Active = false;
            Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource()).Cancel();
        }

        public void Reset()
        {
            TiempoAcumulado = TiempoAcumulado.Subtract(TiempoAcumulado);
            TiempoInicial = System.DateTime.Now;
        }

        public String GetMinutos()
        {
            TimeSpan T = TiempoAcumulado.Add(System.DateTime.Now - TiempoInicial);

            String M = T.Hours.ToString("00") + ":" + T.Minutes.ToString("00") + ":" + T.Seconds.ToString("00");
            return (M);
        }
    }

    public class DispositiveStyle
    {
        private String Dispositivo;

        private String FontFamily_subDirectorio = "";
        private Double FontSizeRelacion = 1;

        private String _fontFamily_1;
        private String _fontFamily_2;

        private Double _fontSize_1;
        private Double _fontSize_2;


        public Color BackgroundColor { set; get; }
        public String fontFamily_1
        {
            set
            {
                _fontFamily_1 = value;
            }
            get
            {
                return FontFamily_subDirectorio + _fontFamily_1;
            }
        }
        public String fontFamily_2
        {
            set
            {
                _fontFamily_2 = value;
            }
            get
            {
                return FontFamily_subDirectorio + _fontFamily_2;
            }
        }

        public Double fontSize_1
        {
            set
            {
                _fontSize_1 = value;
            }
            get
            {
                return _fontSize_1 * FontSizeRelacion;
            }
        }

        public Double fontSize_2
        {
            set
            {
                _fontSize_2 = value;
            }
            get
            {
                return _fontSize_2 * FontSizeRelacion;
            }
        }



        public DispositiveStyle()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                Dispositivo = "Android";
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                Dispositivo = "UWP";
                FontFamily_subDirectorio = "Assets/";
                FontSizeRelacion = 2;
            }
        }

    }

    public partial class MainPage : ContentPage
    {
        private Timer Reloj_Timer;
        private int Numero1_Valor = 0;
        private int Numero2_Valor = 0;
        private int IndicePartido = 0;
        private Object gSender;
        private string[] Desc_Partidos = new string[] { "PARTIDO", "REVANCHA", "BUENO" };


        public MainPage()
        {
            InitializeComponent();

            InitMedia();

            DispositiveStyle DStyle = new DispositiveStyle();

            DStyle.fontFamily_1 = "roboclonestraight3d.ttf#Robo-Clone Straight 3D";
            DStyle.fontFamily_2 = "Storyboo.TTF#Storybook";

            DStyle.fontSize_1 = 48;
            DStyle.fontSize_2 = 18;

            DStyle.BackgroundColor = Color.Red;

            var tapGestureRecognizer1 = new TapGestureRecognizer();
            var tapGestureRecognizer2 = new TapGestureRecognizer();


            tapGestureRecognizer1.Tapped += (s, e) => {
                takePhoto(s, e);
            };
            tapGestureRecognizer2.Tapped += (s, e) => {
                takePhoto(s, e);
            };

            image1.GestureRecognizers.Add(tapGestureRecognizer1);
            image2.GestureRecognizers.Add(tapGestureRecognizer2);

            var tapGestureRecognizerIG = new TapGestureRecognizer();

            tapGestureRecognizerIG.Tapped += (s, e) => {
                StackL_GeneralInput.IsVisible = true;
                gSender = s;
            };
            SubTitulo1.GestureRecognizers.Add(tapGestureRecognizerIG);
            SubTitulo2.GestureRecognizers.Add(tapGestureRecognizerIG);
            Nombre1.GestureRecognizers.Add(tapGestureRecognizerIG);
            Nombre2.GestureRecognizers.Add(tapGestureRecognizerIG);


            SubTitulo1.BackgroundColor = DStyle.BackgroundColor;
            SubTitulo2.BackgroundColor = DStyle.BackgroundColor;
            gTitulo.BackgroundColor = DStyle.BackgroundColor;
            Columna1.BackgroundColor = DStyle.BackgroundColor;
            Columna2.BackgroundColor = DStyle.BackgroundColor;
            Columna3.BackgroundColor = DStyle.BackgroundColor;
            Nombre1.BackgroundColor = DStyle.BackgroundColor;
            Nombre2.BackgroundColor = DStyle.BackgroundColor;


            Reloj.Text = "00:00:00";
            Reloj_Timer = new Timer(TimeSpan.FromSeconds(2), () => Reloj.Text = Reloj_Timer.GetMinutos());

            Reloj.FontSize = DStyle.fontSize_1;
            Reloj.FontFamily = DStyle.fontFamily_1;

            Numero1.FontFamily = DStyle.fontFamily_1;


            Numero2.FontFamily = DStyle.fontFamily_1;
            Nombre1.FontFamily = DStyle.fontFamily_2;

            Numero1.FontSize = DStyle.fontSize_1;
            Numero2.FontSize = DStyle.fontSize_1;
            Numero1.BackgroundColor = Color.Black;
            Numero2.BackgroundColor = Color.Black;
            Numero1.TextColor = Color.White;
            Numero2.TextColor = Color.White;

            Numero1.Text = "0";
            Numero1_Valor = 0;

            Numero2.Text = "0";
            Numero2_Valor = 0;

            SubTitulo1.Text = "Torneo";
            SubTitulo1.FontFamily = DStyle.fontFamily_2;
            SubTitulo1.FontSize = 48;
            SubTitulo1.TextColor = Color.White;

            SubTitulo2.Text = "Ramallo";
            SubTitulo2.FontFamily = DStyle.fontFamily_2;
            SubTitulo2.FontSize = 48;
            SubTitulo2.TextColor = Color.White;

            Nombre1.TextColor = Color.White;
            Nombre2.TextColor = Color.White;

        }

        

        private async void takePhoto(object sender, EventArgs e)
        {
            try
            {

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    DisplayAlert("No Camera", ":( No camera available.", "OK");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "Test",
                    SaveToAlbum = false,
                    CompressionQuality = 75,
                    CustomPhotoSize = 50,
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 2000,
                    DefaultCamera = CameraDevice.Front

                });

                if (file == null)
                    return;

                //DisplayAlert("File Location", file.Path, "OK");
                

                ((Image)sender).Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });

            }
            catch (Exception ex)
            {
                DisplayAlert("Captura de error", ex.Message, "OK");
            }
        }

        private async void Logo_Tapped(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,

            });


            if (file == null)
                return;

            Logo.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }
    


        private async void InitMedia()
        {
            await CrossMedia.Current.Initialize();
        }

        private void Reloj_Tapped(object sender, EventArgs e)
        {
            if (Reloj_Timer.Active == false)
            {
                Reloj_Timer.Start();
            }
            else
            {
                Reloj_Timer.Stop();
            }
        }

        private void Reloj_Tapped2(object sender, EventArgs e)
        {
            if (Reloj_Timer.Active == false)
            {
                Reloj_Timer.Reset();
                Reloj.Text = Reloj_Timer.GetMinutos();
            }
        }

        private void Numero1_Tapped(object sender, EventArgs e)
        {
            Numero1_Valor++;
            Numero1.Text = Numero1_Valor.ToString();
        }

        private void Numero1_Tapped2(object sender, EventArgs e)
        {
            Numero1_Valor = Numero1_Valor -2;
            Numero1.Text = Numero1_Valor.ToString();
        }
        
        private void Numero2_Tapped(object sender, EventArgs e)
        {
            Numero2_Valor++;
            Numero2.Text = Numero2_Valor.ToString();
        }

        private void Numero2_Tapped2(object sender, EventArgs e)
        {
            Numero2_Valor = Numero2_Valor - 2;
            Numero2.Text = Numero2_Valor.ToString();
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if ((e.TotalX > 0) && (IndicePartido < 2))
                IndicePartido++;

            if ((e.TotalX < 0) && (IndicePartido > 0))
                IndicePartido--;

            Partido.Text = Desc_Partidos[IndicePartido];
        }

        private void l_Start_Tapped(object sender, EventArgs e)
        {

        }

        private void b_Ok_GeneralInput_Clicked(object sender, EventArgs e)
        {
            ((Label)gSender).Text = GeneralInput.Text;
            StackL_GeneralInput.IsVisible = false;
        }

        private void b_Cancel_GeneralInput_Clicked(object sender, EventArgs e)
        {
            StackL_GeneralInput.IsVisible = false;
        }
    }
}
