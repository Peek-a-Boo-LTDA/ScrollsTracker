import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";
import { Link } from "react-router-dom";
import { useState, useMemo } from "react"; // useMemo é a chave aqui

const baseUrl = import.meta.env.VITE_API_BASE_URL;

function Biblioteca() {
  const [filtroTitulo, setFiltroTitulo] = useState("");
  const [filtroStatusLeitura, setFiltroStatusLeitura] = useState("");

  const [filtroStatusObra, setFiltroStatusObra] = useState("");

  const { data: todasAsObras = [], isLoading } = useQuery({
    queryKey: ["obras"],
    queryFn: async () => {
      const res = await fetch(`${baseUrl}/api/ScrollsTracker/Obras`);
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
    return <h1 className="text-white p-4">Carregando...</h1>;
  }

  return (
    <div className="space-y-4 p-4 min-h-screen">
      <div className="flex justify-between items-center pl-2 pr-2 mb-1 pt-4">
        <h1 className="text-white text-2xl">Biblioteca</h1>
        <Link
          to="/cadastrar-obra"
          className="text-white lg:block bg-transparent text-primary border hover:bg-primary border-primary hover:text-darkmode px-4 py-2 rounded-lg cursor-pointer"
        >
          Nova Obra
        </Link>
      </div>

      {/* --- SEÇÃO DE FILTROS --- */}
      <div className="flex flex-wrap gap-4 p-2">
        {/* Filtro Título */}
        <input
          type="text"
          placeholder="Buscar por título..."
          className="p-2 rounded-lg bg-gray-700 text-white flex-grow md:flex-grow-0 md:w-1/3"
          value={filtroTitulo}
          onChange={(e) => setFiltroTitulo(e.target.value)}
        />

        <select
          className="p-2 rounded-lg bg-gray-700 text-white"
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
          className="p-2 rounded-lg bg-gray-700 text-white"
          value={filtroStatusObra}
          onChange={(e) => setFiltroStatusObra(e.target.value)}
        >
          <option value="">Status Obra (Todos)</option>
          <option value="Ongoing">Ongoing</option>
          <option value="Completed">Completed</option>
          <option value="Hiatu">Hiatus</option>
        </select>
      </div>

      <div className="border-b-2 border-white pl-2 pr-2">
        <h1 className="text-white text-lg">Obras:</h1>
      </div>

      <MangaShelf obras={obrasFiltradas} />
    </div>
  );
}

export default Biblioteca;
