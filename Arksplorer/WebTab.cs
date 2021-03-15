using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Arksplorer
{
    public class WebTab
    {
        public bool Loading { get; set; }
        public Viewbox LoadingControl { get; set; }
        public string CurrentUrl { get; set; }

        public WebTab(WebView2 browser)
        {
            AttachWebEvents(browser);
        }

        private void AttachWebEvents(WebView2 browser)
        {
            browser.NavigationStarting += NavigationStarting;
            browser.SourceChanged += SourceChanged;
            browser.ContentLoading += ContentLoading;
            browser.NavigationCompleted += NavigationCompleted;
        }

        public void NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            CurrentUrl = args.Uri;
            // If something goes wrong, args.Cancel = true;
            Debug.Print("Navigation starting");
        }

        public void SourceChanged(object sender, CoreWebView2SourceChangedEventArgs args)
        {
            Debug.Print("Source changed");
        }

        public void ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs args)
        {
            LoadingControl.Child = new Spinner();
            Debug.Print("Content loading");
        }

        public void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            Debug.Print("Navigation completed");
            LoadingControl.Child = null; 
        }


    }
}
