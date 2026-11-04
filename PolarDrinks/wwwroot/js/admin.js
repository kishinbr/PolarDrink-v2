// Telas da equipe: comportamento do layout (_Layout.cshtml).

// Menu lateral no celular e no tablet: abre pelo botão do topo, fecha pelo fundo escuro ou pelo Esc
document.addEventListener("DOMContentLoaded", function () {
    const botao = document.getElementById("btnAbrirMenu");
    const fundo = document.getElementById("fundoMenu");

    if (!botao || !fundo) return;

    function fecharMenu() {
        document.body.classList.remove("menu-aberto");
    }

    botao.addEventListener("click", function () {
        document.body.classList.toggle("menu-aberto");
    });

    fundo.addEventListener("click", fecharMenu);

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") fecharMenu();
    });
});
