# Guia de testes

Este documento responde: **como rodar os testes automatizados, o que cada grupo prova, como escrever um novo e como conferir à mão o que os testes não cobrem**. O resultado de uma execução e a lista dos cenários cobertos estão em [RESULTADOS-DOS-TESTES](entrega/RESULTADOS-DOS-TESTES.md); aqui está o "como".

## Sumário

1. [Duas formas de verificar](#1-duas-formas-de-verificar)
2. [Rodar os testes automatizados](#2-rodar-os-testes-automatizados)
3. [O que existe: 123 testes em quatro grupos](#3-o-que-existe-123-testes-em-quatro-grupos)
4. [Como funcionam e como escrever um novo](#4-como-funcionam-e-como-escrever-um-novo)
5. [O que os testes automatizados não cobrem](#5-o-que-os-testes-automatizados-não-cobrem)
6. [Roteiro de teste manual](#6-roteiro-de-teste-manual)
7. [Conferências no banco (SQL)](#7-conferências-no-banco-sql)
8. [Limitações](#8-limitações)

---

## 1. Duas formas de verificar

| Forma | Pega bem | Não pega |
|---|---|---|
| **Testes automatizados** (`dotnet test`) | Regras de negócio, rollback, repetição de operações, fronteiras entre módulos | Telas, JavaScript, permissões, segurança |
| **Roteiro manual** (seção 6) | O fluxo completo como o usuário vê, permissões, integração entre tela, API e banco | Variações em massa; é lento e depende de quem executa |

Um não substitui o outro. A rotina sugerida: rodar `dotnet test` a cada mudança e o roteiro manual antes de entregar ou publicar.

## 2. Rodar os testes automatizados

Na raiz do repositório:

```
dotnet test PolarDrinks/PolarDrinks.sln
```

| Quero... | Comando |
|---|---|
| Ver o nome de cada teste | `dotnet test PolarDrinks/PolarDrinks.sln --logger "console;verbosity=normal"` |
| Só os testes com banco real | `dotnet test PolarDrinks/PolarDrinks.sln --filter "Categoria=Integracao"` |
| Só as regras de arquitetura | `dotnet test PolarDrinks/PolarDrinks.sln --filter "Categoria=Arquitetura"` |
| Rodar um teste pelo nome | `dotnet test PolarDrinks/PolarDrinks.sln --filter "FullyQualifiedName~CancelarVenda_Repetido"` |
| Medir cobertura de código | `dotnet test PolarDrinks/PolarDrinks.sln --collect:"XPlat Code Coverage"` |

No Visual Studio: *Teste > Gerenciador de Testes > Executar Todos os Testes*. Ali também dá para rodar um teste só, com clique direito, e depurá-lo.

**Como ler o resultado.** O resumo final traz aprovados, falhas e **ignorados**. Os testes de integração precisam de um SQL Server:

- Por padrão eles usam `Server=.;Trusted_Connection=True;TrustServerCertificate=True;` (SQL Server local com autenticação do Windows).
- Para usar outro servidor (por exemplo, o do Docker em `localhost,1433`), defina a variável `POLARDRINKS_TESTES_CONEXAO` com a conexão **sem nome de banco**.
- Se o servidor não responde, os 7 testes de integração são **ignorados, não reprovados**. O resumo mostra "ignorados: 7" e a mensagem diz o motivo.

Por isso, ao conferir o resultado, olhe os ignorados: **123 aprovados com 0 ignorados** significa que o rollback foi provado em banco real; com 7 ignorados, não.

| Última execução registrada | Valor |
|---|---|
| Total / aprovados / falhas | 123 / 123 / 0 |
| Ignorados | 0 |
| Data | 20/10/2026 |
| Duração | 2 s |

## 3. O que existe: 123 testes em quatro grupos

Os testes ficam em `PolarDrinks.Tests`, organizados por módulo. A contagem abaixo conta cada caso de teste (um `[Theory]` com várias linhas `InlineData` conta uma vez por linha).

| Grupo | Pasta | Qtde | O que usa |
|---|---|:-:|---|
| Serviço | `Estoque`, `Vendas`, `Compras`, `Loja`, `Dashboard` | 87 | Dependências simuladas (NSubstitute); rápidos, sem banco |
| Integração | `Integracao` | 7 | SQL Server real, banco temporário |
| Arquitetura | `Arquitetura` | 27 | Lê o código-fonte do projeto |
| Fumaça | raiz | 2 | Só o tipo `ResultadoOperacao` |

### 3.1 Testes de serviço (87)

| Pasta / classe | Qtde | Foco |
|---|:-:|---|
| `Estoque/MovimentacaoEstoqueServiceTests` | 19 | Saída, devolução e entrada: quantidade inválida, saldo, origem, devolução repetida |
| `Estoque/EstoqueServiceCadastroTests` | 5 | Cadastro de produto: com e sem imagem, código duplicado, falhas que desfazem o banco e removem a imagem |
| `Estoque/PrecoFinalTests` | 8 | `CalcularPrecoFinal`: promoção, limites, arredondamento |
| `Vendas/VendaServiceFinalizarVendaTests` | 14 | Fluxo válido e entradas inválidas (itens, pagamento, quantidade, saldo, linhas repetidas) |
| `Vendas/VendaServiceCancelamentoTests` | 4 | Cancelar venda: prazo, repetição, falha na devolução |
| `Compras/CompraEstoqueServiceCadastroTests` | 3 | Registrar compra: itens válidos, linhas inválidas ignoradas, nenhum item válido |
| `Compras/CompraEstoqueServiceConfirmarEntregaTests` | 4 | Confirmar entrega: entrada de cada item, repetição, recusa do estoque, compra inexistente |
| `Loja/PedidoServiceCancelamentoTests` | 8 | Cancelar (cliente e após entrega) e expirar pedidos, inclusive conflito |
| `Loja/PedidoControllerContratoTests` | 5 | O que a API devolve (sem dados internos) |
| `Dashboard/DashboardServiceFinanceiroTests` | 4 | Resumo financeiro |
| `Dashboard/PrevisaoLinearTests` | 13 | Regressão linear da previsão |

### 3.2 Testes de integração (7)

Ficam em `Integracao` e têm o rótulo `Categoria=Integracao`. Criam um banco temporário (`PolaDrinks_Testes_<código>`) com as **migrations reais**, e o apagam no fim; nunca tocam o banco `PolaDrinks`.

| Classe | Qtde | Prova |
|---|:-:|---|
| `VendaRollbackIntegracaoTests` | 3 | Venda válida grava tudo; falha no meio desfaz tudo; depois de uma falha o mesmo contexto não reaproveita o estado anterior |
| `CadastroProdutoRollbackIntegracaoTests` | 2 | Falha no cadastro de produto desfaz o banco |
| `BancoDeTestesFumacaTests` | 2 | O banco temporário sobe e as migrations se aplicam |

**Por que banco real.** Simulações e o provedor em memória do Entity Framework não implementam transações de verdade: um teste com eles passaria mesmo que o rollback estivesse quebrado.

### 3.3 Testes de arquitetura (27)

Rótulo `Categoria=Arquitetura`. Há dois tipos:

| Classe | Qtde | O que faz |
|---|:-:|---|
| `ArquiteturaTests` | 11 | Aplica as regras **R1 a R9** ao código-fonte real do projeto e verifica que as pastas dos módulos são as declaradas |
| `RegrasDeArquiteturaTests` | 16 | Testa as **próprias regras**: dá exemplos de código com violação e confirma que são detectados, e que casos permitidos não geram falso alarme |

O segundo grupo existe para que uma regra "que nunca reclama" não passe despercebida: se a verificação ficasse frouxa, estes testes falhariam. As regras estão descritas no [doc 09](09-modulos.md#11-regras-para-mexer-sem-quebrar).

---

## 4. Como funcionam e como escrever um novo

### 4.1 Convenções

- **Nome:** `Metodo_Condicao_ResultadoEsperado`, por exemplo `CancelarVenda_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao`. O nome deve contar a história sem abrir o código.
- **Estrutura:** preparar (Arrange), executar (Act), verificar (Assert).
- **Um comportamento por teste.** Entradas semelhantes com resultados diferentes viram `[Theory]` com `[InlineData]`.
- **Pasta:** a do módulo testado; um novo módulo ganha pasta própria.

### 4.2 Teste de serviço (com dependências simuladas)

O serviço é criado com repositories e `UnitOfWork` **falsos**; o teste controla o que eles devolvem e confere o que foi chamado. Trecho real, resumido:

```csharp
public class VendaServiceCancelamentoTests
{
    private readonly IVendaRepository _vendaRepository = Substitute.For<IVendaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly VendaService _service;

    [Fact]
    public void CancelarVenda_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao()
    {
        var venda = CriarVenda();
        _vendaRepository.ObterPorId(10).Returns(venda);

        var primeira = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);
        var segunda  = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);

        Assert.True(primeira.Sucesso);
        Assert.False(segunda.Sucesso);
        _estoque.ReceivedWithAnyArgs(2).RegistrarDevolucao(0, 0, "", null); // só a 1ª devolveu
        _unitOfWork.Received(1).SaveChanges();
    }
}
```

Roteiro para um teste novo de regra: (1) crie o serviço com `Substitute.For<...>()` nas dependências; (2) configure o que a regra lê (`.Returns(...)`); (3) chame o método; (4) confira o `ResultadoOperacao` **e** os efeitos (o que foi ou não chamado em `_unitOfWork` e nos repositories). Para toda regra de recusa, confira também que **nada foi gravado**.

### 4.3 Teste de integração (banco real)

Use `[FatoIntegracao]` no lugar de `[Fact]` (ele pula o teste se não houver SQL Server), `[Collection(ColecaoBancoDeTestes.Nome)]` na classe e `[Trait("Categoria", "Integracao")]`. A classe recebe o `BancoDeTestes` pelo construtor, cria contextos com `CriarContexto()` e limpa os dados antes de cada cenário. Os testes de integração rodam em sequência, compartilhando um banco por execução.

Para provar um rollback: monte um cenário que falhe **depois** de uma gravação parcial (como o segundo item sem saldo), e depois **consulte o banco com outro contexto** para confirmar que nada ficou.

### 4.4 Quando uma regra de arquitetura falha

A mensagem do teste lista os arquivos e as linhas em violação. Corrija o código (peça ao service do módulo dono, em vez de acessar seu repository) e não a regra. Mudar uma regra só faz sentido quando a decisão de arquitetura mudou, e nesse caso a tabela de dependências do [doc 02](02-arquitetura.md) também muda.

---

## 5. O que os testes automatizados não cobrem

| Lacuna | Impacto | Como conferir hoje |
|---|---|---|
| Retry de concorrência do checkout | Alto | Roteiro T15 (difícil de provocar à mão) |
| Cancelamentos em banco real | Médio | Há testes de serviço; roteiro T07, T12 e T13 |
| Autenticação e permissões (`AuthFilter`, `AdminFilter`, login, JWT) | Alto | Roteiro T01, T02, T11 |
| Telas e JavaScript | Médio | Roteiro manual |
| Tarefa em segundo plano (execução real) | Baixo | Roteiro T14 |
| API de ponta a ponta (HTTP) | Médio | Arquivo `PolarDrinks.http` e roteiro T08 a T12 |
| Desempenho e carga | Baixo | Fora do escopo |
| Módulo Usuários, Fornecedores e Categorias | Baixo | Roteiro T02 e T03 |

---

## 6. Roteiro de teste manual

### 6.1 Preparação

1. Suba o sistema (`dotnet run --project PolarDrinks`, em `http://localhost:5153`, ou `docker compose up -d --build`, em `http://localhost:8080`). Use um banco de testes; este roteiro grava dados.
2. Entre com `admin` / `admin123`.
3. Deixe aberto o arquivo `PolarDrinks.http` (veja o [doc 05](05-api.md#10-como-experimentar)); ajuste o `@host` à porta usada.
4. Observação: a cultura do sistema é fixa em `en-US`; nos formulários, digite decimais **com ponto** (por exemplo, `10.00`).

**Dados base** (crie uma vez, como Admin):

| O que | Valores |
|---|---|
| Categoria | `Cervejas` |
| Produto A | Código `7891234567890`, custo `6.00`, venda `10.00`, promoção `0`, quantidade `20`, mínimo `5`, ativo, categoria Cervejas |
| Produto B | Código `7891234567891`, custo `3.00`, venda `5.00`, promoção `0`, quantidade `8`, mínimo `8`, ativo |
| Fornecedor | Qualquer, com CNPJ de 18 caracteres (`12.345.678/0001-90`), telefone de 15 (`(11) 99999-9999`) e demais campos |

### 6.2 Equipe, estoque, compras e vendas

| ID | Passo | Resultado esperado |
|---|---|---|
| T01 | Login com senha errada | "Usuário ou senha inválidos" |
| T02 | Como Admin, cadastre o usuário `func` (perfil Funcionário) informando a senha do Admin; saia e entre como `func` | O menu tem só links para início, venda, estoque e fornecedores |
| T02b | Como `func`, abra `/Dashboard`, `/Usuario` e `/CompraEstoque` | Redireciona para "Acesso negado" nas três |
| T02c | Como `func`, abra `/PedidoOnline` | Abre (permitido; não há link no menu) |
| T03 | Cadastre um produto repetindo o código `7891234567890` | "Este código de barras já está cadastrado." |
| T04 | Ajuste o estoque do produto A de 20 para 25, descrição "Contagem" | Saldo 25; nas movimentações, uma `Edicao` de +5 |
| T04b | Ajuste de novo para 25 | Saldo igual; **nenhuma** nova movimentação |
| T04c | Ajuste com descrição de mais de 50 caracteres | "A descrição deve ter no máximo 50 caracteres." |
| T05 | Registre compra de 10 unidades do produto A; confira o saldo | Compra "Aguardando"; saldo continua 25 |
| T05b | Confirme a entrega | Saldo 35; movimentação `Entrada`; compra "Concluído" |
| T05c | Abra de novo o endereço de confirmar entrega dessa compra | Saldo continua 35 (sem nova entrada) |
| T05d | Tente excluir a compra concluída | "Não é possível excluir uma compra já concluída." |
| T06 | Como `func`, venda 3 unidades do produto A com Pix | "Venda realizada com sucesso!"; saldo 32; movimentação `Saida` com o usuário `func` |
| T06b | Venda 33 unidades do produto A | "Estoque insuficiente para: ..."; saldo 32 |
| T06c | Venda duas linhas do produto A: 20 e 20 (soma 40). Se a tela juntar as linhas sozinha, use um `POST` direto ou o teste automatizado equivalente | Recusada com "Estoque insuficiente"; saldo 32 |
| T06d | Venda com quantidade `0` | "A quantidade de cada item deve ser maior que zero." |
| T07 | Como Admin, cancele a venda do T06 informando o motivo | Saldo volta a 35; movimentação `Cancelamento`; venda marcada cancelada |
| T07b | Tente cancelar a mesma venda de novo | "Venda já está cancelada."; saldo continua 35 |

### 6.3 Loja online (use o `PolarDrinks.http`)

| ID | Passo | Resultado esperado |
|---|---|---|
| T08 | Cadastre o cliente e faça login | `200` com token (o `.http` guarda o token) |
| T08b | Sem o token, consulte o carrinho | `401` |
| T08c | Adicione 1 unidade do produto B (saldo 8, mínimo 8) | `400`: "... apenas na loja física." |
| T08d | Adicione 31 unidades do produto A (disponível online = 35 − 5 = 30) | `400`: "Apenas 30 unidade(s) de ... disponíveis." |
| T08e | Na tela, aplique 10% de promoção ao produto A (edição rápida); adicione 2 unidades | Carrinho com `precoUnitario` 9.00 e `total` 18.00 |
| T09 | Checkout com `"Dinheiro"` | `400`: "Forma de pagamento inválida." |
| T09b | Checkout com `"Pix"` | `200`; pedido `AguardandoSeparacao` com código de 4 dígitos; saldo do A diminui 2; carrinho vazio; movimentação `SaidaOnline` sem usuário |
| T09c | Checkout de novo, com o carrinho vazio | `400`: "Seu carrinho está vazio." |
| T10 | Como `func`, em `/PedidoOnline`, marque o pedido como separado | "Pedido marcado como separado!" |
| T10b | Tente entregar com código errado | "Código incorreto. Confira com o cliente." |
| T10c | Entregue com o código certo | "Entrega confirmada com sucesso!"; pedido no histórico como concluído |
| T11 | Cadastre um **segundo** cliente (outro e-mail e CPF); com o token dele, consulte o pedido do primeiro | `404` |
| T11b | Com o token do segundo cliente, cancele o pedido do primeiro | `400`: "Pedido não encontrado." |
| T12 | Com o primeiro cliente, adicione 2 unidades ao carrinho de novo, finalize o pedido e cancele-o | `200`; saldo volta; movimentação `CancelamentoOnline` ("Cancelado pelo cliente") |
| T12b | Cancele o mesmo pedido de novo | `400`: "Este pedido não pode mais ser cancelado." |
| T13 | Como Admin, cancele o pedido concluído do T10 (motivo preenchido) | Saldo volta; pedido `CanceladoAdmin` |

### 6.4 Tarefa automática e concorrência

**T14 Expiração.** Não dá para esperar 24 horas; antecipe a data no banco.

1. Faça um pedido (carrinho, checkout) e, como `func`, marque-o como separado.
2. No SQL Server, rebaixe a data de separação (troque `X` pelo `PedidoID`):

```sql
UPDATE Pedidos SET PedidoDataSeparado = DATEADD(HOUR, -25, GETDATE()) WHERE PedidoID = X;
```

3. Reinicie a aplicação (a tarefa roda assim que o sistema sobe e depois a cada 15 minutos) ou aguarde até 15 minutos.
4. **Esperado:** o console mostra `[ExpiracaoPedidosJob] 1 pedido(s) expirado(s) automaticamente.`; o pedido fica `CanceladoNaoRetirado`; o saldo volta; existe uma movimentação `CancelamentoOnline` com a descrição "Expirado".

**T15 Concorrência (avançado).** Com dois clientes e um produto com disponível online igual a 1, faça os dois checkouts quase ao mesmo tempo (por exemplo, dois terminais com `curl`). **Esperado:** um pedido é criado; o outro recebe `400` ("... apenas na loja física.", pois o disponível online chegou a zero), depois de reler o saldo, e o saldo nunca fica negativo. É difícil provocar o conflito exato à mão, e é por isso que esse cenário merece um teste de integração (veja o [doc 11](11-como-evoluir-o-projeto.md)).

---

## 7. Conferências no banco (SQL)

Depois de qualquer rodada de testes manuais, estas consultas devem dar o resultado indicado. Qualquer linha de resposta onde se espera "nenhuma" indica um problema.

```sql
-- 1. Produtos com saldo negativo: esperado nenhuma linha
SELECT ProdutoID, ProdutoNome, ProdutoQtdEstoque FROM Produtos WHERE ProdutoQtdEstoque < 0;

-- 2. Movimentações sem origem, exceto ajustes: esperado nenhuma linha
SELECT MovimentacaoID, MovimentacaoTipo FROM MovimentacoesEstoque
WHERE MovimentacaoTipo <> 'Edicao'
  AND ItemCompraID IS NULL AND ItemVendaID IS NULL AND ItemPedidoID IS NULL;

-- 3. Item de venda devolvido mais de uma vez: esperado nenhuma linha
SELECT ItemVendaID, COUNT(*) AS Devolucoes FROM MovimentacoesEstoque
WHERE MovimentacaoTipo = 'Cancelamento' GROUP BY ItemVendaID HAVING COUNT(*) > 1;

-- 4. Item de pedido devolvido mais de uma vez: esperado nenhuma linha
SELECT ItemPedidoID, COUNT(*) AS Devolucoes FROM MovimentacoesEstoque
WHERE MovimentacaoTipo = 'CancelamentoOnline' GROUP BY ItemPedidoID HAVING COUNT(*) > 1;

-- 5. Venda cancelada com algum item sem devolução: esperado nenhuma linha
SELECT v.VendaID FROM Vendas v
WHERE v.VendaCancelada = 1 AND EXISTS (
    SELECT 1 FROM ItensVenda i
    WHERE i.VendaID = v.VendaID AND NOT EXISTS (
        SELECT 1 FROM MovimentacoesEstoque m
        WHERE m.ItemVendaID = i.ItemVendaID AND m.MovimentacaoTipo = 'Cancelamento'));

-- 6. Itens com quantidade inválida no carrinho (veja doc 05, limitações): esperado nenhuma linha
SELECT CarrinhoItemID, ClienteID, ProdutoID, Quantidade FROM CarrinhoItens WHERE Quantidade <= 0;

-- 7. Histórico de um produto (troque X pelo ProdutoID)
SELECT MovimentacaoData, MovimentacaoTipo, MovimentacaoQtd, MovimentacaoDescricao
FROM MovimentacoesEstoque WHERE ProdutoID = X ORDER BY MovimentacaoData;
```

A consulta 6 existe porque o carrinho aceita quantidade inválida ao adicionar (limitação do [doc 05](05-api.md#11-limitações)); se o ajuste for feito, ela passa a ser uma proteção.

**Atenção ao interpretar o saldo.** O saldo inicial informado no cadastro de produto não gera movimentação (veja o [doc 07](07-casos-de-uso.md#11-limitações-observadas)); portanto, **saldo = estoque inicial + soma das movimentações** e a soma sozinha não fecha o saldo.

---

## 8. Limitações

- **Não há integração contínua.** Nenhum fluxo automático (como o GitHub Actions) roda os testes a cada alteração; quem roda é você, localmente.
- **Os testes de integração dependem do SQL Server local.** Em outra máquina sem SQL Server eles são ignorados e podem passar despercebidos (confira os "ignorados").
- **Não há teste automatizado de tela, JavaScript, autenticação ou permissões.** O roteiro manual é o que cobre isso.
- **Cobertura de código não é medida no fluxo normal.** O pacote para isso (`coverlet.collector`) está no projeto de testes e o comando está na seção 2, mas o resultado não é acompanhado.
- **O roteiro manual grava dados.** Rode-o num banco de testes (ou no Docker com `docker compose down -v` ao final), nunca num banco com dados reais.
