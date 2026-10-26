// Cabeçalho da loja: faixa de aviso, conta, busca, menu de categorias e carrinho lateral.
// Usado em todas as páginas da loja (carregado pelo _LayoutLoja).

// Promessa compartilhada: o catálogo reaproveita as categorias já pedidas aqui.
const categoriasLoja = chamarApi("/api/catalogo/categorias")
    .then(r => (r.ok && Array.isArray(r.dados) ? r.dados : []))
    .catch(() => []);

// Altura do cabeçalho fixo em --altura-header (usada pelo filtro lateral e pelo resumo do carrinho)
function medirCabecalho() {
    const header = document.querySelector(".loja-header");
    if (!header) return;

    const atualizar = () => document.documentElement.style.setProperty("--altura-header", header.offsetHeight + "px");
    atualizar();

    if ("ResizeObserver" in window) {
        new ResizeObserver(atualizar).observe(header);
    }
}

document.addEventListener("DOMContentLoaded", async function () {
    medirCabecalho();
    iniciarTema();
    iniciarFaixaAviso();
    atualizarLinkLogin();
    iniciarBusca();
    await montarMenuCategorias();
    await atualizarContadorCarrinho();
});
function iniciarTema() {
    const botao = document.getElementById("btnTema");
    if (!botao) return;

    atualizarBotaoTema();

    botao.addEventListener("click", function () {
        const escuro = document.documentElement.dataset.tema !== "escuro";

        if (escuro) {
            document.documentElement.dataset.tema = "escuro";
        } else {
            delete document.documentElement.dataset.tema;
        }

        try {
            localStorage.setItem("tema_loja", escuro ? "escuro" : "claro");
        } catch { }

        atualizarBotaoTema();
    });
}

function atualizarBotaoTema() {
    const botao = document.getElementById("btnTema");
    const escuro = document.documentElement.dataset.tema === "escuro";

    botao.innerHTML = escuro ? '<i class="bi bi-sun"></i>' : '<i class="bi bi-moon"></i>';
    botao.title = escuro ? "Tema claro" : "Tema escuro";
    botao.setAttribute("aria-label", escuro ? "Ativar tema claro" : "Ativar tema escuro");
}
// ===== Faixa de aviso =====

function iniciarFaixaAviso() {
    const mensagens = [
        "Peça online e retire gelado na loja",
        "Pagamento por Pix ou cartão",
        "Retire em até 24 horas após a separação"
    ];

    const texto = document.getElementById("textoFaixaAviso");
    if (!texto) return;

    let indice = 0;

    setInterval(function () {
        texto.style.opacity = "0";

        setTimeout(function () {
            indice = (indice + 1) % mensagens.length;
            texto.textContent = mensagens[indice];
            texto.style.opacity = "1";
        }, 350);
    }, 4500);
}

// ===== Conta =====

function atualizarLinkLogin() {
    const linkLogin = document.getElementById("linkLogin");
    const menuConta = document.getElementById("menuConta");

    if (estaLogado()) {
        linkLogin.innerHTML = '<i class="bi bi-person-check"></i>';
        linkLogin.href = "#";
        linkLogin.title = "Minha conta";
        linkLogin.setAttribute("aria-label", "Minha conta");

        linkLogin.addEventListener("click", function (e) {
            e.preventDefault();
            menuConta.style.display = menuConta.style.display === "none" ? "block" : "none";
        });
    } else {
        linkLogin.innerHTML = '<i class="bi bi-person"></i>';
        linkLogin.href = "/loja/conta";
        linkLogin.title = "Entrar";
        linkLogin.setAttribute("aria-label", "Entrar");
    }
}

document.addEventListener("click", function (e) {
    const areaConta = document.getElementById("areaConta");

    if (areaConta && !areaConta.contains(e.target)) {
        document.getElementById("menuConta").style.display = "none";
    }
});

document.getElementById("btnSairMenu").addEventListener("click", function (e) {
    e.preventDefault();

    if (!confirm("Deseja realmente sair da sua conta?")) return;

    removerToken();
    window.location.href = "/loja/catalogo";
});

// ===== Menu de categorias =====

async function montarMenuCategorias() {
    const menu = document.getElementById("menuCategorias");
    if (!menu) return;

    const categorias = await categoriasLoja;

    categorias.forEach(categoria => {
        const link = document.createElement("a");
        link.href = "/loja/catalogo?categoria=" + categoria.categoriaID;
        link.dataset.categoria = categoria.categoriaID;
        link.textContent = categoria.categoriaNome;
        menu.appendChild(link);
    });

    menu.querySelectorAll("a").forEach(link => {
        link.addEventListener("click", function (e) {
            // Na página do catálogo, filtra sem recarregar
            if (typeof filtrarCatalogo === "function") {
                e.preventDefault();
                const id = link.dataset.categoria ? parseInt(link.dataset.categoria) : null;
                filtrarCatalogo({ categoriaId: id, termo: null });
            }
        });
    });

    ligarSetasMenu();

    const params = new URLSearchParams(window.location.search);
    marcarCategoriaAtiva(params.get("categoria") ? parseInt(params.get("categoria")) : null);
}

