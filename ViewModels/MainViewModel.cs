using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PersonalPropertyManager.Models;
using PersonalPropertyManager.Services;

namespace PersonalPropertyManager.ViewModels;

/// <summary>
/// Main view-model bound to MainWindow. Holds the list of properties, the
/// currently-selected property (for the detail editor), and all CRUD commands.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPropertyService _service;

    public MainViewModel(IPropertyService service)
    {
        _service = service;
        Properties = new ObservableCollection<PersonalProperty>();
        FilteredProperties = new ObservableCollection<PersonalProperty>();

        ItemTypes = new[] { (ItemType?)null }
            .Concat(Enum.GetValues<ItemType>().Select(t => (ItemType?)t))
            .ToList();
        DesireStatuses = new[] { (DesireStatus?)null }
            .Concat(Enum.GetValues<DesireStatus>().Select(s => (DesireStatus?)s))
            .ToList();

        // Initial load
        _ = LoadAsync();
    }

    // ------- Bindable state -------

    public ObservableCollection<PersonalProperty> Properties { get; }

    public ObservableCollection<PersonalProperty> FilteredProperties { get; }

    /// <summary>Available types for the filter combo, prefixed with a null "All types" entry.</summary>
    public IReadOnlyList<ItemType?> ItemTypes { get; }

    /// <summary>Available statuses for the filter combo, prefixed with a null "All statuses" entry.</summary>
    public IReadOnlyList<DesireStatus?> DesireStatuses { get; }

    /// <summary>All ItemType values, for the detail editor.</summary>
    public IReadOnlyList<ItemType> AllItemTypes { get; } = Enum.GetValues<ItemType>();

    /// <summary>All DesireStatus values, for the detail editor.</summary>
    public IReadOnlyList<DesireStatus> AllDesireStatuses { get; } = Enum.GetValues<DesireStatus>();

    [ObservableProperty]
    private PersonalProperty? _selectedProperty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ItemType? _selectedType;

    [ObservableProperty]
    private DesireStatus? _selectedStatus;

    [ObservableProperty]
    private decimal _totalValue;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _statusMessage = "Ready.";

    [ObservableProperty]
    private bool _isBusy;

    // ------- Commands -------

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        try
        {
            var results = await _service.SearchAsync(SearchText, SelectedType, SelectedStatus);
            FilteredProperties.Clear();
            foreach (var p in results) FilteredProperties.Add(p);
            StatusMessage = $"Found {FilteredProperties.Count} item(s).";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedType = null;
        SelectedStatus = null;
        _ = SearchAsync();
    }

    [RelayCommand]
    private void AddNew()
    {
        var newItem = new PersonalProperty
        {
            Name = "New Item",
            CurrentValue = 0m,
            DesireStatus = DesireStatus.None,
            ItemType = ItemType.Other,
            AcquiredDate = DateTime.Now
        };
        SelectedProperty = newItem;
        StatusMessage = "Editing a new item. Click Save to commit.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProperty is null) return;

        try
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(SelectedProperty.Name))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProperty.Id == 0)
            {
                var added = await _service.AddAsync(SelectedProperty);
                Properties.Add(added);
                FilteredProperties.Add(added);
                SelectedProperty = added;
                StatusMessage = $"Added \"{added.Name}\".";
            }
            else
            {
                await _service.UpdateAsync(SelectedProperty);
                // Refresh the matching entry in the lists
                var inList = Properties.FirstOrDefault(p => p.Id == SelectedProperty.Id);
                if (inList is not null)
                {
                    var idx = Properties.IndexOf(inList);
                    Properties[idx] = SelectedProperty;
                }
                var inFiltered = FilteredProperties.FirstOrDefault(p => p.Id == SelectedProperty.Id);
                if (inFiltered is not null)
                {
                    var idx = FilteredProperties.IndexOf(inFiltered);
                    FilteredProperties[idx] = SelectedProperty;
                }
                StatusMessage = $"Updated \"{SelectedProperty.Name}\".";
            }
            await UpdateTotalsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Save failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProperty is null || SelectedProperty.Id == 0) return;

        var confirm = MessageBox.Show(
            $"Delete \"{SelectedProperty.Name}\"? This cannot be undone.",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            IsBusy = true;
            var id = SelectedProperty.Id;
            var name = SelectedProperty.Name;
            await _service.DeleteAsync(id);

            var inList = Properties.FirstOrDefault(p => p.Id == id);
            if (inList is not null) Properties.Remove(inList);
            var inFiltered = FilteredProperties.FirstOrDefault(p => p.Id == id);
            if (inFiltered is not null) FilteredProperties.Remove(inFiltered);

            SelectedProperty = null;
            StatusMessage = $"Deleted \"{name}\".";
            await UpdateTotalsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void BrowseImage()
    {
        if (SelectedProperty is null) return;

        var dlg = new OpenFileDialog
        {
            Title = "Select an image for the item",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            // Copy the image into the app's Images folder so it travels with the DB.
            var imagesDir = Path.Combine(AppContext.BaseDirectory, "Resources", "Images");
            Directory.CreateDirectory(imagesDir);

            var ext = Path.GetExtension(dlg.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var dest = Path.Combine(imagesDir, fileName);

            File.Copy(dlg.FileName, dest, overwrite: true);
            SelectedProperty.ImagePath = dest;
            StatusMessage = $"Image attached: {fileName}";
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        if (SelectedProperty is null) return;
        SelectedProperty.ImagePath = null;
        StatusMessage = "Image removed.";
    }

    // ------- Internals -------

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading…";

            var items = await _service.GetAllAsync();
            Properties.Clear();
            FilteredProperties.Clear();
            foreach (var p in items)
            {
                Properties.Add(p);
                FilteredProperties.Add(p);
            }
            SelectedProperty = Properties.FirstOrDefault();

            await UpdateTotalsAsync();
            StatusMessage = $"Loaded {Properties.Count} item(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Load failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Load failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateTotalsAsync()
    {
        TotalCount = Properties.Count;
        TotalValue = await _service.GetTotalValueAsync();
    }

    /// <summary>
    /// Helper for XAML — converts an absolute image path to a BitmapImage.
    /// Returns null when the path is missing or the file doesn't exist.
    /// </summary>
    public static BitmapImage? LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path, UriKind.Absolute);
            img.EndInit();
            img.Freeze();
            return img;
        }
        catch
        {
            return null;
        }
    }
}