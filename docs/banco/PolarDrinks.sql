IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Fornecedores] (
    [FornecedorID] int NOT NULL IDENTITY,
    [FornecedorNome] nvarchar(80) NOT NULL,
    [FornecedorCNPJ] nvarchar(18) NOT NULL,
    [FornecedorTelefone] nvarchar(15) NOT NULL,
    [FornecedorEmail] nvarchar(80) NOT NULL,
    [FornecedorCEP] nvarchar(10) NOT NULL,
    [FornecedorEstado] nvarchar(2) NOT NULL,
    [FornecedorCidade] nvarchar(100) NOT NULL,
    [FornecedorBairro] nvarchar(100) NOT NULL,
    [FornecedorLogradouro] nvarchar(100) NOT NULL,
    [FornecedorNum] nvarchar(10) NOT NULL,
    [FornecedorAtivo] bit NOT NULL,
    CONSTRAINT [PK_Fornecedores] PRIMARY KEY ([FornecedorID])
);

CREATE TABLE [Produtos] (
    [ProdutoID] int NOT NULL IDENTITY,
    [ProdutoNome] nvarchar(100) NOT NULL,
    [ProdutoDescricao] nvarchar(200) NULL,
    [ProdutoPrecoCusto] decimal(18,2) NOT NULL,
    [ProdutoPrecoVenda] decimal(18,2) NOT NULL,
    [ProdutoPromocao] decimal(5,2) NOT NULL,
    [ProdutoCodBarra] nvarchar(20) NOT NULL,
    [ProdutoQtdEstoque] int NOT NULL,
    [ProdutoEstoqueMinimo] int NOT NULL,
    [ProdutoAtivo] bit NOT NULL,
    CONSTRAINT [PK_Produtos] PRIMARY KEY ([ProdutoID])
);

CREATE TABLE [Usuarios] (
    [UsuarioID] int NOT NULL IDENTITY,
    [UsuarioNome] nvarchar(80) NOT NULL,
    [UsuarioLogin] nvarchar(50) NOT NULL,
    [UsuarioSenhaHash] nvarchar(max) NOT NULL,
    [UsuarioPerfil] nvarchar(max) NOT NULL,
    [UsuarioAtivo] bit NOT NULL,
    [UsuarioCriadoEm] datetime2 NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([UsuarioID])
);

