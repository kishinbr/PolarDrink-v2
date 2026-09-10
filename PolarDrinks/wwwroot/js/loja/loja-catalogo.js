let categoriaAtiva = null;
let produtosCarregados = [];
document.addEventListener("DOMContentLoaded", async function () {
    atualizarLinkLogin();
    await carregarCategorias();
    await carregarProdutos();
    await atualizarContadorCarrinho();
});
// Função para verificar se o usuário está logado
function atualizarLinkLogin() {
    const linkLogin = document.getElementById("linkLogin");

    if (estaLogado()) {
        linkLogin.innerText = "Minha Conta";
        linkLogin.href = "/loja/perfil";
    } else {
        linkLogin.innerText = "Login";
        linkLogin.href = "/loja/conta";
    }
}
// Função para carregar categorias e criar botões dinamicamente
async function carregarCategorias() {
    const resultado = await chamarApi("/api/catalogo/categorias");

    if (!resultado.ok) return;

    const container = document.getElementById("listaCategorias");
    container.innerHTML = "";

    const btnTodas = document.createElement("button");
    btnTodas.innerText = "Todas";
    btnTodas.addEventListener("click", function () {
        categoriaAtiva = null;
        carregarProdutos();
    });
    container.appendChild(btnTodas);

    resultado.dados.forEach(categoria => {
        const btn = document.createElement("button");
        btn.innerText = categoria.categoriaNome;
        btn.addEventListener("click", function () {
            categoriaAtiva = categoria.categoriaID;
            carregarProdutos();
        });
        container.appendChild(btn);
    });
}
// Função para carregar produtos com base na categoria ativa e no termo de busca
async function carregarProdutos(termo = null) {
    let url = "/api/catalogo/produtos?";

    if (termo) {
        url += "termo=" + encodeURIComponent(termo) + "&";
    }

    if (categoriaAtiva) {
        url += "categoriaId=" + categoriaAtiva;
    }

    const resultado = await chamarApi(url);

    if (!resultado.ok) return;
    produtosCarregados = resultado.dados;
    const grid = document.getElementById("gridProdutos");
    grid.innerHTML = "";

    if (resultado.dados.length === 0) {
        grid.innerHTML = "<p>Nenhum produto encontrado.</p>";
        return;
    }

    resultado.dados.forEach(produto => {
        const card = document.createElement("div");
        card.style.border = "1px solid #ccc";
        card.style.padding = "10px";

        const imagemUrl = produto.produtoImagemUrl
            ? "/" + produto.produtoImagemUrl
            : "/images/placeholder.jpg";

        const precoFinal = produto.produtoPromocao > 0
            ? produto.produtoPrecoVenda - (produto.produtoPrecoVenda * (produto.produtoPromocao / 100))
            : produto.produtoPrecoVenda;

        card.innerHTML = `
            <img src="${imagemUrl}" alt="${produto.produtoNome}" style="width:100%; height:120px; object-fit:cover;" />
            <p>${produto.produtoNome}</p>
            <p><strong>R$ ${precoFinal.toFixed(2)}</strong></p>
            <button class="btn-adicionar" data-produto-id="${produto.produtoID}">Adicionar ao carrinho</button>
        `;

        grid.appendChild(card);
    });

    document.querySelectorAll(".btn-adicionar").forEach(botao => {
        botao.addEventListener("click", function () {
            adicionarAoCarrinho(parseInt(botao.dataset.produtoId));
        });
    });
}
// Função para adicionar produto ao carrinho
async function adicionarAoCarrinho(produtoId) {
    if (estaLogado()) {
        await chamarApi("/api/carrinho/itens", "POST", {
            produtoID: produtoId,
            quantidade: 1
        });
    } else {
        const carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");

        const itemExistente = carrinhoLocal.find(i => i.produtoID === produtoId);

        if (itemExistente) {
            itemExistente.quantidade += 1;
        } else {
            carrinhoLocal.push({ produtoID: produtoId, quantidade: 1 });
        }

        localStorage.setItem("carrinho_local", JSON.stringify(carrinhoLocal));
    }

    await atualizarContadorCarrinho();
}
// Função para atualizar o contador do carrinho
async function atualizarContadorCarrinho() {
    let quantidade = 0;

    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho");
        if (resultado.ok) {
            quantidade = resultado.dados.itens.reduce((total, item) => total + item.quantidade, 0);
        }
    } else {
        const carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");
        quantidade = carrinhoLocal.reduce((total, item) => total + item.quantidade, 0);
    }

    const contador = document.getElementById("contadorCarrinho");

    if (quantidade > 0) {
        contador.innerText = quantidade;
        contador.style.display = "inline";
    } else {
        contador.style.display = "none";
    }
}
// Abrir o sidecart ao clicar no botão
document.getElementById("btnAbrirSidecart").addEventListener("click", async function () {
    await abrirSidecart();
});
// Fechar o sidecart ao clicar no botão de fechar
document.getElementById("btnFecharSidecart").addEventListener("click", function () {
    fecharSidecart();
});
// Fechar o sidecart ao clicar fora dele
document.getElementById("sidecartOverlay").addEventListener("click", function () {
    fecharSidecart();
});
// Função para fechar o sidecart
function fecharSidecart() {
    document.getElementById("sidecart").style.display = "none";
    document.getElementById("sidecartOverlay").style.display = "none";
}
// Função para abrir o sidecart e carregar os itens
async function abrirSidecart() {
    document.getElementById("sidecart").style.display = "block";
    document.getElementById("sidecartOverlay").style.display = "block";

    const container = document.getElementById("sidecartItens");
    container.innerHTML = "Carregando...";

    let itens = [];
    let total = 0;

    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho");
        if (resultado.ok) {
            itens = resultado.dados.itens;
            total = resultado.dados.total;
        }
    } else {
        const carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");

        itens = carrinhoLocal.map(i => {
            const produto = produtosCarregados.find(p => p.produtoID === i.produtoID);
            const precoUnitario = produto
                ? (produto.produtoPromocao > 0
                    ? produto.produtoPrecoVenda - (produto.produtoPrecoVenda * (produto.produtoPromocao / 100))
                    : produto.produtoPrecoVenda)
                : 0;

            return {
                produtoID: i.produtoID,
                produtoNome: produto ? produto.produtoNome : "Produto #" + i.produtoID,
                quantidade: i.quantidade,
                subtotal: precoUnitario * i.quantidade
            };
        });

        total = itens.reduce((soma, item) => soma + item.subtotal, 0);
    }

    container.innerHTML = "";

    if (itens.length === 0) {
        container.innerHTML = "<p>Seu carrinho está vazio.</p>";
    } else {
        itens.forEach(item => {
            const div = document.createElement("div");
            div.innerHTML = `
                <p>${item.quantidade}x ${item.produtoNome} - R$ ${item.subtotal.toFixed(2)}
                    <button class="btn-remover-sidecart" data-produto-id="${item.produtoID}">Remover</button>
                </p>
            `;
            container.appendChild(div);
        });

        document.querySelectorAll(".btn-remover-sidecart").forEach(botao => {
            botao.addEventListener("click", async function () {
                await removerDoSidecart(parseInt(botao.dataset.produtoId));
            });
        });
    }

    document.getElementById("sidecartTotal").innerText = "R$ " + total.toFixed(2);
}
// Função para remover item do sidecart
async function removerDoSidecart(produtoId) {
    if (estaLogado()) {
        await chamarApi("/api/carrinho/itens/" + produtoId, "DELETE");
    } else {
        let carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");
        carrinhoLocal = carrinhoLocal.filter(i => i.produtoID !== produtoId);
        localStorage.setItem("carrinho_local", JSON.stringify(carrinhoLocal));
    }

    await abrirSidecart();
    await atualizarContadorCarrinho();
}
// Função para prosseguir para o carrinho ou login
document.getElementById("btnProsseguir").addEventListener("click", function () {
    if (estaLogado()) {
        window.location.href = "/loja/carrinho";
    } else {
        window.location.href = "/loja/conta?redirecionarPara=carrinho";
    }
});
let timeoutBusca = null;
// Função para buscar sugestões de produtos enquanto o usuário digita
document.getElementById("campoBusca").addEventListener("input", function () {
    const texto = this.value.trim();

    clearTimeout(timeoutBusca);

    if (texto.length < 3) {
        document.getElementById("dropdownSugestoes").style.display = "none";
        return;
    }

    timeoutBusca = setTimeout(async function () {
        await buscarSugestoes(texto);
    }, 400);
});
// Função para buscar sugestões de produtos com base no texto digitado
async function buscarSugestoes(texto) {
    const resultado = await chamarApi("/api/catalogo/produtos?termo=" + encodeURIComponent(texto));

    if (!resultado.ok) return;

    const dropdown = document.getElementById("dropdownSugestoes");
    dropdown.innerHTML = "";

    if (resultado.dados.length === 0) {
        dropdown.style.display = "none";
        return;
    }

    resultado.dados.slice(0, 6).forEach(produto => {
        const item = document.createElement("div");
        item.style.padding = "5px";
        item.style.cursor = "pointer";
        item.innerText = produto.produtoNome;

        item.addEventListener("click", function () {
            document.getElementById("campoBusca").value = produto.produtoNome;
            dropdown.style.display = "none";
            carregarProdutos(produto.produtoNome);
        });

        dropdown.appendChild(item);
    });

    dropdown.style.display = "block";
}
// Fechar o dropdown de sugestões ao clicar fora dele
document.addEventListener("click", function (e) {
    if (!e.target.closest("#campoBusca") && !e.target.closest("#dropdownSugestoes")) {
        document.getElementById("dropdownSugestoes").style.display = "none";
    }
});
// Executar a busca ao pressionar Enter
document.getElementById("campoBusca").addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        document.getElementById("dropdownSugestoes").style.display = "none";
        carregarProdutos(this.value.trim());
    }
});