import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Link, useOutletContext } from "react-router-dom";
import { useEffect, useState } from "react";
import MangaShelf from "../components/MangaShelf";
import { apiFetch } from "../api";

const tamanho = 18;

function Biblioteca() {
  const { podeCadastrarObras } = useOutletContext();
  const [texto, setTexto] = useState("");
  const [filtros, setFiltros] = useState({ titulo: "", statusLeitura: "", statusObra: "", pagina: 1 });

  useEffect(() => {
    const timer = setTimeout(() => {
      setFiltros((atual) => atual.titulo === texto.trim() ? atual : { ...atual, titulo: texto.trim(), pagina: 1 });
    }, 250);
    return () => clearTimeout(timer);
  }, [texto]);

  const { data, isPending, isError, isPlaceholderData } = useQuery({
    queryKey: ["obras", filtros],
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const parametros = new URLSearchParams({
        pagina: filtros.pagina, tamanho, titulo: filtros.titulo,
        statusObra: filtros.statusObra, statusLeitura: filtros.statusLeitura,
      });
      const resposta = await apiFetch(`/api/ScrollsTracker/Biblioteca?${parametros}`, { signal });
      if (!resposta.ok) throw new Error("Falha ao carregar a biblioteca.");
      return resposta.json();
    },
  });

  function mudarFiltro(evento) {
    const { name, value } = evento.target;
    setFiltros((atual) => ({ ...atual, [name]: value, titulo: texto.trim(), pagina: 1 }));
  }

  function irPara(pagina) {
    setFiltros((atual) => ({ ...atual, pagina }));
  }

  return (
    <div className="page-width page-content">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="page-title">Biblioteca</h1>
        {podeCadastrarObras && <Link to="/cadastrar-obra" className="button button-primary">Nova obra</Link>}
      </div>

      <div className="surface filter-bar">
        <input type="search" aria-label="Buscar por título ou nome alternativo" maxLength={300}
          placeholder="Buscar por título ou nome alternativo..." className="field min-w-0 flex-1 basis-52"
          value={texto} onChange={(evento) => setTexto(evento.target.value)} />
        <select className="field min-w-0 flex-1 basis-44" aria-label="Status de leitura"
          name="statusLeitura" value={filtros.statusLeitura} onChange={mudarFiltro}>
          <option value="">Status de leitura (todos)</option>
          <option>Lendo</option><option>Completo</option><option>Pausado</option>
          <option>Planejo Ler</option><option>Abandonado</option>
        </select>
        <select className="field min-w-0 flex-1 basis-44" aria-label="Status da obra"
          name="statusObra" value={filtros.statusObra} onChange={mudarFiltro}>
          <option value="">Status da obra (todos)</option>
          <option>Ongoing</option><option>Completed</option><option value="Hiatu">Hiatus</option>
        </select>
      </div>

      {isError && <p role="alert" className="surface empty-state">Falha ao carregar a biblioteca.</p>}
      {isPending && <p role="status" className="surface empty-state">Carregando biblioteca...</p>}
      <MangaShelf obras={data?.obras ?? []} disabled={isPlaceholderData}
        mensagemVazia={data && !isError ? "Nenhuma obra encontrada." : null} />
      {data?.totalPaginas > 1 && (
        <nav aria-label="Páginas da biblioteca" className="mt-4 flex flex-wrap items-center justify-between gap-3">
          <button type="button" className="button button-secondary" disabled={isPlaceholderData || data.pagina <= 1}
            onClick={() => irPara(data.pagina - 1)}>Anterior</button>
          <span aria-live="polite" className="text-sm text-gray-300">
            Página {data.pagina} de {data.totalPaginas} · {data.total} obras
          </span>
          <button type="button" className="button button-secondary" disabled={isPlaceholderData || data.pagina >= data.totalPaginas}
            onClick={() => irPara(data.pagina + 1)}>Próxima</button>
        </nav>
      )}
    </div>
  );
}

export default Biblioteca;
