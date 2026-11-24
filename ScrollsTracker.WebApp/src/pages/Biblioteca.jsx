import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";
import { Link, useOutletContext } from "react-router-dom";
import { useState, useMemo } from "react"; // useMemo é a chave aqui

import { apiFetch } from "../api";

function Biblioteca() {
  const { podeCadastrarObras } = useOutletContext();
  const [filtroTitulo, setFiltroTitulo] = useState("");
  const [filtroStatusLeitura, setFiltroStatusLeitura] = useState("");

  const [filtroStatusObra, setFiltroStatusObra] = useState("");

  const { data: todasAsObras = [], isLoading, isError } = useQuery({
    queryKey: ["obras"],
    queryFn: async () => {
      const res = await apiFetch(`/api/ScrollsTracker/Obras`);
      if (!res.ok) throw new Error("Falha ao carregar a biblioteca.");
      return res.json();
    },
  });

  // 2. O useMemo recalcula a lista filtrada QUANDO os filtros ou os dados mudarem.
  const obrasFiltradas = useMemo(() => {
    let obras = todasAsObras;

    if (filtroTitulo) {
      obras = obras.filter((obra) =>
        obra.titulo.toLowerCase().includes(filtroTitulo.toLowerCase())
      );
    }

    if (filtroStatusLeitura) {
      obras = obras.filter(
        (obra) => obra.statusLeitura === filtroStatusLeitura
      );
    }

    if (filtroStatusObra) {
      obras = obras.filter((obra) =>
        (obra.status || "")
          .toLowerCase()
          .includes(filtroStatusObra.toLowerCase())
      );
    }

    return obras;
  }, [
    todasAsObras,
    filtroTitulo,
    filtroStatusLeitura,
    filtroStatusObra, // Adiciona o novo filtro nas dependências
  ]);

  if (isLoading) {
    return (
      <div className="space-y-4 p-4 min-h-screen">
        <h1 className="text-white">Carregando...</h1>
      </div>
    );
  }

  if (isError) return <p role="alert" className="p-4 text-white">Falha ao carregar a biblioteca.</p>;

  return (
    <div className="page-width page-content">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="page-title">Biblioteca</h1>
        {podeCadastrarObras && <Link
          to="/cadastrar-obra"
          className="button button-primary"
        >
          Nova Obra
        </Link>}
      </div>

      {/* --- SEÇÃO DE FILTROS --- */}
      <div className="surface filter-bar">
        {/* Filtro Título */}
        <input
          type="text"
          aria-label="Buscar por título"
          placeholder="Buscar por título..."
          className="field min-w-0 flex-1 basis-52"
          value={filtroTitulo}
          onChange={(e) => setFiltroTitulo(e.target.value)}
        />

        <select
          className="field min-w-0 flex-1 basis-44"
          aria-label="Status de leitura"
          value={filtroStatusLeitura}
          onChange={(e) => setFiltroStatusLeitura(e.target.value)}
        >
          <option value="">Status Leitura (Todos)</option>
          <option value="Lendo">Lendo</option>
          <option value="Completo">Completo</option>
          <option value="Pausado">Pausado</option>
          <option value="Planejo Ler">Planejo Ler</option>
        </select>

        <select
          className="field min-w-0 flex-1 basis-44"
          aria-label="Status da obra"
          value={filtroStatusObra}
          onChange={(e) => setFiltroStatusObra(e.target.value)}
        >
          <option value="">Status Obra (Todos)</option>
          <option value="Ongoing">Ongoing</option>
          <option value="Completed">Completed</option>
          <option value="Hiatu">Hiatus</option>
        </select>
      </div>

      {obrasFiltradas.length > 0 ? <MangaShelf obras={obrasFiltradas} /> : (
        <p className="surface empty-state">Nenhuma obra encontrada.</p>
      )}
    </div>
  );
}

export default Biblioteca;
