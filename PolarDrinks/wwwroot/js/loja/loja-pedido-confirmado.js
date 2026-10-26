document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const pedidoId = params.get("id");
    const conteudo = document.getElementById("conteudoPedido");

    if (!pedidoId) {
        conteudo.innerHTML = `
            <div class="pedido-erro">
                <i class="bi bi-exclamation-circle fs-2"></i>
                <p class="mt-2 mb-0">Pedido não encontrado.</p>
            </div>
        `;
        return;
    }

    const resultado = await chamarApi("/api/pedidos/" + encodeURIComponent(pedidoId));

    if (!resultado.ok) {
        conteudo.innerHTML = `
            <div class="pedido-erro">
                <i class="bi bi-exclamation-circle fs-2"></i>
                <p class="mt-2 mb-0">Não foi possível carregar os detalhes do pedido.</p>
            </div>
        `;
        return;
    }

    const pedido = resultado.dados;

    const itensHtml = pedido.itens.map(item => `
        <li>
            <span>${item.itemPedidoQtd}× ${escaparHtml(item.produto?.produtoNome ?? "Produto")}</span>
            <strong>${formatarPreco(item.itemPedidoPreco * item.itemPedidoQtd)}</strong>
        </li>
    `).join("");

    conteudo.innerHTML = `
        <div class="codigo-box">
            <span>SEU CÓDIGO DE RETIRADA</span>
            <h2>${escaparHtml(pedido.pedidoCodigo)}</h2>
        </div>

        <div class="aviso aviso-info">
            <i class="bi bi-info-circle"></i>
            <div>
                Apresente esse código e o seu nome na loja para retirar o pedido.
                <strong>Você tem até 24 horas após a separação para retirar.</strong>
            </div>
        </div>

        <div class="secao-itens">
            <h3>Itens do pedido #${pedido.pedidoID}</h3>
            <ul id="listaItensPedido" class="lista-itens-pedido">${itensHtml}</ul>
        </div>

        <div class="total-pedido">
            <span>Total do pedido</span>
            <strong>${formatarPreco(pedido.pedidoValorTotal)}</strong>
        </div>

        <div class="pedido-acoes">
            <a href="/loja/catalogo" class="btn-secundario">
                Continuar comprando
            </a>

            <a href="/loja/perfil" class="btn-primario">
                Meus pedidos <i class="bi bi-arrow-right"></i>
            </a>
        </div>
    `;
});
