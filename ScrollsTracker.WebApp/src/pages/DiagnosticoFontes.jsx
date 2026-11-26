import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, Navigate, useOutletContext } from "react-router-dom";
import { apiFetch } from "../api";

const estados = {
  confirmada: "Obra confirmada",
  semCorrespondencia: "Sem correspondência confirmada",
  semDados: "Sem dados",
  falha: "Falha na consulta",
};

function DiagnosticoFontes() {
  const { podeGerenciarObras } = useOutletContext();
  const [pagina, setPagina] = useState(1);
  const resultado = useQuery({
    queryKey: ["diagnostico-fontes", pagina],
    enabled: podeGerenciarObras,
    queryFn: async ({ signal }) => {
      const resposta = await apiFetch(`/api/ScrollsTracker/diagnostico-fontes?pagina=${pagina}`, { signal });
      if (!resposta.ok) throw new Error("Não foi possível carregar o diagnóstico.");
      return resposta.json();
    },
  });

  if (!podeGerenciarObras) return <Navigate to="/biblioteca" replace />;

  return (
    <section className="page-width page-content">
      <h1 className="page-title">Diagnóstico das fontes</h1>
      {resultado.isPending && <p role="status">Carregando consultas...</p>}
      {resultado.isError && <p role="alert" className="text-red-300">{resultado.error.message}</p>}
      {resultado.data?.obras.length === 0 && <p className="surface empty-state">Nenhuma obra cadastrada.</p>}
      {resultado.data?.obras.map(({ id, titulo, consultaFontesEm, ultimaConsultaFontes }) => (
        <article key={id} className="surface grid gap-2 p-4">
          <h2 className="text-lg font-semibold"><Link to={`/atualizar-obra/${id}`} className="hover:underline">{titulo}</Link></h2>
          <p className="text-sm text-neutral-300">
            {consultaFontesEm ? `Última consulta em ${new Date(consultaFontesEm).toLocaleString("pt-BR")}` : "Aguardando a primeira consulta registrada."}
          </p>
          <dl className="grid gap-x-6 gap-y-2 sm:grid-cols-2">
            {Object.entries(ultimaConsultaFontes).map(([fonte, estado]) => (
              <div key={fonte} className="flex flex-wrap justify-between gap-2 border-t border-neutral-700 pt-2 text-sm">
                <dt>{fonte === "MangaUpdate" ? "MangaUpdates" : fonte}</dt>
                <dd className={estado === "falha" ? "text-red-300" : "text-neutral-300"}>{estados[estado]}</dd>
              </div>
            ))}
          </dl>
        </article>
      ))}
      {resultado.data?.paginas > 1 && (
        <nav aria-label="Páginas do diagnóstico" className="flex flex-wrap items-center justify-between gap-3">
          <button className="button button-secondary" disabled={resultado.data.pagina === 1}
            onClick={() => setPagina(resultado.data.pagina - 1)}>Anterior</button>
          <span>Página {resultado.data.pagina} de {resultado.data.paginas}</span>
          <button className="button button-secondary" disabled={resultado.data.pagina === resultado.data.paginas}
            onClick={() => setPagina(resultado.data.pagina + 1)}>Próxima</button>
        </nav>
      )}
    </section>
  );
}

export default DiagnosticoFontes;
