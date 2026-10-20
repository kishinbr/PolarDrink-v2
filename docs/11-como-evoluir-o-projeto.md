# Como evoluir o projeto

Este documento responde: **como fazer mudanças comuns sem quebrar a arquitetura, e o que vale a pena melhorar a seguir**. Ele reúne em um só lugar as limitações apontadas nos outros documentos, em ordem de prioridade. Antes de mexer, leia [09-modulos](09-modulos.md) (onde cada coisa fica) e rode os testes ([10-guia-de-testes](10-guia-de-testes.md)).

## Sumário

1. [Rotina de trabalho](#1-rotina-de-trabalho)
2. [Receitas](#2-receitas)
3. [Mapa de melhorias](#3-mapa-de-melhorias)
4. [Ordem sugerida](#4-ordem-sugerida)
5. [Pendências de documentação](#5-pendências-de-documentação)

---

## 1. Rotina de trabalho

Para qualquer mudança, na mesma ordem:

1. **Branch própria.** No GitHub Desktop, crie uma branch a partir de `refactor/modulos` com um nome que diga o que será feito (por exemplo, `fix/carrinho-quantidade`).
2. **Rode os testes antes de começar** (`dotnet test`), para saber que o ponto de partida está verde.
3. **Mude pouco de cada vez.** Uma correção ou funcionalidade por branch; commits pequenos, com mensagem que explique o **porquê**.
4. **Escreva ou ajuste o teste** da regra mexida (veja o [doc 10](10-guia-de-testes.md#4-como-funcionam-e-como-escrever-um-novo)). Para um bug, primeiro um teste que falha; depois a correção.
5. **Rode todos os testes de novo**, incluindo os de arquitetura e os de integração (confira que os ignorados são 0).
6. **Atualize a documentação** que a mudança tocou (a tabela da seção 5 do [doc 10](10-guia-de-testes.md) e as limitações dos documentos 04 a 08 mudam quando uma limitação é corrigida).
7. **Integre.** Quando a mudança estiver pronta e revisada, junte à branch de trabalho. A integração com a `main` é uma decisão à parte, tomada de propósito.

**Definição de pronto.** Compila sem avisos novos; `dotnet test` verde (ignorados 0); a regra tem teste; a documentação afetada foi atualizada; o roteiro manual do fluxo mexido foi executado.

---

## 2. Receitas

### 2.1 Adicionar um campo a uma tabela existente

Exemplo: marca do produto.

1. Acrescente a propriedade no modelo do módulo dono (`Modules/Estoque/Models/ProdutoModel.cs`), com as anotações de validação (`[StringLength(50)]`, `[Required]` se for o caso).
2. Se for `decimal`, configure a precisão em `Data/ApplicationDbContext.cs` (`HasPrecision(18, 2)`), como os demais.
3. Se o campo for **obrigatório** e a tabela já tem linhas, defina um valor padrão: sem isso a migration falha nos dados existentes.
4. Gere a migration, dentro da pasta `PolarDrinks`:
   ```
   dotnet tool restore
   dotnet ef migrations add AdicionarMarcaAoProduto
   ```
5. **Abra e leia o arquivo gerado** em `Migrations/`: confira que só contém a mudança esperada. Faça o commit dele junto com o `...ModelSnapshot.cs` e o `.Designer.cs`.
6. Ajuste as telas (`Cadastrar.cshtml`, `Editar.cshtml`, listagem) e, se o dado vai para a API, o contrato (`CatalogoProdutoDto`).
7. **Copie o campo no serviço de edição**: `EditarProduto` copia propriedade por propriedade; um campo novo esquecido ali é gravado só no cadastro, nunca na edição.
8. Suba o sistema: a migration é aplicada sozinha na inicialização. Para desfazer uma migration ainda não integrada: `dotnet ef migrations remove`.

**Regras de ouro:** nunca edite uma migration que já foi aplicada em outro banco; crie outra. Nunca altere o saldo de estoque por um campo: use a seção 2.6.

### 2.2 Adicionar uma regra ou funcionalidade a um módulo existente

1. **Decida o dono.** A capacidade pertence a que módulo? Se envolve outro módulo, peça ao *service* dele (regras R1 a R4); se esse service não tem o método, **ele** ganha o método.
2. Declare o método na interface (`IVendaService`) e implemente na classe. Retorne `ResultadoOperacao` (ou `ResultadoOperacao<T>`) com mensagem clara, em vez de lançar exceção para erros previstos.
3. Se o serviço precisa de uma dependência nova, injete-a pelo construtor; se é um serviço novo, **registre em `Program.cs`** (`AddScoped`).
4. Decida a forma de gravar, conforme a [tabela do doc 04](04-comunicacao-com-o-banco.md#3-quem-grava-e-quando): várias entidades e etapas → transação explícita; várias mudanças que cabem num `SaveChanges` → `_unitOfWork.SaveChanges()`; CRUD simples → `SalvarAlteracoes` do repository.
5. Valide **antes** de gravar. Em operações de vários itens, valide tudo antes de alterar qualquer coisa.
6. Exponha a funcionalidade num controller (seções 2.3 e 2.4).
7. Escreva os testes: o caminho feliz, **cada recusa** (conferindo que nada foi gravado) e, se a operação pode ser repetida, a repetição.

### 2.3 Criar uma tela (ação MVC)

1. Crie a ação no controller do módulo. Aplique `[AuthFilter]` (qualquer usuário logado) e `[AdminFilter]` (só Admin), no controller ou na ação, conforme a [tabela do doc 05](05-api.md#9-telas-da-equipe-mvc).
2. **Ações que alteram dados devem ser `[HttpPost]`**; use `GET` somente para exibir (a confirmação de entrega de compra por `GET` é um defeito conhecido, não um modelo).
3. O controller só chama o service: nada de repository ou `DbContext` (R4), e a regra de negócio fica no service (R5 impede tipos da web lá).
4. A view fica em `Modules/<Modulo>/Views/<Controller>/<Acao>.cshtml`. O `ModuloViewLocationExpander` a encontra sozinho (R9).
5. Se a tela deve aparecer no menu, edite `_Layout.cshtml` e, se o Funcionário também a usa, `_LayoutFun.cshtml` (os menus são separados).
6. Mensagens ao usuário: use `TempData["MensagemSucesso"]` e `TempData["MensagemErro"]`, como as telas existentes.

### 2.4 Criar um endpoint na API da loja

1. Coloque o controller em `Modules/Loja/Controllers/Api`, com `[ApiController]` e `[Route("api/...")]`. Exija login com `[Authorize]` (o identificador do cliente vem de `ClaimTypes.NameIdentifier`, nunca do corpo da requisição).
2. Crie **contratos próprios** de entrada e de saída; não receba nem devolva entidades. Na entrada, só o que o cliente pode informar (RNF04); na saída, só o que a loja precisa exibir (siga o modelo de `PedidoResponse`).
3. Siga o padrão de respostas: `200` com o resultado; `400` com `{ "mensagem": ... }` para erro de regra; `404` sem corpo para recurso inexistente **ou de outro cliente**.
4. Se a consulta é de dados do cliente, filtre sempre pelo identificador dele (como `ObterPorIdEcliente`).
5. Acrescente a requisição ao `PolarDrinks.http`, a linha na tabela do [doc 05](05-api.md#3-resumo-dos-endpoints) e um teste de contrato semelhante a `PedidoControllerContratoTests`.

### 2.5 Criar um novo módulo

Use quando a capacidade tem dados e regras próprios (por exemplo, "Entregas").

1. Crie `Modules/<Nome>/` com `Controllers`, `Services`, `Repositories`, `Models` e `Views`. O namespace é igual ao caminho (R8): `PolarDrinks.Modules.<Nome>.Services`.
2. Declare o módulo nas regras de arquitetura, em `PolarDrinks.Tests/Arquitetura/RegrasDeArquitetura.cs`: acrescente-o ao vetor `Modulos` e ao dicionário `DependenciasPermitidas`, **listando só os módulos de que ele realmente precisa**. Sem isso, o teste `PastasDeModulos_DevemSerExatamenteOsDeclaradosNasRegras` falha, e é de propósito: a lista de dependências é uma decisão consciente.
3. Adicione as entidades ao `ApplicationDbContext` (`DbSet` e configuração) e gere a migration (seção 2.1).
4. Registre os serviços e repositories em `Program.cs`.
5. Se o módulo precisa mudar o estoque, **use `IMovimentacaoEstoqueService`** (nunca altere `ProdutoQtdEstoque` diretamente: R3).
6. Se a movimentação precisa apontar para o novo módulo (rastreabilidade), isso é uma alteração da exceção documentada em `MovimentacaoEstoqueModel`; pondere antes, porque aumenta o acoplamento do Estoque.
7. Atualize os diagramas e tabelas dos documentos [02](02-arquitetura.md), [09](09-modulos.md) e do [mapa de responsabilidades](entrega/MAPA-DE-RESPONSABILIDADES.md).

### 2.6 Mexer no estoque

- **Qualquer** mudança de saldo passa por `IMovimentacaoEstoqueService` (saída, devolução ou entrada), que cria a movimentação com a origem.
- Um **novo tipo de movimentação**: acrescente a constante em `MovimentacaoEstoqueModel.Tipos`, trate-a no método adequado e atualize a tabela do [doc 08](08-fluxos-principais.md#2-movimentações-os-seis-tipos) e a tela de movimentações.
- Uma devolução nova deve informar **exatamente uma origem** (item de venda ou de pedido), para a proteção contra devolução repetida continuar valendo.
- Operações que movimentam vários itens: valide tudo, mude o saldo pelos métodos acima, grave **uma vez** (ou use transação) e, em caso de recusa, chame `Rollback`.

### 2.7 Mudar uma regra de arquitetura

Só depois de decidir, em equipe, que o desenho mudou. Altere `RegrasDeArquitetura.cs` (e o teste da regra em `RegrasDeArquiteturaTests`), atualize a seção 3 do [doc 09](09-modulos.md) e o [doc 02](02-arquitetura.md), e registre o motivo no commit.

---

## 3. Mapa de melhorias

Todas as limitações abaixo aparecem nos documentos 03 a 10 (coluna **Origem**). Nada aqui foi alterado no código ainda. **Esforço**: B = baixo (horas), M = médio (um dia ou dois), A = alto (vários dias).

### 3.1 Segurança

| ID | Melhoria | Prioridade | Esforço | Origem |
|---|---|:-:|:-:|---|
| S1 | Trocar "Confirmar entrega" de compra de `GET` para `POST` com botão em formulário | Alta | B | [05](05-api.md#11-limitações) |
| S2 | Ativar validação de antifalsificação (CSRF) nos formulários, de forma global (`AutoValidateAntiforgeryToken`) | Alta | M | [05](05-api.md#11-limitações) |
| S3 | Segredos fora do repositório: chave do JWT e senha do SQL Server por variável de ambiente ou *user-secrets*; obrigar a troca da senha `admin123` | Alta | B | [06](06-autenticacao-e-permissoes.md#7-configuração-e-segredos) |
| S4 | Imagem de produto: tamanho máximo (nome gerado pelo servidor e extensões aceitas já foram feitos) | Alta | B | [04](04-comunicacao-com-o-banco.md#11-limitações), [07](07-casos-de-uso.md#11-limitações-observadas) |
| S5 | Desativar usuário deve encerrar a sessão dele (o filtro consultar a situação do usuário ou uma versão de sessão) | Média | M | [06](06-autenticacao-e-permissoes.md#9-limitações) |
| S6 | HTTPS obrigatório (redirecionamento, HSTS, cookies `Secure`) ou publicação atrás de proxy com TLS | Média | B | [06](06-autenticacao-e-permissoes.md#9-limitações) |
| S7 | Limite de tentativas de login (equipe e cliente) | Média | M | [06](06-autenticacao-e-permissoes.md#9-limitações) |
| S8 | Verificar a situação do cliente a cada chamada autenticada, ou tokens de curta duração com renovação | Média | M | [06](06-autenticacao-e-permissoes.md#9-limitações) |
| S9 | Atualizar o pacote com vulnerabilidade apontada (Pkcs) *(planejado por você)* | Alta | B | – |

### 3.2 Correções de comportamento

| ID | Melhoria | Prioridade | Esforço | Origem |
|---|---|:-:|:-:|---|
| C2 | Link de "Pedidos online" no menu do Funcionário (se ele deve atender pedidos) | Média | B | [06](06-autenticacao-e-permissoes.md#9-limitações) |
| C3 | Troca da própria senha pelo usuário, e redefinição pelo Admin | Média | M | [06](06-autenticacao-e-permissoes.md#4-gestão-de-usuários) |
| C4 | Estoque inicial do cadastro de produto gerar uma movimentação (`Entrada`, "Estoque inicial") | Média | B | [07](07-casos-de-uso.md#11-limitações-observadas) |
| C5 | Cancelamento pelo cliente identificar o cliente na movimentação (hoje só a descrição) | Baixa | M | [08](08-fluxos-principais.md#11-limitações) |
| C6 | Edição de produto: usar transação (gravar a nova imagem antes de apagar a antiga já foi feito) | Média | B | [04](04-comunicacao-com-o-banco.md#8-imagens-o-banco-e-o-disco) |
| C7 | Não devolver o texto técnico da exceção ao usuário (venda e checkout) | Média | B | [05](05-api.md#11-limitações) |
| C8 | Venda no balcão com nova tentativa em conflito de concorrência, como o checkout | Baixa | B | [08](08-fluxos-principais.md#11-limitações) |
| C9 | Código de retirada maior, ou limite de tentativas de código errado | Baixa | B | [08](08-fluxos-principais.md#11-limitações) |
| C10 | Prazos (24 horas), intervalo da tarefa (15 minutos) e tentativas (3) em configuração | Baixa | B | [07](07-casos-de-uso.md#11-limitações-observadas) |

### 3.3 Dados

| ID | Melhoria | Prioridade | Esforço | Origem |
|---|---|:-:|:-:|---|
| D1 | Restrições no banco: saldo maior ou igual a zero, quantidades maiores que zero, valores de status válidos (hoje só o código garante) | Média | M | [03](03-modelo-de-dados.md#5-o-que-o-banco-garante-e-o-que-só-o-código-garante) |
| D2 | Trocar as exclusões em cascata a partir de `Produtos` por `Restrict` (o app nunca exclui produto) | Baixa | M | [03](03-modelo-de-dados.md#4-relacionamentos-e-exclusão) |

### 3.4 Qualidade, desempenho e arquitetura

| ID | Melhoria | Prioridade | Esforço | Origem |
|---|---|:-:|:-:|---|
| Q1 | `AsNoTracking()` nas consultas somente de leitura | Média | B | [04](04-comunicacao-com-o-banco.md#6-rastreamento-de-entidades-tracking) |
| Q2 | Detalhe de produto da loja por consulta direta (hoje carrega o catálogo inteiro) | Média | B | [04](04-comunicacao-com-o-banco.md#11-limitações) |
| Q3 | Dashboard: agrupar no banco (gráficos e previsão) em vez de carregar o histórico | Média | M | [04](04-comunicacao-com-o-banco.md#9-consultas-e-desempenho) |
| Q4 | Paginação nas listas (catálogo, pedidos, vendas) | Média | M | [05](05-api.md#11-limitações) |
| Q5 | Tratamento global de erros: `IExceptionHandler` e *Problem Details* na API | Média | M | [05](05-api.md#11-limitações) |
| Q6 | OpenAPI (documentação gerada da API) mantendo o `.http` | Baixa | B | [05](05-api.md#11-limitações) |
| Q7 | Serviços e controllers assíncronos (`async`/`await`, `SaveChangesAsync`) | Baixa | A | [04](04-comunicacao-com-o-banco.md#11-limitações) |
| Q8 | `TreatWarningsAsErrors` no projeto | Baixa | B | – |
| Q9 | Separar o atendimento de pedidos online num módulo próprio | Baixa | A | [09](09-modulos.md#12-limitações) |

Cada item de **Q7** a **Q9** muda muito código ao mesmo tempo; faça-os separados, com os testes verdes entre um e outro.

### 3.5 Testes e automação

| ID | Melhoria | Prioridade | Esforço | Origem |
|---|---|:-:|:-:|---|
| T1 | Teste de integração do retry de concorrência do checkout | Alta | M | [10](10-guia-de-testes.md#5-o-que-os-testes-automatizados-não-cobrem) |
| T2 | Testes de autenticação e permissões (`AuthFilter`, `AdminFilter`, `Autenticar`, JWT) | Alta | M | [06](06-autenticacao-e-permissoes.md#8-testes) |
| T3 | Integração contínua (GitHub Actions) rodando build e testes, com SQL Server de serviço | Média | M | [10](10-guia-de-testes.md#8-limitações) |
| T4 | Testes de API de ponta a ponta (`WebApplicationFactory`) e de cancelamentos em banco real | Média | A | [10](10-guia-de-testes.md#5-o-que-os-testes-automatizados-não-cobrem) |
| T5 | Roteiros automáticos de fumaça (por exemplo, Playwright) sobre as telas | Baixa | A | – |

---

## 4. Ordem sugerida

Pensada em etapas curtas, cada uma terminando com testes verdes.

| Etapa | Itens | Por quê |
|---|---|---|
| **1. Fechar buracos simples** | S1, S4, S3, C7 | Baixo esforço e alto retorno: corrigem comportamentos incorretos e brechas conhecidas |
| **2. Proteger o que já funciona** | T2, T1, T3 | Testes de autenticação, do retry e CI evitam que as correções seguintes quebrem algo sem aviso |
| **3. Segurança de sessão e formulários** | S2, S5, S6, S7, S8 | Exigem alguns testes novos (da etapa 2) e mudança de comportamento |
| **4. Consistência de dados** | C4, C6, D1, D2 | Alteram o esquema do banco: faça com migration revisada |
| **5. Desempenho e robustez** | Q1, Q2, Q3, Q4, Q5 | Medem-se melhor depois que os testes protegem o comportamento |
| **6. Evolução estrutural** | Q7, Q9, Q6, Q8 | Maiores e menos urgentes; só depois do resto estável |

As melhorias C2, C3, C5, C8, C9 e C10 são decisões de produto: vale conversar sobre elas antes de implementar.

## 5. Pendências de documentação

Itens que ainda precisam de ação nesta documentação (não são de código):

- Acrescentar ao [REGISTRO-DE-ALTERACOES](entrega/REGISTRO-DE-ALTERACOES.md) os achados dos documentos 04 a 10: confirmação de entrega por `GET`, ausência de CSRF, sessão que sobrevive à desativação, entre outros.
- Manter este mapa vivo: ao corrigir um item, remova-o da tabela e da limitação de origem.
