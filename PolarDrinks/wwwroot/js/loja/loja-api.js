const TOKEN_KEY = "polardrinks_token";

function salvarToken(token) {
    localStorage.setItem(TOKEN_KEY, token);
}

function obterToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function removerToken() {
    localStorage.removeItem(TOKEN_KEY);
}

function estaLogado() {
    return obterToken() !== null;
}

async function chamarApi(url, metodo = "GET", corpo = null) {
    const headers = {
        "Content-Type": "application/json"
    };

    const token = obterToken();
    if (token) {
        headers["Authorization"] = "Bearer " + token;
    }

    const opcoes = {
        method: metodo,
        headers: headers
    };

    if (corpo !== null) {
        opcoes.body = JSON.stringify(corpo);
    }

    const resposta = await fetch(url, opcoes);

    if (resposta.status === 401 && token && !url.startsWith("/api/cliente/auth")) {
        removerToken();
        window.location.href = "/loja/catalogo";
        return { ok: false, status: 401, dados: null };
    }
    const dados = await resposta.json().catch(() => null);

    return {
        ok: resposta.ok,
        status: resposta.status,
        dados: dados
    };
}

// ===== Utilitários de exibição =====

// Escapa texto antes de colocá-lo dentro de innerHTML (nomes, descrições, mensagens).
function escaparHtml(valor) {
    return String(valor ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

// 1234.5 -> "R$ 1.234,50"
function formatarPreco(valor) {
    return "R$ " + Number(valor || 0).toLocaleString("pt-BR", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

// Estimativa do preço com promoção, igual à regra do servidor (o valor cobrado é sempre o do servidor).
function calcularPrecoFinal(produto) {
    const preco = Math.max(0, produto.produtoPrecoVenda || 0);
    const promocao = produto.produtoPromocao || 0;
    const final = promocao > 0 && promocao <= 100 ? preco - preco * (promocao / 100) : preco;
    return Math.round(final * 100) / 100;
}

function urlImagemProduto(caminho) {
    if (!caminho) return "/img/polaria.png";

    if (/^https?:\/\//i.test(caminho)) return caminho;

    return "/" + caminho;
}

// ===== Janela de confirmação e avisos da loja (substituem confirm() e alert() do navegador) =====

// Abre uma janela de confirmação. Retorna uma Promise: true se confirmou, false se cancelou.
function confirmarLoja({ titulo, mensagem, textoConfirmar = "Confirmar", textoCancelar = "Voltar", perigo = false }) {
    return new Promise(resolve => {
        // Só uma confirmação por vez: se já houver uma aberta, ignora o novo pedido
        if (document.querySelector(".dialogo-fundo")) {
            resolve(false);
            return;
        }

        const focoAnterior = document.activeElement;

        const fundo = document.createElement("div");
        fundo.className = "dialogo-fundo";
        fundo.innerHTML = `
            <div class="dialogo" role="dialog" aria-modal="true" aria-labelledby="dialogoTitulo" aria-describedby="dialogoMensagem">
                <h2 id="dialogoTitulo" class="dialogo-titulo"></h2>
                <p id="dialogoMensagem" class="dialogo-mensagem"></p>
                <div class="dialogo-botoes">
                    <button type="button" class="dialogo-botao dialogo-cancelar"></button>
                    <button type="button" class="dialogo-botao dialogo-confirmar"></button>
                </div>
            </div>`;

        fundo.querySelector(".dialogo-titulo").textContent = titulo;
        fundo.querySelector(".dialogo-mensagem").textContent = mensagem;

        const btnCancelar = fundo.querySelector(".dialogo-cancelar");
        const btnConfirmar = fundo.querySelector(".dialogo-confirmar");
        btnCancelar.textContent = textoCancelar;
        btnConfirmar.textContent = textoConfirmar;
        if (perigo) btnConfirmar.classList.add("perigo");

        document.body.appendChild(fundo);
        btnCancelar.focus();

        function fechar(resposta) {
            window.removeEventListener("keydown", teclado, true);
            fundo.remove();
            if (focoAnterior && typeof focoAnterior.focus === "function") focoAnterior.focus();
            resolve(resposta);
        }

        // Esc cancela; Tab alterna só entre os dois botões. Captura antes dos outros atalhos da página.
        function teclado(e) {
            if (e.key === "Escape") {
                e.preventDefault();
                e.stopImmediatePropagation();
                fechar(false);
            } else if (e.key === "Tab") {
                e.preventDefault();
                (document.activeElement === btnCancelar ? btnConfirmar : btnCancelar).focus();
            }
        }

        window.addEventListener("keydown", teclado, true);
        btnCancelar.addEventListener("click", () => fechar(false));
        btnConfirmar.addEventListener("click", () => fechar(true));
        fundo.addEventListener("click", e => { if (e.target === fundo) fechar(false); });
    });
}

// Mostra um aviso no canto da tela que some sozinho. tipo: "erro" (padrão) ou "sucesso".
function avisoLoja(mensagem, tipo = "erro") {
    let area = document.getElementById("areaAvisos");
    if (!area) {
        area = document.createElement("div");
        area.id = "areaAvisos";
        area.className = "avisos-flutuantes";
        document.body.appendChild(area);
    }

    const aviso = document.createElement("div");
    aviso.className = "aviso-flutuante aviso-flutuante-" + tipo;
    aviso.setAttribute("role", tipo === "erro" ? "alert" : "status");

    const icone = document.createElement("i");
    icone.className = "bi " + (tipo === "sucesso" ? "bi-check-circle" : "bi-exclamation-circle");

    const texto = document.createElement("span");
    texto.textContent = mensagem;

    aviso.append(icone, texto);
    area.appendChild(aviso);

    setTimeout(() => aviso.classList.add("saindo"), 4000);
    setTimeout(() => aviso.remove(), 4400);
}
