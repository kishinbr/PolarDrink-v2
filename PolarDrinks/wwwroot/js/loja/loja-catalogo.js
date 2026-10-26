// Página inicial / catálogo da loja.
// Carrega o catálogo uma vez e filtra no navegador (categoria, palavra-chave, preço,
// promoção, disponibilidade e ordenação). O endereço guarda ?categoria=, ?termo= e ?ordem=.
// O menu e a busca do cabeçalho chamam filtrarCatalogo().

let produtosCarregados = [];   // catálogo completo (o carrinho lateral também consulta esta lista)
let todasCategorias = [];

const filtros = {
    categoriaId: null,
    termo: null,
    precoMin: null,
    precoMax: null,
    somentePromocao: false,
    somenteDisponivel: false,
    ordem: "padrao"
};

// A vitrine (banner, categorias, FAQ) some quando há categoria ou uma busca feita pelo cabeçalho.
// Digitar a palavra-chave no painel lateral não esconde a vitrine, para a página não "pular".
let modoFocado = false;
let categoriasExpandidas = false;

document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    filtros.categoriaId = params.get("categoria") ? parseInt(params.get("categoria")) : null;
    filtros.termo = params.get("termo") || null;
    filtros.ordem = params.get("ordem") || "padrao";
    modoFocado = Boolean(filtros.categoriaId || filtros.termo);

    mostrarEsqueleto();
    ligarPainelFiltros();

    todasCategorias = await categoriasLoja;
    produtosCarregados = await buscarCatalogo();

    renderizarTilesCategorias(produtosCarregados);
    aplicarFiltros();
});

async function buscarCatalogo() {
    const resultado = await chamarApi("/api/catalogo/produtos");
    return resultado.ok && Array.isArray(resultado.dados) ? resultado.dados : [];
}

// ===== Entrada pública (menu, busca do cabeçalho, blocos de categoria) =====

function filtrarCatalogo({ categoriaId = null, termo = null } = {}) {
    filtros.categoriaId = categoriaId;
    filtros.termo = termo;
    filtros.precoMin = null;
    filtros.precoMax = null;
    modoFocado = Boolean(categoriaId || termo);

    if (!termo) {
        const campo = document.getElementById("campoBusca");
        if (campo) campo.value = "";
    }

    aplicarFiltros();

    if (categoriaId || termo) {
        document.getElementById("produtos").scrollIntoView({ behavior: "smooth", block: "start" });
    } else {
        window.scrollTo({ top: 0, behavior: "smooth" });
    }
}

// ===== Filtragem =====

function normalizar(texto) {
    return String(texto ?? "")
        .normalize("NFD")
        .replace(/[̀-ͯ]/g, "")
        .toLowerCase()
        .trim();
}

function nomeDaCategoria(id) {
    return todasCategorias.find(c => c.categoriaID === id)?.categoriaNome ?? null;
}

function estaDisponivelOnline(produto) {
    return produto.produtoQtdEstoque > 0 && produto.estoqueDisponivelOnline > 0;
}

// Produtos da categoria + palavra-chave: base para a faixa de preço e as contagens
function produtosNoEscopo() {
    const nomeCategoria = filtros.categoriaId ? nomeDaCategoria(filtros.categoriaId) : null;
    const termo = normalizar(filtros.termo);

    return produtosCarregados.filter(p =>
        (!nomeCategoria || (p.categorias || []).includes(nomeCategoria)) &&
        (!termo || normalizar(p.produtoNome).includes(termo)));
}