function marcarCategoriaAtiva(categoriaId) {
    const naVitrine = window.location.pathname.toLowerCase().startsWith("/loja/catalogo");

    document.querySelectorAll("#menuCategorias a").forEach(link => {
        const id = link.dataset.categoria ? parseInt(link.dataset.categoria) : null;
        const ativo = naVitrine && id === categoriaId;
        link.classList.toggle("ativo", ativo);

        // Se a categoria ativa estiver fora da área visível do menu, rola até ela
        if (ativo) {
            const lista = document.getElementById("menuCategorias");
            const inicio = link.offsetLeft - lista.offsetLeft;
            const fim = inicio + link.offsetWidth;

            if (inicio < lista.scrollLeft || fim > lista.scrollLeft + lista.clientWidth) {
                lista.scrollTo({ left: inicio - 48, behavior: "smooth" });
            }
        }
    });
}

// Setas do menu: aparecem só quando há categorias escondidas para aquele lado
function ligarSetasMenu() {
    const trilho = document.getElementById("trilhoCategorias");
    const lista = document.getElementById("menuCategorias");
    if (!trilho || !lista) return;

    const atualizar = () => {
        const max = lista.scrollWidth - lista.clientWidth;
        trilho.classList.toggle("pode-voltar", lista.scrollLeft > 4);
        trilho.classList.toggle("pode-avancar", lista.scrollLeft < max - 4);
    };

    trilho.querySelectorAll(".menu-seta").forEach(seta => {
        seta.addEventListener("click", () => {
            const direcao = parseInt(seta.dataset.direcao);
            lista.scrollBy({ left: direcao * lista.clientWidth * 0.7, behavior: "smooth" });
        });
    });

    // Roda do mouse na vertical também rola o menu na horizontal
    lista.addEventListener("wheel", e => {
        if (Math.abs(e.deltaY) > Math.abs(e.deltaX) && lista.scrollWidth > lista.clientWidth) {
            e.preventDefault();
            lista.scrollLeft += e.deltaY;
        }
    }, { passive: false });

    lista.addEventListener("scroll", atualizar, { passive: true });
    window.addEventListener("resize", atualizar);
    atualizar();
}

// ===== Busca =====

let timeoutBusca = null;

function iniciarBusca() {
    const area = document.getElementById("areaBusca");
    const campo = document.getElementById("campoBusca");
    const dropdown = document.getElementById("dropdownSugestoes");

    document.getElementById("btnAbrirBusca").addEventListener("click", function () {
        const abrir = area.style.display === "none";
        area.style.display = abrir ? "block" : "none";
        if (abrir) campo.focus();
    });

    const termoAtual = new URLSearchParams(window.location.search).get("termo");
    if (termoAtual) {
        campo.value = termoAtual;
        area.style.display = "block";
    }

    campo.addEventListener("input", function () {
        const texto = this.value.trim();

        clearTimeout(timeoutBusca);

        if (texto.length < 3) {
            dropdown.style.display = "none";
            return;
        }

        timeoutBusca = setTimeout(() => buscarSugestoes(texto), 350);
    });

    campo.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            dropdown.style.display = "none";
            executarBusca(this.value.trim());
        }

        if (e.key === "Escape") {
            dropdown.style.display = "none";
        }
    });

    document.addEventListener("click", function (e) {
        if (!e.target.closest("#campoBusca") && !e.target.closest("#dropdownSugestoes")) {
            dropdown.style.display = "none";
        }
    });
}

function executarBusca(termo) {
    if (typeof filtrarCatalogo === "function") {
        filtrarCatalogo({ categoriaId: null, termo: termo || null });
    } else {
        window.location.href = termo
            ? "/loja/catalogo?termo=" + encodeURIComponent(termo)
            : "/loja/catalogo";
    }
}

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
        const item = document.createElement("button");
        item.type = "button";
        item.innerHTML = `<i class="bi bi-search"></i> <span>${escaparHtml(produto.produtoNome)}</span>`;

        item.addEventListener("click", function () {
            document.getElementById("campoBusca").value = produto.produtoNome;
            dropdown.style.display = "none";
            executarBusca(produto.produtoNome);
        });

        dropdown.appendChild(item);
    });

    dropdown.style.display = "block";
}

// ===== Carrinho =====

function lerCarrinhoLocal() {
    try {
        return JSON.parse(localStorage.getItem("carrinho_local") || "[]");
    } catch {
        return [];
    }
}

