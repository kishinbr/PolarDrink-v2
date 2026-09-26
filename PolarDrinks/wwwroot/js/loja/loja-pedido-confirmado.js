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

    const resultado = await chamarApi("/api/pedidos/" + pedidoId);

    if (!resultado.ok) {
        conteudo.innerHTML = `
            <div class="pedido-erro">
                <i class="bi bi-exclamation-circle fs-2"></i>
                <p class="mt-2 mb-0">
                    Não foi possível carregar os detalhes do pedido.
                </p>
            </div>
        `;
        return;
    }

    const pedido = resultado.dados;

    let itensHtml = "";

    pedido.itens.forEach(item => {
        const preco = item.itemPedidoPreco
            .toFixed(2)
            .replace(".", ",");

        itensHtml += `
            <li>
                <span class="item-nome">
                    <i class="bi bi-box-seam"></i>
                    ${item.itemPedidoQtd}x ${item.produto.produtoNome}
                </span>

                <strong class="item-preco">
                    R$ ${preco}
                </strong>
            </li>
        `;
    });

    const total = pedido.pedidoValorTotal
        .toFixed(2)
        .replace(".", ",");

    conteudo.innerHTML = `
        <div class="codigo-box">
            <span>SEU CÓDIGO DE RETIRADA</span>
            <h2>${pedido.pedidoCodigo}</h2>
        </div>

        <div class="aviso-retirada">
            <i class="bi bi-info-circle"></i>

            <div>
                Guarde esse código e apresente-o na loja,
                junto com seu nome, para retirar seu pedido.
                <br><br>
                <strong>
                    Você tem até 24 horas após a separação
                    para retirar o produto.
                </strong>
            </div>
        </div>

        <div class="secao-itens">
            <h3>
                <i class="bi bi-bag"></i>
                Itens do pedido
            </h3>

            <ul id="listaItensPedido">
                ${itensHtml}
            </ul>
        </div>

        <div class="total-pedido">
            <span>Total do pedido</span>
            <strong>R$ ${total}</strong>
        </div>

        <div class="pedido-acoes">
            <a href="/loja/catalogo" class="btn-catalogo">
                <i class="bi bi-shop"></i>
                Continuar comprando
            </a>

            <a href="/loja/perfil" class="btn-pedidos">
                <i class="bi bi-receipt"></i>
                Meus pedidos
            </a>
        </div>
    `;
});