CREATE TABLE [ComprasEstoque] (
    [CompraID] int NOT NULL IDENTITY,
    [CompraData] datetime2 NOT NULL,
    [CompraDataEntrega] datetime2 NULL,
    [CompraStatus] nvarchar(20) NOT NULL,
    [FornecedorID] int NOT NULL,
    [UsuarioID] int NULL,
    CONSTRAINT [PK_ComprasEstoque] PRIMARY KEY ([CompraID]),
    CONSTRAINT [FK_ComprasEstoque_Fornecedores_FornecedorID] FOREIGN KEY ([FornecedorID]) REFERENCES [Fornecedores] ([FornecedorID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ComprasEstoque_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
);

CREATE TABLE [Vendas] (
    [VendaID] int NOT NULL IDENTITY,
    [VendaValorTotal] decimal(18,2) NOT NULL,
    [VendaTipoPagamento] nvarchar(30) NOT NULL,
    [VendaData] datetime2 NOT NULL,
    [VendaCancelada] bit NOT NULL,
    [UsuarioID] int NULL,
    CONSTRAINT [PK_Vendas] PRIMARY KEY ([VendaID]),
    CONSTRAINT [FK_Vendas_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
);

CREATE TABLE [ItensCompra] (
    [ItemCompraID] int NOT NULL IDENTITY,
    [ItemCompraQtd] int NOT NULL,
    [ItemCompraPreco] decimal(18,2) NOT NULL,
    [ProdutoID] int NOT NULL,
    [CompraID] int NOT NULL,
    CONSTRAINT [PK_ItensCompra] PRIMARY KEY ([ItemCompraID]),
    CONSTRAINT [FK_ItensCompra_ComprasEstoque_CompraID] FOREIGN KEY ([CompraID]) REFERENCES [ComprasEstoque] ([CompraID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ItensCompra_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE CASCADE
);

CREATE TABLE [ItensVenda] (
    [ItemVendaID] int NOT NULL IDENTITY,
    [ItemVendaQtd] int NOT NULL,
    [ItemVendaPreco] decimal(18,2) NOT NULL,
    [ItemVendaCusto] decimal(18,2) NOT NULL,
    [ItemVendaTotal] decimal(18,2) NOT NULL,
    [ProdutoID] int NOT NULL,
    [VendaID] int NOT NULL,
    CONSTRAINT [PK_ItensVenda] PRIMARY KEY ([ItemVendaID]),
    CONSTRAINT [FK_ItensVenda_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ItensVenda_Vendas_VendaID] FOREIGN KEY ([VendaID]) REFERENCES [Vendas] ([VendaID]) ON DELETE CASCADE
);

CREATE TABLE [MovimentacoesEstoque] (
    [MovimentacaoID] int NOT NULL IDENTITY,
    [MovimentacaoTipo] nvarchar(30) NULL,
    [MovimentacaoQtd] int NOT NULL,
    [MovimentacaoDescricao] nvarchar(50) NULL,
    [MovimentacaoData] datetime2 NOT NULL,
    [ProdutoID] int NOT NULL,
    [ItemCompraID] int NULL,
    [ItemVendaID] int NULL,
    [UsuarioID] int NULL,
    CONSTRAINT [PK_MovimentacoesEstoque] PRIMARY KEY ([MovimentacaoID]),
    CONSTRAINT [FK_MovimentacoesEstoque_ItensCompra_ItemCompraID] FOREIGN KEY ([ItemCompraID]) REFERENCES [ItensCompra] ([ItemCompraID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimentacoesEstoque_ItensVenda_ItemVendaID] FOREIGN KEY ([ItemVendaID]) REFERENCES [ItensVenda] ([ItemVendaID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimentacoesEstoque_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MovimentacoesEstoque_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ComprasEstoque_FornecedorID] ON [ComprasEstoque] ([FornecedorID]);

CREATE INDEX [IX_ComprasEstoque_UsuarioID] ON [ComprasEstoque] ([UsuarioID]);

CREATE UNIQUE INDEX [IX_Fornecedores_FornecedorCNPJ] ON [Fornecedores] ([FornecedorCNPJ]);

CREATE INDEX [IX_ItensCompra_CompraID] ON [ItensCompra] ([CompraID]);

CREATE INDEX [IX_ItensCompra_ProdutoID] ON [ItensCompra] ([ProdutoID]);

CREATE INDEX [IX_ItensVenda_ProdutoID] ON [ItensVenda] ([ProdutoID]);

CREATE INDEX [IX_ItensVenda_VendaID] ON [ItensVenda] ([VendaID]);

CREATE INDEX [IX_MovimentacoesEstoque_ItemCompraID] ON [MovimentacoesEstoque] ([ItemCompraID]);

CREATE INDEX [IX_MovimentacoesEstoque_ItemVendaID] ON [MovimentacoesEstoque] ([ItemVendaID]);

CREATE INDEX [IX_MovimentacoesEstoque_ProdutoID] ON [MovimentacoesEstoque] ([ProdutoID]);

CREATE INDEX [IX_MovimentacoesEstoque_UsuarioID] ON [MovimentacoesEstoque] ([UsuarioID]);

CREATE UNIQUE INDEX [IX_Usuarios_UsuarioLogin] ON [Usuarios] ([UsuarioLogin]);

CREATE INDEX [IX_Vendas_UsuarioID] ON [Vendas] ([UsuarioID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260422184559_inicial', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE UNIQUE INDEX [IX_Produtos_ProdutoCodBarra] ON [Produtos] ([ProdutoCodBarra]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260905215253_IndiceUnicoCodigoBarras', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ItensVenda]') AND [c].[name] = N'ItemVendaTotal');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ItensVenda] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [ItensVenda] DROP COLUMN [ItemVendaTotal];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260905215606_RemoverColunaItemVendaTotal', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Produtos] ADD [ProdutoImagemUrl] nvarchar(max) NULL;

ALTER TABLE [Produtos] ADD [RowVersion] rowversion NULL;

ALTER TABLE [MovimentacoesEstoque] ADD [ItemPedidoID] int NULL;

CREATE TABLE [Categorias] (
    [CategoriaID] int NOT NULL IDENTITY,
    [CategoriaNome] nvarchar(50) NOT NULL,
    [CategoriaAtiva] bit NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([CategoriaID])
);

CREATE TABLE [Clientes] (
    [ClienteID] int NOT NULL IDENTITY,
    [ClienteNome] nvarchar(100) NOT NULL,
    [ClienteEmail] nvarchar(150) NOT NULL,
    [ClienteSenhaHash] nvarchar(max) NOT NULL,
    [ClienteTelefone] nvarchar(20) NOT NULL,
    [ClienteCPF] nvarchar(14) NOT NULL,
    [ClienteCriadoEm] datetime2 NOT NULL,
    [ClienteAtivo] bit NOT NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([ClienteID])
);

CREATE TABLE [ProdutoCategorias] (
    [ProdutoID] int NOT NULL,
    [CategoriaID] int NOT NULL,
    CONSTRAINT [PK_ProdutoCategorias] PRIMARY KEY ([ProdutoID], [CategoriaID]),
    CONSTRAINT [FK_ProdutoCategorias_Categorias_CategoriaID] FOREIGN KEY ([CategoriaID]) REFERENCES [Categorias] ([CategoriaID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProdutoCategorias_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE CASCADE
);

CREATE TABLE [CarrinhoItens] (
    [CarrinhoItemID] int NOT NULL IDENTITY,
    [ClienteID] int NOT NULL,
    [ProdutoID] int NOT NULL,
    [Quantidade] int NOT NULL,
    [AdicionadoEm] datetime2 NOT NULL,
    CONSTRAINT [PK_CarrinhoItens] PRIMARY KEY ([CarrinhoItemID]),
    CONSTRAINT [FK_CarrinhoItens_Clientes_ClienteID] FOREIGN KEY ([ClienteID]) REFERENCES [Clientes] ([ClienteID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CarrinhoItens_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE NO ACTION
);

CREATE TABLE [Pedidos] (
    [PedidoID] int NOT NULL IDENTITY,
    [PedidoCodigo] nvarchar(4) NOT NULL,
    [ClienteID] int NOT NULL,
    [PedidoData] datetime2 NOT NULL,
    [PedidoStatus] nvarchar(30) NOT NULL,
    [PedidoValorTotal] decimal(18,2) NOT NULL,
    [PedidoDataSeparado] datetime2 NULL,
    [UsuarioSeparouID] int NULL,
    [PedidoDataConcluido] datetime2 NULL,
    [UsuarioEntregouID] int NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY ([PedidoID]),
    CONSTRAINT [FK_Pedidos_Clientes_ClienteID] FOREIGN KEY ([ClienteID]) REFERENCES [Clientes] ([ClienteID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Pedidos_Usuarios_UsuarioEntregouID] FOREIGN KEY ([UsuarioEntregouID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Pedidos_Usuarios_UsuarioSeparouID] FOREIGN KEY ([UsuarioSeparouID]) REFERENCES [Usuarios] ([UsuarioID]) ON DELETE NO ACTION
);

CREATE TABLE [ItensPedido] (
    [ItemPedidoID] int NOT NULL IDENTITY,
    [PedidoID] int NOT NULL,
    [ProdutoID] int NOT NULL,
    [ItemPedidoQtd] int NOT NULL,
    [ItemPedidoPreco] decimal(18,2) NOT NULL,
    [ItemPedidoCusto] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ItensPedido] PRIMARY KEY ([ItemPedidoID]),
    CONSTRAINT [FK_ItensPedido_Pedidos_PedidoID] FOREIGN KEY ([PedidoID]) REFERENCES [Pedidos] ([PedidoID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ItensPedido_Produtos_ProdutoID] FOREIGN KEY ([ProdutoID]) REFERENCES [Produtos] ([ProdutoID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_MovimentacoesEstoque_ItemPedidoID] ON [MovimentacoesEstoque] ([ItemPedidoID]);

CREATE INDEX [IX_CarrinhoItens_ClienteID] ON [CarrinhoItens] ([ClienteID]);

CREATE INDEX [IX_CarrinhoItens_ProdutoID] ON [CarrinhoItens] ([ProdutoID]);

CREATE UNIQUE INDEX [IX_Clientes_ClienteCPF] ON [Clientes] ([ClienteCPF]);

CREATE UNIQUE INDEX [IX_Clientes_ClienteEmail] ON [Clientes] ([ClienteEmail]);

CREATE INDEX [IX_ItensPedido_PedidoID] ON [ItensPedido] ([PedidoID]);

CREATE INDEX [IX_ItensPedido_ProdutoID] ON [ItensPedido] ([ProdutoID]);

CREATE INDEX [IX_Pedidos_ClienteID] ON [Pedidos] ([ClienteID]);

CREATE INDEX [IX_Pedidos_UsuarioEntregouID] ON [Pedidos] ([UsuarioEntregouID]);

CREATE INDEX [IX_Pedidos_UsuarioSeparouID] ON [Pedidos] ([UsuarioSeparouID]);

CREATE INDEX [IX_ProdutoCategorias_CategoriaID] ON [ProdutoCategorias] ([CategoriaID]);

ALTER TABLE [MovimentacoesEstoque] ADD CONSTRAINT [FK_MovimentacoesEstoque_ItensPedido_ItemPedidoID] FOREIGN KEY ([ItemPedidoID]) REFERENCES [ItensPedido] ([ItemPedidoID]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260906023314_AdicionarLojaOnline', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Pedidos] ADD [PedidoTipoPagamento] nvarchar(20) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260912002419_AdicionarTipoPagamentoPedido', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;

EXEC(N'
CREATE OR ALTER TRIGGER TR_Usuarios_ProtegerUltimoAdmin
ON Usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted WHERE UsuarioAtivo = 1 AND UsuarioPerfil = N''Admin'')
       AND NOT EXISTS (SELECT 1 FROM Usuarios WHERE UsuarioAtivo = 1 AND UsuarioPerfil = N''Admin'')
    BEGIN
        THROW 50001, N''O sistema precisa ter pelo menos um administrador ativo.'', 1;
    END
END
')


EXEC(N'
CREATE OR ALTER TRIGGER TR_Produtos_ImpedirEstoqueNegativo
ON Produtos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE ProdutoQtdEstoque < 0)
    BEGIN
        THROW 50002, N''O estoque de um produto não pode ficar negativo.'', 1;
    END
END
')


EXEC(N'
CREATE OR ALTER PROCEDURE sp_ProdutosAbaixoDoMinimo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.ProdutoID,
        p.ProdutoNome,
        p.ProdutoCodBarra,
        p.ProdutoQtdEstoque,
        p.ProdutoEstoqueMinimo,
        CASE
            WHEN p.ProdutoEstoqueMinimo > p.ProdutoQtdEstoque
                THEN p.ProdutoEstoqueMinimo - p.ProdutoQtdEstoque
            ELSE 0
        END AS QuantidadeFaltante,
        p.ProdutoPrecoCusto
    FROM Produtos p
    WHERE p.ProdutoAtivo = 1
      AND p.ProdutoQtdEstoque <= p.ProdutoEstoqueMinimo
    ORDER BY p.ProdutoQtdEstoque, p.ProdutoNome;
END
')


EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoVendasPorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FimExclusivo DATETIME2 = DATEADD(DAY, 1, CAST(@Fim AS DATETIME2));

    SELECT
        COUNT(CASE WHEN v.VendaCancelada = 0 THEN 1 END) AS QuantidadeVendas,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 THEN v.VendaValorTotal END), 0) AS Faturamento,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 THEN i.LucroVenda END), 0) AS Lucro,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Dinheiro'' THEN v.VendaValorTotal END), 0) AS TotalDinheiro,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Pix'' THEN v.VendaValorTotal END), 0) AS TotalPix,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Cartão'' THEN v.VendaValorTotal END), 0) AS TotalCartao,
        COUNT(CASE WHEN v.VendaCancelada = 1 THEN 1 END) AS QuantidadeCanceladas,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 1 THEN v.VendaValorTotal END), 0) AS ValorCancelado
    FROM Vendas v
    OUTER APPLY (
        SELECT SUM((iv.ItemVendaPreco - iv.ItemVendaCusto) * iv.ItemVendaQtd) AS LucroVenda
        FROM ItensVenda iv
        WHERE iv.VendaID = v.VendaID
    ) i
    WHERE v.VendaData >= @Inicio
      AND v.VendaData < @FimExclusivo;
END
')

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261022230750_AdicionarTriggersEProcedures', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;

EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoPedidosOnlinePorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FimExclusivo DATETIME2 = DATEADD(DAY, 1, CAST(@Fim AS DATETIME2));

    SELECT
        COUNT(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN 1 END) AS QuantidadeVendas,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN p.PedidoValorTotal END), 0) AS Faturamento,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN i.LucroPedido END), 0) AS Lucro,
        CAST(0 AS DECIMAL(18,2)) AS TotalDinheiro,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') AND p.PedidoTipoPagamento = N''Pix'' THEN p.PedidoValorTotal END), 0) AS TotalPix,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') AND p.PedidoTipoPagamento = N''Cartao'' THEN p.PedidoValorTotal END), 0) AS TotalCartao,
        COUNT(CASE WHEN p.PedidoStatus IN (N''CanceladoCliente'', N''CanceladoAdmin'') THEN 1 END) AS QuantidadeCanceladas,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''CanceladoCliente'', N''CanceladoAdmin'') THEN p.PedidoValorTotal END), 0) AS ValorCancelado
    FROM Pedidos p
    OUTER APPLY (
        SELECT SUM((ip.ItemPedidoPreco - ip.ItemPedidoCusto) * ip.ItemPedidoQtd) AS LucroPedido
        FROM ItensPedido ip
        WHERE ip.PedidoID = p.PedidoID
    ) i
    WHERE p.PedidoData >= @Inicio
      AND p.PedidoData < @FimExclusivo;
