const blocoLogin = document.getElementById("blocoLogin");
const blocoCadastro = document.getElementById("blocoCadastro");
const mensagemErro = document.getElementById("mensagemErro");

document.getElementById("linkIrCadastro").addEventListener("click", function (e) {
    e.preventDefault();

    blocoLogin.style.display = "none";
    blocoCadastro.style.display = "block";
    mensagemErro.style.display = "none";
});

document.getElementById("linkIrLogin").addEventListener("click", function (e) {
    e.preventDefault();

    blocoCadastro.style.display = "none";
    blocoLogin.style.display = "block";
    mensagemErro.style.display = "none";
});

function mostrarErro(texto) {
    mensagemErro.innerText = texto;
    mensagemErro.style.display = "flex";
}

// Enter envia o formulário visível
document.querySelectorAll("#blocoLogin input").forEach(campo => {
    campo.addEventListener("keydown", e => {
        if (e.key === "Enter") document.getElementById("btnLogin").click();
    });
});

document.querySelectorAll("#blocoCadastro input").forEach(campo => {
    campo.addEventListener("keydown", e => {
        if (e.key === "Enter") document.getElementById("btnCadastrar").click();
    });
});

// Já logado: não faz sentido mostrar o login
if (estaLogado()) {
    const destino = new URLSearchParams(window.location.search).get("redirecionarPara");
    window.location.href = destino === "carrinho" ? "/loja/carrinho" : "/loja/perfil";
}

document.getElementById("btnLogin").addEventListener("click", async function () {
    const email = document.getElementById("loginEmail").value;
    const senha = document.getElementById("loginSenha").value;
    const botao = this;

    botao.disabled = true;
    botao.innerHTML = `
        <span class="spinner-border spinner-border-sm"></span>
        Entrando...
    `;

    const resultado = await chamarApi("/api/cliente/auth/login", "POST", {
        email: email,
        senha: senha
    });

    if (!resultado.ok) {
        mostrarErro(resultado.dados?.mensagem || "Erro ao entrar.");

        botao.disabled = false;
        botao.innerHTML = `Entrar <i class="bi bi-arrow-right"></i>`;

        return;
    }

    salvarToken(resultado.dados.token);

    await mesclarCarrinhoLocal();

    redirecionarAposLogin();
});

async function mesclarCarrinhoLocal() {
    const carrinhoLocal = JSON.parse(
        localStorage.getItem("carrinho_local") || "[]"
    );

    if (carrinhoLocal.length > 0) {
        await chamarApi(
            "/api/carrinho/mesclar",
            "POST",
            carrinhoLocal
        );

        localStorage.removeItem("carrinho_local");
    }
}

function redirecionarAposLogin() {
    const params = new URLSearchParams(window.location.search);
    const redirecionarPara = params.get("redirecionarPara");

    if (redirecionarPara === "carrinho") {
        window.location.href = "/loja/carrinho";
    } else {
        window.location.href = "/loja/catalogo";
    }
}

document.getElementById("btnCadastrar").addEventListener("click", async function () {
    const botao = this;

    const dados = {
        nome: document.getElementById("cadNome").value,
        email: document.getElementById("cadEmail").value,
        senha: document.getElementById("cadSenha").value,
        confirmacaoSenha: document.getElementById("cadConfirmacaoSenha").value,
        telefone: document.getElementById("cadTelefone").value,
        cpf: document.getElementById("cadCpf").value
    };

    botao.disabled = true;
    botao.innerHTML = `
        <span class="spinner-border spinner-border-sm"></span>
        Criando conta...
    `;

    const resultado = await chamarApi(
        "/api/cliente/auth/cadastrar",
        "POST",
        dados
    );

    if (!resultado.ok) {
        mostrarErro(
            resultado.dados?.mensagem ||
            "Erro ao criar conta."
        );

        botao.disabled = false;
        botao.innerHTML = `Criar conta <i class="bi bi-arrow-right"></i>`;

        return;
    }

    salvarToken(resultado.dados.token);

    await mesclarCarrinhoLocal();

    redirecionarAposLogin();
});

// ===== Máscaras do cadastro: telefone (00) 00000-0000 e CPF 000.000.000-00 =====

function mascaraTelefone(valor) {
    let d = valor.replace(/\D/g, "");
    if (d.length > 11 && d.startsWith("55")) d = d.slice(2); // colado com +55
    d = d.slice(0, 11);

    if (d.length === 0) return "";
    if (d.length <= 2) return "(" + d;

    const ddd = "(" + d.slice(0, 2) + ") ";
    const numero = d.slice(2);
    const corte = d.length === 11 ? 5 : 4; // celular: 5 + 4 dígitos; fixo: 4 + 4

    if (numero.length <= corte) return ddd + numero;
    return ddd + numero.slice(0, corte) + "-" + numero.slice(corte);
}

function mascaraCpf(valor) {
    const d = valor.replace(/\D/g, "").slice(0, 11);

    return d
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d{1,2})$/, "$1-$2");
}

const campoTelefone = document.getElementById("cadTelefone");
const campoCpf = document.getElementById("cadCpf");

campoTelefone.addEventListener("input", function() {
    campoTelefone.value = mascaraTelefone(campoTelefone.value);
});

campoCpf.addEventListener("input", function() {
    campoCpf.value = mascaraCpf(campoCpf.value);
});