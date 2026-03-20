using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;
using Sales.Application.Mappers;

namespace Sales.WebApi.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ILogger<SalesController> _logger;
    private readonly ISalesOrderService _salesOrderService;
    private const string _getByIdRouteName = "GetSalesOrderHeaderById";

    public SalesController(ILogger<SalesController> logger, ISalesOrderService salesOrderService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _salesOrderService = salesOrderService ?? throw new ArgumentNullException(nameof(salesOrderService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrderHeaderDto>>> GetSalesOrderHeadersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var salesOrderHeaderDtos = await _salesOrderService.GetSalesOrderHeadersAsync(cancellationToken);
            return Ok(salesOrderHeaderDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when getting all sales order headers.");
            return BadRequest();
        }
    }

    [HttpGet("{id:int}", Name = _getByIdRouteName)]
    public async Task<ActionResult<SalesOrderHeaderDto>> GetSalesOrderHeaderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesOrderHeaderDto = await _salesOrderService.GetSalesOrderHeaderByIdAsync(id, cancellationToken);

            if (salesOrderHeaderDto == null) return NotFound();

            return Ok(salesOrderHeaderDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when getting sales order header by {Id}.", id);
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrderHeaderDto>> CreateSalesOrderAsync(SalesOrderHeaderForCreationDto salesOrderHeaderForCreationDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var createdSalesOrderHeaderDto = await _salesOrderService.AddSalesOrderHeaderAsync(salesOrderHeaderForCreationDto, cancellationToken);

            if (createdSalesOrderHeaderDto == null) return BadRequest();

            var routeValues = new
            {
                createdSalesOrderHeaderDto.Id
            };

            return CreatedAtRoute(_getByIdRouteName, routeValues, createdSalesOrderHeaderDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when creating sales order header.");
            return BadRequest();
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SalesOrderHeaderDto>> PutSalesOrderHeaderAsync(int id, SalesOrderHeaderForUpdateDto salesOrderHeaderForUpdateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var updatedSalesOrderHeaderDto = await _salesOrderService.UpdateSalesOrderHeaderAsync(id, salesOrderHeaderForUpdateDto, cancellationToken);

            if (updatedSalesOrderHeaderDto == null) return BadRequest();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when (put) updating sales order header by {Id}.", id);
            return BadRequest();
        }
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<SalesOrderHeaderDto>> PatchSalesOrderHeaderAsync(int id, JsonPatchDocument<SalesOrderHeaderForUpdateDto> patchDocument, CancellationToken cancellationToken = default)
    {
        try
        {
            var updatedSalesOrderHeaderDto = await _salesOrderService.PatchDocumentAsync(id, patchDocument, cancellationToken);

            if (updatedSalesOrderHeaderDto == null) return BadRequest();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when (patch) updating sales order header by {Id}.", id);
            return BadRequest();
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSalesOrderHeaderAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasDeleted = await _salesOrderService.DeleteSalesOrderHeaderAsync(id, cancellationToken);

            if (!hasDeleted) return BadRequest();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the API when deleting sales order header by {Id}.", id);
            return BadRequest();
        }
    }
}