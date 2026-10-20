# Resultados dos testes

Este documento mostra **como reproduzir** os testes, **em que ambiente** eles rodam e **o que cada cenário prova**: fluxos válidos, entradas inválidas, rollback e operações repetidas. Os nomes dos testes são os do código em `PolarDrinks.Tests`.

## Sumário

1. [Resultado da execução](#1-resultado-da-execução)
2. [Como reproduzir](#2-como-reproduzir)
3. [Ambiente e configuração](#3-ambiente-e-configuração)
4. [Tipos de teste](#4-tipos-de-teste)
5. [Cenários cobertos](#5-cenários-cobertos)
6. [O que os testes não cobrem](#6-o-que-os-testes-não-cobrem)

---

## 1. Resultado da execução

| Item | Valor |
|---|---|
| Comando | dotnet test PolarDrinks/PolarDrinks.sln (na raiz do repositório).|
| Total de testes | 123 |
| Aprovados | 123 |
| Falhas | 0 |
| Código testado | branch `refactor/modulos` |
| Data da execução | _preencher_ |
| Duração | _preencher_ |

> Os testes de integração são **pulados** (e aparecem como "ignorados", não como aprovados) quando o SQL Server de testes não está acessível. Por isso, confira no resumo do `dotnet test` que o número de **ignorados é 0**; só assim os 123 incluem o rollback real no banco.

## 2. Como reproduzir

Na pasta onde está o arquivo `PolarDrinks.sln`:

```
dotnet test
```

Para ver o nome de cada teste na saída:

```
dotnet test --logger "console;verbosity=normal"
```

Para rodar por tipo (os testes têm o rótulo `Categoria`):

```
dotnet test --filter "Categoria=Integracao"
dotnet test --filter "Categoria=Arquitetura"
```

Os testes sem rótulo (os de serviço, que usam dependências simuladas) rodam com o `dotnet test` completo.

No Visual Studio: menu *Teste > Gerenciador de Testes > Executar Todos os Testes*.

## 3. Ambiente e configuração

- **.NET 10** e as ferramentas de teste do projeto: xUnit 2.9.3 e NSubstitute 6.2.0.
- **SQL Server** acessível para os testes de integração. Por padrão eles se conectam com `Server=.;Trusted_Connection=True;TrustServerCertificate=True;` (SQL Server local com autenticação do Windows).
- Para usar outro servidor (por exemplo, o do Docker), defina a variável de ambiente `POLARDRINKS_TESTES_CONEXAO` com a cadeia de conexão **sem** nome de banco.
- **Isolamento:** os testes de integração criam um banco temporário próprio (`PolaDrinks_Testes_` seguido de um identificador único), aplicam as migrations reais e **apagam o banco ao final**. Eles nunca usam o banco `PolaDrinks` do dia a dia nem credenciais de produção. Cada cenário limpa os dados antes de rodar (`LimparDados`).
- Os testes de integração compartilham um banco por execução e rodam em sequência, não em paralelo.

## 4. Tipos de teste

| Tipo | O que usa | Para que serve |
|---|---|---|
| **Serviço** | Dependências simuladas (NSubstitute) | Regras de negócio, rejeições e repetições, de forma rápida |
| **Integração** (`Categoria=Integracao`) | SQL Server real, banco temporário | Provar que o rollback desfaz tudo no banco. Simulações e provider em memória não provam isso |
| **Arquitetura** (`Categoria=Arquitetura`) | Leitura do código-fonte | Garantir as fronteiras entre módulos (regras R1 a R9) |

## 5. Cenários cobertos

### 5.1 Fluxos válidos (controle)

| Cenário | Teste |
|---|---|
| Venda válida calcula o preço e pede a saída de estoque | `FinalizarVenda_VendaValida_DeveCalcularPrecoESolicitarSaidaDeEstoque` |
| Venda válida: servidor define data, usuário e status | `FinalizarVenda_VendaValida_ServidorDefineDataUsuarioEStatus` |
| Venda válida persiste venda, itens, movimentações e estoque (banco real) | `FinalizarVenda_Valida_DevePersistirVendaItensMovimentacoesEEstoque` |
| Tipos de pagamento válidos são aceitos | `FinalizarVenda_TipoPagamentoValido_DeveAceitar` |
| Produto cadastrado com categoria válida (banco real) | `CadastrarProduto_CategoriaValida_DevePersistirProdutoECategoria` |
| Cadastro de produto com imagem grava e confirma | `CadastrarProduto_ComImagem_DeveGravarEConfirmar` |
| Cancelamento de venda recente devolve cada item | `CancelarVenda_VendaRecente_DevolveCadaItemEMarcaComoCancelada` |
| Cancelamento de pedido devolve itens e muda o status | `CancelarPeloCliente_PedidoAguardando_DevolveItensEMudaStatus`, `CancelarAposEntrega_PedidoConcluido_DevolveItensEMudaStatus` |
| Confirmar entrega de compra pede a entrada de cada item | `ConfirmarEntrega_CompraAguardando_SolicitaEntradaDeCadaItemEConclui` |
| Saída, devolução e entrada de estoque com saldo | `RegistrarSaida_ComSaldo_...`, `RegistrarDevolucao_CancelamentoDeVenda_...`, `RegistrarEntrada_DeveSomarEstoque...` |
| Preço: R$ 100,00 com 15% resulta em R$ 85,00 | `CemReaisCom15PorCento_DeveSerOitentaECinco` |

### 5.2 Entradas inválidas (rejeitadas sem efeitos)

| Cenário | Teste |
|---|---|
| Quantidade zero ou negativa na venda | `FinalizarVenda_QuantidadeZeroOuNegativa_DeveRejeitarSemEfeitos` |
| Mesmo produto em duas linhas (cinco em estoque, duas linhas de três) | `FinalizarVenda_MesmoProdutoEmDuasLinhas_DeveSomarQuantidadesEVerificarSaldo` |
| Produto inativo | `FinalizarVenda_ProdutoInativo_DeveRejeitarSemEfeitos` |
| Tipo de pagamento inválido | `FinalizarVenda_TipoPagamentoInvalido_DeveRejeitarSemEfeitos` |
| Saldo insuficiente, produto inexistente, quantidade inválida no estoque | `RegistrarSaida_SaldoInsuficiente_...`, `RegistrarSaida_ProdutoInexistente_...`, `RegistrarSaida_QuantidadeZeroOuNegativa_...` |
| Devolução sem origem, com duas origens ou com tipo errado | `RegistrarDevolucao_SemOrigemOuComDuasOrigens_...`, `RegistrarDevolucao_TipoQueNaoEDevolucao_...` |
| Cancelar venda depois de 24 horas | `CancelarVenda_Apos24Horas_DeveRejeitarSemDevolver` |
| Código de barras duplicado no cadastro | `CadastrarProduto_CodigoDeBarrasDuplicado_DeveRejeitarSemAbrirTransacao` |
| Compra sem itens válidos | `CadastrarCompra_SemItensValidos_DeveRejeitarSemGravar` |
| Promoção fora de 0 a 100 é ignorada | `PromocaoInvalida_DeveSerIgnorada` |
| Campos extras não fazem parte do contrato | `FinalizarVendaRequest_DeveConterApenasOsCamposPermitidos`, `PedidoResponse_DeveConterApenasOsCamposPermitidos` |
| Resposta de pedido não expõe campos internos (custo, `RowVersion`) | `ObterDetalhe_RespostaNaoDeveExporCamposInternos`, `ListarPedidos_...`, `Checkout_...` |

### 5.3 Rollback (nada fica pela metade)

| Cenário | Teste |
|---|---|
| Falha no meio da venda desfaz tudo **no banco real** | `FinalizarVenda_FalhaNoMeio_DeveDesfazerTudoNoBanco` |
| Depois de uma falha, o mesmo contexto não reaproveita o estado da tentativa anterior | `FinalizarVenda_DepoisDeUmaFalha_NoMesmoContexto_NaoDeveReaproveitarEstadoDaTentativaAnterior` |
| Cadastro de produto com falha: sem produto e sem arquivo (banco real) | `CadastrarProduto_CategoriaInexistente_NaoDeveDeixarProdutoNemArquivo` |
| Falha ao gravar categorias: desfaz e remove a imagem | `CadastrarProduto_FalhaAoGravarCategorias_DeveDesfazerERemoverAImagem` |
| Falha ao gravar o produto: nenhum arquivo salvo | `CadastrarProduto_FalhaAoGravarOProduto_NaoDeveSalvarArquivoNenhum` |
| Estoque recusa a saída na venda: desfaz tudo | `FinalizarVenda_QuandoEstoqueRecusaASaida_DeveDesfazerTudo` |
| Estoque recusa a devolução: não cancela nem grava | `CancelarVenda_QuandoEstoqueRecusaADevolucao_...`, `CancelarPeloCliente_QuandoEstoqueRecusa_...` |
| Estoque recusa a entrada: compra não é concluída | `ConfirmarEntrega_QuandoEstoqueRecusaAEntrada_NaoDeveConcluirNemGravar` |
| Expiração: um pedido com devolução recusada não afeta os demais | `ExpirarPedidos_PedidoComDevolucaoRecusada_NaoAfetaOsDemais` |
| Expiração: conflito de concorrência não quebra nem conta o pedido | `ExpirarPedidos_ConflitoDeConcorrenciaNaGravacao_NaoDeveContarNemQuebrar` |

### 5.4 Operações repetidas

| Cenário | Teste |
|---|---|
| Cancelar a mesma venda duas vezes: a segunda é rejeitada, sem nova devolução | `CancelarVenda_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao` |
| Cancelar o mesmo pedido duas vezes (cliente e após a entrega) | `CancelarPeloCliente_Repetido_...`, `CancelarAposEntrega_Repetido_...` |
| Devolver duas vezes o mesmo item ao estoque | `RegistrarDevolucao_ItemJaDevolvido_DeveRejeitarSemEfeitos` |
| Confirmar duas vezes a entrega de uma compra: sem nova entrada | `ConfirmarEntrega_Repetido_NaoDeveGerarNovaEntrada` |

### 5.5 Dashboard e cálculos

| Cenário | Teste |
|---|---|
| Totais financeiros vêm dos resumos calculados no banco | `Financeiro_DeveUsarOsResumosCalculadosNoBanco` |
| Pedidos só contam no faturamento se concluídos ou não retirados | `Pedidos_SoContamNoFaturamento_ConcluidosENaoRetirados` |
| Sem vendas nem pedidos, os tickets valem zero | `SemVendasNemPedidos_TicketsDevemSerZero` |
| Previsão: tendência crescente, decrescente, constante, dois pontos, salto, negativa, série vazia, um ponto | `PrevisaoLinearTests` (13 casos com resultados conhecidos) |

### 5.6 Fronteiras de arquitetura

| Regra | Teste |
|---|---|
| R1: dependências entre módulos só nas direções permitidas | `R1_ModulosSoDependemUnsDosOutrosNasDirecoesPermitidas` |
| R2: só o Estoque usa o repository de movimentações | `R2_SoOEstoqueUsaORepositoryDeMovimentacoes` |
| R3: só o Estoque altera o saldo dos produtos | `R3_SoOEstoqueAlteraOSaldoDosProdutos` |
| R4: controllers não acessam repositories nem o banco | `R4_ControllersNaoAcessamRepositoriosNemOBanco` |
| R5: services e repositories não usam tipos da camada web | `R5_ServicesERepositoriesNaoUsamTiposDaCamadaWeb` |
| R6: models não dependem das camadas de cima | `R6_ModelsNaoDependemDasCamadasDeCima` |
| R7: `Compartilhado` não depende de módulos | `R7_CompartilhadoNaoDependeDeModulos` |
| R8: namespace igual ao caminho da pasta | `R8_NamespaceDeCadaArquivoIgualAoCaminhoDaPasta` |
| R9: Views no mesmo módulo do controller | `R9_PastasDeViewsFicamNoMesmoModuloDoController` |

Em `RegrasDeArquiteturaTests` há ainda testes que provam que **cada regra realmente detecta uma violação** (código de exemplo que quebra a regra precisa ser acusado). Assim, uma regra não passa "por não estar olhando para nada".

## 6. O que os testes não cobrem

- **Retry de concorrência do checkout:** o descarte do estado após uma falha está testado, mas não há teste de integração que provoque o conflito de concorrência do checkout e confirme as novas tentativas.
- **Telas e JavaScript:** não há teste automatizado de interface; essas partes foram conferidas manualmente.
- **Segurança e desempenho:** não fazem parte do escopo dos testes.
- **Autenticação administrativa:** o `Autenticar` do `UsuarioService` não tem teste automatizado próprio.
