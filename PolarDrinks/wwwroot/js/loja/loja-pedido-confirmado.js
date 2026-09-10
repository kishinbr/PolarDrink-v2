document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const pedidoId = params.get("id");

    if (!pedidoId) {
        document.getElementById("conteudoPedido").innerText = "Pedido não encontrado.";
        return;
    }

    const resultado = await chamarApi("/api/pedidos/" + pedidoId);

    if (!resultado.ok) {
        document.getElementById("conteudoPedido").innerText = "Não foi possível carregar os detalhes do pedido.";
        return;
    }

    const pedido = resultado.dados;

    let itensHtml = "";
    pedido.itens.forEach(item => {
        itensHtml += `<li>${item.itemPedidoQtd}x ${item.produto.produtoNome} - R$ ${item.itemPedidoPreco.toFixed(2)}</li>`;
    });

    document.getElementById("conteudoPedido").innerHTML = `
        <p>Seu código de retirada é:</p>
        <h1>${pedido.pedidoCodigo}</h1>
        <p>Guarde esse código e apresente-o na loja, junto com seu nome, para retirar seu pedido.</p>

        <p><strong>Você tem até 48 horas após a separação para retirar o produto.</strong></p>

        <h3>Itens do pedido:</h3>
        <ul>${itensHtml}</ul>

        <p><strong>Total: R$ ${pedido.pedidoValorTotal.toFixed(2)}</strong></p>
    `;
});