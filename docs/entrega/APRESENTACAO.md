# Roteiro da apresentação

Roteiro de fala para a discussão com o professor. Cada pergunta tem uma **resposta curta** (para falar) e **onde mostrar** no código ou nos testes. Detalhes completos em [MAPA-DE-RESPONSABILIDADES.md](MAPA-DE-RESPONSABILIDADES.md) e [REGISTRO-DE-ALTERACOES.md](REGISTRO-DE-ALTERACOES.md).

## Sumário

1. [Por que o projeto é um monólito?](#1-por-que-o-projeto-é-um-monólito)
2. [Como MVC e camadas convivem?](#2-como-mvc-e-camadas-convivem)
3. [O que torna uma funcionalidade um módulo?](#3-o-que-torna-uma-funcionalidade-um-módulo)
4. [Quem controla o preço?](#4-quem-controla-o-preço)
5. [Quem controla o estoque?](#5-quem-controla-o-estoque)
6. [Quais campos o cliente pode informar?](#6-quais-campos-o-cliente-pode-informar)
7. [O que acontece quando uma operação falha no meio?](#7-o-que-acontece-quando-uma-operação-falha-no-meio)
8. [Sequência sugerida de demonstração](#8-sequência-sugerida-de-demonstração)
9. [Perguntas difíceis e respostas honestas](#9-perguntas-difíceis-e-respostas-honestas)

---

## 1. Por que o projeto é um monólito?

**Resposta.** Porque é uma única aplicação e um único banco, e isso é o certo para este tamanho. Uma venda precisa criar a venda, baixar o estoque e registrar a movimentação **na mesma transação**. Num monólito isso é uma transação de banco; em serviços separados seria consistência distribuída, com custo e risco muito maiores sem benefício para o nosso volume.

**Mas não é uma bagunça.** Um único projeto não impede modularidade: o que importa é ter responsáveis, contratos públicos e dependências permitidas, e nós **verificamos isso por teste**.

**Mostrar.** `Modules/` na árvore do projeto; testes de categoria `Arquitetura`.

## 2. Como MVC e camadas convivem?

**Resposta.** São decisões diferentes. **MVC** organiza a interação com a tela: Controller recebe a requisição, View (Razor) mostra. **Camadas** organizam a responsabilidade técnica: `Controller → Service → Repository → Banco`. Cada módulo contém as duas coisas.

- Controller: recebe e responde HTTP. Não acessa repository nem banco (regra R4).
- Service: decide as regras. Não usa tipos da camada web (regra R5).
- Repository: consulta e prepara alterações com o Entity Framework.

**Mostrar.** O fluxo de uma venda: `VendaController.FinalizarVenda` → `VendaService.FinalizarVenda` → repositories.

## 3. O que torna uma funcionalidade um módulo?

**Resposta.** Três coisas, todas no código:
1. **Dono claro** das suas tabelas e regras (a tabela de módulos no mapa).
2. **Contrato público**: os outros módulos falam com ele por interface de serviço, por exemplo `IMovimentacaoEstoqueService`.
3. **Dependências permitidas** e verificadas: Compras, Vendas e Loja dependem só do Estoque e de Usuários; o Dashboard só lê.

**Mostrar.** `RegrasDeArquitetura.DependenciasPermitidas` e o teste `R1_ModulosSoDependemUnsDosOutrosNasDirecoesPermitidas`. Se alguém importar um módulo proibido, o teste falha.

## 4. Quem controla o preço?

**Resposta.** O **Estoque**, por `ProdutoModel.CalcularPrecoFinal()`. Há uma fórmula só: preço base nunca negativo, promoção só entre 0 e 100, arredondamento de 2 casas. Venda, carrinho e checkout chamam esse mesmo método. O JavaScript só estima; **o servidor sempre recalcula**.

**Mostrar.** `PrecoFinalTests`: R$ 100,00 com 15% resulta em R$ 85,00.

## 5. Quem controla o estoque?

**Resposta.** Também o **Estoque**, por `IMovimentacaoEstoqueService`. Venda, pedido e compra **pedem** a saída, a devolução ou a entrada; nenhum deles altera a quantidade por conta própria (regra R3). A devolução recusa repetir o mesmo item, então cancelar duas vezes não devolve duas vezes.

**Mostrar.** Os chamadores `RegistrarSaida` em `VendaService`/`PedidoService`, `RegistrarEntrada` em `CompraEstoqueService`, e o teste `CancelarPeloCliente_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao`.

## 6. Quais campos o cliente pode informar?

**Resposta.** Só os de um contrato restrito:
- **Venda:** tipo de pagamento e, por item, produto e quantidade (`FinalizarVendaRequest`). Preço, custo, total, data, usuário e status são do servidor.
- **Checkout:** só o tipo de pagamento.
- **Edição de produto:** o serviço copia campo a campo; saldo e promoção ficam de fora.
- **Resposta de pedido:** `PedidoResponse`, sem custo, sem `RowVersion` e só o nome do produto. O código de retirada está na resposta **de propósito**, porque o cliente precisa dele.

**Mostrar.** `FinalizarVendaRequest_DeveConterApenasOsCamposPermitidos` e `Checkout_RespostaNaoDeveExporCamposInternos`.

## 7. O que acontece quando uma operação falha no meio?

**Resposta.** Depende da operação, mas a regra é: **ou tudo acontece, ou nada**.
- **Venda:** validações antes de gravar; depois, transação. Qualquer erro dá `Rollback`: sem venda, sem baixa de estoque, sem movimentação.
- **Checkout:** transação; em conflito de concorrência tenta de novo até 3 vezes. O `Rollback` limpa o estado rastreado, então a nova tentativa começa limpa.
- **Produto com imagem:** transação no banco e, se falhar, o arquivo da imagem é apagado, porque a transação SQL não desfaz arquivo.
- **Cancelamentos e confirmação de compra:** uma única gravação no fim; falha em um item desfaz tudo.
- **Expiração de pedidos:** cada pedido é processado separadamente; se um falhar, os outros seguem.

**Mostrar.** `FinalizarVenda_FalhaNoMeio_DeveDesfazerTudoNoBanco` (banco SQL Server real, não simulado).

## 8. Sequência sugerida de demonstração

1. **Estrutura:** abrir `Modules/` e o mapa de dependências.
2. **Teste de arquitetura:** rodar `dotnet test PolarDrinks/PolarDrinks.sln --filter "Categoria=Arquitetura"`. Se der tempo, quebrar uma regra de propósito e mostrar o teste falhando (desfazer em seguida).
3. **Venda inválida:** no sistema ou pelo teste, enviar quantidade negativa, produto repetido acima do saldo e produto inativo; mostrar a rejeição sem efeitos.
4. **Venda válida:** conferir preço, total, saldo e movimentação.
5. **Rollback real:** rodar `dotnet test PolarDrinks/PolarDrinks.sln --filter "Categoria=Integracao"` e citar o teste de falha no meio.
6. **Cancelamento repetido:** cancelar um pedido e tentar de novo; mostrar que o estoque voltou uma vez só.

Antes de apresentar: confirmar que o SQL Server está ligado (os testes de integração criam e apagam o próprio banco temporário).

## 9. Perguntas difíceis e respostas honestas

| Pergunta | Resposta |
|---|---|
| Os módulos ainda leem produto direto do repository? | Sim. Vendas, Compras e Loja leem produtos por `IProdutoRepository`; não há contrato de leitura próprio do Estoque. A **escrita** do saldo passa só pelo Estoque. Está nas limitações. |
| Por que `MovimentacaoEstoqueModel` referencia outros módulos? | Exceção documentada de rastreabilidade: cada movimentação aponta o item de venda, compra ou pedido de origem. Só esse arquivo pode, e um teste garante. |
| O retry do checkout está testado? | O descarte do estado após falha está testado em banco real; o retry de concorrência do checkout em si não tem teste de integração. |
| O dashboard ainda carrega histórico? | Os totais financeiros são somados no banco, mas gráficos e previsão ainda carregam parte do histórico. |
| A previsão considera dias sem venda? | Não: usa só os dias com movimento, numerados em sequência. É um limite documentado. |
| Pedido não retirado entra no faturamento? | Sim, junto com os concluídos. É decisão de negócio e pode ser revista. |
| Segurança está completa? | Não era o escopo. Limitações conhecidas: segredos no repositório, upload de imagem sem restrição e um pacote com aviso de vulnerabilidade a atualizar. |