function produtosFiltrados() {
    let lista = produtosNoEscopo().filter(p => {
        const preco = calcularPrecoFinal(p);

        if (filtros.precoMin !== null && preco < filtros.precoMin) return false;
        if (filtros.precoMax !== null && preco > filtros.precoMax) return false;
        if (filtros.somentePromocao && !(p.produtoPromocao > 0)) return false;
        if (filtros.somenteDisponivel && !estaDisponivelOnline(p)) return false;

        return true;
    });

    const porPreco = (a, b) => calcularPrecoFinal(a) - calcularPrecoFinal(b);

    switch (filtros.ordem) {
        case "menor-preco":
            lista = lista.slice().sort(porPreco);
            break;
        case "maior-preco":
            lista = lista.slice().sort((a, b) => porPreco(b, a));
            break;
        case "desconto":
            lista = lista.slice().sort((a, b) => (b.produtoPromocao || 0) - (a.produtoPromocao || 0));
            break;
        case "nome":
            lista = lista.slice().sort((a, b) => a.produtoNome.localeCompare(b.produtoNome, "pt-BR"));
            break;
        default:
            // Destaques: disponíveis primeiro, mantendo a ordem do catálogo
            lista = lista.slice().sort((a, b) => Number(estaDisponivelOnline(b)) - Number(estaDisponivelOnline(a)));
    }

    return lista;
}

function aplicarFiltros() {
    const lista = produtosFiltrados();

    renderizarProdutos(lista);
    atualizarPainelFiltros(lista.length);
    atualizarCabecalhoProdutos(lista.length);
    atualizarEndereco();
    ajustarPainelFixo();
}

// O painel acompanha a rolagem logo abaixo do cabeçalho. Se for mais alto que a janela,
// o "top" fica negativo: ele rola junto até o fim aparecer e então para, sem cortar nada.
function ajustarPainelFixo() {
    const lateral = document.querySelector(".filtros-lateral");
    if (!lateral) return;

    const alturaHeader = document.querySelector(".loja-header")?.offsetHeight ?? 0;
    const topo = Math.min(alturaHeader + 16, window.innerHeight - lateral.offsetHeight - 16);

    lateral.style.setProperty("--topo-filtros", topo + "px");
}

window.addEventListener("resize", ajustarPainelFixo);

function atualizarEndereco() {
    const params = new URLSearchParams();
    if (filtros.categoriaId) params.set("categoria", filtros.categoriaId);
    if (filtros.termo) params.set("termo", filtros.termo);
    if (filtros.ordem !== "padrao") params.set("ordem", filtros.ordem);

    const query = params.toString();
    history.replaceState(null, "", "/loja/catalogo" + (query ? "?" + query : ""));
}

function filtrosAtivos() {
    return Boolean(
        filtros.categoriaId || filtros.termo ||
        filtros.precoMin !== null || filtros.precoMax !== null ||
        filtros.somentePromocao || filtros.somenteDisponivel);
}

function atualizarCabecalhoProdutos(quantidade) {
    const focado = modoFocado || Boolean(filtros.categoriaId);
    document.getElementById("paginaCatalogo").classList.toggle("modo-filtro", focado);
    document.getElementById("btnVoltarTodos").style.display = focado ? "" : "none";

    const titulo = document.getElementById("tituloProdutos");
    const subtitulo = document.getElementById("subtituloProdutos");
    const textoQuantidade = quantidade === 1 ? "1 produto" : quantidade + " produtos";

    if (filtros.termo) {
        titulo.textContent = `Resultados para "${filtros.termo}"`;
    } else if (filtros.categoriaId) {
        titulo.textContent = nomeDaCategoria(filtros.categoriaId) ?? "Categoria";
    } else {
        titulo.textContent = "Nossos produtos";
    }

    subtitulo.textContent = focado || filtrosAtivos()
        ? textoQuantidade
        : "Preços com promoção já aplicada.";

    if (typeof marcarCategoriaAtiva === "function") {
        marcarCategoriaAtiva(filtros.termo ? -1 : filtros.categoriaId);
    }
}

// ===== Painel de filtros =====

let timeoutPalavra = null;

