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

    const statusAndamento = [
        "AguardandoSeparacao",
        "Separado"
    ];

    const emAndamento = resultado.dados.filter(p =>
        statusAndamento.includes(p.pedidoStatus)
    );

    const concluidos = resultado.dados.filter(p =>
        !statusAndamento.includes(p.pedidoStatus)
    );

    renderizarListaPedidos(
        "listaPedidosAndamento",
        emAndamento,
        "Nenhum pedido em andamento."
    );

    renderizarListaPedidos(
        "listaPedidosConcluidos",
        concluidos,
        "Nenhum pedido concluído ainda."
    );
}

function renderizarListaPedidos(idContainer, pedidos, mensagemVazio) {
    const container = document.getElementById(idContainer);

    container.innerHTML = "";

    if (pedidos.length === 0) {
        container.innerHTML = `
            <div class="lista-vazia">
                <i class="bi bi-inbox"></i>
                ${mensagemVazio}
            </div>
        `;
        return;
    }

    pedidos.forEach(pedido => {
        const div = document.createElement("div");

        div.className = "pedido-card";

        div.innerHTML = `
            <div class="pedido-card-topo">
                <div>
                    <h4>
                        <i class="bi bi-receipt"></i>
                        Pedido #${pedido.pedidoID}
                    </h4>

                    <span class="pedido-codigo">
                        Código: ${pedido.pedidoCodigo}
                    </span>
                </div>

                <span class="status-pedido status-${pedido.pedidoStatus}">
                    ${traduzirStatus(pedido.pedidoStatus)}
                </span>
            </div>

            <div class="pedido-card-info">
                <span>
                    <i class="bi bi-calendar3"></i>
                    ${new Date(pedido.pedidoData).toLocaleDateString("pt-BR")}
                </span>

                <strong>
                    R$ ${pedido.pedidoValorTotal.toFixed(2).replace(".", ",")}
                </strong>
            </div>
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
        "CanceladoCliente": "Você Cancelou",
        "CanceladoNaoRetirado": "Expirado",
        "CanceladoAdmin": "Pedido Cancelado",
    };

    return traducoes[status] || status;
}

function abrirModalPedido(pedido) {
    let itensHtml = "";

    pedido.itens.forEach(item => {
        itensHtml += `
            <li>
                <span>
                    ${item.itemPedidoQtd}x ${item.produto.produtoNome}
                </span>

                <strong>
                    R$ ${item.itemPedidoPreco.toFixed(2).replace(".", ",")}
                </strong>
            </li>
        `;
    });

    let historicoHtml = `
        <div class="modal-historico">
            <p>
                <i class="bi bi-calendar3"></i>
                Comprado em:
                ${new Date(pedido.pedidoData).toLocaleString("pt-BR")}
            </p>
    `;

    if (pedido.pedidoDataSeparado) {
        historicoHtml += `
            <p>
                <i class="bi bi-box-seam"></i>
                Separado em:
                ${new Date(pedido.pedidoDataSeparado).toLocaleString("pt-BR")}
            </p>
        `;
    }

    if (pedido.pedidoDataConcluido) {
        historicoHtml += `
            <p>
                <i class="bi bi-check-circle"></i>
                Retirado em:
                ${new Date(pedido.pedidoDataConcluido).toLocaleString("pt-BR")}
            </p>
        `;
    }

    historicoHtml += `</div>`;

    const podeCancel =
        pedido.pedidoStatus === "AguardandoSeparacao" ||
        pedido.pedidoStatus === "Separado";

    document.getElementById("conteudoModalPedido").innerHTML = `
        <div class="modal-pedido-titulo">
            <h3>
                <i class="bi bi-receipt"></i>
                Pedido #${pedido.pedidoID}
            </h3>

            <span class="modal-codigo">
                Código de retirada: ${pedido.pedidoCodigo}
            </span>
        </div>

        <div class="modal-info">
            <div class="modal-info-item">
                <span>Status</span>
                <strong>${traduzirStatus(pedido.pedidoStatus)}</strong>
            </div>

            <div class="modal-info-item">
                <span>Pagamento</span>
                <strong>${traduzirPagamento(pedido.pedidoTipoPagamento)}</strong>
            </div>
        </div>

        ${historicoHtml}

        <div class="modal-secao">
            <h4>
                <i class="bi bi-bag"></i>
                Itens do pedido
            </h4>

            <ul class="modal-itens">
                ${itensHtml}
            </ul>
        </div>

        <div class="modal-total">
            <span>Total</span>
            <strong>
                R$ ${pedido.pedidoValorTotal.toFixed(2).replace(".", ",")}
            </strong>
        </div>

        ${podeCancel ? `
            <button
                id="btnCancelarPedidoModal"
                class="btn-cancelar-pedido"
                data-pedido-id="${pedido.pedidoID}">
                <i class="bi bi-x-circle"></i>
                Cancelar Pedido
            </button>
        ` : ""}
    `;

    document.getElementById("overlayModal").style.display = "block";
    document.getElementById("modalDetalhePedido").style.display = "block";

    if (podeCancel) {
        document.getElementById("btnCancelarPedidoModal").addEventListener("click", async function () {
            const confirmar = confirm(
                "Tem certeza que deseja cancelar este pedido?"
            );

            if (!confirmar) return;

            const botao = this;

            botao.disabled = true;
            botao.innerHTML = `
                <span class="spinner-border spinner-border-sm"></span>
                Cancelando...
            `;

            const resultado = await chamarApi(
                "/api/pedidos/" + pedido.pedidoID + "/cancelar",
                "POST"
            );

            if (!resultado.ok) {
                alert(
                    resultado.dados?.mensagem ||
                    "Não foi possível cancelar o pedido."
                );

                botao.disabled = false;
                botao.innerHTML = `
                    <i class="bi bi-x-circle"></i>
                    Cancelar Pedido
                `;

                return;
            }

            alert("Pedido cancelado com sucesso!");

            fecharModal();

            await carregarPedidos();
        });
    }
}

document.getElementById("btnFecharModal").addEventListener(
    "click",
    fecharModal
);

document.getElementById("overlayModal").addEventListener(
    "click",
    fecharModal
);

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