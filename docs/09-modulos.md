# Módulos

Este documento responde: **o que cada módulo do PolarDrinks é dono, o que oferece aos outros, do que depende e onde ficam seus arquivos**. É o guia de quem vai abrir o código para alterar algo. A visão geral e o porquê dos módulos estão em [02-arquitetura](02-arquitetura.md); a lista de regras verificadas por teste, no [mapa de responsabilidades](entrega/MAPA-DE-RESPONSABILIDADES.md).

## Sumário

1. [Como um módulo é organizado](#1-como-um-módulo-é-organizado)
2. [Onde mexer para cada tipo de mudança](#2-onde-mexer-para-cada-tipo-de-mudança)
3. [Quadro dos módulos](#3-quadro-dos-módulos)
4. [Usuários](#4-usuários)
5. [Estoque](#5-estoque)
6. [Compras](#6-compras)
7. [Vendas](#7-vendas)
8. [Loja](#8-loja)
9. [Dashboard](#9-dashboard)
10. [Compartilhado e raiz do projeto](#10-compartilhado-e-raiz-do-projeto)
11. [Regras para mexer sem quebrar](#11-regras-para-mexer-sem-quebrar)
12. [Limitações](#12-limitações)

---

## 1. Como um módulo é organizado

Um módulo é uma pasta em `PolarDrinks/Modules/<Modulo>` que reúne **tudo** de uma capacidade de negócio, de cima a baixo:

```
Modules/<Modulo>/
├── Controllers/    recebem a requisição e devolvem tela ou JSON
├── Services/       regras de negócio (a interface I...Service fica ao lado)
├── Repositories/   consultas e alterações preparadas no banco
├── Models/         entidades, contratos de entrada e saída, view models
└── Views/          telas Razor (uma subpasta por controller)
```

O namespace segue a pasta (`PolarDrinks.Modules.Vendas.Services`), regra R8 verificada por teste. A fronteira entre módulos é **por pasta e por teste**, não por projeto separado: todos compilam no mesmo projeto, e é a suíte de arquitetura que impede dependências proibidas.

## 2. Onde mexer para cada tipo de mudança

| Quero... | Vá para |
|---|---|
| Mudar uma regra de venda, pedido ou compra | `Modules/<Modulo>/Services` |
| Mudar uma consulta ou o que é carregado do banco | `Modules/<Modulo>/Repositories` |
| Adicionar um campo a uma tabela | `Models` do módulo dono, depois uma migration (veja [03](03-modelo-de-dados.md#8-migrations)) |
| Mudar uma tela | `Modules/<Modulo>/Views/<Controller>` |
| Mudar o menu ou o layout | `Views/Shared` (`_Layout`, `_LayoutFun`, `_LayoutLogin`) |
| Mudar um endpoint da loja | `Modules/Loja/Controllers/Api` (e o contrato em `Models`) |
| Mudar como o saldo de estoque varia | `Modules/Estoque/Services/MovimentacaoEstoqueService.cs`, e **só ali** |
| Mudar quem pode acessar uma tela | O filtro no controller ou na ação; os filtros ficam em `Compartilhado/Filters` |
| Mudar transações | `Compartilhado/Repositories/UnitOfWork.cs` e os serviços que o chamam |
| Registrar um novo serviço ou repository | `Program.cs` (a injeção de dependência é toda registrada ali) |
| Mudar JavaScript | `wwwroot/js` (a loja em `wwwroot/js/loja`) |

## 3. Quadro dos módulos

| Módulo | Dono das tabelas | Depende de | Quem depende dele |
|---|---|---|---|
| **Usuários** | Usuarios | – | Todos (só pelo tipo `UsuarioModel`, para registrar o responsável) |
| **Estoque** | Produtos, Categorias, ProdutoCategorias, MovimentacoesEstoque | Usuários (e, só no modelo de movimentação, Compras, Vendas e Loja, pela rastreabilidade) | Compras, Vendas, Loja, Dashboard |
| **Compras** | Fornecedores, ComprasEstoque, ItensCompra | Estoque, Usuários | – |
| **Vendas** | Vendas, ItensVenda | Estoque, Usuários | Dashboard |
| **Loja** | Clientes, CarrinhoItens, Pedidos, ItensPedido | Estoque, Usuários | Dashboard |
| **Dashboard** | (nenhuma) | Estoque, Vendas, Loja | – |

As dependências permitidas estão desenhadas no [doc 02](02-arquitetura.md); o teste de arquitetura (regra R1) falha se aparecer uma dependência fora desta tabela.

---

## 4. Usuários

**Responsabilidade:** quem trabalha no sistema: cadastro, login, troca de senha e desativação (RF01 a RF04). Não depende de nenhum outro módulo.

| Parte | Itens |
|---|---|
| Controllers | `AuthController` (login e logout, públicos), `UsuarioController` (Admin) |
| Service | `IUsuarioService`: `ListarUsuarios`, `ObterUsuario`, `Autenticar`, `CadastrarUsuario`, `AlterarSenha`, `Desativar` |
| Repository | `IUsuarioRepository` |
| Models | `UsuarioModel` |
| Views | `Auth` (Login, Deslogar), `Usuario` (Index, Cadastrar, AlterarSenha, Desativar); a tela "Acesso negado" fica em `Views/Shared` |
| Testes | Nenhum automatizado (veja [06, seção 8](06-autenticacao-e-permissoes.md#8-testes)) |

**Observação.** O cliente da loja **não** é um usuário: ele fica no módulo Loja (`ClienteModel`), com autenticação própria.

## 5. Estoque

**Responsabilidade:** o catálogo de produtos, as categorias e, principalmente, o **saldo**. É o único módulo que altera o saldo de um produto.

| Parte | Itens |
|---|---|
| Controllers | `EstoqueController`, `CategoriaController` |
| Services | `IEstoqueService` (cadastrar e editar produto, edição rápida, ajustar estoque, movimentações, alertas), `ICategoriaService`, `IArmazenamentoService` (imagens), `IMovimentacaoEstoqueService` (**a porta de entrada do saldo**) |
| Repositories | `IProdutoRepository`, `ICategoriaRepository`, `IMovimentacaoEstoqueRepository` |
| Models | `ProdutoModel` (inclui `CalcularPrecoFinal`), `CategoriaModel`, `ProdutoCategoriaModel`, `MovimentacaoEstoqueModel`, `CatalogoProdutoDto` |
| Views | `Estoque` (Index, Cadastrar, Editar, Movimentacoes), `Categoria` (Index, Cadastrar, Editar) |
| Testes | `EstoqueServiceCadastroTests`, `MovimentacaoEstoqueServiceTests`, `PrecoFinalTests` |

**O que oferece aos outros módulos**

| Item | Para quê | Usado por |
|---|---|---|
| `IMovimentacaoEstoqueService`: `RegistrarSaida`, `RegistrarDevolucao`, `RegistrarEntrada` | Mudar o saldo com movimentação e origem | Compras, Vendas, Loja |
| `IProdutoRepository` | Consultar produtos (`ObterPorId`, `ObterPorIds`, `ObterCatalogo`...) | Compras, Vendas, Loja, Dashboard |
| `ICategoriaRepository` | Categorias ativas para a loja | Loja |
| `IEstoqueService.ContarAlertasDeEstoque` | Contagens para o dashboard | Dashboard |
| `ProdutoModel.CalcularPrecoFinal()` | Preço final cobrado | Vendas, Loja |

**Por que `MovimentacaoEstoqueModel` aponta para Compras, Vendas e Loja.** Cada movimentação guarda a origem (`ItemCompraID`, `ItemVendaID` ou `ItemPedidoID`) para rastreabilidade. É a **única exceção** à regra de dependências, e o teste a lista explicitamente.

**Regras do módulo:** só o Estoque usa `IMovimentacaoEstoqueRepository` (R2) e só o Estoque altera o saldo (R3).

## 6. Compras

**Responsabilidade:** fornecedores e compras para reposição. A compra só mexe no estoque quando a entrega é confirmada, e o faz pedindo ao Estoque.

| Parte | Itens |
|---|---|
| Controllers | `CompraEstoqueController` (Admin), `FornecedorController` (lista para a equipe; cadastrar e editar para Admin) |
| Services | `ICompraEstoqueService` (listar, detalhes, cadastrar compra, confirmar entrega, excluir), `IFornecedorService` |
| Repositories | `ICompraEstoqueRepository`, `IFornecedorRepository` |
| Models | `CompraEstoqueModel`, `ItemCompraModel`, `FornecedorModel`, `CompraCreateViewModel`, `CompraIndexViewModel`, `CompraDetalhesViewModel` |
| Views | `CompraEstoque` (Index, Cadastrar, Detalhes, Excluir), `Fornecedor` (Index, Cadastrar, Editar) |
| Testes | `CompraEstoqueServiceCadastroTests`, `CompraEstoqueServiceConfirmarEntregaTests` |

**Depende de:** Estoque (`IProdutoRepository`, `IMovimentacaoEstoqueService`, `ProdutoModel`) e Usuários (quem registrou a compra).

## 7. Vendas

**Responsabilidade:** a venda no balcão, do registro ao cancelamento.

| Parte | Itens |
|---|---|
| Controller | `VendaController` |
| Service | `IVendaService`: `FinalizarVenda`, `CancelarVenda`, `ListarVendas`, `ObterDetalhesVenda`, `ObterVendaParaCancelamento`, `ListarProdutosAtivos` |
| Repository | `IVendaRepository` (inclui o resumo financeiro do módulo) |
| Models | `VendaModel` (com os tipos de pagamento), `ItemVendaModel` e `FinalizarVendaRequest` (contrato de entrada) |
| Views | `Venda` (Cadastrar, Index, Detalhes, Cancelar) |
| Testes | `VendaServiceFinalizarVendaTests`, `VendaServiceCancelamentoTests`, `VendaRollbackIntegracaoTests` (banco real) |

**Depende de:** Estoque (`IProdutoRepository`, `IMovimentacaoEstoqueService`, `ProdutoModel`) e Usuários. O serviço usa `IUnitOfWork` para a transação da venda. Não conhece Compras nem Loja.

## 8. Loja

**Responsabilidade:** tudo o que envolve o cliente e o pedido online, **inclusive o atendimento** do pedido pela equipe. É o maior módulo e reúne quatro coisas:

| Faceta | Onde fica |
|---|---|
| API do cliente (REST) | `Controllers/Api`: `CatalogoController`, `ClienteAuthController`, `CarrinhoController`, `PedidoController` |
| Páginas da loja (HTML que carrega o JavaScript) | `LojaController` + `Views/Loja` |
| Atendimento do pedido pela equipe | `PedidoOnlineController` + `Views/PedidoOnline` |
| Tarefa em segundo plano | `Jobs/ExpiracaoPedidosJob` |

| Parte | Itens |
|---|---|
| Services | `ICatalogoService`, `IClienteAuthService` (cadastrar e login), `ITokenService` (gera o JWT), `ICarrinhoService`, `IPedidoService` (checkout, consultas, cancelamentos, separação, entrega, expiração) |
| Repositories | `IClienteRepository`, `ICarrinhoRepository`, `IPedidoRepository` |
| Models | `ClienteModel`, `CarrinhoItemModel`, `PedidoModel` (com `Status` e `TipoPagamento`), `ItemPedidoModel`, `CarrinhoDto`/`CarrinhoItemDto`, `PedidoResponse` (contrato de saída da API) |
| Views | `Loja` (Catalogo, Conta, Carrinho, PedidoConfirmado, Perfil, `_HeaderLoja`) e `PedidoOnline` (Index, Detalhes, Historico) |
| Testes | `PedidoServiceCancelamentoTests` (inclui a expiração), `PedidoControllerContratoTests` |

**Depende de:** Estoque (`IProdutoRepository`, `ICategoriaRepository`, `IMovimentacaoEstoqueService`, `ProdutoModel`, `CatalogoProdutoDto`) e Usuários (quem separou e quem entregou). Usa `IUnitOfWork` no checkout e nos cancelamentos.

**Contratos:** a API devolve `PedidoResponse`, nunca a entidade `PedidoModel`, para não expor custo nem dados internos (RNF04). O detalhamento das rotas está no [doc 05](05-api.md).

## 9. Dashboard

**Responsabilidade:** números do negócio para o Admin. Não possui tabelas. Tem um único repository próprio, `RelatorioRepository`, só de leitura: ele chama as procedures de resumo de vendas (veja o [doc 03](03-modelo-de-dados.md#11-triggers-procedures-e-script-do-banco)).

| Parte | Itens |
|---|---|
| Controller | `DashboardController` (Admin) |
| Services | `IDashboardService.GerarDashboard()`, `IRelatorioService.GerarResumoPorPeriodo` (relatório de vendas por canal e período) |
| Repository | `IRelatorioRepository` (procedures de resumo: balcão, online e geral) |
| Outros | `PrevisaoLinear` (a regressão linear da previsão) |
| Models | `DashboardViewModel`, `ResumoVendasDto` |
| Views | `Dashboard/Index`, `Dashboard/RelatorioVendas` |
| Testes | `DashboardServiceFinanceiroTests`, `PrevisaoLinearTests` |

**Depende de:** Estoque (`IProdutoRepository`, `IEstoqueService`), Vendas (`IVendaRepository`) e Loja (`IPedidoRepository`). Ninguém depende do Dashboard, e por isso ele é o módulo mais seguro de alterar.

## 10. Compartilhado e raiz do projeto

`Compartilhado` guarda o que **vários módulos usam e que não pertence a nenhum**. A regra R7 impede que ele dependa de qualquer módulo.

| Pasta | Conteúdo |
|---|---|
| `Common` | `ResultadoOperacao` e `ResultadoOperacao<T>`: o retorno padrão dos serviços (sucesso, mensagem, campo com erro, dado) |
| `Filters` | `AuthFilter` e `AdminFilter` |
| `Repositories` | `IUnitOfWork` e `UnitOfWork` (transações e `SaveChanges`) |
| `Mvc` | `ModuloViewLocationExpander` |
| `Controllers` | `HomeController` (tela inicial e erro) |
| `Models` | `ErrorViewModel` |

**Na raiz do projeto**, fora dos módulos:

| Item | Papel |
|---|---|
| `Program.cs` | Configuração, injeção de dependência, autenticação, sessão, tarefa em segundo plano, migrations e usuário inicial |
| `Data/ApplicationDbContext.cs` | Mapeamento das 14 tabelas |
| `Migrations/` | Histórico do esquema do banco |
| `Views/Home`, `Views/Shared` | Tela inicial, layouts, erro e acesso negado |
| `wwwroot/` | CSS, JavaScript, imagens e `uploads/produtos` |
| `appsettings*.json`, `Dockerfile` | Configuração e imagem de contêiner |

**Como as views são encontradas.** Por padrão o MVC procuraria em `Views/<Controller>`. O `ModuloViewLocationExpander` descobre o módulo pelo namespace do controller (`PolarDrinks.Modules.<Modulo>.Controllers`) e acrescenta `Modules/<Modulo>/Views/<Controller>/<View>.cshtml` às pastas de busca. O `HomeController`, fora dos módulos, continua usando `Views/Home`. A regra R9 garante que a view esteja no mesmo módulo do controller.

---

## 11. Regras para mexer sem quebrar

A suíte de arquitetura (`PolarDrinks.Tests/Arquitetura`) roda com os demais testes e falha se uma destas regras for quebrada:

| Regra | Em palavras simples |
|---|---|
| R1 | Um módulo só usa os módulos que a tabela da seção 3 permite |
| R2 | Só o Estoque usa o repository de movimentação |
| R3 | Só o Estoque altera o saldo de produtos |
| R4 | Controller não acessa repository nem o banco: fala com o service |
| R5 | Service e repository não usam tipos da web (`HttpContext`, `IFormFile`, `IActionResult`, `ViewBag`, `TempData`...) |
| R6 | Models não dependem de service, repository, controller, filtro nem do banco |
| R7 | `Compartilhado` não depende de módulo |
| R8 | O namespace é igual ao caminho da pasta |
| R9 | A view fica no módulo do controller que a usa |

Na prática: se você precisa de algo de outro módulo, **peça ao service dele**. Se não existe um método para isso, é o módulo dono quem deve ganhar o método. O roteiro para criar um novo módulo ou funcionalidade está em [11-como-evoluir-o-projeto](11-como-evoluir-o-projeto.md).

## 12. Limitações

- **Fronteira por convenção e teste, não por compilador.** Os módulos são pastas de um mesmo projeto; sem rodar os testes, nada impede um `using` indevido. Separar em projetos (como no exemplo de referência) daria a garantia em tempo de compilação, ao custo de mais estrutura.
- **O módulo Loja acumula responsabilidades** (API, páginas, atendimento e tarefa agendada). Se crescer, o atendimento de pedidos pela equipe é o candidato natural a virar um módulo próprio.
- **Entidades circulam entre camadas.** Controllers e views usam as entidades (`ProdutoModel`, `VendaModel`) diretamente, exceto onde há contrato específico (`FinalizarVendaRequest`, `PedidoResponse`, `CatalogoProdutoDto`).
- **`Dashboard` e `Vendas` consultam repositories de outros módulos** em vez de serviços. É permitido pelas regras, mas acopla à forma como os dados são guardados.
- **Registro de dependências centralizado.** Todo serviço novo precisa ser registrado em `Program.cs`; esquecer o registro só aparece em execução.