function ligarPainelFiltros() {
    const painel = document.getElementById("painelFiltros");
    const botaoAlternar = document.getElementById("btnAlternarFiltros");

    botaoAlternar.addEventListener("click", function () {
        const aberto = painel.classList.toggle("aberto");
        botaoAlternar.setAttribute("aria-expanded", aberto ? "true" : "false");
    });

    document.getElementById("filtroPalavra").addEventListener("input", function () {
        const valor = this.value.trim();
        clearTimeout(timeoutPalavra);

        timeoutPalavra = setTimeout(function () {
            filtros.termo = valor || null;

            const campoCabecalho = document.getElementById("campoBusca");
            if (campoCabecalho) campoCabecalho.value = valor;

            aplicarFiltros();
        }, 300);
    });

    const lerPreco = valor => {
        const numero = parseFloat(String(valor).replace(",", "."));
        return Number.isFinite(numero) && numero >= 0 ? numero : null;
    };

    ["filtroPrecoMin", "filtroPrecoMax"].forEach(id => {
        document.getElementById(id).addEventListener("input", function () {
            filtros.precoMin = lerPreco(document.getElementById("filtroPrecoMin").value);
            filtros.precoMax = lerPreco(document.getElementById("filtroPrecoMax").value);
            aplicarFiltros();
        });
    });

    document.getElementById("filtroPromocao").addEventListener("change", function () {
        filtros.somentePromocao = this.checked;
        aplicarFiltros();
    });

    document.getElementById("filtroDisponivel").addEventListener("change", function () {
        filtros.somenteDisponivel = this.checked;
        aplicarFiltros();
    });

    document.getElementById("filtroOrdem").addEventListener("change", function () {
        filtros.ordem = this.value;
        aplicarFiltros();
    });

    document.getElementById("btnLimparFiltros").addEventListener("click", function () {
        filtros.precoMin = null;
        filtros.precoMax = null;
        filtros.somentePromocao = false;
        filtros.somenteDisponivel = false;
        filtros.ordem = "padrao";
        filtrarCatalogo({ categoriaId: null, termo: null });
    });

    document.getElementById("btnVoltarTodos").addEventListener("click", function () {
        filtrarCatalogo({ categoriaId: null, termo: null });
    });
}

// Mantém os campos do painel iguais ao estado atual
function atualizarPainelFiltros(quantidade) {
    const palavra = document.getElementById("filtroPalavra");
    if (document.activeElement !== palavra) palavra.value = filtros.termo ?? "";

    const precoMin = document.getElementById("filtroPrecoMin");
    const precoMax = document.getElementById("filtroPrecoMax");
    if (document.activeElement !== precoMin) precoMin.value = filtros.precoMin ?? "";
    if (document.activeElement !== precoMax) precoMax.value = filtros.precoMax ?? "";

    document.getElementById("filtroPromocao").checked = filtros.somentePromocao;
    document.getElementById("filtroDisponivel").checked = filtros.somenteDisponivel;
    document.getElementById("filtroOrdem").value = filtros.ordem;

    renderizarOpcoesCategoria();

    // Faixa de preço da categoria/busca atual
    const escopo = produtosNoEscopo();
    const precos = escopo.map(calcularPrecoFinal);
    const info = document.getElementById("faixaPrecoInfo");

    if (precos.length > 0) {
        const onde = filtros.categoriaId ? "nesta categoria" : (filtros.termo ? "nesta busca" : "na loja");
        info.textContent = `De ${formatarPreco(Math.min(...precos))} a ${formatarPreco(Math.max(...precos))} ${onde}`;
        precoMin.placeholder = Math.floor(Math.min(...precos));
        precoMax.placeholder = Math.ceil(Math.max(...precos));
    } else {
        info.textContent = "Nenhum produto nesta seleção";
    }

    const textoQuantidade = quantidade === 1 ? "1 produto" : quantidade + " produtos";
    document.getElementById("contagemFiltros").textContent = textoQuantidade;
    document.getElementById("resumoFiltrosMobile").textContent = textoQuantidade;
    document.getElementById("btnLimparFiltros").style.visibility = filtrosAtivos() || filtros.ordem !== "padrao" ? "visible" : "hidden";
}

