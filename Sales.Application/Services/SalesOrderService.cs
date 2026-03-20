using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;
using Sales.Application.Mappers;
using Sales.Domain.Interfaces;

namespace Sales.Application.Services;

public class SalesOrderService(ILogger<SalesOrderService> logger, ISalesOrderRepository salesOrderRepository)
    : ISalesOrderService
{
    private readonly ILogger<SalesOrderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISalesOrderRepository _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));

    public async Task<IEnumerable<SalesOrderHeaderDto>> GetSalesOrderHeadersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _salesOrderRepository.GetAllAsync(cancellationToken);
            return Mapper.MapFromEntitiesToDtos(entities);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred GetAllAsync");
            return new List<SalesOrderHeaderDto>();
        }
    }

    public async Task<SalesOrderHeaderDto?> GetSalesOrderHeaderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);

        return entity == null
            ? null
            : Mapper.MapFromEntityToDto(entity);
    }

    public async Task<SalesOrderHeaderDto?> AddSalesOrderHeaderAsync(SalesOrderHeaderForCreationDto salesOrderHeaderForCreationDto,
        CancellationToken cancellationToken = default)
    {
        var entityToBeCreated = Mapper.MapCreationDtoToEntity(salesOrderHeaderForCreationDto);

        var createdEntity = _salesOrderRepository.Add(entityToBeCreated);

        var hasSaved = await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        if (hasSaved) return Mapper.MapFromEntityToDto(createdEntity);

        _logger.LogError("Error creating SalesOrderHeader. AddSalesOrderHeaderAsync.");
        return null;
    }


    public async Task<SalesOrderHeaderDto?> UpdateSalesOrderHeaderAsync(int id, SalesOrderHeaderForUpdateDto salesOrderHeaderForUpdateDto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);

        if (entity == null)
        {
            _logger.LogError("Not found SalesOrderHeader with {Id}.... UpdateSalesOrderHeaderAsync", id);
            return null;
        }

        var updatedEntity = Mapper.UpdateEntityWithDto(salesOrderHeaderForUpdateDto, entity);

        var dbUpdatedEntity = _salesOrderRepository.Update(updatedEntity);

        var hasSaved = await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        if (hasSaved) return Mapper.MapFromEntityToDto(dbUpdatedEntity);

        _logger.LogError("Error updating SalesOrderHeader with {Id}.... UpdateSalesOrderHeaderAsync", id);
        return null;
    }

    public async Task<SalesOrderHeaderDto?> PatchDocumentAsync(int id, JsonPatchDocument<SalesOrderHeaderForUpdateDto> patchDocument,
        CancellationToken cancellationToken = default)
    {
        var entity = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var saleOrderHeaderToPatch = Mapper.MapEntityDtoToUpdateDto(entity);

        patchDocument.ApplyTo(saleOrderHeaderToPatch);

        var updatedEntity = Mapper.UpdateEntityWithDto(saleOrderHeaderToPatch, entity);

        var dbUpdatedEntity = _salesOrderRepository.Update(updatedEntity);

        var hasSaved = await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        if (hasSaved) return Mapper.MapFromEntityToDto(dbUpdatedEntity);

        _logger.LogError("Error updating SalesOrderHeader with {Id}.... UpdateSalesOrderHeaderAsync", id);
        return null;
    }

    public async Task<bool> DeleteSalesOrderHeaderAsync(int id, CancellationToken cancellationToken = default)
    {
        var entityToBeDeleted = await _salesOrderRepository.GetByIdAsync(id, cancellationToken);

        if (entityToBeDeleted == null)
        {
            _logger.LogError("Not found SalesOrderHeader with {Id}.... DeleteSalesOrderHeaderAsync", id);
            return false;
        }

        _ = _salesOrderRepository.Delete(entityToBeDeleted);

        var hasDeleted = await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        if (!hasDeleted)
        {
            _logger.LogError("Error deleting SalesOrderHeader with {Id}.... DeleteSalesOrderHeaderAsync", id);
        }

        return hasDeleted;
    }
}
