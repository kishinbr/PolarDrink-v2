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
        avisoDiv.innerHTML = carrinho.avisosRemocao.join("<br>");
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

    carrinho.itens.forEach(item => {
        const imagemUrl = item.produtoImagemUrl
            ? "/" + item.produtoImagemUrl
            : "/images/placeholder.jpg";

        const div = document.createElement("div");
        div.style.display = "flex";
        div.style.gap = "10px";
        div.style.borderBottom = "1px solid #eee";
        div.style.padding = "10px 0";

        div.innerHTML = `
            <img src="${imagemUrl}" style="width:70px; height:70px; object-fit:cover;" />
            <div style="flex:1;">
                <p>${item.produtoNome}</p>
                <p>R$ ${item.precoUnitario.toFixed(2)} cada</p>
                <button class="btn-diminuir" data-produto-id="${item.produtoID}" ${item.quantidade <= 1 ? "disabled" : ""}>-</button>
                <span>${item.quantidade}</span>
                <button class="btn-aumentar" data-produto-id="${item.produtoID}">+</button>
                <button class="btn-excluir-item" data-produto-id="${item.produtoID}">Excluir</button>
            </div>
            <div>
                <strong>R$ ${item.subtotal.toFixed(2)}</strong>
            </div>
        `;

        lista.appendChild(div);
    });

    document.getElementById("totalCarrinho").innerText = "R$ " + carrinho.total.toFixed(2);

    religarBotoesItens();
}
function religarBotoesItens() {
    document.querySelectorAll(".btn-aumentar").forEach(botao => {
        botao.addEventListener("click", async function () {
            await alterarQuantidade(parseInt(botao.dataset.produtoId), 1);
        });
    });

    document.querySelectorAll(".btn-diminuir").forEach(botao => {
        botao.addEventListener("click", async function () {
            await alterarQuantidade(parseInt(botao.dataset.produtoId), -1);
        });
    });

    document.querySelectorAll(".btn-excluir-item").forEach(botao => {
        botao.addEventListener("click", async function () {
            await excluirItem(parseInt(botao.dataset.produtoId));
        });
    });
}

async function alterarQuantidade(produtoId, delta) {
    const resultado = await chamarApi("/api/carrinho");
    if (!resultado.ok) return;

    const item = resultado.dados.itens.find(i => i.produtoID === produtoId);
    if (!item) return;

    const novaQuantidade = item.quantidade + delta;

    await chamarApi("/api/carrinho/itens/" + produtoId, "PUT", {
        quantidade: novaQuantidade
    });

    await carregarCarrinho();
}

async function excluirItem(produtoId) {
    await chamarApi("/api/carrinho/itens/" + produtoId, "DELETE");
    await carregarCarrinho();
}
document.getElementById("btnFinalizarCompra").addEventListener("click", async function () {
    const botao = this;
    botao.disabled = true;
    botao.innerText = "Processando...";

    const resultado = await chamarApi("/api/pedidos/checkout", "POST");

    if (!resultado.ok) {
        alert(resultado.dados?.mensagem || "Erro ao finalizar a compra. Tente novamente.");
        botao.disabled = false;
        botao.innerText = "Finalizar Compra";
        return;
    }

    window.location.href = "/loja/PedidoConfirmado?id=" + resultado.dados.pedidoID;
});
document.getElementById("btnLimparCarrinho").addEventListener("click", async function () {
    const confirmar = confirm("Tem certeza que deseja excluir todos os itens do carrinho?");
    if (!confirmar) return;

    await chamarApi("/api/carrinho", "DELETE");
    await carregarCarrinho();
});