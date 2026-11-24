import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";

import { apiFetch } from "../api";

function ObraForm({
  initialData,
  onSubmit,
  isSubmitting,
  submitButtonText = "Salvar",
  Type,
  isDeleting,
  onDelete,
  somenteLeitura = false,
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
      status: "",
      statusLeitura: "Lendo",
    },
  });

  const navigate = useNavigate();

  const procurarObraApi = async (titulo) => {
    if (!titulo) {
      throw new Error("O título não pode ser vazio.");
    }
    const response = await apiFetch(
      `/api/ScrollsTracker/ProcurarObraNasApisExternas?titulo=${encodeURIComponent(
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
    <div className="w-full max-w-4xl p-4">
      <form
        onSubmit={handleSubmit((dados) => onSubmit({
          ...dados,
          totalCapitulos: dados.totalCapitulos?.toString() ?? null,
          ultimoCapituloLido: dados.ultimoCapituloLido?.toString() || null,
          statusLeitura: dados.statusLeitura || null,
        }))}
        className="surface w-full p-4 sm:p-7"
      >
        <div className="flex flex-col gap-6 sm:flex-row sm:items-start">
          {imageUrl && (
            <div className="mx-auto w-36 shrink-0 sm:w-48">
              <img
                key={imageUrl}
                src={imageUrl}
                alt="Pré-visualização da capa"
                className="w-full h-auto object-contain rounded-md border-2 border-gray-600"
                onError={(e) => {
                  e.target.style.display = "none";
                }}
              />
            </div>
          )}
          <div className="grid min-w-0 flex-1 grid-cols-1 gap-6">
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
                readOnly={somenteLeitura}
                className={`field block w-full read-only:bg-slate-900/70 ${
                  errors.titulo ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Ex: One Piece"
                {...register("titulo", { required: !somenteLeitura && "O título é obrigatório." })}
              />
              {errors.titulo && (
                <p className="mt-1 w-full text-xs text-red-400">
                  {errors.titulo.message}
                </p>
              )}
            </div>
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
                readOnly={somenteLeitura}
                rows="3"
                className={`field block w-full read-only:bg-slate-900/70 ${
                  errors.descricao ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Uma breve descrição da obra..."
                {...register("descricao", {
                  required: !somenteLeitura && "A descrição é obrigatória.",
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
                readOnly={somenteLeitura}
                className={`field block w-full read-only:bg-slate-900/70 ${
                  errors.imagem ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="https://exemplo.com/capa.jpg"
                {...register("imagem", {
                  required: !somenteLeitura && "A URL da imagem é obrigatória.",
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
                  type="text"
                  id="totalCapitulos"
                  readOnly={somenteLeitura}
                  step="any"
                  pattern="[0-9]*[.]?[0-9]*"
                  className={`field block w-full read-only:bg-slate-900/70 ${
                    errors.totalCapitulos ? "border-red-500" : "border-gray-600"
                  }`}
                  placeholder="1100"
                  {...register("totalCapitulos", {
                    required: !somenteLeitura && "Campo obrigatório.",
                    min: somenteLeitura ? undefined : {
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
                  type="text"
                  id="ultimoCapituloLido"
                  inputMode="decimal"
                  pattern="[0-9]+([.,][0-9]+)?"
                  maxLength={24}
                  className={`field block w-full read-only:bg-slate-900/70 ${
                    errors.ultimoCapituloLido
                      ? "border-red-500"
                      : "border-gray-600"
                  }`}
                  placeholder="1050"
                  {...register("ultimoCapituloLido", {
                    required: Type === "Cadastrar" && "Campo obrigatório.",
                    min: {
                      value: 0,
                      message: "O valor não pode ser negativo.",
                    },
                    validate: (value) => {
                      const total = Number(String(totalCapitulosValue ?? "").replace(",", "."));
                      const capitulo = Number(String(value ?? "").replace(",", "."));
                      return !Number.isFinite(total) || total <= 0 || capitulo <= total ||
                        `O último capítulo lido deve ser no máximo ${totalCapitulosValue}.`;
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
                className={`field block w-full ${
                  errors.statusLeitura ? "border-red-500" : "border-gray-600"
                }`}
                {...register("statusLeitura")}
              >
                <option value="">Sem status</option>
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
                readOnly={somenteLeitura}
                className={`field block w-full read-only:bg-slate-900/70 ${
                  errors.status ? "border-red-500" : "border-gray-600"
                }`}
                placeholder="Em Andamento"
                {...register("status", {
                  required: !somenteLeitura && "O status da obra é obrigatório.",
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
                className="button button-primary w-full"
              >
                {isSubmitting ? "Salvando..." : submitButtonText}
              </button>
            </div>
            {/* Botão de Deletar */}
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
            <button
              type="button"
              onClick={() => {
                navigate("/biblioteca");
              }}
              className="button button-secondary"
            >
              Voltar
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}

export default ObraForm;
