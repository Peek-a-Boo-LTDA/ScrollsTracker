import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";
import ResumoLancamentos from "../components/ResumoLancamentos";
import { apiFetch } from "../api";

async function carregarObras(rota, signal) {
  const resposta = await apiFetch(`/api/ScrollsTracker/${rota}`, { signal });
  if (!resposta.ok) throw new Error("Não foi possível carregar as obras.");
  return resposta.json();
}

function Home() {
  const [soSeguidas, setSoSeguidas] = useState(false);
  const continuar = useQuery({
    queryKey: ["continuar-lendo"],
    queryFn: ({ signal }) => carregarObras("ContinuarLendo", signal),
  });
  const lancamentos = useQuery({
    queryKey: ["lancamentos", soSeguidas],
    queryFn: ({ signal }) => carregarObras(`ObterLancamentos?soSeguidas=${soSeguidas}`, signal),
  });

  return (
    <div className="page-width page-content">
      <section className="grid gap-4" aria-labelledby="titulo-continuar">
        <h1 id="titulo-continuar" className="page-title">Continuar lendo</h1>
        {continuar.isPending && <p role="status">Carregando leituras...</p>}
        {continuar.isError && <p role="alert">Não foi possível carregar as leituras.</p>}
        {continuar.data && (
          <MangaShelf obras={continuar.data} mostrarPendentes mensagemVazia="Nenhuma leitura em andamento com capítulos pendentes." />
        )}
      </section>
      <section className="grid gap-4" aria-labelledby="titulo-lancamentos">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 id="titulo-lancamentos" className="page-title">Últimos lançamentos</h2>
          <label className="inline-flex items-center gap-2 text-sm text-neutral-200">
            <input type="checkbox" checked={soSeguidas} onChange={(evento) => setSoSeguidas(evento.target.checked)} />
            Só obras que eu sigo
          </label>
        </div>
        {lancamentos.isPending && <p role="status">Carregando lançamentos...</p>}
        {lancamentos.isError && <p role="alert">Não foi possível carregar os lançamentos.</p>}
        {lancamentos.data && (
          <MangaShelf obras={lancamentos.data} mensagemVazia={soSeguidas
            ? "Nenhum lançamento por ler nas obras que você segue."
            : "Nenhum lançamento recente por ler."} />
        )}
      </section>
      <ResumoLancamentos />
    </div>
  );
}

export default Home;