function renderizarOpcoesCategoria() {
    const container = document.getElementById("listaCategorias");
    container.innerHTML = "";

    const termo = normalizar(filtros.termo);
    const contar = nome => produtosCarregados.filter(p =>
        (!nome || (p.categorias || []).includes(nome)) &&
        (!termo || normalizar(p.produtoNome).includes(termo))).length;

    const opcoes = [{ categoriaID: null, categoriaNome: "Todas" }, ...todasCategorias];

    // Muitas categorias: mostra as primeiras (e sempre a selecionada) e um botão "Mostrar mais"
    const LIMITE = 7;
    const indiceAtivo = opcoes.findIndex(c => (c.categoriaID ?? null) === filtros.categoriaId);
    const recolher = !categoriasExpandidas && opcoes.length > LIMITE + 1;
    const visiveis = recolher
        ? opcoes.filter((c, i) => i < LIMITE || i === indiceAtivo)
        : opcoes;

    visiveis.forEach(categoria => {
        const ativo = (categoria.categoriaID ?? null) === filtros.categoriaId;

        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = "filtro-opcao" + (ativo ? " ativo" : "");
        botao.setAttribute("role", "radio");
        botao.setAttribute("aria-checked", ativo ? "true" : "false");

        botao.innerHTML = `
            <span>${escaparHtml(categoria.categoriaNome)}</span>
            <small>${contar(categoria.categoriaID ? categoria.categoriaNome : null)}</small>
        `;

        botao.addEventListener("click", function () {
            filtrarCatalogo({ categoriaId: categoria.categoriaID ?? null, termo: filtros.termo });
        });

        container.appendChild(botao);
    });

    if (opcoes.length > LIMITE + 1) {
        const escondidas = opcoes.length - visiveis.length;

        const alternar = document.createElement("button");
        alternar.type = "button";
        alternar.className = "filtro-mais";
        alternar.innerHTML = categoriasExpandidas
            ? `Mostrar menos <i class="bi bi-chevron-up"></i>`
            : `Mostrar mais ${escondidas} <i class="bi bi-chevron-down"></i>`;

        alternar.addEventListener("click", function () {
            categoriasExpandidas = !categoriasExpandidas;
            renderizarOpcoesCategoria();
            ajustarPainelFixo();
        });

        container.appendChild(alternar);
    }
}

// ===== Blocos de categoria (imagem = primeiro produto com foto da categoria) =====

function renderizarTilesCategorias(produtos) {
    const secao = document.getElementById("secaoCategorias");
    const container = document.getElementById("tilesCategorias");

    const categorias = todasCategorias.slice(0, 8);

    if (categorias.length === 0) {
        secao.style.display = "none";
        return;
    }

    container.innerHTML = "";

    categorias.forEach(categoria => {
        const produtoComFoto = produtos.find(p =>
            p.produtoImagemUrl && (p.categorias || []).includes(categoria.categoriaNome));

        const tile = document.createElement("button");
        tile.type = "button";
        tile.className = "tile-categoria" + (produtoComFoto ? "" : " tile-sem-imagem");

        tile.innerHTML = `
            ${produtoComFoto ? `<img src="${escaparHtml(urlImagemProduto(produtoComFoto.produtoImagemUrl))}" alt="" loading="lazy" />` : ""}
            <span>${escaparHtml(categoria.categoriaNome)}</span>
            <small>Ver produtos <i class="bi bi-arrow-right"></i></small>
        `;

        tile.addEventListener("click", function () {
            filtrarCatalogo({ categoriaId: categoria.categoriaID, termo: null });
        });

        container.appendChild(tile);
    });

    secao.style.display = "";
}

// ===== Grade de produtos =====

function mostrarEsqueleto() {
    const grid = document.getElementById("gridProdutos");

    grid.innerHTML = Array.from({ length: 8 }, () => `
        <div class="produto-card produto-esqueleto" aria-hidden="true">
            <div class="produto-midia"></div>
            <div class="esqueleto-linha"></div>
            <div class="esqueleto-linha curta"></div>
        </div>
    `).join("");
}