END
')


EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoGeralPorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Resumo (
        QuantidadeVendas INT,
        Faturamento DECIMAL(18,2),
        Lucro DECIMAL(18,2),
        TotalDinheiro DECIMAL(18,2),
        TotalPix DECIMAL(18,2),
        TotalCartao DECIMAL(18,2),
        QuantidadeCanceladas INT,
        ValorCancelado DECIMAL(18,2)
    );

    INSERT INTO #Resumo EXEC sp_ResumoVendasPorPeriodo @Inicio, @Fim;
    INSERT INTO #Resumo EXEC sp_ResumoPedidosOnlinePorPeriodo @Inicio, @Fim;

    SELECT
        SUM(QuantidadeVendas) AS QuantidadeVendas,
        SUM(Faturamento) AS Faturamento,
        SUM(Lucro) AS Lucro,
        SUM(TotalDinheiro) AS TotalDinheiro,
        SUM(TotalPix) AS TotalPix,
        SUM(TotalCartao) AS TotalCartao,
        SUM(QuantidadeCanceladas) AS QuantidadeCanceladas,
        SUM(ValorCancelado) AS ValorCancelado
    FROM #Resumo;
END
')

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261023014048_AdicionarProceduresOnlineEGeral', N'10.0.5');

COMMIT;
GO

