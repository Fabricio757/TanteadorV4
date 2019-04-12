using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TanteadorV4;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(TanteadorV4.Droid.BorderlessEntryRenderer))]
[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(TanteadorV4.Droid.BorderlessEditorRenderer))]
namespace TanteadorV4.Droid
{
        public class BorderlessEntryRenderer : EntryRenderer
        {
            public BorderlessEntryRenderer(Context context) : base(context)
            {
                AutoPackage = false;
            }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
            {
                base.OnElementChanged(e);
                if (e.OldElement == null)
                {
                    Control.Background = null;
                }
            }
        }

    public class BorderlessEditorRenderer : EditorRenderer
    {
        public BorderlessEditorRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Background = null;
            }
        }
    }
}