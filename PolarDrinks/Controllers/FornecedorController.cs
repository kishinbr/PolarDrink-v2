using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Data;
using PolarDrinks.Filters;
using PolarDrinks.Models;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class FornecedorController : Controller
    {
        //atibuto para acessar o banco de dados , por leitura somente
        readonly ApplicationDbContext _db;

        //construtor para injetar o banco de dados
        public FornecedorController(ApplicationDbContext db)
        {
            _db = db;
        }

        //ação para exibir a lista de fornecedores
        public IActionResult Index()
        {
            IEnumerable<FornecedorModel> fornecedores = _db.Fornecedores;

            return View(fornecedores);
        }


        //ação para exibir o formulário de edição de fornecedor
        [HttpGet]
        [AdminFilter]
        public IActionResult Editar(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var fornecedor = _db.Fornecedores.FirstOrDefault(x => x.FornecedorID == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Editar(FornecedorModel fornecedor)
        {
            // Validação dos dados do formulário
            bool cnpjExiste = _db.Fornecedores
            .Any(x => x.FornecedorCNPJ == fornecedor.FornecedorCNPJ
                   && x.FornecedorID != fornecedor.FornecedorID);
            if (cnpjExiste)
            {
                ModelState.AddModelError("FornecedorCNPJ", "Este CNPJ já está cadastrado.");
                return View(fornecedor);
            }
            if (!ModelState.IsValid)
            {
                // Se os dados forem inválidos, exibe a mensagem de erro e retorna para a view de edição
                TempData["MensagemErro"] = "Erro ao editar fornecedor.";
                return View(fornecedor);
            }
            // Verificar se o fornecedor existe no banco de dados
            var fornecedorDb = _db.Fornecedores.FirstOrDefault(x => x.FornecedorID == fornecedor.FornecedorID);

            // Se o fornecedor não for encontrado, exibe a mensagem de erro e retorna para a view de edição
            if (fornecedorDb == null)
            {
                TempData["MensagemErro"] = "Fornecedor não encontrado.";
                return View();
            }


            // Atualizando campos
            fornecedorDb.FornecedorNome = fornecedor.FornecedorNome;
            fornecedorDb.FornecedorCNPJ = fornecedor.FornecedorCNPJ;
            fornecedorDb.FornecedorTelefone = fornecedor.FornecedorTelefone;
            fornecedorDb.FornecedorEmail = fornecedor.FornecedorEmail;
            fornecedorDb.FornecedorCEP = fornecedor.FornecedorCEP;
            fornecedorDb.FornecedorCidade = fornecedor.FornecedorCidade;
            fornecedorDb.FornecedorEstado = fornecedor.FornecedorEstado;
            fornecedorDb.FornecedorBairro = fornecedor.FornecedorBairro;
            fornecedorDb.FornecedorLogradouro = fornecedor.FornecedorLogradouro;
            fornecedorDb.FornecedorNum = fornecedor.FornecedorNum;
            fornecedorDb.FornecedorAtivo = fornecedor.FornecedorAtivo;

            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Fornecedor editado com sucesso!";
            return RedirectToAction("Index");
        }

        //ação para exibir o formulário de cadastro de fornecedor
        [AdminFilter]
        public IActionResult Cadastrar()
        {
            return View();
        }

        //ação para processar o formulário de cadastro de fornecedor
        [HttpPost]
        [AdminFilter]
        public IActionResult Cadastrar(FornecedorModel fornecedor)
        {
            bool cnpjExiste = _db.Fornecedores
                .Any(x => x.FornecedorCNPJ == fornecedor.FornecedorCNPJ);

            if (cnpjExiste)
            {
                ModelState.AddModelError("FornecedorCNPJ", "Este CNPJ já está cadastrado.");
                return View(fornecedor);
            }
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao cadastrar fornecedor.";
                return View(fornecedor);
            }

            // Verificar se o CNPJ já existe no banco de dados
            

            _db.Fornecedores.Add(fornecedor);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Fornecedor cadastrado com sucesso!";
            return RedirectToAction("Index");
        }
    }
}
