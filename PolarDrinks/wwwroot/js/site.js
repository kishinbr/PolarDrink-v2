
$(document).ready(function () {

    $('#Produtos').DataTable({
        pageLength: 10,
        lengthMenu: [10, 20, 30],
        scrollX: true,
        paging: true,

        columnDefs: [
            { orderable: false, targets: [6] }, // desabilita ordenação na coluna Ações
            { className: "text-end", targets: [3, 4] }, // preço e quantidade alinhados à direita
            { className: "text-center", targets: [5, 6] }, // status e ações centralizados
            
        ],

        language: {
            "decimal": "",
            "emptyTable": "Nenhum produto cadastrado",
            "info": "Mostrando de _START_ a _END_ de um total de _TOTAL_ produtos",
            "infoEmpty": "Mostrando de 0 a 0 de 0 produtos",
            "infoFiltered": "(filtrado de _MAX_ produtos no total)",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ produtos",
            "loadingRecords": "Carregando...",
            "search": "Procurar:",
            "zeroRecords": "Produto não encontrado",
            "paginate": {
                "first": "Primeiro",
                "last": "Último",
                "next": "Próximo",
                "previous": "Anterior"
            },
            "aria": {
                "orderable": "Ordenar por esta coluna",
                "orderableReverse": "Ordem reversa desta coluna"
            }
        }
    });
    

    setTimeout(function () {
        $(".alert").fadeOut("slow", function () {
            $(this).alert('close');
        });
    }, 3000);

});

$(document).ready(function () {

    $('#tabelaEstoqueConsulta').DataTable({
        pageLength: 5,
        lengthMenu: [5, 10, 15],
        scrollX: true,
        scrollCollapse: true,
        paging: true,

        columnDefs: [
            { className: "text-center", targets: [1, 2, 3] }
        ],
        language: {
            "emptyTable": "Nenhum produto encontrado",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ produtos",
            "infoEmpty": "Mostrando 0 produtos",
            "infoFiltered": "(filtrado de _MAX_)",
            "lengthMenu": "Mostrar _MENU_ produtos",
            "search": "Procurar:",
            "zeroRecords": "Produto não encontrado",
            "paginate": {
                "first": "Primeiro",
                "last": "Último",
                "next": "Próximo",
                "previous": "Anterior"
            }
        }
    });

    setTimeout(function () {
        $(".alert").fadeOut("slow", function () {
            $(this).alert('close');
        });
    }, 3000);

});
$(document).ready(function () {

    $('#Fornecedores').DataTable({
        pageLength: 10,
        lengthMenu: [10, 20, 30],
        scrollY: "50vh",
        scrollCollapse: true,
        paging: true,

        columnDefs: [
            //{ orderable: false, targets: [8] }, // desabilita ordenação na coluna Ações
            { className: "text-start", targets: [1, 2, 3, 4] },
            //{ className: "text-center", targets: [8] } // centraliza os botões de Ações
        ],

        language: {
            "decimal": "",
            "emptyTable": "Nenhum fornecedor cadastrado",
            "info": "Mostrando de _START_ a _END_ de um total de _TOTAL_ fornecedores",
            "infoEmpty": "Mostrando de 0 a 0 de 0 fornecedores",
            "infoFiltered": "(filtrado de _MAX_ fornecedores no total)",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ fornecedores",
            "loadingRecords": "Carregando...",
            "search": "Procurar:",
            "zeroRecords": "Fornecedor não encontrado",
            "paginate": {
                "first": "Primeiro",
                "last": "Último",
                "next": "Próximo",
                "previous": "Anterior"
            },
            "aria": {
                "orderable": "Ordenar por esta coluna",
                "orderableReverse": "Ordem reversa desta coluna"
            }
        }
    });

    setTimeout(function () {
        $(".alert").fadeOut("slow", function () {
            $(this).alert('close');
        });
    }, 3000);

});

