# Registro das alterações

Este documento registra, para cada ponto do relatório de avaliação (versão examinada: branch `main`, commit `9c3b4e6`), **o problema, a solução, os arquivos alterados e a evidência de validação**. A numeração segue a do relatório: os pontos 2 a 8 são as sete frentes e o ponto 9 reúne os ajustes complementares. O ponto 1 do relatório é o diagnóstico, tratado na [reorganização em módulos](#reorganização-em-módulos).

Os caminhos abaixo usam a estrutura **atual** (`Modules/<Modulo>/...`). Os testes citados estão em `PolarDrinks.Tests`; os resultados completos estão em [RESULTADOS-DOS-TESTES.md](RESULTADOS-DOS-TESTES.md) e as responsabilidades em [MAPA-DE-RESPONSABILIDADES.md](MAPA-DE-RESPONSABILIDADES.md).

## Sumário

- [Ponto 2: Dashboard](#ponto-2-dashboard)
- [Ponto 3: Contratos HTTP](#ponto-3-contratos-http)
- [Ponto 4: Validações de venda](#ponto-4-validações-de-venda)
- [Ponto 5: Preço e estoque centralizados](#ponto-5-preço-e-estoque-centralizados)
- [Ponto 6: Persistência e transações](#ponto-6-persistência-e-transações)
- [Ponto 7: Fronteiras entre funcionalidades](#ponto-7-fronteiras-entre-funcionalidades)
- [Ponto 8: Testes dos fluxos críticos](#ponto-8-testes-dos-fluxos-críticos)
- [Ponto 9: Ajustes complementares](#ponto-9-ajustes-complementares)
- [Reorganização em módulos](#reorganização-em-módulos)
- [Decisões de projeto](#decisões-de-projeto)
- [Banco de dados: triggers, procedures e relatórios](#banco-de-dados-triggers-procedures-e-relatórios)
- [Limitações e melhorias futuras](#limitações-e-melhorias-futuras)

---

## Ponto 2: Dashboard

**Problema.** `DashboardController.Index` consultava vendas e produtos, carregava vários períodos e todo o histórico, calculava indicadores e executava a regressão linear. Persistência, regra analítica e apresentação ficavam na mesma action.

**Solução.**
- A action passou a só coordenar: chama `IDashboardService` e escolhe a View.
- `DashboardService` foi dividido em métodos `Preencher*` (12 no total), cada um responsável por um bloco do painel, reunidos em `DadosDashboard`.
- **Resumo financeiro no banco:** `ObterResumoFinanceiro` (em `IVendaRepository` e `IPedidoRepository`) calcula quantidade, faturamento e lucro com `COUNT` e `SUM` no SQL, sem carregar as entidades.
- **Previsão de vendas:** a regressão foi extraída para `PrevisaoLinear` (`Ajustar`, `PreverProximo`, `Inclinacao`). Método e limite documentados: usa **apenas os dias com movimento**, numerados em sequência; dias sem venda não entram na tendência.
- **Indicadores de estoque:** `IEstoqueService.ContarAlertasDeEstoque()` pertence ao Estoque. Considera só produtos ativos e os grupos "sem estoque" e "abaixo do mínimo" **se sobrepõem**. O dashboard apenas usa o resultado.

**Arquivos.** `Modules/Dashboard/Controllers/DashboardController.cs`, `Modules/Dashboard/Services/DashboardService.cs`, `IDashboardService.cs`, `PrevisaoLinear.cs`, `Modules/Vendas/Repositories/VendaRepository.cs`, `Modules/Loja/Repositories/PedidoRepository.cs`, `Modules/Estoque/Services/EstoqueService.cs`.

**Evidência.** `PrevisaoLinearTests` (13 casos, com resultados conhecidos), `DashboardServiceFinanceiroTests` (4 casos). Commits: `615e2ea`, `457d5ec`, `e5dbd31`, `44fb144`.

## Ponto 3: Contratos HTTP

**Problema.** `VendaController` recebia `VendaModel` e a API de pedidos devolvia `PedidoModel` com itens e produtos carregados, deixando a estrutura do banco decidir o que entra e sai.

**Solução.**
- **Entrada da venda:** `FinalizarVendaRequest` e `ItemVendaRequest` contêm só tipo de pagamento, produto e quantidade. Data, usuário, preço, custo, total e status são definidos pelo servidor.
- **Resposta de pedido:** `PedidoResponse`, `ItemPedidoResponse` e `ProdutoPedidoResponse`. O código de retirada **faz parte** do contrato (o cliente precisa dele e só vê seus próprios pedidos). Custo e `RowVersion` não são devolvidos; do produto sai apenas o nome.
- **Edição de produto:** o serviço copia campo a campo (nome, descrição, código de barras, preço de venda, ativo, estoque mínimo, preço de custo, imagem e categorias). Saldo e promoção ficam fora; o saldo só muda por `AjustarEstoque` e a promoção por `EdicaoRapida`.

**Arquivos.** `Modules/Vendas/Models/FinalizarVendaRequest.cs`, `Modules/Vendas/Controllers/VendaController.cs`, `Modules/Loja/Models/PedidoResponse.cs`, `Modules/Loja/Controllers/Api/PedidoController.cs`, `Modules/Estoque/Services/EstoqueService.cs`.

**Evidência.** `FinalizarVendaRequest_DeveConterApenasOsCamposPermitidos`, `PedidoResponse_DeveConterApenasOsCamposPermitidos`, `ObterDetalhe_`, `ListarPedidos_` e `Checkout_RespostaNaoDeveExporCamposInternos` (verificam o JSON), `FinalizarVenda_VendaValida_ServidorDefineDataUsuarioEStatus`. Commits: `1ad4a52`, `c9c1dc6`.

**Decisão.** Não foi criado `EditarProdutoViewModel`. O mapeamento explícito no serviço já impede que campos extras alterem o estado interno, e criar outro modelo só duplicaria o `ProdutoModel`.

## Ponto 4: Validações de venda

**Problema.** `FinalizarVenda` não rejeitava quantidade zero ou negativa, não somava linhas repetidas do mesmo produto antes de checar o saldo e não verificava `ProdutoAtivo`.

**Solução.** Em `VendaService.FinalizarVenda`:
- carrinho vazio, tipo de pagamento ausente ou inválido (válidos: `Dinheiro`, `Pix`, `Cartão`, em `VendaModel.TipoPagamento.EhValido`) e quantidade menor ou igual a zero são rejeitados **antes** de abrir a transação;
- linhas repetidas são **agrupadas** por produto (`GroupBy`) e a soma é conferida contra o saldo;
- o estado atual do produto é consultado e produto inativo é recusado.

**Arquivos.** `Modules/Vendas/Services/VendaService.cs`, `Modules/Vendas/Models/VendaModel.cs`.

**Evidência.** `FinalizarVenda_QuantidadeZeroOuNegativa_DeveRejeitarSemEfeitos`, `FinalizarVenda_MesmoProdutoEmDuasLinhas_DeveSomarQuantidadesEVerificarSaldo` (cinco em estoque, duas linhas de três), `FinalizarVenda_ProdutoInativo_DeveRejeitarSemEfeitos`, `FinalizarVenda_TipoPagamentoInvalido_DeveRejeitarSemEfeitos`, mais a venda válida de controle. Commits: `ae7c244`, `58ef72e`.

## Ponto 5: Preço e estoque centralizados

**Problema.** O desconto era calculado em `VendaService`, `PedidoService` e `CarrinhoService`; vendas, compras e pedidos alteravam saldo e criavam movimentações cada um à sua maneira.

**Solução.**
- **Preço:** `ProdutoModel.CalcularPrecoFinal()` é a única fórmula (preço base nunca negativo, promoção válida só entre 0 e 100, arredondamento de 2 casas `AwayFromZero`). Os três serviços chamam esse método.
- **Estoque:** `IMovimentacaoEstoqueService` (`RegistrarSaida`, `RegistrarDevolucao`, `RegistrarEntrada`) é o único caminho para alterar o saldo. As operações entram na transação do caso de uso.
- **Devolução sem repetição:** `RegistrarDevolucao` consulta `IMovimentacaoEstoqueRepository.ExisteMovimentacao` e recusa devolver duas vezes o mesmo item.

**Arquivos.** `Modules/Estoque/Models/ProdutoModel.cs`, `Modules/Estoque/Services/MovimentacaoEstoqueService.cs`, `IMovimentacaoEstoqueService.cs`, `Modules/Estoque/Repositories/MovimentacaoEstoqueRepository.cs`, e os chamadores `VendaService`, `PedidoService`, `CarrinhoService`, `CompraEstoqueService`.

**Evidência.** `PrecoFinalTests` (7 casos, incluindo R$ 100,00 com 15% = R$ 85,00), `MovimentacaoEstoqueServiceTests` (15 casos). Teste de arquitetura R3: só o Estoque altera o saldo. Commits: `f7d7c06`, `bb1407b`, `9f0ba95`, `02c44bf`, `4ca69ea`, `105179f`.

## Ponto 6: Persistência e transações

**Problema.** Alguns serviços usavam `IUnitOfWork`, outros chamavam `SalvarAlteracoes` do repository, deixando o momento da gravação pouco claro. O cadastro de produto com imagem não tinha recuperação de falha, e o `Rollback` não limpava o estado rastreado.

**Solução.** Política adotada (documentada em [MAPA-DE-RESPONSABILIDADES.md](MAPA-DE-RESPONSABILIDADES.md), seção 6):
- operações com várias gravações (venda, checkout, cadastro de produto) usam transação explícita do `IUnitOfWork`;
- operações que terminam em uma única gravação (cancelamentos, confirmações) acumulam e chamam `SaveChanges` uma vez, o que já é atômico;
- CRUDs de uma entidade podem usar `SalvarAlteracoes` do repository;
- `UnitOfWork.Rollback` chama `ChangeTracker.Clear()`, então uma nova tentativa no mesmo contexto não reaproveita o estado da tentativa anterior (relevante para o retry de até 3 vezes do checkout);
- **cadastro de produto:** gravações do banco numa transação e, se falhar, o arquivo da imagem recém-salva é removido (`RemoverImagemProduto`).

**Arquivos.** `Compartilhado/Repositories/UnitOfWork.cs`, `Modules/Estoque/Services/EstoqueService.cs`, `Modules/Compras/Services/CompraEstoqueService.cs`, `Modules/Loja/Services/PedidoService.cs`.

**Evidência (banco SQL Server real).** `FinalizarVenda_FalhaNoMeio_DeveDesfazerTudoNoBanco`, `FinalizarVenda_DepoisDeUmaFalha_NoMesmoContexto_NaoDeveReaproveitarEstadoDaTentativaAnterior`, `CadastrarProduto_CategoriaInexistente_NaoDeveDeixarProdutoNemArquivo`, `ConfirmarEntrega_QuandoEstoqueRecusaAEntrada_NaoDeveConcluirNemGravar`. Commits: `78b9391`, `5fdd941`, `0dccab8`.

## Ponto 7: Fronteiras entre funcionalidades

**Problema.** `PedidoService` acessava repositories e entidades compartilhados, o saldo era alterado em vários lugares e não havia fronteira verificável entre funcionalidades.

**Solução.**
- Módulos em `Modules/` com responsáveis e dependências permitidas (tabela e diagrama no mapa de responsabilidades).
- Estoque é dono do saldo e das movimentações; Vendas, Compras e Loja pedem saída, devolução ou entrada pela interface `IMovimentacaoEstoqueService`, que não devolve entidade rastreada para o outro módulo alterar.
- Compras confirma a entrega e solicita a entrada ao Estoque **na mesma operação** (uma só gravação no final; falha em qualquer item desfaz tudo).
- O Dashboard só lê, por consultas próprias dos repositórios de Vendas, Loja e Estoque.
- As fronteiras são **verificadas por testes automatizados** (regras R1 a R9), não apenas combinadas.

**Arquivos.** `PolarDrinks.Tests/Arquitetura/RegrasDeArquitetura.cs`, `ArquiteturaTests.cs`, `RegrasDeArquiteturaTests.cs` e toda a pasta `Modules/`.

**Evidência.** Os testes de categoria `Arquitetura` (9 regras, 2 de sanidade e 16 que provam que cada regra detecta uma violação). Commits: `d07dd0e`, `d38622e`.

**Limite assumido.** Vendas, Compras e Loja ainda leem produtos por `IProdutoRepository`; não existe um contrato de leitura próprio. A escrita do saldo, porém, passa só pelo Estoque.

## Ponto 8: Testes dos fluxos críticos

**Problema.** Não havia projeto de testes.

**Solução.** Projeto `PolarDrinks.Tests` (xUnit + NSubstitute) com:
- testes de serviço (regras, rejeições e repetições), com dependências simuladas;
- testes de integração em **banco SQL Server temporário** criado e removido por cada execução (`BancoDeTestes`), sem usar o banco do dia a dia nem credenciais de produção. A conexão vem da variável `POLARDRINKS_TESTES_CONEXAO` (padrão: `Server=.;Trusted_Connection=True;TrustServerCertificate=True;`);
- testes de arquitetura (seção anterior).

| Cenário pedido no relatório | Teste |
|---|---|
| Venda com produto repetido | `FinalizarVenda_MesmoProdutoEmDuasLinhas_DeveSomarQuantidadesEVerificarSaldo` |
| Falha durante a venda, com banco real | `FinalizarVenda_FalhaNoMeio_DeveDesfazerTudoNoBanco` |
| Cancelamento repetido | `CancelarPeloCliente_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao`, `CancelarAposEntrega_Repetido_...` |

**Como rodar.** `dotnet test PolarDrinks/PolarDrinks.sln` na raiz; para separar: `--filter "Categoria=Integracao"` ou `"Categoria=Arquitetura"`. Resultados em [RESULTADOS-DOS-TESTES.md](RESULTADOS-DOS-TESTES.md).

## Ponto 9: Ajustes complementares

### Autenticação administrativa

**Problema.** `AuthController` consultava usuários e verificava a senha diretamente.

**Solução.** `AuthController` chama `IUsuarioService.Autenticar(login, senha)`, que consulta pelo repository, exige usuário ativo, confere o hash com BCrypt e devolve a mesma mensagem genérica para qualquer falha ("Usuário ou senha inválidos"). O controller mantém a criação da sessão e o redirecionamento. Administração continua usando sessão e filtros; o cliente da loja continua com JWT, pois os públicos não precisam do mesmo mecanismo.

**Arquivos.** `Modules/Usuarios/Controllers/AuthController.cs`, `Modules/Usuarios/Services/UsuarioService.cs`.

**Decisão.** A autenticação ficou em `UsuarioService`, sem um serviço separado, para não criar uma abstração sem necessidade.

### Serviços independentes da apresentação

**Problema.** `EstoqueService` recebia `IFormFile` e `CompraEstoqueService` usava tipos de ViewModel.

**Solução.** `EstoqueService` recebe a imagem como `(Stream Conteudo, string NomeArquivo)?`; o controller faz a conversão do `IFormFile` e `ArmazenamentoService` lida com o arquivo. `CompraEstoqueService.CadastrarCompra` recebe uma lista de tuplas `(ProdutoId, Quantidade, Preco)`. O teste de arquitetura R5 impede que Services e Repositories usem tipos da camada web.

**Pendência resolvida.** A segunda versão de `CadastrarCompra`, que recebia `ItemCompraCreateVM` e não era usada, foi removida (veja [Limpeza de código sem uso](#limpeza-de-código-sem-uso)).

## Reorganização em módulos

**Antes:** pastas técnicas na raiz (`Controllers`, `Services`, `Repositories`, `Models`, `Views`), com subpastas como `Loja` em algumas.

**Depois:** `Modules/{Usuarios, Estoque, Compras, Vendas, Dashboard, Loja}`, cada um com suas `Controllers`, `Services`, `Repositories`, `Models` e `Views`. `Compartilhado` guarda o que é de todos; `Data/` e `Migrations/` permanecem na raiz.

- 141 arquivos movidos preservando o histórico do git (renomeações).
- Namespaces atualizados para refletir a pasta (regra R8).
- `ModuloViewLocationExpander` (em `Compartilhado/Mvc`) faz o Razor encontrar as Views dentro do módulo, usando o namespace do controller; registrado em `Program.cs`.
- `Categorias` foi para o Estoque e `Fornecedores` para Compras; `AuthFilter` e `AdminFilter` foram para `Compartilhado`.
- Nenhuma mudança de comportamento: todos os testes continuaram passando. Commit: `d07dd0e`.

## Decisões de projeto

| Decisão | Motivo |
|---|---|
| Monólito em camadas com módulos, em vez de vários projetos | Mantém uma transação única para venda e estoque; fronteiras verificadas por teste dispensam custo de assemblies separados |
| Categorias no Estoque, Fornecedores em Compras | São dados que só esses módulos usam |
| Sem `EditarProdutoViewModel` | Mapeamento explícito no serviço já protege os campos internos |
| Código de retirada no `PedidoResponse` | O cliente precisa dele e só acessa os próprios pedidos |
| `DashboardViewModel` permanece no serviço | Evita um modelo duplicado só para trafegar os mesmos dados |
| Exceção de rastreabilidade em `MovimentacaoEstoqueModel` | Cada movimentação precisa apontar a venda, compra ou pedido de origem |

## E-mail e CPF no cadastro do cliente

**Problema.** O CPF era gravado do jeito que foi digitado: `123.456.789-00` e `12345678900` contavam como CPFs diferentes, e a mesma pessoa conseguia criar duas contas. O e-mail era conferido como digitado, mas gravado sem espaços e em minúsculas; com um espaço no início, a verificação não encontrava a conta existente e o banco recusava a gravação com um erro genérico.

**Solução.** `ClienteAuthService.Cadastrar` normaliza o e-mail (sem espaços nas pontas e em minúsculas) e o CPF (só dígitos) antes das verificações, grava os valores normalizados e exige 11 dígitos no CPF ("O CPF deve ter 11 dígitos.").

**Evidência.** Verificação manual: CPF com e sem máscara é reconhecido como o mesmo; CPF incompleto é recusado; e-mail com espaço e maiúsculas é reconhecido como já cadastrado; o login continua funcionando.

## Imagem do produto

**Problema.** A imagem era gravada com o nome enviado pelo navegador: dois produtos com imagens de mesmo nome sobrescreviam um ao outro, um nome com caminho podia gravar fora da pasta de imagens e qualquer tipo de arquivo era aceito. Na edição, a imagem antiga era apagada antes de a nova ser gravada.

**Solução.** `ArmazenamentoService` gera o nome (`<ProdutoID>_<identificador único>.<extensão>`) e aceita só `.jpg`, `.jpeg`, `.png`, `.webp` e `.gif`; `EstoqueService` recusa outros formatos com a mensagem "Envie uma imagem JPG, JPEG, PNG, WEBP ou GIF." antes de gravar qualquer coisa. Na edição, a imagem nova é gravada primeiro e a antiga só é apagada depois que o banco foi atualizado.

**Evidência.** Verificação manual: cadastro com imagem gera o nome no servidor; arquivo `.txt` é recusado sem gravar; a troca de imagem na edição remove a antiga; os 123 testes continuam passando.

## Carrinho com quantidade inválida

**Problema.** `CarrinhoService.AdicionarItem` (usado em `POST /api/carrinho/itens` e na mesclagem) só conferia o limite superior: quantidade zero ou negativa entrava no carrinho, e uma quantidade muito grande estourava o `int` e passava pela validação. O checkout passava a ser recusado até o cliente remover o item.

**Solução.** O método recusa quantidade menor ou igual a zero antes de qualquer consulta e soma as quantidades em `long`.

**Evidência.** Verificação manual pela API: quantidades `-5`, `0` e `2147483647` respondem 400; uma adição normal continua funcionando.

## Limpeza de código sem uso

**Problema.** Restavam código e arquivos que nenhuma parte do sistema usava, o que confundia a leitura e contradizia a regra R5.

**Solução.** Foram removidos:

- a segunda versão de `CompraEstoqueService.CadastrarCompra` (recebia `ItemCompraCreateVM`, um tipo de tela) e o `using Microsoft.AspNetCore.Http` sem uso em `EstoqueService`;
- `VendaDetalhesViewModel`;
- as ações `HomeController.Privacy()` e `HomeController.Estoque()`, que não tinham view;
- os arquivos estáticos não referenciados: `darka.css`, `site.css`, `pix.png`, `polaria2.png`, `_ValidationScriptsPartial.cshtml` e as bibliotecas `jquery-validation`;
- a migration morta `20260422170112_criandobd.cs`, sem `.Designer.cs` e ignorada pelo Entity Framework.

**Evidência.** O projeto compila e os 123 testes continuam passando. Nenhuma migration nova foi necessária, porque nenhum desses itens fazia parte do banco.

## Banco de dados: triggers, procedures e relatórios

**Problema.** O banco não tinha triggers nem procedures: todas as regras e consultas ficavam no C#, e não havia um script SQL completo do banco.

**Solução.**
- Triggers `TR_Usuarios_ProtegerUltimoAdmin` (o sistema nunca fica sem administrador ativo) e `TR_Produtos_ImpedirEstoqueNegativo` (o saldo nunca fica negativo), declaradas no `ApplicationDbContext` com `HasTrigger` para o Entity Framework continuar gravando.
- Procedures de leitura: `sp_ProdutosAbaixoDoMinimo`, `sp_ResumoVendasPorPeriodo`, `sp_ResumoPedidosOnlinePorPeriodo` e `sp_ResumoGeralPorPeriodo` (esta soma as duas anteriores, sem recalcular).
- Relatórios em PDF (página pronta para "Salvar como PDF"), só para o Admin: reposição, na tela de Estoque e no dashboard; vendas por período e canal (Presencial, Online e Geral), no dashboard, com atalhos de período.
- Script completo em `docs/banco/PolarDrinks.sql`, gerado a partir das migrations.

**Arquivos.** `Data/ApplicationDbContext.cs`; migrations `AdicionarTriggersEProcedures` e `AdicionarProceduresOnlineEGeral`; Estoque: `ProdutoReposicaoDto`, `IProdutoRepository`/`ProdutoRepository`, `IEstoqueService`/`EstoqueService`, `EstoqueController`, `Views/Estoque/RelatorioReposicao.cshtml` e `Views/Estoque/Index.cshtml`; Dashboard: `ResumoVendasDto`, `IRelatorioRepository`/`RelatorioRepository`, `IRelatorioService`/`RelatorioService`, `DashboardController`, `Views/Dashboard/RelatorioVendas.cshtml` e `Views/Dashboard/Index.cshtml`; `Program.cs`; `docs/banco/PolarDrinks.sql`.

**Evidência.** Verificação manual no SQL Server Management Studio: as triggers bloqueiam a desativação de todos os administradores e o estoque negativo (testado com `BEGIN TRAN`/`ROLLBACK`); as procedures de resumo batem com o dashboard nos três canais; o script foi executado num banco vazio e criou todos os objetos. No sistema, venda, edição de produto, desativação de usuário e troca de senha continuam funcionando, e os 123 testes passam.

## Acessibilidade em LIBRAS

**Problema.** O VLibras estava desativado no Admin, e no Funcionário o botão "Desativar LIBRAS" não funcionava. A versão 7 do VLibras passou a criar o próprio botão ao carregar o script, e ignora a `div` que o layout escondia.

**Solução.** O código foi para `Views/Shared/_VLibras.cshtml`, usado pelos dois layouts. O script do VLibras só é carregado quando a pessoa ativa pelo menu; desativar esconde o botão e a janela do intérprete. A escolha fica salva no navegador.

**Arquivos.** `Views/Shared/_VLibras.cshtml`, `Views/Shared/_Layout.cshtml` e `Views/Shared/_LayoutFun.cshtml`.

**Evidência.** Verificação manual como Admin e como Funcionário: a página abre sem o VLibras quando está desativado; ativar mostra o botão; desativar esconde o botão e o intérprete; a escolha se mantém ao trocar de página. A loja online não exibe o VLibras.

## Limitações e melhorias futuras

- **Pacote com aviso de vulnerabilidade:** `System.Security.Cryptography.Pkcs` (CVE-2026-84008 e CVE-2026-70162, afetam as versões 10.0.0 a 10.0.12, corrigidas na 10.0.13). Risco de negação de serviço; o código não chama `SignedCms` nem `EnvelopedCms`. Atualizar o pacote.
- **Segredos no repositório:** a chave JWT e a conexão estão em arquivo de configuração versionado; mover para variáveis de ambiente.
- **Upload de imagem sem limite de tamanho** (o nome e o tipo do arquivo já são controlados pelo servidor).
- **Retry de concorrência do checkout** sem teste de integração.
- **Dashboard** ainda carrega parte do histórico para gráficos e previsão.
- **Leitura de produtos** por outros módulos sem contrato próprio, e duas consultas de exibição (`VendaRepository.ObterMotivoCancelamento` e `CompraEstoqueRepository.ObterUsuarioConfirmacaoEntrada`) que leem movimentações direto do banco, apenas para mostrar dados.