function renderizarProdutos(produtos) {
    const grid = document.getElementById("gridProdutos");
    grid.innerHTML = "";

    if (produtos.length === 0) {
        grid.innerHTML = `
            <div class="sem-produtos">
                <i class="bi bi-search"></i>
                <h5>Nenhum produto encontrado</h5>
                <p>Ajuste os filtros ou escolha outra categoria.</p>
            </div>
        `;
        return;
    }

    produtos.forEach(produto => grid.appendChild(criarCardProduto(produto)));
}

function criarCardProduto(produto) {
    const precoFinal = calcularPrecoFinal(produto);
    const possuiPromocao = produto.produtoPromocao > 0;

    const esgotado = produto.produtoQtdEstoque <= 0;
    const soNaLoja = !esgotado && produto.estoqueDisponivelOnline <= 0;
    const disponivel = !esgotado && !soNaLoja;

    const selos = [];

    if (esgotado) {
        selos.push(`<span class="selo">Esgotado</span>`);
    } else if (soNaLoja) {
        selos.push(`<span class="selo">Só na loja</span>`);
    } else if (produto.estoqueDisponivelOnline <= 3) {
        selos.push(`<span class="selo selo-alerta">Últimas ${produto.estoqueDisponivelOnline}</span>`);
    }

    if (possuiPromocao && disponivel) {
        selos.push(`<span class="selo selo-promo">-${Math.round(produto.produtoPromocao)}%</span>`);
    }

    const categoria = (produto.categorias || [])[0] || "";

    const card = document.createElement("article");
    card.className = "produto-card" + (disponivel ? "" : " produto-indisponivel");

    card.innerHTML = `
        <div class="produto-midia">
            <div class="produto-selos">${selos.join("")}</div>
            <img src="${escaparHtml(urlImagemProduto(produto.produtoImagemUrl))}" alt="${escaparHtml(produto.produtoNome)}" loading="lazy" />
            ${disponivel ? `
                <button type="button" class="btn-adicionar" data-produto-id="${produto.produtoID}">
                    <i class="bi bi-bag-plus"></i> Adicionar
                </button>` : ""}
        </div>

        <div class="produto-info">
            <p class="produto-categoria">${escaparHtml(categoria) || "&nbsp;"}</p>
            <h3 class="produto-nome">${escaparHtml(produto.produtoNome)}</h3>
            <div class="produto-precos">
                ${possuiPromocao ? `<span class="preco-antigo">${formatarPreco(produto.produtoPrecoVenda)}</span>` : ""}
                <span class="preco-final">${formatarPreco(precoFinal)}</span>
            </div>
        </div>
    `;

    const botao = card.querySelector(".btn-adicionar");

    if (botao) {
        botao.addEventListener("click", async function () {
            botao.disabled = true;

            const adicionou = await adicionarAoCarrinho(produto.produtoID);

            if (adicionou) {
                botao.classList.add("adicionado");
                botao.innerHTML = '<i class="bi bi-check-lg"></i> Adicionado';
            }

            setTimeout(() => {
                botao.classList.remove("adicionado");
                botao.innerHTML = '<i class="bi bi-bag-plus"></i> Adicionar';
                botao.disabled = false;
            }, 1200);
        });
    }

    return card;
}

async function adicionarAoCarrinho(produtoId) {
    if (estaLogado()) {
        const resultado = await chamarApi("/api/carrinho/itens", "POST", {
            produtoID: produtoId,
            quantidade: 1
        });

        if (!resultado.ok) {
            alert(resultado.dados?.mensagem || "Não foi possível adicionar o produto ao carrinho.");
            return false;
        }
    } else {
        const carrinhoLocal = lerCarrinhoLocal();
        const itemExistente = carrinhoLocal.find(i => i.produtoID === produtoId);

        if (itemExistente) {
            itemExistente.quantidade += 1;
        } else {
            carrinhoLocal.push({ produtoID: produtoId, quantidade: 1 });
        }

        localStorage.setItem("carrinho_local", JSON.stringify(carrinhoLocal));
    }

    await atualizarContadorCarrinho();
    return true;
}
