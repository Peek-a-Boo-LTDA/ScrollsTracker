import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "../api";

function HistoricoLancamentos({ id }) {
  const { data: lancamentos = [], isPending, isError } = useQuery({
    queryKey: ["historico-lancamentos", id],
    queryFn: async ({ signal }) => {
      const resposta = await apiFetch(`/api/ScrollsTracker/Obras/${id}/lancamentos`, { signal });
      if (!resposta.ok) throw new Error("Não foi possível carregar o histórico.");
      return resposta.json();
    },
  });

  return (
    <section className="surface mt-4 w-full max-w-4xl p-4 sm:p-7">
      <h2 className="mb-3 text-lg font-semibold text-white">Histórico de lançamentos</h2>
      {isPending && <p role="status" className="text-sm text-gray-300">Carregando histórico...</p>}
      {isError && <p role="alert" className="text-sm text-red-300">Não foi possível carregar o histórico.</p>}
      {!isPending && !isError && (lancamentos.length ? (
        <ul className="divide-y divide-slate-700">
          {lancamentos.map(({ numero, fonte, detectadoEm }) => (
            <li key={numero} className="flex flex-wrap items-center justify-between gap-2 py-2 text-sm">
              <span className="font-medium text-slate-100">Capítulo {numero}</span>
              <span className="text-slate-300">
                {fonte === "MangaUpdate" ? "MangaUpdates" : fonte} · {new Date(detectadoEm).toLocaleString("pt-BR")}
              </span>
            </li>
          ))}
        </ul>
      ) : <p className="text-sm text-gray-300">Ainda não há avanços de capítulo registrados para esta obra.</p>)}
    </section>
  );
}

export default HistoricoLancamentos;
