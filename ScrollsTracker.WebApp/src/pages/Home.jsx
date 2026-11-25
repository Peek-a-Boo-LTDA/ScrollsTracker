import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";

import { apiFetch } from "../api";

function Home() {
  const { data: obras = [], isLoading, isError } = useQuery({
    queryKey: ["lancamentos"],
    queryFn: async () => {
      const res = await apiFetch(`/api/ScrollsTracker/ObterLancamentos`);
      if (!res.ok) throw new Error("Falha ao carregar os lançamentos.");
      return res.json();
    },
  });

  if (isLoading) {
    return (
      <div className="space-y-4 p-4 min-h-screen">
        <h1 className="text-white">Carregando...</h1>
      </div>
    );
  }

  if (isError) return <p role="alert" className="p-4 text-white">Falha ao carregar os lançamentos.</p>;

  return (
    <section className="page-width page-content">
      <h1 className="page-title">Últimos lançamentos</h1>
      <MangaShelf obras={obras} mensagemVazia="Nenhum lançamento recente por ler." />
    </section>
  );
}

export default Home;
