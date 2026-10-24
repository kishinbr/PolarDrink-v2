# Visão, escopo e requisitos

Este documento responde: **o que o PolarDrinks faz, para quem, e o que ele garante**. Os requisitos têm identificadores (RF e RNF) que os outros documentos da pasta usam para apontar de volta para cá.

## Sumário

1. [Visão](#1-visão)
2. [Quem usa o sistema](#2-quem-usa-o-sistema)
3. [Requisitos funcionais](#3-requisitos-funcionais)
4. [Requisitos não funcionais](#4-requisitos-não-funcionais)
5. [Regras de negócio principais](#5-regras-de-negócio-principais)
6. [Fora do escopo](#6-fora-do-escopo)
7. [Glossário](#7-glossário)

---

## 1. Visão

O PolarDrinks é um sistema de gestão para uma **loja de bebidas** com duas frentes na mesma aplicação:

- **Área administrativa:** a equipe da loja registra vendas no balcão, controla estoque, compras e fornecedores, atende os pedidos online e acompanha os números do negócio.
- **Loja online:** o cliente consulta o catálogo, monta um carrinho, faz o pedido e **retira na loja** com um código.

As duas frentes compartilham o mesmo estoque, e é isso que dá sentido ao sistema: uma venda no balcão e um pedido online baixam o mesmo saldo, sempre pelo mesmo caminho, e todo movimento fica registrado.

## 2. Quem usa o sistema

| Ator | Como acessa | O que faz |
|---|---|---|
| **Admin** | Telas administrativas, com login | Tudo que o Funcionário faz, mais gestão de produtos, categorias, compras, usuários, cancelamentos e dashboard |
| **Funcionário** | Telas administrativas, com login | Registra vendas no balcão e pode atender os pedidos online (separar, voltar para separação e entregar); o menu dele não traz o atalho para os pedidos, então o acesso é pelo endereço `/PedidoOnline` (veja [06](06-autenticacao-e-permissoes.md#9-limitações)) |
| **Cliente da loja** | Telas da loja e API, com conta própria (JWT) | Consulta o catálogo, usa o carrinho, finaliza pedidos e cancela os seus |
| **Sistema (tarefa automática)** | Execução a cada 15 minutos | Cancela pedidos separados e não retirados em 24 horas e devolve o estoque |

O Admin e o Funcionário usam o mesmo login e a mesma sessão; o que muda é o perfil gravado no usuário. O Cliente é outro cadastro, separado dos usuários da equipe. O detalhamento de cada permissão está no [mapa de responsabilidades](entrega/MAPA-DE-RESPONSABILIDADES.md), seção 8.

## 3. Requisitos funcionais

Na coluna **Quem**, `Admin` significa que só o perfil Admin executa; `Equipe` significa Admin ou Funcionário logado.

### 3.1 Acesso e usuários

| ID | Requisito | Quem |
|---|---|---|
| RF01 | Entrar e sair do sistema administrativo com login e senha; a sessão expira após 30 minutos sem uso. | Equipe |
| RF02 | Cadastrar usuário com nome, login único, senha de pelo menos 6 caracteres (com confirmação) e perfil Admin ou Funcionário. | Admin |
| RF03 | Alterar a senha de um usuário informando a senha atual dele e a nova (mínimo de 6 caracteres, com confirmação). Todo o cadastro de usuários é restrito ao Admin, então o Funcionário não tem tela para trocar a própria senha. | Admin |
| RF04 | Desativar usuário, exceto a própria conta e o único Admin ativo. | Admin |

### 3.2 Estoque

| ID | Requisito | Quem |
|---|---|---|
| RF05 | Cadastrar e editar produto: nome, descrição, código de barras (único, 13 a 20 caracteres), preço de venda, custo, estoque mínimo, situação (ativo ou inativo), imagem e categorias. | Admin |
| RF06 | Alterar preço e promoção (0 a 100%) pela edição rápida. | Admin |
| RF07 | Ajustar o saldo de um produto informando uma justificativa de até 50 caracteres; o ajuste vira uma movimentação. | Admin |
| RF08 | Consultar as movimentações de estoque de um produto (entradas, saídas, cancelamentos, ajustes), com a origem de cada uma. | Admin |
| RF09 | Cadastrar e editar categorias (nome obrigatório e único). | Admin |
| RF10 | Listar produtos e ver quais estão com estoque baixo. | Equipe |

### 3.3 Vendas no balcão

| ID | Requisito | Quem |
|---|---|---|
| RF11 | Registrar venda com um ou mais produtos e forma de pagamento (Dinheiro, Pix ou Cartão). O servidor define preço, custo, total, data e usuário. | Equipe |
| RF12 | Rejeitar a venda inteira se houver item com quantidade zero ou negativa, produto inexistente, produto inativo ou saldo insuficiente (somando linhas repetidas do mesmo produto). | Equipe |
| RF13 | Consultar o histórico de vendas, com filtro por período, e o detalhe de cada venda. | Admin |
| RF14 | Cancelar uma venda em até 24 horas, com motivo; o estoque volta uma única vez. | Admin |

### 3.4 Compras e fornecedores

| ID | Requisito | Quem |
|---|---|---|
| RF15 | Cadastrar e editar fornecedores. | Admin |
| RF16 | Registrar compra de produtos de um fornecedor (ela nasce como pendente, sem alterar o estoque). | Admin |
| RF17 | Confirmar a entrega de uma compra, o que dá entrada de cada item no estoque. Confirmar de novo não repete a entrada. | Admin |
| RF18 | Excluir compra pendente. | Admin |

### 3.5 Pedidos online (atendimento)

| ID | Requisito | Quem |
|---|---|---|
| RF19 | Listar pedidos por situação e ver o detalhe de cada um. | Equipe |
| RF20 | Marcar o pedido como separado e, se necessário, devolvê-lo para a separação. | Equipe |
| RF21 | Confirmar a entrega ao cliente conferindo o código de retirada. | Equipe |
| RF22 | Cancelar um pedido já entregue, em até 24 horas da entrega, devolvendo o estoque. | Admin |
| RF23 | Consultar o histórico de pedidos finalizados e cancelados. | Equipe |

### 3.6 Dashboard

| ID | Requisito | Quem |
|---|---|---|
| RF24 | Mostrar faturamento, lucro, ticket médio e quantidade de vendas e pedidos (hoje, mês e total), separados por vendas do balcão e pedidos online. | Admin |
| RF25 | Mostrar formas de pagamento, produtos mais vendidos, giro de estoque, alertas de estoque (sem estoque e abaixo do mínimo), cancelamentos e gráficos. | Admin |
| RF26 | Mostrar uma previsão de vendas por tendência linear, usando apenas os dias que tiveram movimento. | Admin |

### 3.7 Loja online

| ID | Requisito | Quem |
|---|---|---|
| RF27 | Consultar o catálogo: listar e buscar produtos ativos por termo e categoria, ver o detalhe e listar categorias. | Público |
| RF28 | Criar conta (nome, e-mail, CPF e senha de pelo menos 6 caracteres, com confirmação; telefone opcional) e entrar com e-mail e senha; e-mail e CPF não podem se repetir. | Público |
| RF29 | Usar o carrinho: ver, adicionar, alterar a quantidade, remover item, esvaziar e mesclar o carrinho do navegador com o da conta. | Cliente |
| RF30 | Limitar o carrinho ao que está disponível para venda online: estoque menos o estoque mínimo (o mínimo fica reservado para o balcão). | Cliente |
| RF31 | Finalizar o pedido (checkout) com pagamento em Pix ou Cartão. O pedido recebe um código de retirada e o estoque é baixado. | Cliente |
| RF32 | Consultar os próprios pedidos e o detalhe de cada um (nunca os de outro cliente). | Cliente |
| RF33 | Cancelar o próprio pedido em até 24 horas, enquanto estiver aguardando separação ou separado; o estoque volta. | Cliente |
| RF34 | Cancelar automaticamente pedidos separados e não retirados em 24 horas, devolvendo o estoque. | Sistema |

## 4. Requisitos não funcionais

| ID | Requisito | Como é atendido |
|---|---|---|
| RNF01 | Executar localmente com .NET 10 e SQL Server, ou em contêineres. | `dotnet run` ou `docker compose up` (veja o [README](../README.md)) |
| RNF02 | Um único estoque, com um único responsável pelo saldo. | `IMovimentacaoEstoqueService`; regra de arquitetura R3 |
| RNF03 | Operações com várias gravações são tudo ou nada. | `IUnitOfWork` com transação e rollback; testes em banco real |
| RNF04 | O servidor decide preço, total e demais campos internos; o cliente só informa o necessário. | Contratos HTTP restritos e `CalcularPrecoFinal` |
| RNF05 | Uma mesma operação repetida não pode duplicar efeitos (devolver estoque duas vezes, por exemplo). | Verificação de origem na devolução; testes de repetição |
| RNF06 | Resistir a conflitos de concorrência no estoque. | `RowVersion` no produto e novas tentativas no checkout (até 3) |
| RNF07 | Proteger acessos: senha com hash, cliente com token, equipe com sessão e perfil. | BCrypt, JWT (HS256, 24 horas), `AuthFilter` e `AdminFilter` |
| RNF08 | Manter fronteiras claras entre módulos. | Pastas por módulo e regras R1 a R9 verificadas por teste |
| RNF09 | Ter testes automatizados dos fluxos críticos. | 123 testes (serviço, integração com banco e arquitetura) |
| RNF10 | Executar a expiração de pedidos sem intervenção. | Tarefa em segundo plano a cada 15 minutos |
| RNF11 | Oferecer acessibilidade em LIBRAS no sistema administrativo. | VLibras, ativado pelo item "Ativar LIBRAS" do menu (Admin e Funcionário) |

## 5. Regras de negócio principais

| Regra | Valor |
|---|---|
| Formas de pagamento no balcão | Dinheiro, Pix, Cartão |
| Formas de pagamento online | Pix, Cartão (o sistema registra a forma escolhida; não processa o pagamento) |
| Prazo para cancelar venda | 24 horas após a venda |
| Prazo para cancelar pedido (cliente) | 24 horas após o pedido |
| Prazo para cancelar pedido já entregue (Admin) | 24 horas após a entrega |
| Prazo para retirada de pedido separado | 24 horas após a separação; depois, cancelado automaticamente |
| Estoque disponível online | Estoque menos o estoque mínimo (nunca menor que zero) |
| Preço final | Preço de venda menos a promoção válida (0 a 100%), arredondado em 2 casas |
| Pedidos que contam no faturamento | Concluído e Cancelado por não retirada |
| Sessão da equipe | 30 minutos sem uso |
| Token do cliente | 24 horas |

As justificativas de cada regra (por exemplo, por que o pedido não retirado entra no faturamento) estão no [mapa de responsabilidades](entrega/MAPA-DE-RESPONSABILIDADES.md), seção 9.

## 6. Fora do escopo

Não existem hoje, e a documentação não os trata como funcionalidades:

- **Processamento de pagamento:** não há integração com meio de pagamento. O sistema só registra a forma escolhida.
- **Entrega em domicílio e frete:** todo pedido online é retirado na loja.
- **Recuperação de senha por e-mail** e envio de e-mails ou notificações.
- **Nota fiscal.**
- **Mais de uma loja ou filial.**
- **Exclusão lógica e auditoria completa** de alterações de cadastro.
- **Acesso à API para a equipe:** a API REST atende somente a loja online; a administração usa as telas web.

## 7. Glossário

| Termo | Significado |
|---|---|
| **Balcão** | Venda feita presencialmente na loja, registrada pela equipe. |
| **Pedido online** | Pedido feito pelo cliente na loja online, retirado na loja. |
| **Código de retirada** | Código que o cliente apresenta para retirar o pedido; a API só o devolve ao cliente dono do pedido. |
| **Movimentação** | Registro de toda mudança no saldo de um produto (entrada, saída, devolução ou ajuste), com origem, data e responsável. |
| **Estoque mínimo** | Quantidade que o produto deve manter; na loja online, é a parte do saldo reservada ao balcão. |
| **Estoque disponível online** | Saldo menos o estoque mínimo. |
| **Módulo** | Pasta de `Modules/` que agrupa uma capacidade de negócio (Estoque, Vendas, Compras, Loja, Dashboard, Usuários). |
| **Service / Repository** | Camadas internas de cada módulo: o service decide as regras; o repository consulta e prepara dados. |
| **Unit of Work** | Componente que controla a transação: grava tudo junto ou desfaz tudo. |
| **JWT** | Token assinado que identifica o cliente da loja nas chamadas à API. |
| **Rollback** | Desfazer todas as alterações de uma operação que falhou no meio. |
