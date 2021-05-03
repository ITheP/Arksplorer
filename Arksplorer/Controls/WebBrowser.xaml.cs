using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                if (element is Button button)
                    button.Click -= Navigate_Click;
            }
            Shortcuts.Children.Clear();

            foreach (var shortcut in shortcuts)
                _ = AddShortcut(shortcut);
        }

        public Button AddShortcut(string description, string toolTip, string url, Visibility visibility = Visibility.Visible)
        {
            Button button = new();
            button.Style = this.FindResource("ShortcutButton") as Style;
            button.Content = description;
            button.ToolTip = toolTip;
            button.Tag = url;
            button.Click += Navigate_Click;
            button.Visibility = visibility;

            Shortcuts.Children.Add(button);

            return button;
        }

        public Button AddShortcut(ListInfoItem shortcut, Visibility visibility = Visibility.Visible)
        {
            return AddShortcut(shortcut.Description, shortcut.Details, shortcut.Value, visibility);
        }

        public Button AddEmptyShortcut()
        {
            return AddShortcut(null, null, null, Visibility.Collapsed);
        }

        private Button[] RotatingShortcuts { get; set; }

        public void InitRotatingShortcuts(int count)
        {
            if (count < 1)
                return;

            RotatingShortcuts = new Button[count];

            for (int i = 0; i < count; i++)
            {
                var button = AddEmptyShortcut();
                button.Background = Brushes.Lavender;
                RotatingShortcuts[i] = button;
            }
        }

        public void UpdateRotatingShortcuts(string description, string toolTip, string url)
        {
            int count = RotatingShortcuts.Length;

            var button = RotatingShortcuts[count-1];
            if ((string)button.Content == description && (string)button.Tag == url)
                return;

            for (int i = 0; i < RotatingShortcuts.Length - 1; i++)
            {
                var b1 = RotatingShortcuts[i];
                var b2 = RotatingShortcuts[i + 1];

                b1.Content = b2.Content;
                b1.ToolTip = b2.ToolTip;
                b1.Tag = b2.Tag;
                b1.Visibility = b2.Visibility;
            }

            button.Content = description;
            button.ToolTip = toolTip;
            button.Tag = url;
            button.Visibility = Visibility.Visible;
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
