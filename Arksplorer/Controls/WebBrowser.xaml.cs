using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace Arksplorer
{
    /// <summary>
    /// Interaction logic for WebTab.xaml
    /// </summary>
    public partial class WebBrowser : UserControl
    {
        public string Description { get; set; }
        public bool Loading { get; set; }
        public string CurrentUrl { get; set; }
        public TabItem Tab { get; set; }

        public WebBrowser()
        {
            InitializeComponent();

            // Remove loading spinner shown for dev
            LoadingSpinner.Child = null;
        }

        public void Init(string description, string defaultUrl)
        {
            SetBrowserSource(defaultUrl);
            SetHomeUrl(defaultUrl);

            Init(description);
        }

        public void Init(string description)
        {
            Description = description;

            // Load up shortcut buttons (if we can find any defined) - Uses filename based on description with any spaces removed
            string filename = $"./Data/Shortcuts-{Description.Replace(" ", "")}.json";
            if (File.Exists(filename))
            {
                try
                {
                    List<ListInfoItem> shortcuts = JsonSerializer.Deserialize<List<ListInfoItem>>(File.ReadAllText(filename));

                    SetShortcuts(shortcuts);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"There was an error loading shortcuts from {filename}: {ex.Message}", "Error loading shortcuts", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            AttachWebEvents(Browser);
        }

        public void SetBrowserSource(string url)
        {
            Browser.Source = new Uri(url);
        }

        public void SetHomeUrl(string url)
        {
            Home.Tag = url;
        }

        public void SetShortcuts(List<ListInfoItem> shortcuts)
        {
            // When clearing buttons, make sure any event's are cleared too - incase of references hanging around
            foreach (var element in Shortcuts.Children)
            {
                if (element is Button)
                    ((Button)element).Click -= Navigate_Click;
            }
            Shortcuts.Children.Clear();

            foreach (var shortcut in shortcuts)
                AddShortcut(shortcut);
        }

        public void AddShortcut(ListInfoItem shortcut)
        {
            Button button = new();
            button.Style = this.FindResource("ShortcutButton") as Style;
            button.Content = shortcut.Description;
            button.ToolTip = shortcut.Details;
            button.Tag = shortcut.Value;
            button.Click += Navigate_Click;

            Shortcuts.Children.Add(button);
        }

        private void AttachWebEvents(WebView2 browser)
        {
            browser.NavigationStarting += NavigationStarting;
            browser.SourceChanged += SourceChanged;
            browser.ContentLoading += ContentLoading;
            browser.NavigationCompleted += NavigationCompleted;
        }

        public void Navigate(string url, TabItem jumpToTab = null)
        {
            try
            {
                if (Browser.CoreWebView2 == null)
                    Browser.Source = new Uri(url);
                else
                    Browser.CoreWebView2?.Navigate(url);

                if (jumpToTab != null)
                    jumpToTab.Focus();
            }
            catch (Exception ex)
            {
                Debug.Print($"Error navigating to '{url}': {ex.Message}");
            }
        }

        public void NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            CurrentUrl = args.Uri;
            // If something goes wrong, args.Cancel = true;
            Debug.Print($"{Description} Browser: Navigation starting [{CurrentUrl}]");
        }

        public void SourceChanged(object sender, CoreWebView2SourceChangedEventArgs args)
        {
            Debug.Print($"{Description} Browser: Source changed");
        }

        public void ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs args)
        {
            LoadingSpinner.Child = new Spinner();
            Debug.Print($"{Description} Browser: Content loading");
        }

        public void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            Debug.Print($"{Description} Browser: Navigation completed");
            LoadingSpinner.Child = null;
        }

        private void GoBack()
        {
            if (Browser.CanGoBack)
                Browser.GoBack();
        }

        private void GoForward()
        {
            if (Browser.CanGoForward)
                Browser.GoForward();
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate((string)((Button)sender).Tag);
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            GoForward();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void OpenExternal_Click(object sender, RoutedEventArgs e)
        {
            Web.OpenUrlInExternalBrowser(CurrentUrl);
        }
    }
}
