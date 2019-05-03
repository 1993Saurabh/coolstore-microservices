using System;
using System.Linq;
using System.Threading.Tasks;
using Coolstore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VND.CoolStore.Services.OpenApiV1.v1.Grpc;
using static Coolstore.CatalogService;
using static VND.CoolStore.Services.Inventory.v1.Grpc.InventoryService;

namespace VND.CoolStore.Services.OpenApiV1.v2
{
    [ApiVersion("2.0")]
    [Route("catalog/api/products")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger;
        private readonly CatalogServiceClient _catalogServiceClient;
        private readonly InventoryServiceClient _inventoryServiceClient;

        public CatalogController(ILoggerFactory loggerFactory, CatalogServiceClient catalogServiceClient, InventoryServiceClient inventoryServiceClient)
        {
            _logger = loggerFactory.CreateLogger<CatalogController>();
            _catalogServiceClient = catalogServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("admin-ping")]
        public IActionResult AdminPing()
        {
            return Ok();
        }

        [HttpGet("{currentPage:int}/{highPrice:int}")]
        public async ValueTask<IActionResult> GetProductWithPageAndPrice(int currentPage, int highPrice)
        {
            return await HttpContext.EnrichGrpcWithHttpContext(
                "catalog-service",
                async headers =>
                {
                    _logger.LogInformation("headers:", headers);

                    var request = new GetProductsRequest
                    {
                        CurrentPage = currentPage,
                        HighPrice = highPrice
                    };
                    var response = await _catalogServiceClient.GetProductsAsync(request, headers);
                    var extraResponse = response.Products
                    .Select(x =>
                        new
                        {
                            x.Id,
                            x.Name,
                            x.Desc,
                            x.Price,
                            x.ImageUrl,
                            IsHot = new Random().Next() % 2 == 0
                        });

                    return Ok(extraResponse);
                });
        }

        [HttpGet("{productId:guid}")]
        public async ValueTask<IActionResult> GetProductById(Guid productId)
        {
            return await HttpContext.EnrichGrpcWithHttpContext(
                "catalog-service",
                async headers =>
                {
                    _logger.LogInformation("headers:", headers);

                    var request = new GetProductByIdRequest
                    {
                        ProductId = productId.ToString()
                    };

                    var response = await _catalogServiceClient.GetProductByIdAsync(request, headers);
                    if (response?.Product == null)
                        throw new Exception($"Couldn't find product with id#{productId}.");

                    var inventory = await _inventoryServiceClient.GetInventoryAsync(
                        new Inventory.v1.Grpc.GetInventoryRequest
                        {
                            Id = response.Product.InventoryId
                        }, headers);
                    if (inventory == null)
                        throw new Exception($"Couldn't find inventory of product with id#{productId}.");

                    return Ok(new
                    {
                        response.Product.Id,
                        response.Product.Name,
                        response.Product.Desc,
                        response.Product.Price,
                        response.Product.ImageUrl,
                        InventoryId = inventory.Result.Id,
                        InventoryLink = inventory.Result.Link,
                        InventoryLocation = inventory.Result.Location,
                    });
                });
        }

        [HttpPost]
        public async ValueTask<IActionResult> CreateProduct(CreateProductRequest request)
        {
            return await HttpContext.EnrichGrpcWithHttpContext(
                "catalog-service",
                async headers =>
                {
                    _logger.LogInformation("headers:", headers);

                    var response = await _catalogServiceClient.CreateProductAsync(request, headers);
                    return Ok(response);
                });
        }
    }
}
