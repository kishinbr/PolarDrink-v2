document.addEventListener("DOMContentLoaded", () => {

    // ==================== ELEMENTOS ====================
    const buscaInput = document.getElementById("buscaProduto");
    const lista = document.getElementById("listaSugestoes");

    const qtdInput = document.getElementById("quantidade");
    qtdInput.addEventListener("input", () => {
        let value = qtdInput.value;

        value = value.replace(/[^0-9]/g, '');

        if (value.length > 1) {
            value = value.replace(/^0+/, '');
        }

        value = value.substring(0, 4);

        qtdInput.value = value;

        calcularTotal();
    });

    const valorUnitario = document.getElementById("valorUnitario");
    const valorTotal = document.getElementById("valorTotal");
    const tabela = document.getElementById("tabelaItens");
    const totalVendaLabel = document.getElementById("totalVenda");
    const itensHidden = document.getElementById("itensHidden");

    const btnConfirmar = document.getElementById("btnConfirmar");
    const radiosPagamento = document.querySelectorAll('input[name="VendaTipoPagamento"]');

    let totalVenda = 0;
    let produtoSelecionado = null;
    let indiceSelecionado = -1;

    // ==================== FUNÇÕES AUXILIARES ====================
    function formatarMoeda(valor) {
        return "R$ " + parseFloat(valor).toFixed(2);
    }

    function parseMoeda(texto) {
        return parseFloat(texto.replace("R$", "").trim()) || 0;
    }

    function calcularPrecoComDesconto(p) {
        let preco = p.produtoPrecoVenda;
        if (p.produtoPromocao > 0) {
            preco = preco - (preco * (p.produtoPromocao / 100));
        }
        return preco;
    }

    function atualizarBotaoConfirmar() {
        const pagamentoSelecionado = document.querySelector('input[name="VendaTipoPagamento"]:checked');
        const itens = itensHidden.querySelectorAll('input[name$=".ProdutoID"]');
        btnConfirmar.disabled = !(pagamentoSelecionado && itens.length > 0);
    }

    // ==================== BUSCA DE PRODUTOS ====================
    function renderLista(filtro = "") {
        lista.innerHTML = "";
        indiceSelecionado = -1;

        const filtrados = produtos
            .filter(p =>
                p.produtoNome.toLowerCase().includes(filtro) ||
                p.produtoCodBarra.toLowerCase().includes(filtro)
            )
            .slice(0, 8);

        filtrados.forEach(p => {

            const item = document.createElement("a");
            item.classList.add("list-group-item", "list-group-item-action");

            const precoOriginal = parseFloat(p.produtoPrecoVenda || 0);
            const precoFinal = p.produtoPromocao > 0
                ? precoOriginal - (precoOriginal * (p.produtoPromocao / 100))
                : precoOriginal;

            item.innerHTML = `
                <div style="display:flex; justify-content:space-between; align-items:center; white-space:nowrap; gap:10px;">
                    <div style="overflow:hidden; text-overflow:ellipsis;">
                        ${p.produtoNome} [${p.produtoCodBarra}]
                    </div>
                    <div class="text-secondary">
                        QTD ${p.produtoQtdEstoque}
                    </div>
                    <div class="text-end">
                        ${p.produtoPromocao > 0
                    ? `<small class="text-secondary" style="text-decoration:line-through;">
                                   R$ ${precoOriginal.toFixed(2)}
                               </small>
                               <strong class="text-success ms-1">
                                   R$ ${precoFinal.toFixed(2)}
                               </strong>`
                    : `<strong>R$ ${precoOriginal.toFixed(2)}</strong>`
                }
                    </div>
                </div>
            `;

            item.onclick = () => selecionarProduto(p);
            lista.appendChild(item);
        });
    }

    function selecionarProduto(p) {
        produtoSelecionado = p;
        buscaInput.value = p.produtoNome;
        lista.innerHTML = "";

        const preco = calcularPrecoComDesconto(p);
        produtoSelecionado.precoCalculado = preco;

        valorUnitario.value = formatarMoeda(preco);
        calcularTotal();
    }

    buscaInput.addEventListener("focus", () => renderLista(buscaInput.value.toLowerCase()));
    buscaInput.addEventListener("input", () => renderLista(buscaInput.value.toLowerCase()));

    document.addEventListener("click", e => {
        if (!buscaInput.contains(e.target) && !lista.contains(e.target)) lista.innerHTML = "";
    });

    buscaInput.addEventListener("keydown", e => {
        let itens = lista.querySelectorAll(".list-group-item");
        if (!itens.length) return;

        if (e.key === "ArrowDown") { e.preventDefault(); indiceSelecionado = (indiceSelecionado + 1) % itens.length; }
        if (e.key === "ArrowUp") { e.preventDefault(); indiceSelecionado = (indiceSelecionado - 1 + itens.length) % itens.length; }
        if (e.key === "Enter") { e.preventDefault(); if (indiceSelecionado >= 0) itens[indiceSelecionado].click(); return; }

        itens.forEach(i => i.classList.remove("active"));
        if (indiceSelecionado >= 0) itens[indiceSelecionado].classList.add("active");
    });

    // ==================== CÁLCULO DE TOTAL ====================
    function calcularTotal() {
        const qtd = parseFloat(qtdInput.value) || 0;
        const precoTexto = valorUnitario.value.replace("R$", "").trim();
        const preco = parseFloat(precoTexto) || 0;
        valorTotal.value = formatarMoeda(qtd * preco);
    }

    qtdInput.addEventListener("input", calcularTotal);

    // ==================== ADICIONAR ITEM ====================
    document.getElementById("btnAdicionar").addEventListener("click", () => {

        let msgAdicionar = document.getElementById("mensagemErroAdicionar");
        if (!msgAdicionar) {
            msgAdicionar = document.createElement("div");
            msgAdicionar.id = "mensagemErroAdicionar";
            msgAdicionar.classList.add("alert", "bg-danger", "text-light", "mt-2");
            msgAdicionar.style.display = "none";
            document.getElementById("formVenda").prepend(msgAdicionar);
        }

        function mostrarErro(texto) {
            msgAdicionar.innerText = texto;
            msgAdicionar.style.opacity = 1;
            msgAdicionar.style.display = "block";
            msgAdicionar.style.transition = "opacity 0.5s";
            setTimeout(() => {
                msgAdicionar.style.opacity = 0;
                setTimeout(() => {
                    msgAdicionar.style.display = "none";
                    msgAdicionar.innerText = "";
                }, 500);
            }, 2000);
        }

        msgAdicionar.style.display = "none";
        msgAdicionar.innerText = "";

        if (!produtoSelecionado) {
            mostrarErro("Selecione um produto antes de adicionar!");
            return;
        }

        let qtd = parseInt(qtdInput.value) || 0;
        if (qtd <= 0) {
            mostrarErro("Quantidade inválida!");
            return;
        }

        // Verifica quantidade já adicionada para validar estoque
        let qtdJaAdicionada = 0;
        let linhaExistente = null;
        tabela.querySelectorAll("tr").forEach(linha => {
            if (parseInt(linha.getAttribute("data-id")) === produtoSelecionado.produtoID) {
                qtdJaAdicionada += parseInt(linha.children[1].innerText);
                linhaExistente = linha;
            }
        });

        if (qtd + qtdJaAdicionada > produtoSelecionado.produtoQtdEstoque) {
            mostrarErro(`Estoque insuficiente! Disponível: ${produtoSelecionado.produtoQtdEstoque - qtdJaAdicionada}`);
            return;
        }

        const precoTexto = valorUnitario.value.replace("R$", "").trim();
        const preco = parseFloat(precoTexto) || 0;
        const total = qtd * preco;

        // ===== MERGE: produto já existe na tabela =====
        if (linhaExistente) {
            const novaQtd = qtdJaAdicionada + qtd;
            const novoTotal = novaQtd * preco;
            const totalAntigo = parseFloat(linhaExistente.getAttribute("data-total")) || 0;

            // Atualiza células da linha existente
            linhaExistente.children[1].innerText = novaQtd;
            linhaExistente.children[3].innerText = formatarMoeda(novoTotal);
            linhaExistente.setAttribute("data-total", novoTotal);

            // Atualiza inputs hidden correspondentes
            const index = Array.from(tabela.children).indexOf(linhaExistente);
            const hiddenQtd = itensHidden.querySelector(`[name="Itens[${index}].ItemVendaQtd"]`);
            const hiddenTotal = itensHidden.querySelector(`[name="Itens[${index}].ItemVendaTotal"]`);
            if (hiddenQtd) hiddenQtd.value = novaQtd;
            if (hiddenTotal) hiddenTotal.value = novoTotal;

            // Atualiza total da venda
            totalVenda += total;
            if (totalVenda < 0) totalVenda = 0;
            totalVendaLabel.innerText = formatarMoeda(totalVenda);

            // ===== NOVO: produto ainda não está na tabela =====
        } else {
            const linha = document.createElement("tr");
            linha.setAttribute("data-id", produtoSelecionado.produtoID);
            linha.setAttribute("data-total", total);
            linha.innerHTML = `
                <td>${produtoSelecionado.produtoNome}</td>
                <td class="text-end">${qtd}</td>
                <td class="text-end">${formatarMoeda(preco)}</td>
                <td class="text-end">${formatarMoeda(total)}</td>
                <td class="text-center">
                    <button type="button" class="btn btn-danger btn-sm btn-remover " title="Remover Produto">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            tabela.appendChild(linha);

            const index = tabela.children.length - 1;
            itensHidden.insertAdjacentHTML('beforeend', `
                <input type="hidden" name="Itens[${index}].ProdutoID"      value="${produtoSelecionado.produtoID}" />
                <input type="hidden" name="Itens[${index}].ItemVendaQtd"   value="${qtd}" />
                <input type="hidden" name="Itens[${index}].ItemVendaPreco" value="${preco}" />
                <input type="hidden" name="Itens[${index}].ItemVendaTotal" value="${total}" />
            `);

            totalVenda += total;
            totalVendaLabel.innerText = formatarMoeda(totalVenda);
        }

        // Limpa campos
        produtoSelecionado = null;
        buscaInput.value = "";
        qtdInput.value = 0;
        valorUnitario.value = "";
        valorTotal.value = "";

        atualizarBotaoConfirmar();
    });

    // ==================== REMOVER ITEM ====================
    tabela.addEventListener("click", e => {
        const btn = e.target.closest(".btn-remover");
        if (!btn) return;

        const row = btn.closest("tr");
        const index = Array.from(tabela.children).indexOf(row);

        const total = parseFloat(row.getAttribute("data-total")) || 0;

        totalVenda -= total;
        if (totalVenda < 0) totalVenda = 0;
        totalVendaLabel.innerText = formatarMoeda(totalVenda);

        row.remove();

        itensHidden.querySelectorAll(`[name^="Itens[${index}]"]`).forEach(i => i.remove());

        atualizarBotaoConfirmar();
    });

    // ==================== RADIO PAGAMENTO ====================
    radiosPagamento.forEach(radio => {
        radio.addEventListener("change", atualizarBotaoConfirmar);
    });

    atualizarBotaoConfirmar();
});