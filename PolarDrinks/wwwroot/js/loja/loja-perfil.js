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
    renderizarListaPedidos("listaPedidosConcluidos", concluidos, "Nenhum pedido finalizado ainda.");
}

function renderizarListaPedidos(idContainer, pedidos, mensagemVazio) {
    const container = document.getElementById(idContainer);

    container.innerHTML = "";

    if (pedidos.length === 0) {
        container.innerHTML = `
            <div class="lista-vazia">
                <i class="bi bi-inbox"></i>
                ${escaparHtml(mensagemVazio)}
            </div>
        `;
        return;
    }

    pedidos.forEach(pedido => {
        const quantidadeItens = pedido.itens.reduce((total, item) => total + item.itemPedidoQtd, 0);

        const div = document.createElement("div");
        div.className = "pedido-item";
        div.tabIndex = 0;
        div.setAttribute("role", "button");

        div.innerHTML = `
            <div class="pedido-item-topo">
                <div>
                    <h4>Pedido #${pedido.pedidoID}</h4>
                    <span class="pedido-codigo">Código ${escaparHtml(pedido.pedidoCodigo)}</span>
                </div>

                <span class="status-pedido status-${escaparHtml(pedido.pedidoStatus)}">
                    ${escaparHtml(traduzirStatus(pedido.pedidoStatus))}
                </span>
            </div>

            <div class="pedido-item-info">
                <span>
                    ${new Date(pedido.pedidoData).toLocaleDateString("pt-BR")}
                    · ${quantidadeItens} ${quantidadeItens === 1 ? "item" : "itens"}
                </span>

                <strong>${formatarPreco(pedido.pedidoValorTotal)}</strong>
            </div>
        `;

        div.addEventListener("click", () => abrirModalPedido(pedido));
        div.addEventListener("keydown", e => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                abrirModalPedido(pedido);
            }
        });

        container.appendChild(div);
    });
}

function traduzirStatus(status) {
    const traducoes = {
        "AguardandoSeparacao": "Aguardando separação",
        "Separado": "Pronto para retirada",
        "Concluido": "Retirado",
        "CanceladoCliente": "Você cancelou",
        "CanceladoNaoRetirado": "Expirado",
        "CanceladoAdmin": "Cancelado"
    };

    return traducoes[status] || status;
}

function traduzirPagamento(tipo) {
    const traducoes = {
        "Cartao": "Cartão",
        "Pix": "Pix"
    };

    return traducoes[tipo] || tipo;
}

function abrirModalPedido(pedido) {
    const itensHtml = pedido.itens.map(item => `
        <li>
            <span>${item.itemPedidoQtd}× ${escaparHtml(item.produto?.produtoNome ?? "Produto")}</span>
            <strong>${formatarPreco(item.itemPedidoPreco * item.itemPedidoQtd)}</strong>
        </li>
    `).join("");

    let historicoHtml = `
        <p><i class="bi bi-calendar3"></i> Comprado em ${new Date(pedido.pedidoData).toLocaleString("pt-BR")}</p>
    `;

    if (pedido.pedidoDataSeparado) {
        historicoHtml += `
            <p><i class="bi bi-box-seam"></i> Separado em ${new Date(pedido.pedidoDataSeparado).toLocaleString("pt-BR")}</p>
        `;
    }

    if (pedido.pedidoDataConcluido) {
        historicoHtml += `
            <p><i class="bi bi-check-circle"></i> Retirado em ${new Date(pedido.pedidoDataConcluido).toLocaleString("pt-BR")}</p>
        `;
    }

    const podeCancelar =
        pedido.pedidoStatus === "AguardandoSeparacao" ||
        pedido.pedidoStatus === "Separado";

    document.getElementById("conteudoModalPedido").innerHTML = `
        <div class="modal-pedido-titulo">
            <h3>Pedido #${pedido.pedidoID}</h3>
            <span class="modal-codigo">Código de retirada: ${escaparHtml(pedido.pedidoCodigo)}</span>
        </div>

        <div class="modal-info">
            <div class="modal-info-item">
                <span>Status</span>
                <strong>${escaparHtml(traduzirStatus(pedido.pedidoStatus))}</strong>
            </div>

            <div class="modal-info-item">
                <span>Pagamento</span>
                <strong>${escaparHtml(traduzirPagamento(pedido.pedidoTipoPagamento))}</strong>
            </div>
        </div>

        <div class="modal-historico">${historicoHtml}</div>

        <div class="modal-secao">
            <h4>Itens do pedido</h4>
            <ul class="modal-itens">${itensHtml}</ul>
        </div>

        <div class="modal-total">
            <span>Total</span>
            <strong>${formatarPreco(pedido.pedidoValorTotal)}</strong>
        </div>

        ${podeCancelar ? `
            <button type="button" id="btnCancelarPedidoModal" class="btn-cancelar-pedido">
                <i class="bi bi-x-circle"></i> Cancelar pedido
            </button>
        ` : ""}
    `;

    document.getElementById("overlayModal").style.display = "block";
    document.getElementById("modalDetalhePedido").style.display = "block";
    document.body.style.overflow = "hidden";

    if (podeCancelar) {
        document.getElementById("btnCancelarPedidoModal").addEventListener("click", async function () {
            if (!confirm("Tem certeza que deseja cancelar este pedido?")) return;

            const botao = this;

            botao.disabled = true;
            botao.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Cancelando...`;

            const resultado = await chamarApi("/api/pedidos/" + pedido.pedidoID + "/cancelar", "POST");

            if (!resultado.ok) {
                alert(resultado.dados?.mensagem || "Não foi possível cancelar o pedido.");

                botao.disabled = false;
                botao.innerHTML = `<i class="bi bi-x-circle"></i> Cancelar pedido`;
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

document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") fecharModal();
});

function fecharModal() {
    document.getElementById("overlayModal").style.display = "none";
    document.getElementById("modalDetalhePedido").style.display = "none";
    document.body.style.overflow = "";
}
