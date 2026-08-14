using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalPropertyManager.Models;

namespace PersonalPropertyManager.Services;

/// <summary>
/// Abstraction so view-models can be unit-tested without a real DbContext.
/// </summary>
public interface IPropertyService
{
    Task<List<PersonalProperty>> GetAllAsync();
    Task<PersonalProperty?> GetByIdAsync(int id);
    Task<PersonalProperty> AddAsync(PersonalProperty item);
    Task UpdateAsync(PersonalProperty item);
    Task DeleteAsync(int id);
    Task<List<PersonalProperty>> SearchAsync(string? searchText, ItemType? type, DesireStatus? status);
    Task<decimal> GetTotalValueAsync();
}