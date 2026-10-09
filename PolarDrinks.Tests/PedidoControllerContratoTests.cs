using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PolarDrinks.Controllers.Api;
using PolarDrinks.Models;
using PolarDrinks.Models.Loja;
using PolarDrinks.Services.Common;
using PolarDrinks.Services.Loja;
using PolarDrinks.Models.Responses;

namespace PolarDrinks.Tests;

public class PedidoControllerContratoTests
{
    private const int ClienteId = 5;

    // Campos internos que NUNCA podem aparecer no JSON enviado ao cliente da loja
    private static readonly string[] CamposInternos =
    {
        "itemPedidoCusto",
        "produtoPrecoCusto",
        "rowVersion",
        "produtoQtdEstoque",
        "produtoEstoqueMinimo",
        "produtoCodBarra",
        "clienteSenhaHash",
        "hash-secreto",
        "clienteCPF",
        "clienteEmail",
        "usuarioSeparouID",
        "usuarioEntregouID",
        "clienteID"
    };

    private readonly IPedidoService _pedidoService = Substitute.For<IPedidoService>();
    private readonly PedidoController _controller;

    public PedidoControllerContratoTests()
    {
        _controller = new PedidoController(_pedidoService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, ClienteId.ToString()) },
                        "teste"))
                }
            }
        };
    }

    [Fact]
    public void ObterDetalhe_RespostaNaoDeveExporCamposInternos()
    {
        _pedidoService.ObterDetalhePedido(ClienteId, 1).Returns(CriarPedidoCompleto());

        var json = Serializar(_controller.ObterDetalhe(1));

        AssertContemCamposDoContrato(json);
        AssertNaoContemCamposInternos(json);
    }

    [Fact]
    public void ListarPedidos_RespostaNaoDeveExporCamposInternos()
    {
        _pedidoService.ListarPedidosDoCliente(ClienteId)
            .Returns(new List<PedidoModel> { CriarPedidoCompleto() });

        var json = Serializar(_controller.ListarPedidos());

        AssertContemCamposDoContrato(json);
        AssertNaoContemCamposInternos(json);
    }

    [Fact]
    public void Checkout_RespostaNaoDeveExporCamposInternos()
    {
        _pedidoService.Checkout(ClienteId, "Pix")
            .Returns(ResultadoOperacao<PedidoModel>.Ok(CriarPedidoCompleto(), "Pedido realizado"));

        var json = Serializar(_controller.Checkout(new PedidoController.CheckoutRequest { TipoPagamento = "Pix" }));

        AssertContemCamposDoContrato(json);
        AssertNaoContemCamposInternos(json);
    }

    // ---------- Proteção do contrato ----------

    [Fact]
    public void PedidoResponse_De_DeveCopiarOsCamposDoContrato()
    {
        var resposta = PedidoResponse.De(CriarPedidoCompleto());

        Assert.Equal(1, resposta.PedidoID);
        Assert.Equal("A1B2", resposta.PedidoCodigo);
        Assert.Equal(PedidoModel.Status.Separado, resposta.PedidoStatus);
        Assert.Equal(170m, resposta.PedidoValorTotal);
        Assert.Equal(PedidoModel.TipoPagamento.Pix, resposta.PedidoTipoPagamento);

        var item = Assert.Single(resposta.Itens);
        Assert.Equal(2, item.ItemPedidoQtd);
        Assert.Equal(85m, item.ItemPedidoPreco);
        Assert.Equal("Vodka Gelada", item.Produto!.ProdutoNome);
    }

    [Fact]
    public void PedidoResponse_DeveConterApenasOsCamposPermitidos()
    {
        Assert.Equal(
            Ordenar(new[]
            {
                "PedidoID", "PedidoCodigo", "PedidoData", "PedidoStatus", "PedidoValorTotal",
                "PedidoTipoPagamento", "PedidoDataSeparado", "PedidoDataConcluido", "Itens"
            }),
            Ordenar(typeof(PedidoResponse).GetProperties().Select(p => p.Name)));

        Assert.Equal(
            Ordenar(new[] { "ItemPedidoQtd", "ItemPedidoPreco", "Produto" }),
            Ordenar(typeof(ItemPedidoResponse).GetProperties().Select(p => p.Name)));

        Assert.Equal(
            Ordenar(new[] { "ProdutoNome" }),
            Ordenar(typeof(ProdutoPedidoResponse).GetProperties().Select(p => p.Name)));
    }

    private static string[] Ordenar(IEnumerable<string> nomes) =>
        nomes.OrderBy(n => n, StringComparer.Ordinal).ToArray();
    // ---------- Métodos auxiliares ----------

    private static PedidoModel CriarPedidoCompleto()
    {
        var produto = new ProdutoModel
        {
            ProdutoID = 10,
            ProdutoNome = "Vodka Gelada",
            ProdutoPrecoCusto = 40m,
            ProdutoPrecoVenda = 100m,
            ProdutoCodBarra = "7890000000001",
            ProdutoQtdEstoque = 50,
            ProdutoEstoqueMinimo = 5,
            ProdutoAtivo = true,
            ProdutoImagemUrl = "/uploads/produtos/vodka.jpg",
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        var pedido = new PedidoModel
        {
            PedidoID = 1,
            PedidoCodigo = "A1B2",
            ClienteID = ClienteId,
            Cliente = new ClienteModel
            {
                ClienteID = ClienteId,
                ClienteNome = "Maria",
                ClienteEmail = "maria@teste.com",
                ClienteSenhaHash = "hash-secreto",
                ClienteCPF = "00000000000"
            },
            PedidoData = new DateTime(2026, 10, 8, 14, 0, 0),
            PedidoStatus = PedidoModel.Status.Separado,
            PedidoValorTotal = 170m,
            PedidoTipoPagamento = PedidoModel.TipoPagamento.Pix,
            UsuarioSeparouID = 3,
            UsuarioEntregouID = 4
        };

        pedido.Itens.Add(new ItemPedidoModel
        {
            ItemPedidoID = 99,
            PedidoID = 1,
            ProdutoID = 10,
            Produto = produto,
            ItemPedidoQtd = 2,
            ItemPedidoPreco = 85m,
            ItemPedidoCusto = 40m
        });

        return pedido;
    }

    // Serializa como a aplicação faz (camelCase, mesma configuração do Program.cs)
    private static string Serializar(IActionResult resultado)
    {
        var ok = Assert.IsType<OkObjectResult>(resultado);

        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        return JsonSerializer.Serialize(ok.Value, opcoes);
    }

    // Campos que a loja (JavaScript) realmente usa
    private static void AssertContemCamposDoContrato(string json)
    {
        string[] esperados =
        {
            "pedidoID", "pedidoCodigo", "A1B2", "pedidoStatus", "pedidoData",
            "pedidoValorTotal", "pedidoTipoPagamento",
            "itens", "itemPedidoQtd", "itemPedidoPreco",
            "produto", "produtoNome", "Vodka Gelada"
        };

        foreach (var campo in esperados)
        {
            Assert.Contains(campo, json, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertNaoContemCamposInternos(string json)
    {
        foreach (var campo in CamposInternos)
        {
            Assert.DoesNotContain(campo, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}