document.addEventListener("DOMContentLoaded", async function () {
    if (!estaLogado()) {
        window.location.href = "/loja/conta";
        return;
    }

    await carregarPedidos();
});

async function carregarPedidos() {
    const resultado = await chamarApi("/api/pedidos");

    if (!resultado.ok) return;

    const statusAndamento = ["AguardandoSeparacao", "Separado"];

    const emAndamento = resultado.dados.filter(p => statusAndamento.includes(p.pedidoStatus));
    const concluidos = resultado.dados.filter(p => !statusAndamento.includes(p.pedidoStatus));

    renderizarListaPedidos("listaPedidosAndamento", emAndamento, "Nenhum pedido em andamento.");
    renderizarListaPedidos("listaPedidosConcluidos", concluidos, "Nenhum pedido concluído ainda.");
}
function renderizarListaPedidos(idContainer, pedidos, mensagemVazio) {
    const container = document.getElementById(idContainer);
    container.innerHTML = "";

    if (pedidos.length === 0) {
        container.innerHTML = `<p>${mensagemVazio}</p>`;
        return;
    }

    pedidos.forEach(pedido => {
        const div = document.createElement("div");
        div.style.border = "1px solid #ccc";
        div.style.padding = "10px";
        div.style.marginBottom = "10px";
        div.style.cursor = "pointer";

        div.innerHTML = `
            <p><strong>Pedido #${pedido.pedidoID}</strong> - Código: ${pedido.pedidoCodigo}</p>
            <p>Status: ${traduzirStatus(pedido.pedidoStatus)}</p>
            <p>Total: R$ ${pedido.pedidoValorTotal.toFixed(2)}</p>
        `;

        div.addEventListener("click", function () {
            abrirModalPedido(pedido);
        });

        container.appendChild(div);
    });
}

function traduzirStatus(status) {
    const traducoes = {
        "AguardandoSeparacao": "Aguardando Separação",
        "Separado": "Pronto para Retirada",
        "Concluido": "Concluído",
        "CanceladoCliente": "Cancelado por Você",
        "CanceladoNaoRetirado": "Cancelado - Não Retirado",
        "CanceladoAdmin": "Pedido Cancelado"
    };

    return traducoes[status] || status;
}

function abrirModalPedido(pedido) {
    let itensHtml = "";
    pedido.itens.forEach(item => {
        itensHtml += `<li>${item.itemPedidoQtd}x ${item.produto.produtoNome} - R$ ${item.itemPedidoPreco.toFixed(2)}</li>`;
    });

    let historicoHtml = `<p>Comprado em: ${new Date(pedido.pedidoData).toLocaleString("pt-BR")}</p>`;

    if (pedido.pedidoDataSeparado) {
        historicoHtml += `<p>Separado em: ${new Date(pedido.pedidoDataSeparado).toLocaleString("pt-BR")}</p>`;
    }

    if (pedido.pedidoDataConcluido) {
        historicoHtml += `<p>Retirado em: ${new Date(pedido.pedidoDataConcluido).toLocaleString("pt-BR")}</p>`;
    }

    const podeCancel = pedido.pedidoStatus === "AguardandoSeparacao" || pedido.pedidoStatus === "Separado";

    document.getElementById("conteudoModalPedido").innerHTML = `
        <h3>Pedido #${pedido.pedidoID}</h3>
        <p>Código: <strong>${pedido.pedidoCodigo}</strong></p>
        <p>Status: ${traduzirStatus(pedido.pedidoStatus)}</p>
        <p>Forma de pagamento: ${traduzirPagamento(pedido.pedidoTipoPagamento)}</p>
        ${historicoHtml}

        <h4>Itens:</h4>
        <ul>${itensHtml}</ul>

        <p><strong>Total: R$ ${pedido.pedidoValorTotal.toFixed(2)}</strong></p>

        ${podeCancel ? `<button id="btnCancelarPedidoModal" data-pedido-id="${pedido.pedidoID}">Cancelar Pedido</button>` : ""}
    `;

    document.getElementById("overlayModal").style.display = "block";
    document.getElementById("modalDetalhePedido").style.display = "block";

    if (podeCancel) {
        document.getElementById("btnCancelarPedidoModal").addEventListener("click", async function () {
            const confirmar = confirm("Tem certeza que deseja cancelar este pedido?");
            if (!confirmar) return;

            const resultado = await chamarApi("/api/pedidos/" + pedido.pedidoID + "/cancelar", "POST");

            if (!resultado.ok) {
                alert(resultado.dados?.mensagem || "Não foi possível cancelar o pedido.");
                return;
            }

            alert("Pedido cancelado com sucesso!");
            fecharModal();
            await carregarPedidos();
        });
    }
}

document.getElementById("btnFecharModal").addEventListener("click", fecharModal);
document.getElementById("overlayModal").addEventListener("click", fecharModal);

function fecharModal() {
    document.getElementById("overlayModal").style.display = "none";
    document.getElementById("modalDetalhePedido").style.display = "none";
}
function traduzirPagamento(tipo) {
    const traducoes = {
        "Cartao": "Cartão",
        "Pix": "Pix"
    };

    return traducoes[tipo] || tipo;
}