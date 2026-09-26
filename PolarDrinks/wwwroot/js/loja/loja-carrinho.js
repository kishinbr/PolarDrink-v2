document.addEventListener("DOMContentLoaded", async function () {
    if (!estaLogado()) {
        window.location.href = "/loja/conta?redirecionarPara=carrinho";
        return;
    }

    await carregarCarrinho();
});

async function carregarCarrinho() {
    const resultado = await chamarApi("/api/carrinho");

    if (!resultado.ok) return;

    const carrinho = resultado.dados;

    const avisoDiv = document.getElementById("mensagemAviso");

    if (carrinho.avisosRemocao.length > 0) {
        avisoDiv.innerHTML = `
            <i class="bi bi-exclamation-triangle"></i>
            ${carrinho.avisosRemocao.join("<br>")}
        `;
        avisoDiv.style.display = "block";
    } else {
        avisoDiv.style.display = "none";
    }

    if (carrinho.itens.length === 0) {
        document.getElementById("carrinhoVazio").style.display = "block";
        document.getElementById("carrinhoConteudo").style.display = "none";
        return;
    }

    document.getElementById("carrinhoVazio").style.display = "none";
    document.getElementById("carrinhoConteudo").style.display = "flex";

    const lista = document.getElementById("listaItensCarrinho");
    lista.innerHTML = "";

    carrinho.itens.forEach(function (item) {
        const imagemUrl = item.produtoImagemUrl
            ? "/" + item.produtoImagemUrl
            : "/images/placeholder.jpg";

        const div = document.createElement("div");
        div.className = "item-carrinho";

        const imagem = document.createElement("img");
        imagem.src = imagemUrl;
        imagem.className = "imagem-item-carrinho";
        imagem.alt = item.produtoNome;

        const info = document.createElement("div");
        info.className = "info-item-carrinho";

        const topo = document.createElement("div");
        topo.className = "item-carrinho-topo";

        const dadosProduto = document.createElement("div");

        const nome = document.createElement("h4");
        nome.textContent = item.produtoNome;

        const preco = document.createElement("span");
        preco.className = "preco-unitario";
        preco.textContent = "R$ " + item.precoUnitario.toFixed(2).replace(".", ",") + " cada";

        dadosProduto.appendChild(nome);
        dadosProduto.appendChild(preco);

        const subtotal = document.createElement("strong");
        subtotal.className = "subtotal-item";
        subtotal.textContent = "R$ " + item.subtotal.toFixed(2).replace(".", ",");

        topo.appendChild(dadosProduto);
        topo.appendChild(subtotal);

        const acoes = document.createElement("div");
        acoes.className = "item-carrinho-acoes";

        const controle = document.createElement("div");
        controle.className = "controle-quantidade";

        const diminuir = document.createElement("button");
        diminuir.className = "btn-diminuir";
        diminuir.dataset.produtoId = item.produtoID;
        diminuir.disabled = item.quantidade <= 1;
        diminuir.innerHTML = '<i class="bi bi-dash"></i>';

        const quantidade = document.createElement("span");
        quantidade.textContent = item.quantidade;

        const aumentar = document.createElement("button");
        aumentar.className = "btn-aumentar";
        aumentar.dataset.produtoId = item.produtoID;
        aumentar.innerHTML = '<i class="bi bi-plus"></i>';

        controle.appendChild(diminuir);
        controle.appendChild(quantidade);
        controle.appendChild(aumentar);

        const excluir = document.createElement("button");
        excluir.className = "btn-excluir-item";
        excluir.dataset.produtoId = item.produtoID;
        excluir.innerHTML = '<i class="bi bi-trash3"></i> Excluir';

        acoes.appendChild(controle);
        acoes.appendChild(excluir);

        info.appendChild(topo);
        info.appendChild(acoes);

        div.appendChild(imagem);
        div.appendChild(info);

        lista.appendChild(div);
    });

    document.getElementById("totalCarrinho").innerText =
        "R$ " + carrinho.total.toFixed(2).replace(".", ",");

    religarBotoesItens();
}

function religarBotoesItens() {
    document.querySelectorAll(".btn-aumentar").forEach(function (botao) {
        botao.addEventListener("click", async function () {
            await alterarQuantidade(
                parseInt(botao.dataset.produtoId),
                1
            );
        });
    });

    document.querySelectorAll(".btn-diminuir").forEach(function (botao) {
        botao.addEventListener("click", async function () {
            await alterarQuantidade(
                parseInt(botao.dataset.produtoId),
                -1
            );
        });
    });

    document.querySelectorAll(".btn-excluir-item").forEach(function (botao) {
        botao.addEventListener("click", async function () {
            await excluirItem(
                parseInt(botao.dataset.produtoId)
            );
        });
    });
}

async function alterarQuantidade(produtoId, delta) {
    const resultado = await chamarApi("/api/carrinho");

    if (!resultado.ok) return;

    const item = resultado.dados.itens.find(function (i) {
        return i.produtoID === produtoId;
    });

    if (!item) return;

    const novaQuantidade = item.quantidade + delta;

    if (novaQuantidade < 1) return;

    const resultadoAlteracao = await chamarApi(
        "/api/carrinho/itens/" + produtoId,
        "PUT",
        {
            quantidade: novaQuantidade
        }
    );

    if (!resultadoAlteracao.ok) return;

    await carregarCarrinho();
}

async function excluirItem(produtoId) {
    const resultado = await chamarApi(
        "/api/carrinho/itens/" + produtoId,
        "DELETE"
    );

    if (!resultado.ok) return;

    await carregarCarrinho();
}

let tipoPagamentoSelecionado = null;

document.querySelectorAll(".btn-pagamento").forEach(function (botao) {
    botao.addEventListener("click", function () {
        tipoPagamentoSelecionado = botao.dataset.tipo;

        document.querySelectorAll(".btn-pagamento").forEach(function (b) {
            b.classList.remove("ativo");
        });

        botao.classList.add("ativo");

        document.getElementById("btnFinalizarCompra").disabled = false;
    });
});

document.getElementById("btnFinalizarCompra").addEventListener("click", async function () {
    const botao = this;

    botao.disabled = true;

    botao.innerHTML = `
        <span class="spinner-border spinner-border-sm"></span>
        Processando...
    `;

    const resultado = await chamarApi(
        "/api/pedidos/checkout",
        "POST",
        {
            tipoPagamento: tipoPagamentoSelecionado
        }
    );

    if (!resultado.ok) {
        alert(
            resultado.dados?.mensagem ||
            "Erro ao finalizar a compra. Tente novamente."
        );

        botao.disabled = false;

        botao.innerHTML = `
            Finalizar Compra <i class="bi bi-arrow-right"></i>
        `;

        return;
    }

    window.location.href =
        "/loja/PedidoConfirmado?id=" +
        resultado.dados.pedidoID;
});

document.getElementById("btnLimparCarrinho").addEventListener("click", async function () {
    const confirmar = confirm(
        "Tem certeza que deseja excluir todos os itens do carrinho?"
    );

    if (!confirmar) return;

    const resultado = await chamarApi("/api/carrinho", "DELETE");

    if (!resultado.ok) return;

    await carregarCarrinho();
});