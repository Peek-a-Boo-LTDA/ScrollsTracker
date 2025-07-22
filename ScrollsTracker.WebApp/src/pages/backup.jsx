import React from "react";
import { useForm } from "react-hook-form";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

function CadastrarObra() {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    getValues,
    watch,
  } = useForm();

  const imageUrl = watch("imagem");
  const navigate = useNavigate();

  const procurarObraApi = async (titulo) => {
    if (!titulo) {
      throw new Error("O título não pode ser vazio.");
    }
    const response = await fetch(
      `https://localhost:7071/api/ScrollsTracker/ProcurarObraNasApisExternas?titulo=${encodeURIComponent(
        titulo
      )}`
    );
    if (!response.ok) {
      throw new Error("Não foi possível encontrar dados para este título.");
    }
    return response.json();
  };

  const cadastrarObraApi = async (data) => {
    const response = await fetch(
      "https://localhost:7071/api/ScrollsTracker/CadastrarObra",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      }
    );
    if (!response.ok) {
      throw new Error("Falha ao cadastrar a obra.");
    }
    return response.json();
  };

  const { mutate: procurarObra, isPending: isProcurando } = useMutation({
    mutationFn: procurarObraApi,
    onSuccess: (data) => {
      console.log("Dados recebidos:", data);
      data.statusLeitura = "Lendo";
      reset(data);
    },
    onError: (error) => {
      console.error("Erro ao procurar obra:", error);
      alert(error.message);
    },
  });

  const { mutate: cadastrarObra, isPending: isCadastrando } = useMutation({
    mutationFn: cadastrarObraApi,
    onSuccess: (data) => {
      console.log("Resposta do servidor:", data);
      alert("Obra cadastrada com sucesso!");
      navigate("/biblioteca");
      // Poderia adicionar uma notificação de sucesso aqui (ex: react-toastify)
    },
    onError: (error) => {
      console.error("Erro ao cadastrar obra:", error);
      alert(error.message);
    },
  });

  const handleProcurarClick = () => {
    procurarObra(getValues("titulo"));
  };

  const onSubmit = (data) => {
    cadastrarObra(data);
  };

  return (
    <div className="min-h-screen bg-gray-900 flex items-center justify-center p-4">
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="w-full max-w-3xl bg-gray-800 p-8 rounded-lg shadow-md border border-gray-700"
      >
        <div className="flex justify-center mb-4 gap-4">
          {imageUrl && (
            <div className="mt-4">
              <img
                src={imageUrl}
                alt="Pré-visualização da capa"
                className="object-cover rounded-md border-2 border-gray-600"
                onError={(e) => {
                  e.target.style.display = "none";
                }}
              />
            </div>
          )}
          <div className="grid grid-cols-1 gap-6">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-2xl font-bold text-white">Cadastrar Obra</h2>
              <button
                type="button"
                onClick={handleProcurarClick}
                disabled={isProcurando}
                className="px-3 py-1 border border-indigo-500 text-indigo-400 text-sm font-medium rounded-md hover:bg-indigo-500 hover:text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isProcurando ? "Carregando..." : "Carregar Dados"}
              </button>
            </div>
            {/* Campo Titulo */}
            <div>
              <label
                htmlFor="titulo"
                className="block text-sm font-medium text-gray-300 mb-1"
              >
                Título
              </label>
              <input
                type="text"
                id="titulo"
                className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                  errors.titulo ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Ex: One Piece"
                {...register("titulo", { required: "O título é obrigatório." })}
              />
              {errors.titulo && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.titulo.message}
                </p>
              )}
            </div>

            {/* Campo Descrição */}
            <div>
              <label
                htmlFor="descricao"
                className="block text-sm font-medium text-gray-300 mb-1"
              >
                Descrição
              </label>
              <textarea
                id="descricao"
                rows="3"
                className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                  errors.descricao ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Uma breve descrição da obra..."
                {...register("descricao", {
                  required: "A descrição é obrigatória.",
                })}
              ></textarea>
              {errors.descricao && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.descricao.message}
                </p>
              )}
            </div>

            {/* Campo Imagem (URL) */}
            <div>
              <label
                htmlFor="imagem"
                className="block text-sm font-medium text-gray-300 mb-1"
              >
                URL da Imagem de Capa
              </label>
              <input
                type="text"
                id="imagem"
                className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                  errors.imagem ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="https://exemplo.com/capa.jpg"
                {...register("imagem", {
                  required: "A URL da imagem é obrigatória.",
                })}
              />
              {errors.imagem && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.imagem.message}
                </p>
              )}
            </div>

            {/* Grid para os campos numéricos */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Campo Total de Capítulos */}
              <div>
                <label
                  htmlFor="totalCapitulos"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Total de Capítulos
                </label>
                <input
                  type="number"
                  id="totalCapitulos"
                  className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                    errors.totalCapitulos ? "border-red-500" : "border-gray-600"
                  }`}
                  placeholder="1100"
                  {...register("totalCapitulos", {
                    required: "Campo obrigatório.",
                    min: {
                      value: 0,
                      message: "O valor não pode ser negativo.",
                    },
                  })}
                />
                {errors.totalCapitulos && (
                  <p className="mt-1 text-xs text-red-400">
                    {errors.totalCapitulos.message}
                  </p>
                )}
              </div>

              {/* Campo Último Capítulo Lido */}
              <div>
                <label
                  htmlFor="ultimoCapituloLido"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Último Capítulo Lido
                </label>
                <input
                  type="number"
                  id="ultimoCapituloLido"
                  className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                    errors.ultimoCapituloLido
                      ? "border-red-500"
                      : "border-gray-600"
                  }`}
                  placeholder="1050"
                  {...register("ultimoCapituloLido", {
                    required: "Campo obrigatório.",
                    min: {
                      value: 0,
                      message: "O valor não pode ser negativo.",
                    },
                  })}
                />
                {errors.ultimoCapituloLido && (
                  <p className="mt-1 text-xs text-red-400">
                    {errors.ultimoCapituloLido.message}
                  </p>
                )}
              </div>
            </div>

            {/* Campo Status Leitura */}
            <div>
              <label
                htmlFor="statusLeitura"
                className="block text-sm font-medium text-gray-300 mb-1"
              >
                Status Leitura
              </label>
              <select
                id="statusLeitura"
                className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                  errors.statusLeitura ? "border-red-500" : "border-gray-600"
                }`}
                {...register("statusLeitura", {
                  required: "Selecione um status.",
                })}
              >
                <option className="bg-gray-700 text-white">Lendo</option>
                <option className="bg-gray-700 text-white">Completo</option>
                <option className="bg-gray-700 text-white">Planejo Ler</option>
                <option className="bg-gray-700 text-white">Pausado</option>
                <option className="bg-gray-700 text-white">Abandonado</option>
              </select>
              {errors.statusLeitura && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.statusLeitura.message}
                </p>
              )}
            </div>

            {/* Campo Status Obra */}
            <div>
              <label
                htmlFor="status"
                className="block text-sm font-medium text-gray-300 mb-1"
              >
                Status da Obra
              </label>
              <input
                type="text"
                id="status"
                className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                  errors.status ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Em Andamento"
                {...register("status", {
                  required: "O status da leitura é obrigatório.",
                })}
              />
              {errors.status && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.status.message}
                </p>
              )}
            </div>

            {/* Botão de Envio */}
            <div className="mt-4">
              <button
                type="submit"
                disabled={isCadastrando}
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCadastrando ? "Cadastrando..." : "Cadastrar Obra"}
              </button>
            </div>
          </div>
        </div>
      </form>
    </div>
  );
}

export default CadastrarObra;
