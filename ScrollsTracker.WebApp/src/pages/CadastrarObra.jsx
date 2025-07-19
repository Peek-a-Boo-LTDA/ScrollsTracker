import React from "react";
import { useForm } from "react-hook-form";

function CadastrarObra() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  // Esta função será chamada apenas se o formulário for válido
  const onSubmit = (data) => {
    console.log("Dados enviados:", data);
    // Aqui você faria a chamada para a sua API para salvar os dados
  };

  return (
    <div className="min-h-screen bg-gray-900 flex items-center justify-center p-4">
      {/* O evento onSubmit do formulário deve ser gerenciado pelo handleSubmit do hook */}
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="w-full max-w-lg bg-gray-800 p-8 rounded-lg shadow-md border border-gray-700"
      >
        <h2 className="text-2xl font-bold text-white mb-6 text-center">
          Cadastrar Nova Obra
        </h2>
        <div className="grid grid-cols-1 gap-6">
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
            {/* Mensagem de erro para o título */}
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
                  min: { value: 0, message: "O valor não pode ser negativo." },
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
                  min: { value: 0, message: "O valor não pode ser negativo." },
                })}
              />
              {errors.ultimoCapituloLido && (
                <p className="mt-1 text-xs text-red-400">
                  {errors.ultimoCapituloLido.message}
                </p>
              )}
            </div>
          </div>

          {/* Campo Status */}
          <div>
            <label
              htmlFor="status"
              className="block text-sm font-medium text-gray-300 mb-1"
            >
              Status
            </label>
            <select
              id="status"
              className={`block w-full px-3 py-2 bg-gray-700 border rounded-md shadow-sm text-white focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 ${
                errors.status ? "border-red-500" : "border-gray-600"
              }`}
              {...register("status", { required: "Selecione um status." })}
            >
              <option className="bg-gray-700 text-white">Lendo</option>
              <option className="bg-gray-700 text-white">Completo</option>
              <option className="bg-gray-700 text-white">Planejo Ler</option>
              <option className="bg-gray-700 text-white">Pausado</option>
              <option className="bg-gray-700 text-white">Abandonado</option>
            </select>
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
              className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Cadastrar Obra
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}

export default CadastrarObra;
