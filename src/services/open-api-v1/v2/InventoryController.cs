using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VND.CoolStore.Services.Inventory.v1.Grpc;
using VND.CoolStore.Services.OpenApiV1.v1.Grpc;
using static VND.CoolStore.Services.Inventory.v1.Grpc.InventoryService;

namespace VND.CoolStore.Services.OpenApiV1.v2
{
    [ApiVersion("2.0")]
    [Route("inventory/api/availabilities")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryServiceClient _inventoryServiceClient;

        public InventoryController(InventoryServiceClient inventoryServiceClient)
        {
            _inventoryServiceClient = inventoryServiceClient;
        }

        [HttpGet("ping")]
        public IActionResult Index()
        {
            return Ok();
        }

        [HttpGet]
        public async ValueTask<IActionResult> GetInventories()
        {
            return await HttpContext.EnrichGrpcWithHttpContext(
                "inventory-service",
                async headers =>
                {
                    var response = await _inventoryServiceClient.GetInventoriesAsync(new Google.Protobuf.WellKnownTypes.Empty(), headers);
                    return Ok(response);
                });
        }

        [HttpGet("{id:guid}")]
        public async ValueTask<IActionResult> GetInventoryById(Guid id)
        {
            return await HttpContext.EnrichGrpcWithHttpContext(
                "inventory-service",
                async headers =>
                {
                    var request = new GetInventoryRequest {
                        Id = id.ToString()
                    };
                    var response = await _inventoryServiceClient.GetInventoryAsync(request, headers);
                    return Ok(response);
                });
        }
    }
}
