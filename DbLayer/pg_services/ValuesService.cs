using DbLayer.Models;
using DbLayer;
using Domain;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

internal class ValuesService : IValuesService, IValuesServiceInternal
{
  private readonly PgDbContext _dbContext;

  public ValuesService(PgDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Dictionary<string, ValueDTO>> GetListByIdsAsync(List<string> ids)
  {
    var guids = ids.Select(Domain.Utils.ConvertObjectIdToGuid).Where(g => g != null).ToList();
    var entities = await _dbContext.Values
      .Where(v => guids.Contains(v.id))
      .ToListAsync();

    return entities.ToDictionary(
      v => Domain.Utils.ConvertGuidToObjectId(v.id),
      v => ConvertDB2DTO(v));
  }

  public async Task UpdateListAsync(List<ValueDTO> valuesToUpdate)
  {
    foreach (var dto in valuesToUpdate)
    {
      var entity = ConvertDTO2DB(dto);
      _dbContext.Values.Update(entity);
    }

    await _dbContext.SaveChangesAsync();
  }

  public async Task RemoveAsync(List<string> ids)
  {
    var guids = ids.Select(Domain.Utils.ConvertObjectIdToGuid).Where(g => g != null).ToList();
    var toRemove = await _dbContext.Values
      .Where(v => guids.Contains(v.id))
      .ExecuteDeleteAsync();
  }

  public async Task<Dictionary<string, ValueDTO>> GetListByOwnersAsync(List<string> owners)
  {
    var guids = owners.Select(id => Domain.Utils.ConvertObjectIdToGuid(id)).ToList();

    var entities = await _dbContext.Values
      .Where(v => guids.Contains(v.owner_id))
      .ToListAsync();

    return entities.ToDictionary(
      v => Domain.Utils.ConvertGuidToObjectId(v.id),
      v => ConvertDB2DTO(v));
  }

  public static DBValue ConvertDTO2DB(ValueDTO dto)
  {
    return new DBValue
    {
      id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Guid.NewGuid(),
      name = dto.name,
      owner_id = Domain.Utils.ConvertObjectIdToGuid(dto.owner_id) ?? Guid.NewGuid(),
      value = JsonSerializer.SerializeToDocument(dto.value)
    };
  }

  public static ValueDTO ConvertDB2DTO(DBValue dbo)
  {
    return new ValueDTO
    {
      id = Domain.Utils.ConvertGuidToObjectId(dbo.id),
      name = dbo.name,
      owner_id = Domain.Utils.ConvertGuidToObjectId(dbo.owner_id),
      value = dbo.value?.Deserialize<object>(),
      Version = dbo.Version
    };
  }

  public async Task<Dictionary<string, ValueDTO>> UpdateValuesFilteredByNameAsync(List<ValueDTO> values)
  {
    var result = new Dictionary<string, ValueDTO>();

    foreach (var dto in values)
    {
      var ownerGuid = Domain.Utils.ConvertObjectIdToGuid(dto.owner_id) ?? Guid.NewGuid();

      var existing = await _dbContext.Values
        .FirstOrDefaultAsync(v => v.owner_id == ownerGuid && v.name == dto.name);

      if (existing != null)
      {
        existing.value = JsonSerializer.SerializeToDocument(dto.value);
        _dbContext.Values.Update(existing);

        result[existing.id.ToString()] = new ValueDTO
        {
          id = existing.id.ToString(),
          name = existing.name,
          owner_id = Domain.Utils.ConvertGuidToObjectId(existing.owner_id),
          value = dto.value
        };
      }
      else
      {
        var newId = Guid.NewGuid();
        var newValue = new DBValue
        {
          id = newId,
          name = dto.name,
          owner_id = ownerGuid,
          value = JsonSerializer.SerializeToDocument(dto.value)
        };

        await _dbContext.Values.AddAsync(newValue);

        result[newId.ToString()] = new ValueDTO
        {
          id = newId.ToString(),
          name = newValue.name,
          owner_id = Domain.Utils.ConvertGuidToObjectId(ownerGuid),
          value = dto.value
        };
      }
    }

    await _dbContext.SaveChangesAsync();
    return result;
  }


}
