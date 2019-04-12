using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TanteadorV4;
using Xamarin.Forms;


[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(TanteadorV4.UWP.BorderlessEntryRenderer))]
[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(TanteadorV4.UWP.BorderlessEditorRenderer))]
namespace TanteadorV4.UWP
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
            }
        }
    }

    public class BorderlessEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
            }
        }
    }
}
