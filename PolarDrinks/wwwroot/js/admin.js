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
    const LIMITE_INATIVIDADE = 10 * 60 * 1000; // 10 minutos sem uso
    let ultimaAtividade = Date.now();

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
    let temporizador = null;

    function iniciar() {
        if (temporizador !== null) return;
        atualizar();
        temporizador = setInterval(verificar, INTERVALO);
    }

    function parar() {
        clearInterval(temporizador);
        temporizador = null;
    }

    // Sem uso por 10 minutos: para de consultar. Assim a sessão expira normalmente
    // e o contêiner e o banco na nuvem podem desligar.
    function verificar() {
        if (Date.now() - ultimaAtividade > LIMITE_INATIVIDADE) {
            parar();
            return;
        }
        atualizar();
    }

    // Qualquer uso da tela volta a consultar
    function registrarAtividade() {
        ultimaAtividade = Date.now();
        if (!document.hidden) iniciar();
    }

    ["mousemove", "keydown", "click", "scroll", "touchstart"].forEach(function (evento) {
        document.addEventListener(evento, registrarAtividade, { passive: true });
    });

    document.addEventListener("visibilitychange", function () {
        if (document.hidden) {
            parar();
        } else {
            registrarAtividade();
        }
    });

    if (!document.hidden) {
        iniciar();
    }
});

// Campo de imagem (cadastrar/editar produto): mostra nome e prévia do arquivo escolhido
// e permite desfazer a escolha antes de salvar. Não envia nada ao servidor.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("[data-upload]").forEach(function (area) {
        const input = area.querySelector(".upload-input");
        const nome = area.querySelector(".upload-nome");
        const remover = area.querySelector(".upload-remover");
        const previa = area.querySelector(".upload-previa");
        if (!input || !nome || !remover) return;

        const textoPadrao = nome.textContent;
        let urlPrevia = null;

        function limparPrevia() {
            if (urlPrevia) {
                URL.revokeObjectURL(urlPrevia);
                urlPrevia = null;
            }
            if (previa) {
                previa.hidden = true;
                previa.removeAttribute("src");
            }
        }

        input.addEventListener("change", function () {
            limparPrevia();
            const arquivo = input.files[0];

            if (!arquivo) {
                nome.textContent = textoPadrao;
                remover.hidden = true;
                return;
            }

            nome.textContent = arquivo.name;
            remover.hidden = false;

            if (previa && arquivo.type.startsWith("image/")) {
                urlPrevia = URL.createObjectURL(arquivo);
                previa.src = urlPrevia;
                previa.hidden = false;
            }
        });

        remover.addEventListener("click", function () {
            input.value = "";
            input.dispatchEvent(new Event("change"));
        });
    });
});
