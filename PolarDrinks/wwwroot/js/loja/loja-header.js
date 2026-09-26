document.addEventListener("DOMContentLoaded", async function () {
    atualizarLinkLogin();
    await atualizarContadorCarrinho();
});

function atualizarLinkLogin() {
    const linkLogin = document.getElementById("linkLogin");
    const menuConta = document.getElementById("menuConta");

    if (estaLogado()) {
        linkLogin.innerHTML = '<i class="bi bi-person-check"></i><span>Minha Conta</span>';
        linkLogin.href = "#";

        linkLogin.addEventListener("click", function (e) {
            e.preventDefault();
            menuConta.style.display = menuConta.style.display === "none" ? "block" : "none";
        });
    } else {
        linkLogin.innerHTML = '<i class="bi bi-person"></i><span>Login</span>';
        linkLogin.href = "/loja/conta";
    }
}

document.addEventListener("click", function (e) {
    const areaConta = document.getElementById("areaConta");

    if (!areaConta.contains(e.target)) {
        document.getElementById("menuConta").style.display = "none";
    }
});

document.getElementById("btnSairMenu").addEventListener("click", function (e) {
    e.preventDefault();

    const confirmar = confirm("Deseja realmente sair da sua conta?");

    if (!confirmar) return;

    removerToken();
    window.location.href = "/loja/catalogo";
});

async function atualizarContadorCarrinho() {
    let quantidade = 0;

    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho");

        if (resultado.ok) {
            quantidade = resultado.dados.itens.reduce(
                (total, item) => total + item.quantidade,
                0
            );
        }
    } else {
        const carrinhoLocal = JSON.parse(
            localStorage.getItem("carrinho_local") || "[]"
        );

        quantidade = carrinhoLocal.reduce(
            (total, item) => total + item.quantidade,
            0
        );
    }

    const contador = document.getElementById("contadorCarrinho");

    if (quantidade > 0) {
        contador.innerText = quantidade;
        contador.style.display = "flex";
    } else {
        contador.style.display = "none";
    }
}

document.getElementById("btnAbrirSidecart").addEventListener("click", async function () {
    await abrirSidecart();
});

document.getElementById("btnFecharSidecart").addEventListener("click", function () {
    fecharSidecart();
});

document.getElementById("sidecartOverlay").addEventListener("click", function () {
    fecharSidecart();
});

function fecharSidecart() {
    document.getElementById("sidecart").style.display = "none";
    document.getElementById("sidecartOverlay").style.display = "none";
}

async function abrirSidecart() {
    document.getElementById("sidecart").style.display = "block";
    document.getElementById("sidecartOverlay").style.display = "block";

    const container = document.getElementById("sidecartItens");
    container.innerHTML = `
        <div class="text-center py-4" style="color:#8faabd;">
            <div class="spinner-border spinner-border-sm"></div>
            <div class="mt-2">Carregando carrinho...</div>
        </div>
    `;

    let itens = [];
    let total = 0;

    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho");

        if (resultado.ok) {
            itens = resultado.dados.itens;
            total = resultado.dados.total;
        }
    } else {
        const carrinhoLocal = JSON.parse(
            localStorage.getItem("carrinho_local") || "[]"
        );

        itens = carrinhoLocal.map(i => {
            const produto = typeof produtosCarregados !== "undefined"
                ? produtosCarregados.find(p => p.produtoID === i.produtoID)
                : null;

            const precoUnitario = produto
                ? (produto.produtoPromocao > 0
                    ? produto.produtoPrecoVenda -
                    (produto.produtoPrecoVenda * (produto.produtoPromocao / 100))
                    : produto.produtoPrecoVenda)
                : 0;

            return {
                produtoID: i.produtoID,
                produtoNome: produto
                    ? produto.produtoNome
                    : "Produto #" + i.produtoID,
                quantidade: i.quantidade,
                subtotal: precoUnitario * i.quantidade
            };
        });

        total = itens.reduce(
            (soma, item) => soma + item.subtotal,
            0
        );
    }

    container.innerHTML = "";

    if (itens.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5" style="color:#8faabd;">
                <i class="bi bi-cart-x" style="font-size:42px;"></i>
                <p class="mt-3 mb-0">Seu carrinho está vazio.</p>
            </div>
        `;
    } else {
        itens.forEach(item => {
            const div = document.createElement("div");
            div.className = "sidecart-item";

            div.innerHTML = `
                <div class="d-flex justify-content-between align-items-start gap-2">
                    <div>
                        <div style="color:#edf8ff;font-weight:600;">
                            ${item.produtoNome}
                        </div>
                        <small style="color:#8faabd;">
                            ${item.quantidade}x
                            R$ ${(item.subtotal / item.quantidade).toFixed(2).replace(".", ",")}
                        </small>
                    </div>

                    <strong style="color:#7dd3fc;white-space:nowrap;">
                        R$ ${item.subtotal.toFixed(2).replace(".", ",")}
                    </strong>
                </div>

                <button class="btn-remover-sidecart mt-2"
                        data-produto-id="${item.produtoID}">
                    <i class="bi bi-trash3"></i> Remover
                </button>
            `;

            container.appendChild(div);
        });

        document.querySelectorAll(".btn-remover-sidecart").forEach(botao => {
            botao.addEventListener("click", async function () {
                await removerDoSidecart(
                    parseInt(botao.dataset.produtoId)
                );
            });
        });
    }

    document.getElementById("sidecartTotal").innerText =
        "R$ " + total.toFixed(2).replace(".", ",");
}

async function removerDoSidecart(produtoId) {
    if (estaLogado()) {
        await chamarApi(
            "/api/carrinho/itens/" + produtoId,
            "DELETE"
        );
    } else {
        let carrinhoLocal = JSON.parse(
            localStorage.getItem("carrinho_local") || "[]"
        );

        carrinhoLocal = carrinhoLocal.filter(
            i => i.produtoID !== produtoId
        );

        localStorage.setItem(
            "carrinho_local",
            JSON.stringify(carrinhoLocal)
        );
    }

    await abrirSidecart();
    await atualizarContadorCarrinho();
}

document.getElementById("btnProsseguir").addEventListener("click", function () {
    if (estaLogado()) {
        window.location.href = "/loja/carrinho";
    } else {
        window.location.href = "/loja/conta?redirecionarPara=carrinho";
    }
});