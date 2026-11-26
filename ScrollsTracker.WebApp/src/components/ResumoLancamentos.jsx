import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { apiFetch } from "../api";

const rota = "/api/conta/resumo-lancamentos";
const chave = ["resumo-lancamentos"];

function ResumoLancamentos() {
  const queryClient = useQueryClient();
  const resumo = useQuery({
    queryKey: chave,
    queryFn: async ({ signal }) => {
      const resposta = await apiFetch(rota, { signal });
      if (!resposta.ok) throw new Error("Não foi possível carregar o resumo.");
      return resposta.json();
    },
  });
  const alterar = useMutation({
    mutationFn: async (frequencia) => {
      const resposta = await apiFetch(rota, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ frequencia }),
      });
      if (!resposta.ok) throw new Error("Não foi possível salvar a frequência.");
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: chave }),
  });
  const frequencia = resumo.data?.frequencia ?? "desativado";

  return (
    <section className="surface grid gap-4 p-4 sm:p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-xl font-semibold">Resumo de lançamentos</h2>
        <select aria-label="Frequência do resumo" className="field"
          value={alterar.isPending ? alterar.variables : frequencia}
          disabled={alterar.isPending || !resumo.data}
          onChange={(evento) => alterar.mutate(evento.target.value)}>
          <option value="desativado">Desativado</option>
          <option value="diario">Diário</option>
          <option value="semanal">Semanal</option>
        </select>
      </div>
      {resumo.isError && <p role="alert" className="text-sm text-red-300">{resumo.error.message}</p>}
      {alterar.isError && <p role="alert" className="text-sm text-red-300">{alterar.error.message}</p>}
      {frequencia !== "desativado" && (
        <>
          <p className="text-sm text-neutral-300">
            Capítulos detectados nas obras que você segue {frequencia === "diario" ? "nas últimas 24 horas" : "nos últimos 7 dias"}.
          </p>
          {resumo.data.obras.length === 0 ? <p className="text-sm text-neutral-400">Nenhum lançamento no período.</p> : (
            <ul className="divide-y divide-neutral-700">
              {resumo.data.obras.map(({ obraId, titulo, quantidade, ultimo }) => (
                <li key={obraId} className="flex flex-wrap justify-between gap-2 py-2">
                  <Link to={`/atualizar-obra/${obraId}`} className="font-medium hover:underline">{titulo}</Link>
                  <span className="text-sm text-neutral-300">
                    {quantidade === 1 ? "1 lançamento" : `${quantidade} lançamentos`} · Último capítulo {ultimo.numero}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </section>
  );
}

export default ResumoLancamentos;
