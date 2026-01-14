using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Threading.Tasks;

namespace Diagnost.Misc.Avalonia
{
    // Custom Avalonia UserControl
    public class CustomUC : UserControl
    {
        protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            TopLevel? topLevel = TopLevel.GetTopLevel(this);

            if (topLevel is Window window)
            {
                double targetWidth = double.IsNaN(this.Width) ? window.Width : this.Width;
                double targetHeight = double.IsNaN(this.Height) ? window.Height : this.Height;

                if (double.IsNaN(this.Width) && double.IsNaN(this.Height))
                {
                    return;
                }

                await Task.Delay(50);

                window.Width = targetWidth;
                window.Height = targetHeight;
            }

            if (OperatingSystem.IsBrowser())
            {
                Width = double.NaN;
                Height = double.NaN;
            }
        }
    }
}
