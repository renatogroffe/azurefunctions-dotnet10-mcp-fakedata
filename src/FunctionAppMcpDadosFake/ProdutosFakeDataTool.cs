using Bogus;
using FunctionAppMcpDadosFake.Models;
using FunctionAppMcpDadosFake.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace FunctionAppMcpDadosFake;

public class ProdutosFakeDataTool
{
    private ILogger<ProdutosFakeDataTool> _logger;

    public ProdutosFakeDataTool(ILogger<ProdutosFakeDataTool> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProdutosFakeDataTool))]
    public IActionResult Run(
        [McpToolTrigger(nameof(ProdutosFakeDataTool), "Gera uma lista com dados fake de produtos.")] ToolInvocationContext context,
        [McpToolProperty(nameof(numberOfRecords), "Quantidade de registros.")] int numberOfRecords
    )
    {
        try
        {
            var result = NumberOfRecordsValidator<Produto>.Validate(numberOfRecords)!;
            if (result.IsSuccess!.Value)
            {
                var random = new Random();
                var fakeProdutos = new Faker<Produto>("pt_BR").StrictMode(false)
                    .RuleFor(p => p.Nome, f => f.Commerce.Product())
                    .RuleFor(p => p.CodigoBarras, f => f.Commerce.Ean13())
                    .RuleFor(p => p.Preco, f => random.Next(10, 30))
                    .Generate(numberOfRecords);
                result.Data = fakeProdutos;
                result.Message = $"{numberOfRecords} produto(s) fake gerado(s) com sucesso!";
                _logger.LogInformation(result.Message);
            }
            else
                _logger.LogWarning(result.Message!);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new Result<Produto>
            {
                IsSuccess = false,
                Message = $"Erro ao gerar dados fake de produtos: {ex.Message}"
            });
        }
    }
}