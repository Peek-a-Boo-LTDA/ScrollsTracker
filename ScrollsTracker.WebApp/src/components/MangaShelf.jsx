import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import MangaItem from "./MangaItem.jsx";
import { apiFetch } from "../api";

function progressoAtual(obra) {
  return { ultimoCapituloLido: obra.ultimoCapituloLido ?? null, statusLeitura: obra.statusLeitura ?? null };
}

function MangaShelf({ obras, mensagemVazia, disabled = false, mostrarPendentes = false }) {
  const [ultimaAcao, setUltimaAcao] = useState(null);
  const queryClient = useQueryClient();
  const atualizarConsultas = (obra) => Promise.all([
    ...["obras", "lancamentos", "continuar-lendo", "resumo-lancamentos"].map((chave) =>
      queryClient.invalidateQueries({ queryKey: [chave] })),
    queryClient.invalidateQueries({ queryKey: ["obra", String(obra.id)] }),
  ]);
  const seguir = useMutation({
    mutationFn: async (obra) => {
      const resposta = await apiFetch(`/api/ScrollsTracker/Obras/${obra.id}/seguindo`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ seguindo: !obra.seguindo }),
      });
      if (!resposta.ok) throw new Error("Não foi possível atualizar as obras seguidas.");
    },
    onSuccess: (_, obra) => atualizarConsultas(obra),
  });
  const leitura = useMutation({
    mutationFn: async ({ obra, dados }) => {
      const resposta = await apiFetch(`/api/ScrollsTracker/Obras/${obra.id}/leitura`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dados),
      });
      if (!resposta.ok) throw new Error("Não foi possível salvar a leitura. Tente novamente.");
    },
    onSuccess: (_, { obra, desfazer }) => {
      setUltimaAcao(desfazer ? null : { obra, anterior: progressoAtual(obra) });
      return atualizarConsultas(obra);
    },
  });

  function marcar(obra, capitulo) {
    leitura.mutate({ obra, dados: {
      ...progressoAtual(obra), ultimoCapituloLido: capitulo,
      statusLeitura: obra.statusLeitura ?? "Lendo",
    } });
  }

  return (
    <>
      {leitura.error && <p role="alert" className="mb-3 text-sm text-red-300">{leitura.error.message}</p>}
      {seguir.error && <p role="alert" className="mb-3 text-sm text-red-300">{seguir.error.message}</p>}
      {ultimaAcao && (
        <div className="surface mb-3 flex flex-wrap items-center justify-between gap-2 px-4 py-2">
          <span role="status" className="text-sm text-gray-300">Leitura de {ultimaAcao.obra.titulo} atualizada.</span>
          <button type="button" className="button button-secondary" disabled={leitura.isPending}
            onClick={() => leitura.mutate({ obra: ultimaAcao.obra, dados: ultimaAcao.anterior, desfazer: true })}>
            Desfazer
          </button>
        </div>
      )}
      {obras.length ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {obras.map((obra) => <MangaItem key={obra.id} obra={obra} onMarcar={marcar}
            onSeguir={seguir.mutate} mostrarPendentes={mostrarPendentes}
            disabled={disabled || leitura.isPending || seguir.isPending} />)}
        </div>
      ) : mensagemVazia && <p className="surface empty-state">{mensagemVazia}</p>}
    </>
  );
}

export default MangaShelf;
