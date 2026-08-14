using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalPropertyManager.Models;

/// <summary>
/// Represents a personal belonging/property item tracked by the user.
/// </summary>
public class PersonalProperty : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string? _description;
    private decimal _currentValue;
    private string? _imagePath;
    private DesireStatus _desireStatus = DesireStatus.None;
    private ItemType _itemType = ItemType.Other;
    private DateTime _acquiredDate = DateTime.Now;
    private string? _notes;
    private string? _location;
    private DateTime _createdAt = DateTime.Now;
    private DateTime _updatedAt = DateTime.Now;

    [Key]
    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    [Required]
    [MaxLength(200)]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Detailed description of the item, including work experiences and skills related to it.
    /// </summary>
    [MaxLength(4000)]
    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// Current value of the item in US Dollars.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentValue
    {
        get => _currentValue;
        set => SetField(ref _currentValue, value);
    }

    /// <summary>
    /// File path (absolute or relative) of the item's picture.
    /// </summary>
    [MaxLength(500)]
    public string? ImagePath
    {
        get => _imagePath;
        set => SetField(ref _imagePath, value);
    }

    public DesireStatus DesireStatus
    {
        get => _desireStatus;
        set => SetField(ref _desireStatus, value);
    }

    public ItemType ItemType
    {
        get => _itemType;
        set => SetField(ref _itemType, value);
    }

    public DateTime AcquiredDate
    {
        get => _acquiredDate;
        set => SetField(ref _acquiredDate, value);
    }

    [MaxLength(2000)]
    public string? Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    [MaxLength(200)]
    public string? Location
    {
        get => _location;
        set => SetField(ref _location, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetField(ref _createdAt, value);
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => SetField(ref _updatedAt, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        if (propertyName is not null) OnPropertyChanged(propertyName);
        return true;
    }
}
