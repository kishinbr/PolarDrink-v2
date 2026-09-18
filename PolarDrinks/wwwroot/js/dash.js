let graficoPagamentos;
let graficoVendas;
function atualizarBotaoPeriodoVendas(periodo) {
    document.querySelectorAll('.filtro-vendas').forEach(botao => {
        botao.classList.toggle('active', botao.dataset.periodo === periodo);
    });
}


document.addEventListener("DOMContentLoaded", () => {

    let canalVendasAtual = "presencial";
    let periodoVendasAtual = "semana";

        // ================== PAGAMENTOS ==================
    let canalAtual = "presencial";
    let periodoPagamentoAtual = "hoje";
    function atualizarBotaoPeriodoPagamento(periodo) {
    document.querySelectorAll('.filtro-pagamento').forEach(botao => {
        botao.classList.toggle('active', botao.dataset.periodo === periodo);
    });
    }

       graficoPagamentos = new Chart(document.getElementById('graficoPagamentos'), {
        type: 'doughnut',
        data: {
            labels: ['Pix', 'Cartão', 'Dinheiro'],
            datasets: [{
                data: [0, 0, 0],
                backgroundColor: ['#00BDAE', 'mediumpurple', '#198754'],
                borderWidth: 2,
                borderColor: '#13161d'
            }]
        },
        options: {
            cutout: '65%',
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        color: '#fff',
                        padding: 10
                    }
                }
            }
        }
    });

    window.atualizarPagamentos = function (periodo) {
        periodoPagamentoAtual = periodo;
        atualizarBotaoPeriodoPagamento(periodo);
        renderizarPagamentos();
    };

    window.atualizarCanalPagamentos = function (canal) {
        canalAtual = canal;
        renderizarPagamentos();
    };

    function renderizarPagamentos() {
        const resumo = window.dashData.pagamentosPorCanal[canalAtual][periodoPagamentoAtual];
        
        const dados = [resumo.QtdPix, resumo.QtdCartao, resumo.QtdDinheiro];

        graficoPagamentos.data.datasets[0].data = dados;
        graficoPagamentos.update();

        const total = dados.reduce((a, b) => a + b, 0);
        document.getElementById("qtdPagamentosLabel").innerText = `Total de vendas: ${total}`;

        document.getElementById("lblPix").innerText = resumo.QtdPix;
        document.getElementById("lblCartao").innerText = resumo.QtdCartao;
        document.getElementById("lblDinheiro").innerText = resumo.QtdDinheiro;

        const fmt = v => v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

        document.getElementById("valPix").innerText = fmt(resumo.TotalPix);
        document.getElementById("valCartao").innerText = fmt(resumo.TotalCartao);
        document.getElementById("valDinheiro").innerText = fmt(resumo.TotalDinheiro);
    }
    // ================== VENDAS ==================
    graficoVendas = new Chart(document.getElementById('graficoVendas'), {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Vendas',
                data: [],
                backgroundColor: '#4f8ef7',
                tension: 0.4
            }]
        },

        options: {
            scales: {

                // 🔵 EIXO X (embaixo das colunas)
                x: {
                    ticks: {
                        color: '#ffffff' // 👈 TEXTO BRANCO
                    },
                    grid: {
                        color: 'rgba(255,255,255,0.05)' // opcional (linhas suaves)
                    }
                },

                // 🔵 EIXO Y (lado esquerdo)
                y: {
                    ticks: {
                        color: '#ffffff' // 👈 TEXTO BRANCO
                    },
                    grid: {
                        color: 'rgba(255,255,255,0.05)' // opcional
                    }
                }
            },

            plugins: {
                legend: {
                    labels: {
                        color: '#ffffff' // 👈 "Vendas" legenda
                    }
                }
            }
        }
    });

        window.atualizarVendas = function (periodo) {
        periodoVendasAtual = periodo;
        atualizarBotaoPeriodoVendas(periodo);
        renderizarVendas();
    };

    window.atualizarCanalVendas = function (canal) {
        canalVendasAtual = canal;
        renderizarVendas();
    };

    function renderizarVendas() {
        const dados = window.dashData.vendasPorCanal[canalVendasAtual][periodoVendasAtual];
        let labels = [];

        if (periodoVendasAtual === "hoje") {
            labels = dados.map((_, i) => `${i}h`);
        }

        else if (periodoVendasAtual === "semana") {
            const dias = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];
            const hoje = new Date();

            labels = dados.map((_, i) => {
                let data = new Date();
                data.setDate(hoje.getDate() - (dados.length - 1 - i));
                return dias[data.getDay()];
            });
        }

        else if (periodoVendasAtual === "mes") {
            labels = dados.map((_, i) => `${i + 1}`);
        }

        else if (periodoVendasAtual === "ano") {
            const meses = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
                "Jul", "Ago", "Set", "Out", "Nov", "Dez"];
            labels = dados.map((_, i) => meses[i]);
        }

        graficoVendas.data.labels = labels;
        graficoVendas.data.datasets[0].data = dados;
        graficoVendas.update();

        const total = dados.reduce((a, b) => a + b, 0);
        document.getElementById("totalVendasLabel").innerText =
            `Total de vendas: ${total}`;
    }

    // inicialização
    atualizarVendas("semana");
    atualizarPagamentos("hoje");
    atualizarCanalPagamentos("presencial");
    atualizarCanalVendas("presencial");
});