document.addEventListener("DOMContentLoaded", function () {

    // ================= CEP =================
    const cepInput = document.getElementById("FornecedorCEP");

    if (cepInput) {
        cepInput.addEventListener("input", function (e) {
            let value = e.target.value.replace(/\D/g, "").substring(0, 8);

            if (value.length > 5) {
                value = value.replace(/^(\d{5})(\d{0,3})$/, "$1-$2");
            }

            e.target.value = value;
        });
    }

    // ================= TELEFONE =================
    const telefoneInput = document.getElementById("FornecedorTelefone");

    if (telefoneInput) {
        telefoneInput.addEventListener("input", function (e) {
            let value = e.target.value.replace(/\D/g, "").substring(0, 11);

            if (value.length > 6) {
                value = value.replace(/^(\d{2})(\d{5})(\d{0,4})$/, "($1) $2-$3");
            } else if (value.length > 2) {
                value = value.replace(/^(\d{2})(\d{0,5})$/, "($1) $2");
            } else {
                value = value.replace(/^(\d*)$/, "($1");
            }

            e.target.value = value;
        });
    }

    // ================= CNPJ =================
    const cnpjInput = document.getElementById("FornecedorCNPJ");

    if (cnpjInput) {
        cnpjInput.addEventListener("input", function (e) {
            let value = e.target.value.replace(/\D/g, "").substring(0, 14);

            value = value.replace(/^(\d{2})(\d)/, "$1.$2");
            value = value.replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3");
            value = value.replace(/\.(\d{3})(\d)/, ".$1/$2");
            value = value.replace(/(\d{4})(\d)/, "$1-$2");

            e.target.value = value;
        });
    }

});

$(document).ready(function () {

    var table = $('#tabelaConcluidos').DataTable({
        pageLength: 10,
        lengthMenu: [10, 20, 30],
        scrollY: "50vh",
        scrollCollapse: true,
        paging: true,
        order: [[0, "desc"]],

        columnDefs: [
            { orderable: false, targets: [5] },
            { className: "text-center", targets: [1,2,5] }
        ],

        language: {
            "emptyTable": "Nenhum pedido concluído",
            "info": "Mostrando de _START_ a _END_ de _TOTAL_ pedidos",
            "lengthMenu": "Mostrar _MENU_ pedidos",
            "search": "Pesquisar:",
            "zeroRecords": "Nenhum resultado encontrado",
            "paginate": {
                "next": "Próximo",
                "previous": "Anterior"
            }
        }
    });

    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        if (settings.nTable.id !== "tabelaConcluidos") return true;

        let dataInicio = $('#dataInicio').val();
        let dataFim = $('#dataFim').val();

        let linha = table.row(dataIndex).node();
        let dataCompra = linha.children[1].getAttribute("data-order");

        if (!dataInicio && !dataFim) return true;

        if (dataInicio && dataCompra < dataInicio) return false;
        if (dataFim && dataCompra > dataFim) return false;

        return true;
    });


    $('#dataInicio, #dataFim').on('change', function () {
        table.draw();
    });

});
$(document).ready(function () {

    // Inicializa o DataTable após a tabela estar totalmente renderizada
    var table = $('#tabelaVendas').DataTable({
        pageLength: 10,
        lengthMenu: [10, 15, 20],
        paging: true,
        scrollX: true,
        scrollCollapse: true,

        columnDefs: [
            { orderable: false, targets: [5] },
            { className: "text-center", targets: [0, 1, 3, 4, 5] }
        ],
        order: [[0, 'desc']], // 
        language: {
            "decimal": "",
            "emptyTable": "Nenhuma venda registrada",
            "info": "Mostrando de _START_ a _END_ de um total de _TOTAL_ vendas",
            "infoEmpty": "Mostrando de 0 a 0 de 0 vendas",
            "infoFiltered": "(filtrado de _MAX_ vendas no total)",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ vendas",
            "loadingRecords": "Carregando...",
            "search": "Procurar:",
            "zeroRecords": "Venda não encontrada",
            "paginate": {
                "first": "Primeiro",
                "last": "Último",
                "next": "Próximo",
                "previous": "Anterior"
            },
            "aria": {
                "orderable": "Ordenar por esta coluna",
                "orderableReverse": "Ordem reversa desta coluna"
            }
        }
    });

    // Ajusta colunas para a primeira carga
    table.columns.adjust().draw();


    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        if (settings.nTable.id !== "tabelaVendas") return true; // só aplica nesta tabela

        let dataInicio = $('#dataInicio').val();
        let dataFim = $('#dataFim').val();

        let linha = table.row(dataIndex).node();
        let dataVenda = linha.children[1].getAttribute("data-order"); 

        if (!dataInicio && !dataFim) return true;

        if (dataInicio && dataVenda < dataInicio) return false;
        if (dataFim && dataVenda > dataFim) return false;

        return true;
    });

    // Redesenha a tabela quando os inputs de data mudarem
    $('#dataInicio, #dataFim').on('change', function () {
        table.draw();
    });

    setTimeout(function () {
        $(".alert").fadeOut("slow", function () {
            $(this).alert('close');
        });
    }, 3000);

});