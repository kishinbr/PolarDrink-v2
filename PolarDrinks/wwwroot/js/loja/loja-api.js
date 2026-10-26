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
    return caminho ? "/" + caminho : "/img/polaria.png";
}