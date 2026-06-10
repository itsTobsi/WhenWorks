using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Text.Json;
using WhenWorks.Models;
using WhenWorks.ViewModels;

namespace WhenWorks.Views
{
    public partial class MainWindow : Window
    {
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnSave(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            try
            {
                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save availability",
                    SuggestedFileName = "availability.json",
                    DefaultExtension = "json",
                    FileTypeChoices = new[]
                    { new FilePickerFileType("JSON") { Patterns = new[] { "*.json"} } }
                });

                if (file == null)
                {
                    return;
                }

                await using var stream = await file.OpenWriteAsync();
                await JsonSerializer.SerializeAsync(stream, vm.SaveToFile(), jsonOptions);

                vm.Status = $"Saved to {file.Name}";
            }
            catch (Exception ex)
            {
                vm.Status = $"Save failed: {ex.Message}";
                throw;
            }
        }

        private async void OnLoad(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load availability",
                AllowMultiple = false,
                FileTypeFilter = new[]
                { new FilePickerFileType("JSON") { Patterns = new[] { "*.json"} } }
            });

            if (file.Count == 0)
            {
                return;
            }

            await using var stream = await file[0].OpenReadAsync();
            var data = await JsonSerializer.DeserializeAsync<SaveFile>(stream, jsonOptions);

            if (data != null)
            {
                vm.LoadFromFile(data);
            }
        }
    }
}