let categoriaAtiva = null;
let produtosCarregados = [];

document.addEventListener("DOMContentLoaded", async function () {
    await carregarCategorias();
    await carregarProdutos();
});

async function carregarCategorias() {
    const resultado = await chamarApi("/api/catalogo/categorias");

    if (!resultado.ok) return;

    const container = document.getElementById("listaCategorias");
    container.innerHTML = "";

    const btnTodas = document.createElement("button");
    btnTodas.innerHTML = '<i class="bi bi-grid"></i> Todas';
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
        grid.innerHTML = `
            <div class="sem-produtos">
                <i class="bi bi-box-seam" style="font-size:40px;"></i>
                <h5 class="mt-3">Nenhum produto encontrado</h5>
                <p>Tente buscar por outro produto ou categoria.</p>
            </div>
        `;
        return;
    }

    resultado.dados.forEach(produto => {
        const card = document.createElement("div");
        card.className = "produto-card";

        const imagemUrl = produto.produtoImagemUrl
            ? "/" + produto.produtoImagemUrl
            : "/images/placeholder.jpg";

        const precoFinal = produto.produtoPromocao > 0
            ? produto.produtoPrecoVenda - (produto.produtoPrecoVenda * (produto.produtoPromocao / 100))
            : produto.produtoPrecoVenda;

        const possuiPromocao = produto.produtoPromocao > 0;

        let selo = "";
        let botao = "";

        if (produto.produtoQtdEstoque <= 0) {
            selo = `<div style="color:#f87171;font-size:12px;font-weight:bold;">Esgotado</div>`;
        } else if (produto.estoqueDisponivelOnline <= 0) {
            selo = `<div style="color:#f87171;font-size:12px;font-weight:bold;">Disponível apenas na loja física</div>`;
        } else if (produto.estoqueDisponivelOnline <= 3) {
            selo = `<div style="color:#facc15;font-size:12px;font-weight:bold;">Últimas ${produto.estoqueDisponivelOnline} unidades</div>`;
        }

        if (produto.produtoQtdEstoque > 0 && produto.estoqueDisponivelOnline > 0) {
            botao = `
                <button class="btn-produto btn-adicionar" data-produto-id="${produto.produtoID}">
                    <i class="bi bi-cart-plus"></i> Adicionar
                </button>
            `;
        }

        let precoHtml = "";

        if (possuiPromocao) {
            precoHtml = `
                <div class="produto-preco" style="display:flex;align-items:baseline;gap:7px;white-space:nowrap;">
                    <span style="font-size:12px;color:#7f8b96;text-decoration:line-through;">
                        R$ ${produto.produtoPrecoVenda.toFixed(2).replace(".", ",")}
                    </span>
                    <span>
                        R$ ${precoFinal.toFixed(2).replace(".", ",")}
                    </span>
                    <span style="font-size:12px;color:#7dd3fc;font-weight:bold;">
                        ${produto.produtoPromocao}% OFF
                    </span>
                </div>
            `;
        } else {
            precoHtml = `
                <div class="produto-preco">
                    R$ ${precoFinal.toFixed(2).replace(".", ",")}
                </div>
            `;
        }

        card.innerHTML = `
            <img src="${imagemUrl}" alt="${produto.produtoNome}" class="produto-imagem" />

            <div class="produto-info">
                <div class="produto-nome">${produto.produtoNome}</div>

                <div style="height:18px;margin-bottom:3px;display:flex;align-items:center;">
                    ${selo}
                </div>

                ${precoHtml}

                ${botao}
            </div>
        `;

        grid.appendChild(card);
    });

    document.querySelectorAll(".btn-adicionar").forEach(botao => {
        botao.addEventListener("click", async function () {
            await adicionarAoCarrinho(parseInt(botao.dataset.produtoId));

            const textoOriginal = botao.innerHTML;
            botao.innerHTML = '<i class="bi bi-check-lg"></i> Adicionado';
            botao.disabled = true;

            setTimeout(() => {
                botao.innerHTML = textoOriginal;
                botao.disabled = false;
            }, 900);
        });
    });
}

async function adicionarAoCarrinho(produtoId) {
    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho/itens", "POST", {
            produtoID: produtoId,
            quantidade: 1
        });
        if (!resultado.ok) {
            alert(resultado.dados?.mensagem || "Não foi possível adicionar o produto ao carrinho.");
            return;
        }
    } else {
        const carrinhoLocal = JSON.parse(localStorage.getItem("carrinho_local") || "[]");

        const itemExistente = carrinhoLocal.find(i => i.produtoID === produtoId);

        if (itemExistente) {
            itemExistente.quantidade += 1;
        } else {
            carrinhoLocal.push({
                produtoID: produtoId,
                quantidade: 1
            });
        }

        localStorage.setItem("carrinho_local", JSON.stringify(carrinhoLocal));
    }

    await atualizarContadorCarrinho();
}

let timeoutBusca = null;

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

async function buscarSugestoes(texto) {
    const resultado = await chamarApi(
        "/api/catalogo/produtos?termo=" + encodeURIComponent(texto)
    );

    if (!resultado.ok) return;

    const dropdown = document.getElementById("dropdownSugestoes");
    dropdown.innerHTML = "";

    if (resultado.dados.length === 0) {
        dropdown.style.display = "none";
        return;
    }

    resultado.dados.slice(0, 6).forEach(produto => {
        const item = document.createElement("div");

        item.innerHTML = `
    < i class="bi bi-search" ></i >
        ${ produto.produtoNome }
`;

        item.addEventListener("click", function () {
            document.getElementById("campoBusca").value = produto.produtoNome;
            dropdown.style.display = "none";
            carregarProdutos(produto.produtoNome);
        });

        dropdown.appendChild(item);
    });

    dropdown.style.display = "block";
}

document.addEventListener("click", function (e) {
    if (!e.target.closest("#campoBusca") && !e.target.closest("#dropdownSugestoes")) {
        document.getElementById("dropdownSugestoes").style.display = "none";
    }
});

document.getElementById("campoBusca").addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        document.getElementById("dropdownSugestoes").style.display = "none";
        carregarProdutos(this.value.trim());
    }
});