async function atualizarContadorCarrinho() {
    let quantidade = 0;

    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho");

        if (resultado.ok) {
            quantidade = resultado.dados.itens.reduce((total, item) => total + item.quantidade, 0);
        }
    } else {
        quantidade = lerCarrinhoLocal().reduce((total, item) => total + item.quantidade, 0);
    }

    const contador = document.getElementById("contadorCarrinho");

    if (quantidade > 0) {
        contador.innerText = quantidade > 99 ? "99+" : quantidade;
        contador.style.display = "flex";
    } else {
        contador.style.display = "none";
    }
}

document.getElementById("btnAbrirSidecart").addEventListener("click", abrirSidecart);
document.getElementById("btnFecharSidecart").addEventListener("click", fecharSidecart);
document.getElementById("sidecartOverlay").addEventListener("click", fecharSidecart);

document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") fecharSidecart();
});

function fecharSidecart() {
    document.getElementById("sidecart").classList.remove("aberto");
    document.getElementById("sidecartOverlay").classList.remove("aberto");
    document.body.style.overflow = "";
}

// Itens do carrinho sem login: busca o produto no catálogo já carregado ou na API
async function montarItensLocais() {
    const carrinhoLocal = lerCarrinhoLocal();

    const carregados = typeof produtosCarregados !== "undefined" ? produtosCarregados : [];

    return Promise.all(carrinhoLocal.map(async i => {
        let produto = carregados.find(p => p.produtoID === i.produtoID);

        if (!produto) {
            const resultado = await chamarApi("/api/catalogo/produtos/" + i.produtoID);
            produto = resultado.ok ? resultado.dados : null;
        }

        const precoUnitario = produto ? calcularPrecoFinal(produto) : 0;

        return {
            produtoID: i.produtoID,
            produtoNome: produto ? produto.produtoNome : "Produto #" + i.produtoID,
            produtoImagemUrl: produto ? produto.produtoImagemUrl : null,
            quantidade: i.quantidade,
            precoUnitario: precoUnitario,
            subtotal: precoUnitario * i.quantidade
        };
    }));
}

async function abrirSidecart() {
    document.getElementById("sidecart").classList.add("aberto");
    document.getElementById("sidecartOverlay").classList.add("aberto");
    document.body.style.overflow = "hidden";

    const container = document.getElementById("sidecartItens");
    container.innerHTML = `
        <div class="carregando">
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
        itens = await montarItensLocais();
        total = itens.reduce((soma, item) => soma + item.subtotal, 0);
    }

    container.innerHTML = "";

    if (itens.length === 0) {
        container.innerHTML = `
            <div class="sidecart-vazio">
                <i class="bi bi-bag-x"></i>
                Seu carrinho está vazio.
            </div>
        `;
    } else {
        itens.forEach(item => {
            const precoUnitario = item.precoUnitario ?? item.subtotal / item.quantidade;

            const div = document.createElement("div");
            div.className = "sidecart-item";

            div.innerHTML = `
                <img class="sidecart-item-imagem" src="${escaparHtml(urlImagemProduto(item.produtoImagemUrl))}" alt="" />

                <div class="sidecart-item-info">
                    <div class="sidecart-item-nome">${escaparHtml(item.produtoNome)}</div>
                    <div class="sidecart-item-detalhe">${item.quantidade} × ${formatarPreco(precoUnitario)}</div>

                    <div class="sidecart-item-linha">
                        <button type="button" class="btn-remover-sidecart" data-produto-id="${item.produtoID}">Remover</button>
                        <strong>${formatarPreco(item.subtotal)}</strong>
                    </div>
                </div>
            `;

            container.appendChild(div);
        });

        container.querySelectorAll(".btn-remover-sidecart").forEach(botao => {
            botao.addEventListener("click", async function () {
                await removerDoSidecart(parseInt(botao.dataset.produtoId));
            });
        });
    }

    document.getElementById("sidecartTotal").innerText = formatarPreco(total);
    document.getElementById("btnProsseguir").disabled = itens.length === 0;
}

async function removerDoSidecart(produtoId) {
    if (estaLogado()) {
        await chamarApi("/api/carrinho/itens/" + produtoId, "DELETE");
    } else {
        const carrinhoLocal = lerCarrinhoLocal().filter(i => i.produtoID !== produtoId);
        localStorage.setItem("carrinho_local", JSON.stringify(carrinhoLocal));
    }

    await abrirSidecart();
    await atualizarContadorCarrinho();

    // Se estiver na página do carrinho, atualiza a lista também
    if (typeof carregarCarrinho === "function") {
        await carregarCarrinho();
    }
}

document.getElementById("btnProsseguir").addEventListener("click", function () {
    if (estaLogado()) {
        window.location.href = "/loja/carrinho";
    } else {
        window.location.href = "/loja/conta?redirecionarPara=carrinho";
    }
});
