using Bogus;
using FunctionAppMcpDadosFake.Models;
using FunctionAppMcpDadosFake.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace FunctionAppMcpDadosFake;

public class EmpresasFakeDataTool
{
    private ILogger<EmpresasFakeDataTool> _logger;

    public EmpresasFakeDataTool(ILogger<EmpresasFakeDataTool> logger)
    {
        _logger = logger;
    }

    [Function(nameof(EmpresasFakeDataTool))]
    public IActionResult Run(
        [McpToolTrigger(nameof(EmpresasFakeDataTool), "Gera uma lista com dados fake de empresas.")] ToolInvocationContext context,
        [McpToolProperty(nameof(numberOfRecords), "Quantidade de registros.")] int numberOfRecords
    )
    {
        try
        {
            var result = NumberOfRecordsValidator<Empresa>.Validate(numberOfRecords)!;
            if (result.IsSuccess!.Value)
            {
                var fakeEmpresas = new Faker<Empresa>("pt_BR").StrictMode(false)
                    .RuleFor(e => e.Nome, f => f.Company.CompanyName())
                    .RuleFor(e => e.Cidade, f => f.Address.City())
                    .RuleFor(e => e.Pais, f => "Brasil")
                    .Generate(numberOfRecords);
                result.Data = fakeEmpresas;
                result.Message = $"{numberOfRecords} empresa(s) fake gerada(s) com sucesso!";
                _logger.LogInformation(result.Message);
            }
            else
                _logger.LogWarning(result.Message!);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new Result<Empresa>
            {
                IsSuccess = false,
                Message = $"Erro ao gerar dados fake de empresas: {ex.Message}"
            });
        }
    }
}