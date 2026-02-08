using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Kk.Kharts.Maui.Models;

/// <summary>
/// Represents a group of devices belonging to a company.
/// </summary>
public sealed class CompanyGroup : ObservableCollection<DeviceModel>, INotifyPropertyChanged
{
    public string CompanyName { get; }
    public int DeviceCount => Count;

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public CompanyGroup(string companyName, IEnumerable<DeviceModel> devices) : base(devices)
    {
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? "Sans entreprise" : companyName;
    }
}
