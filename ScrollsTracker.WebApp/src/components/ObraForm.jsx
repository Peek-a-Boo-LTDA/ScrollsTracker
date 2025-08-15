import React, { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useMutation } from "@tanstack/react-query";

function ObraForm({
  initialData,
  onSubmit,
  isSubmitting,
  submitButtonText = "Salvar",
  Type,
  isDeleting,
  onDelete,
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    getValues,
    watch,
    trigger,
  } = useForm({
    mode: "onChange",
    defaultValues: initialData || {
      titulo: "",
      descricao: "",
      totalCapitulos: 0,
      ultimoCapituloLido: 0,
      imagem: "",
      status: "Lendo",
    },
  });

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

  const imageUrl = watch("imagem");
  const tituloValue = watch("titulo");
  const totalCapitulosValue = watch("totalCapitulos");

  useEffect(() => {
    // 3. Sempre que 'totalCapitulos' mudar, dispare a validação de 'ultimoCapituloLido'
    trigger("ultimoCapituloLido");
  }, [totalCapitulosValue, trigger]);

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

  useEffect(() => {
    if (initialData) {
      reset(initialData);
    }
  }, [initialData, reset]);

  const handleProcurarClick = () => {
    procurarObra(getValues("titulo"));
  };

  return (
    <div className="min-h-screen w-4xl flex items-center justify-center p-4">
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="w-full bg-gray-800 p-8 rounded-lg shadow-md border border-gray-700"
      >
        <div className="flex justify-center mb-4 gap-4 w-full">
          {imageUrl && (
            <div className="mt-4">
              <img
                src={imageUrl}
                alt="Pré-visualização da capa"
                className="w-4xs h-110 object-cover rounded-md border-2 border-gray-600"
                onError={(e) => {
                  e.target.style.display = "none";
                }}
              />
            </div>
          )}
          <div className="grid grid-cols-1 gap-6 w-xl">
            <button
              type="button"
              onClick={handleProcurarClick}
              disabled={isProcurando || !tituloValue}
              className={`${
                Type === "Cadastrar" ? "" : "hidden"
              } px-3 py-1 border border-indigo-500 text-indigo-400 text-sm font-medium rounded-md hover:bg-indigo-500 hover:text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed`}
            >
              {isProcurando ? "Carregando..." : "Carregar Dados"}
            </button>
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
                <p className="mt-1 w-full text-xs text-red-400">
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
              <div className="h-18">
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
                    validate: (value) => {
                      if (!totalCapitulosValue || isNaN(totalCapitulosValue))
                        return true;
                      return (
                        parseInt(value) <= parseInt(totalCapitulosValue) ||
                        "Não pode ser maior que o total de capítulos."
                      );
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

            {/* Botão de Envio Dinâmico */}
            <div className="mt-4">
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting ? "Salvando..." : submitButtonText}
              </button>
            </div>
            {/* Botão de Deletar (se aplicável) */}
            {Type === "Atualizar" && onDelete && (
              <div>
                <button
                  type="button"
                  onClick={() => onDelete(true)}
                  disabled={isDeleting}
                  className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isDeleting ? "Deletando..." : "Deletar Obra"}
                </button>
              </div>
            )}
          </div>
        </div>
      </form>
    </div>
  );
}

export default ObraForm;
