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
    mensagemErro.style.display = "block";
}
document.getElementById("btnLogin").addEventListener("click", async function () {
    const email = document.getElementById("loginEmail").value;
    const senha = document.getElementById("loginSenha").value;

    const resultado = await chamarApi("/api/cliente/auth/login", "POST", {
        email: email,
        senha: senha
    });

    if (!resultado.ok) {
        mostrarErro(resultado.dados?.mensagem || "Erro ao entrar.");
        return;
    }

    salvarToken(resultado.dados.token);

    await mesclarCarrinhoLocal();

    redirecionarAposLogin();
});

async function mesclarCarrinhoLocal() {
    const carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");

    if (carrinhoLocal.length > 0) {
        await chamarApi("/api/carrinho/mesclar", "POST", carrinhoLocal);
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
    const dados = {
        nome: document.getElementById("cadNome").value,
        email: document.getElementById("cadEmail").value,
        senha: document.getElementById("cadSenha").value,
        confirmacaoSenha: document.getElementById("cadConfirmacaoSenha").value,
        telefone: document.getElementById("cadTelefone").value,
        cpf: document.getElementById("cadCpf").value
    };

    const resultado = await chamarApi("/api/cliente/auth/cadastrar", "POST", dados);

    if (!resultado.ok) {
        mostrarErro(resultado.dados?.mensagem || "Erro ao criar conta.");
        return;
    }

    salvarToken(resultado.dados.token);

    await mesclarCarrinhoLocal();

    redirecionarAposLogin();
});