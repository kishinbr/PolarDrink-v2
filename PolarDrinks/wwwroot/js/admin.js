// Telas da equipe: comportamento do layout (_Layout.cshtml).

// Menu lateral no celular e no tablet: abre pelo botão do topo, fecha pelo fundo escuro ou pelo Esc
document.addEventListener("DOMContentLoaded", function () {
    const botao = document.getElementById("btnAbrirMenu");
    const fundo = document.getElementById("fundoMenu");

    if (!botao || !fundo) return;

    function fecharMenu() {
        document.body.classList.remove("menu-aberto");
    }

    botao.addEventListener("click", function () {
        document.body.classList.toggle("menu-aberto");
    });

    fundo.addEventListener("click", fecharMenu);

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") fecharMenu();
    });
});

// Pedidos online aguardando separação: número no item "Pedidos online" do menu
// e um aviso quando chega pedido novo. Consulta o servidor ao abrir a página e a cada 30 segundos.
document.addEventListener("DOMContentLoaded", function () {
    const contador = document.getElementById("contadorPedidos");
    if (!contador) return;

    const CHAVE = "pedidosAguardando";
    const INTERVALO = 30000;

    function lerUltimaQuantidade() {
        try {
            const valor = sessionStorage.getItem(CHAVE);
            return valor === null ? null : parseInt(valor, 10);
        } catch {
            return null;
        }
    }

    function salvarUltimaQuantidade(quantidade) {
        try {
            sessionStorage.setItem(CHAVE, quantidade);
        } catch { }
    }

    function mostrarAvisoPedidoNovo(novos) {
        const aviso = document.createElement("a");
        aviso.href = "/PedidoOnline/Index";
        aviso.className = "aviso-pedido-novo";
        aviso.innerHTML =
            '<i class="bi bi-bag-check"></i>' +
            "<span><b>" + (novos === 1 ? "Novo pedido online!" : novos + " novos pedidos online!") + "</b>" +
            "<small>Clique para abrir a separação</small></span>";

        document.body.appendChild(aviso);

        setTimeout(function () { aviso.classList.add("saindo"); }, 7000);
        setTimeout(function () { aviso.remove(); }, 7600);
    }

    async function atualizar() {
        try {
            const resposta = await fetch("/PedidoOnline/ContarPendentes", { headers: { "Accept": "application/json" } });
            if (!resposta.ok) return;

            const dados = await resposta.json();
            const quantidade = dados.aguardando;

            contador.textContent = quantidade > 99 ? "99+" : quantidade;
            contador.hidden = quantidade === 0;
            document.body.classList.toggle("tem-pedidos", quantidade > 0);

            const anterior = lerUltimaQuantidade();
            if (anterior !== null && quantidade > anterior) {
                mostrarAvisoPedidoNovo(quantidade - anterior);
            }

            salvarUltimaQuantidade(quantidade);
        } catch {
            // Sem conexão ou sessão expirada: tenta de novo na próxima rodada
        }
    }

    atualizar();
    setInterval(atualizar, INTERVALO);
});
