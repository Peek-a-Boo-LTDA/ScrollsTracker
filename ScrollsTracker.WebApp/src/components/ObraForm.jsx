import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";

import { apiFetch } from "../api";

function lerTitulosAlternativos(texto = "") {
  return texto.split(/\r?\n/).map((titulo) => titulo.trim()).filter(Boolean);
}

function dadosDoFormulario(obra) {
  return {
    ...obra,
    numeroComFonte: obra.totalCapitulos,
    titulosAlternativosTexto: (obra.titulosAlternativos ?? []).join("\n"),
  };
}

const padronizarCapitulo = (valor) => String(valor ?? "").trim().replace(",", ".");

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
  const [busca, setBusca] = useState(null);
  const [mensagemBusca, setMensagemBusca] = useState("");
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
    defaultValues: dadosDoFormulario(initialData || {
      titulo: "",
      descricao: "",
      totalCapitulos: "",
      ultimoCapituloLido: 0,
      imagem: "",
      status: "",
      statusLeitura: "Lendo",
    }),
  });

  const navigate = useNavigate();

  const procurarObraApi = async ({ titulo, aliases }) => {
    const parametros = new URLSearchParams({ titulo, resumir: "true" });
    aliases.forEach((alias) => parametros.append("titulosAlternativos", alias));
    const response = await apiFetch(`/api/ScrollsTracker/CandidatosExternos?${parametros}`);
    if (!response.ok) {
      throw new Error("Não foi possível consultar as fontes agora.");
    }
    return response.json();
  };

  const imageUrl = watch("imagem");
  const tituloValue = watch("titulo");
  const totalCapitulosValue = watch("totalCapitulos");
  const fonteCapitulo = watch("fonteUltimoCapitulo");
  const dataDeteccao = watch("ultimoCapituloDetectadoEm");
  const capituloComFonte = watch("numeroComFonte");
  const mostrarFonte = fonteCapitulo && padronizarCapitulo(totalCapitulosValue) === padronizarCapitulo(capituloComFonte);

  function aplicarObra(obra, manterTitulo = false) {
    const atuais = getValues();
    const titulos = [
      ...lerTitulosAlternativos(atuais.titulosAlternativosTexto),
      ...(obra.titulosAlternativos ?? []),
      ...(manterTitulo ? [obra.titulo] : []),
    ].filter(Boolean);
    reset({
      ...atuais,
      titulo: manterTitulo ? atuais.titulo : obra.titulo,
      titulosAlternativosTexto: [...new Set(titulos)].slice(0, 50).join("\n"),
      vinculosExternos: obra.vinculosExternos,
      descricao: obra.descricao ?? atuais.descricao,
      imagem: obra.imagem ?? atuais.imagem,
      status: obra.status ?? atuais.status,
      totalCapitulos: obra.totalCapitulos ?? atuais.totalCapitulos,
      ...(obra.totalCapitulos != null && {
        numeroComFonte: obra.totalCapitulos,
        fonteUltimoCapitulo: obra.fonteUltimoCapitulo,
        ultimoCapituloDetectadoEm: obra.ultimoCapituloDetectadoEm,
      }),
    });
    setBusca(null);
    setMensagemBusca("Dados preenchidos. Revise antes de salvar.");
  }

  useEffect(() => {
    // 3. Sempre que 'totalCapitulos' mudar, dispare a validação de 'ultimoCapituloLido'
    trigger("ultimoCapituloLido");
  }, [totalCapitulosValue, trigger]);

  const { mutate: procurarObra, isPending: isProcurando } = useMutation({
    mutationFn: procurarObraApi,
    onSuccess: (data, { titulo }) => {
      if (titulo !== getValues("titulo").trim()) return;
      if (data.obra) {
        aplicarObra(data.obra, Type === "Atualizar");
      } else if (data.candidatos.length > 0) {
        setBusca({ titulo, candidatos: data.candidatos });
      } else {
        setMensagemBusca("Nenhuma obra encontrada. Tente outro título ou adicione um título alternativo.");
      }
    },
    onError: (error) => {
      setMensagemBusca(error.message);
    },
  });

  const selecao = useMutation({
    mutationFn: async ({ chave }) => {
      const resposta = await apiFetch(`/api/ScrollsTracker/CandidatosExternos/${chave}`);
      if (resposta.status === 404) throw new Error("As opções expiraram. Faça uma nova busca.");
      if (!resposta.ok) throw new Error("Não foi possível carregar os dados da opção. Tente novamente.");
      return resposta.json();
    },
    onSuccess: (obra, { titulo }) => {
      if (titulo === getValues("titulo").trim()) aplicarObra(obra);
    },
    onError: (erro) => setMensagemBusca(erro.message),
  });

  function escolherCandidato({ chave, obra }) {
    setMensagemBusca("");
    if (chave) selecao.mutate({ chave, titulo: getValues("titulo").trim() });
    else aplicarObra(obra);
  }

  useEffect(() => {
    if (initialData) {
      reset(dadosDoFormulario(initialData));
    }
  }, [initialData, reset]);

  const handleProcurarClick = () => {
    setBusca(null);
    setMensagemBusca("");
    procurarObra({
      titulo: getValues("titulo").trim(),
      aliases: lerTitulosAlternativos(getValues("titulosAlternativosTexto")).slice(0, 10),
    });
  };

  return (
    <div className="w-full max-w-4xl p-4">
      <form
        onSubmit={handleSubmit(({ titulosAlternativosTexto, numeroComFonte, ...dados }) => onSubmit({
          ...dados,
          titulosAlternativos: lerTitulosAlternativos(titulosAlternativosTexto),
          totalCapitulos: padronizarCapitulo(dados.totalCapitulos) || null,
          fonteUltimoCapitulo: padronizarCapitulo(dados.totalCapitulos) === padronizarCapitulo(numeroComFonte)
            ? dados.fonteUltimoCapitulo : null,
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
                maxLength={300}
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
            <div>
              <label htmlFor="titulosAlternativos" className="block text-sm font-medium text-gray-300 mb-1">
                Títulos alternativos
              </label>
              <textarea
                id="titulosAlternativos"
                rows={2}
                readOnly={somenteLeitura}
                placeholder="Um título por linha"
                className="field block w-full read-only:bg-slate-900/70"
                {...register("titulosAlternativosTexto", {
                  validate: (texto) => {
                    const aliases = lerTitulosAlternativos(texto);
                    return (aliases.length <= 50 && aliases.every((alias) => alias.length <= 300))
                      || "Use até 50 títulos alternativos de no máximo 300 caracteres.";
                  },
                })}
              />
              {errors.titulosAlternativosTexto && (
                <p className="mt-1 text-xs text-red-400">{errors.titulosAlternativosTexto.message}</p>
              )}
            </div>
            {!somenteLeitura && (
              <div className="space-y-3">
                <button
                  type="button"
                  onClick={handleProcurarClick}
                  disabled={isProcurando || selecao.isPending || !tituloValue?.trim()}
                  className="button button-secondary w-full disabled:opacity-50"
                >
                  {isProcurando ? "Buscando nas fontes..." : "Carregar dados"}
                </button>
                <p role="status" className={mensagemBusca ? "text-sm text-gray-300" : "sr-only"}>{mensagemBusca}</p>
                {busca && busca.titulo === tituloValue?.trim() && (
                  <section aria-label="Resultados da busca externa" className="space-y-2">
                    <h3 className="text-sm font-semibold text-gray-200">Selecione a obra</h3>
                    {busca.candidatos.map((candidato, indice) => {
                      const { obra, ano, formato, fontes, chave } = candidato;
                      const carregando = selecao.isPending && selecao.variables?.chave === chave;
                      return (
                      <button
                        key={chave ?? `${obra.titulo}-${ano}-${indice}`}
                        type="button"
                        disabled={selecao.isPending}
                        aria-busy={carregando}
                        onClick={() => escolherCandidato(candidato)}
                        className="flex min-h-16 w-full items-center gap-3 rounded-lg border border-slate-700 p-2 text-left hover:border-indigo-400 disabled:opacity-65"
                      >
                        {obra.imagem && <img src={obra.imagem} alt="" className="h-14 w-10 rounded object-cover" />}
                        <span className="min-w-0">
                          <span className="block break-words font-medium text-gray-100">{obra.titulo}</span>
                          <span className="block text-xs text-gray-400">{[ano, formato, ...fontes].filter(Boolean).join(" · ")}</span>
                          {carregando && <span role="status" className="block text-xs text-indigo-300">Carregando dados...</span>}
                        </span>
                      </button>
                      );
                    })}
                    <button type="button" className="text-sm text-indigo-300 hover:underline" disabled={selecao.isPending}
                      onClick={() => setBusca(null)}>
                      Preencher manualmente
                    </button>
                  </section>
                )}
              </div>
            )}
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
                {...register("descricao")}
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
                {...register("imagem")}
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
                  Último capítulo disponível
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
                {mostrarFonte && (
                  <p className="mt-1 text-xs text-gray-400">
                    Fonte: {fonteCapitulo === "MangaUpdate" ? "MangaUpdates" : fonteCapitulo}
                    {dataDeteccao ? ` · ${new Date(dataDeteccao).toLocaleString("pt-BR")}` : ""}
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
                {...register("status")}
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
                disabled={isSubmitting || isProcurando || selecao.isPending}
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
