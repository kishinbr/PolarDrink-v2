let categoriaAtiva = null;
let produtosCarregados = [];
document.addEventListener("DOMContentLoaded", async function () {
    await carregarCategorias();
    await carregarProdutos();
});
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