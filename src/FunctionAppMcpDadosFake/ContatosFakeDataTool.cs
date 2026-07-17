using Bogus;
using FunctionAppMcpDadosFake.Models;
using FunctionAppMcpDadosFake.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace FunctionAppMcpDadosFake;

public class ContatosFakeDataTool
{
    private ILogger<ContatosFakeDataTool> _logger;

    public ContatosFakeDataTool(ILogger<ContatosFakeDataTool> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ContatosFakeDataTool))]
    public IActionResult Run(
        [McpToolTrigger(nameof(ContatosFakeDataTool), "Gera uma lista com dados fake de contatos.")] ToolInvocationContext context,
        [McpToolProperty(nameof(numberOfRecords), "Quantidade de registros.")] int numberOfRecords
    )
    {
        try
        {
            var result = NumberOfRecordsValidator<Contato>.Validate(numberOfRecords)!;
            if (result.IsSuccess!.Value)
            {
                var fakeContatos = new Faker<Contato>("pt_BR").StrictMode(false)
                    .RuleFor(c => c.Nome, f => f.Company.CompanyName())
                    .RuleFor(c => c.Telefone, f => f.Phone.PhoneNumber())
                    .Generate(numberOfRecords);
                result.Data = fakeContatos;
                result.Message = $"{numberOfRecords} contato(s) fake gerado(s) com sucesso!";
                _logger.LogInformation(result.Message);
            }
            else
                _logger.LogWarning(result.Message!);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new Result<Contato>
            {
                IsSuccess = false,
                Message = $"Erro ao gerar dados fake de contatos: {ex.Message}"
            });
        }
    }
}