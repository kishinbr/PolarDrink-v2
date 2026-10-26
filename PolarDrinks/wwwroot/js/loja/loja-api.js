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
    const dados = await resposta.json().catch(() => null);

    return {
        ok: resposta.ok,
        status: resposta.status,
        dados: dados
    };
}